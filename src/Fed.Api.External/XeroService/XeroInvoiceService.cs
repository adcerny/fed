using Fed.Core.Entities;
using Fed.Core.Extensions;
using Fed.Core.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xero.Api.Core.Model;
using Xero.Api.Core.Model.Status;
using Xero.Api.Core.Model.Types;

namespace Fed.Api.External.XeroService
{
    public class XeroInvoiceService : XeroService, IExternalInvoiceService
    {
        public XeroInvoiceService(XeroSettings settings, ILogger logger) :
            base(settings, logger)
        {
        }

        public async Task<Xero.Api.Core.Model.Invoice> RaiseInvoice(Fed.Core.Entities.Invoice fedInvoice)
        {
            try
            {
                var createdInvoice = await CreateInvoice(fedInvoice);

                var payments = await CreatePayments(fedInvoice, createdInvoice);

                var creditNote = await CreateCreditNote(fedInvoice, createdInvoice, payments?.Count() == 0);

                if (creditNote != null && fedInvoice.Refunds.OrEmptyIfNull().Count() > 0)
                    await CreateRefunds(fedInvoice, creditNote);

                return createdInvoice;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error raising invoice for contact Id {fedInvoice.ContactId} using Xero API. Error was {ex.ToString()}.");
                throw new Exception(ex.ToString());
            }
        }

        private async Task<Xero.Api.Core.Model.Invoice> CreateInvoice(Core.Entities.Invoice fedInvoice)
        {
            var contact = await GetXeroContact(fedInvoice.Deliveries.FirstOrDefault().Orders.FirstOrDefault().CustomerShortId);

            _logger.LogInformation($"Raising invoice for contact {contact.Name}...");

            var invoice = new Xero.Api.Core.Model.Invoice
            {
                Contact = await GetXeroContact(fedInvoice.Deliveries.FirstOrDefault().Orders.FirstOrDefault().CustomerShortId),
                Reference = fedInvoice.Reference,
                Type = InvoiceType.AccountsReceivable,
                Status = InvoiceStatus.Authorised,
                LineAmountTypes = LineAmountType.Inclusive,
                CurrencyCode = "GBP",
                DueDate = fedInvoice.ToDate.AddDays(21),
                LineItems = GetLineItems(fedInvoice.Deliveries)
            };

            var createdInvoice = await _api.Invoices.CreateAsync(invoice);
            _logger.LogInformation($"Xero invoice number {createdInvoice.Number} for contact Id {fedInvoice.ContactId} successfully raised.");
            Thread.Sleep(1000); //sleep for 1 second to avoid api rate limit of 60 calls per minute
            return createdInvoice;
        }

        private async Task<List<Payment>> CreatePayments(Core.Entities.Invoice fedInvoice, Xero.Api.Core.Model.Invoice invoice)
        {
            var payments = new List<Payment>();

            foreach (var transaction in fedInvoice.Payments.OrEmptyIfNull())
            {
                _logger.LogInformation($"Raising payment of £{transaction.AmountCaptured} for {invoice.Contact.Name}...");

                var payment = new Payment
                {
                    Invoice = invoice,
                    Amount = transaction.AmountCaptured,
                    Account = new Account { Code = "750" },
                    Reference = transaction.Id.ToString(),
                    Date = transaction.TimeModified,
                    Status = PaymentStatus.Authorised
                };

                var createdPayment = await _api.Payments.CreateAsync(payment);
                _logger.LogInformation($"Payment {createdPayment.Id} for {invoice.Contact.Name} successfully raised.");
                Thread.Sleep(1000); //sleep for 1 second to avoid api rate limit of 60 calls per minute

                payments.Add(createdPayment);
            }

            return payments;
        }

        private async Task<CreditNote> CreateCreditNote(Fed.Core.Entities.Invoice fedInvoice, Xero.Api.Core.Model.Invoice xeroInvoice, bool allocate)
        {
            var shortages = fedInvoice.Deliveries.SelectMany(d => d.DeliveryShortages.OrEmptyIfNull()).ToList();

            if (shortages.Count == 0)
                return null;

            _logger.LogInformation($"Creating credit note for contact {xeroInvoice.Contact.Name}...");

            var creditNote = new CreditNote
            {
                Contact = xeroInvoice.Contact,
                Status = InvoiceStatus.Authorised,
                Type = CreditNoteType.AccountsReceivable,
                LineAmountTypes = LineAmountType.Inclusive,
                LineItems = new List<LineItem>()
            };

            foreach (var shortage in shortages)
            {
                var delivery = fedInvoice.Deliveries.Where(d => d.Orders
                                                    .Where(o => o.Id == shortage.OrderId).Any())
                                                    .Single();

                var orderItem = delivery.Orders
                                        .SelectMany(o => o.OrderItems)
                                        .Where(i => i.OrderId == shortage.OrderId && i.ProductId == shortage.ProductId)
                                        .Single();

                var replacements = shortage.Replacements.OrEmptyIfNull();


                int suppliedQuantity = shortage.DesiredQuantity - shortage.ActualQuantity;

                var lineItem = new LineItem
                {
                    Description = $"{delivery.DeliveryDate.Value.ToString("MMMM d")} - undelivered- {orderItem.ProductName}",
                    AccountCode = "101",
                    Quantity = shortage.DesiredQuantity - shortage.ActualQuantity,
                    UnitAmount = orderItem.RefundablePrice,
                    ItemCode = orderItem.ProductCode.Truncate(XeroConstants.MaxItemCodeLength),
                    TaxType = orderItem.IsTaxable ? "OUTPUT2" : "ZERORATEDOUTPUT"
                };
                creditNote.LineItems.Add(lineItem);

                if (shortage.Replacements != null && shortage.Replacements.Any())
                {
                    decimal refundAmount = Math.Min((orderItem.RefundablePrice * suppliedQuantity), replacements.Sum(r => r.ProductPrice * r.Quantity));
                    int totalQty = shortage.Replacements.Sum(r => r.Quantity);
                    decimal unitPrice = refundAmount / totalQty;

                    foreach (var replacement in shortage.Replacements)
                    {
                        var replacementItem = new LineItem
                        {
                            Description = $"{delivery.DeliveryDate.Value.ToString("MMMM d")} - replacement - {replacement.ProductName}",
                            AccountCode = "101",
                            Quantity = replacement.Quantity,
                            UnitAmount = -unitPrice,
                            ItemCode = replacement.ProductCode.Truncate(XeroConstants.MaxItemCodeLength),
                            TaxType = replacement.IsTaxable ? "OUTPUT2" : "ZERORATEDOUTPUT"
                        };
                        creditNote.LineItems.Add(replacementItem);
                    }
                }
            }

            var createdCreditNote = await _api.CreditNotes.CreateAsync(creditNote);
            _logger.LogInformation($"Credit note {createdCreditNote.Id} for contact {xeroInvoice.Contact.Name} successfully raised.");
            Thread.Sleep(1000);

            if (allocate && createdCreditNote.RemainingCredit != 0)
            {
                var result = await _api.Allocations.AddAsync(new CreditNoteAllocation
                {
                    AppliedAmount = createdCreditNote.Total,
                    CreditNote = new CreditNote { Id = createdCreditNote.Id },
                    Invoice = new Xero.Api.Core.Model.Invoice { Id = xeroInvoice.Id },
                });
                _logger.LogInformation($"Credit note {createdCreditNote.Id} for contact {xeroInvoice.Contact.Name} successfully allocated to invoice {xeroInvoice.Number} .");
                Thread.Sleep(1000);
            }

            return createdCreditNote;
        }

        private async Task<List<Payment>> CreateRefunds(Core.Entities.Invoice fedInvoice, CreditNote creditNote)
        {
            var createdRefunds = new List<Payment>();

            var refunds = fedInvoice.Refunds.Select(p => new Payment
            {
                Amount = Math.Abs(p.AmountCaptured.Value),
                Account = new Account { Code = "750" },
                Reference = p.Id.ToString(),
                Date = p.TimeModified,
                Status = PaymentStatus.Authorised,
                CreditNote = new CreditNote { Id = creditNote.Id }
            }).ToList();

            foreach (var refund in refunds)
            {
                _logger.LogInformation($"Creating refund of £{refund.Amount}...");
                var createdRefund = await _api.Payments.CreateAsync(refund);
                _logger.LogInformation($"Refund of £{refund.Amount} successfully allocated to creditNote {creditNote.Number}");
                Thread.Sleep(1000); //sleep for 1 second to avoid api rate limit of 60 calls per minute
                createdRefunds.Add(createdRefund);
            }

            return createdRefunds;
        }

        private async Task<Xero.Api.Core.Model.Contact> GetXeroContact(string customerShortId)
        {
            var contacts = await _api.Contacts
                .Where(string.Format("AccountNumber == \"{0}\"", customerShortId))
                .FindAsync();

            Thread.Sleep(1000);

            return contacts.Single();
        }

        public async Task<byte[]> GetExternalInvoicePdf(string externalInvoiceId)
        {
            Guid id;
            if (!Guid.TryParse(externalInvoiceId, out id))
                throw new ArgumentException("externalInvoiceId must be a Guid");

            var onlineInvoiceUrl = await _api.Invoices.RetrieveOnlineInvoiceUrlAsync(id);
            Xero.Api.Core.File.BinaryFile pdfFile = await _api.PdfFiles.GetAsync(PdfEndpointType.Invoices, id);
            return pdfFile.Content;
        }

        public async Task<IList<Fed.Core.Entities.Invoice>> GetInvoices(IEnumerable<Guid> externalInvoiceIds)
        {
            var invoices = await _api.Invoices.Ids(externalInvoiceIds)
                .FindAsync();

            return invoices?.Select(i => new Fed.Core.Entities.Invoice
            {
                DateGenerated = i.Date.Value,
                ExternalInvoiceNumber = i.Number,
                TotalAmount = i.Total
            }).ToList();
        }

        public async Task VoidInvoices(DateTime date)
        {
            var invoices = await _api.Invoices
                .Where(string.Format("Date == DateTime({0},{1},{2})", date.Year, date.Month, date.Day))
                .FindAsync();

            foreach (var invoice in invoices)
            {
                foreach (var payment in invoice.Payments)
                {
                    var resultAfterDeletePayments = await _api.UpdateAsync(new Payment
                    {
                        Id = payment.Id,
                        Status = PaymentStatus.Deleted
                    });
                    Thread.Sleep(1000);
                }

                invoice.Status = InvoiceStatus.Voided;
                await _api.UpdateAsync(invoice);
                Thread.Sleep(1000);
                Console.WriteLine($"Invoice {invoice.Number} Voided");
            }
        }

        private List<LineItem> GetLineItems(List<Delivery> deliveries)
        {
            var lineItems = new List<LineItem>();
            foreach (var delivery in deliveries.OrderBy(o => o.DeliveryDate))
            {
                foreach (var order in delivery.Orders.OrderBy(o => o.DeliveryDate))
                {
                    foreach (var item in order.OrderItems.OrEmptyIfNull().OrderBy(i => i.ProductName))
                    {
                        lineItems.Add(new LineItem
                        {
                            AccountCode = "100",
                            ItemCode = item.ProductCode.Truncate(XeroConstants.MaxItemCodeLength),
                            Description = $"{order.DeliveryDate.Value.ToString("MMMM d")} - {item.ProductName}",
                            UnitAmount = order.IsFree ? 0 : item.Price,
                            Quantity = item.Quantity,
                            TaxType = item.IsTaxable ? "OUTPUT2" : "ZERORATEDOUTPUT"
                        });
                        if (!order.IsFree && item.SalePrice != null)
                        {
                            lineItems.Add(new LineItem
                            {
                                // 23/08/2019:
                                // 260 (Direct Wages) is not an output account, therefore we (Dustin + Kia) changed it to 110 (= Promotions)
                                // The error might have happened that the demo account has 260 assigned to "Other Revenue", but this is not
                                // the case in the live production system.
                                AccountCode = "110",
                                Description = $"{order.DeliveryDate.Value.ToString("MMMM d")} - {item.ProductName} discount",
                                ItemCode = item.ProductCode.Truncate(XeroConstants.MaxItemCodeLength),
                                UnitAmount = item.Quantity * (item.SalePrice - item.Price),
                                TaxType = item.IsTaxable ? "OUTPUT2" : "ZERORATEDOUTPUT"
                            });
                        }
                    }
                    AddDiscount(lineItems, order);
                }

                if (delivery.DeliveryCharge > 0)
                    lineItems.Add(new LineItem
                    {
                        AccountCode = "170",
                        Description = $"{delivery.DeliveryDate.Value.ToString("MMMM d")} - Delivery charge",
                        UnitAmount = delivery.DeliveryCharge,
                        Quantity = 1
                    });
            }

            return lineItems;
        }

        private static void AddDiscount(List<LineItem> lineItems, Order order)
        {
            foreach (var discount in order.OrderDiscounts.OrEmptyIfNull())
            {
                //work out proportion of cost that is taxable, and apply discount proportionally
                var total = lineItems.Where(i => i.AccountCode == "100" || i.AccountCode == "110").Sum(i => i.UnitAmount * i.Quantity);

                var nonTaxableTotal = lineItems.Where(i => i.AccountCode == "100" && i.TaxType == "ZERORATEDOUTPUT").Sum(i => i.UnitAmount * i.Quantity);

                var nonTaxableDiscount = decimal.Round((discount.OrderTotalDeduction * (nonTaxableTotal / total)).Value, 2, MidpointRounding.AwayFromZero);

                var taxableDiscount = discount.OrderTotalDeduction - nonTaxableDiscount;

                if (taxableDiscount > 0)
                    lineItems.Add(new LineItem
                    {
                        AccountCode = "115",
                        Description = $"{order.DeliveryDate.Value.ToString("MMMM d")} - {discount.Name} for VAT items",
                        UnitAmount = -taxableDiscount,
                        TaxType = "OUTPUT2"
                    });
                if (nonTaxableDiscount > 0)
                    lineItems.Add(new LineItem
                    {
                        AccountCode = "115",
                        Description = $"{order.DeliveryDate.Value.ToString("MMMM d")} - {discount.Name} for non-VAT items",
                        UnitAmount = -nonTaxableDiscount
                    });
            }
        }


    }
}
