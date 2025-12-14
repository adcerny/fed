using Fed.Core.Data.Commands;
using Fed.Core.Data.Handlers;
using Fed.Core.Data.Queries;
using Fed.Core.Entities;
using Fed.Core.Services.Interfaces;
using Fed.Core.ValueTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fed.Core.Services
{
    public class InvoiceService : IInvoiceService
    {
        private readonly IInvoicesHandler _invoicesHandler;
        private readonly IOrdersHandler _ordersHandler;
        private readonly IExternalInvoiceService _externalInvoicesService;

        public InvoiceService(
            IInvoicesHandler invoiceHandler,
            IOrdersHandler ordersHandler,
            IExternalInvoiceService externalInvoicesService)
        {
            _invoicesHandler = invoiceHandler ?? throw new ArgumentNullException(nameof(invoiceHandler));
            _ordersHandler = ordersHandler ?? throw new ArgumentNullException(nameof(ordersHandler));
            _externalInvoicesService = externalInvoicesService ?? throw new ArgumentNullException(nameof(externalInvoicesService));
        }

        public async Task<IList<Invoice>> GenerateInvoices(DateRange dateRange)
        {
            var invoices = await _invoicesHandler.ExecuteAsync(dateRange);

            return invoices;
        }

        public async Task<IList<Invoice>> GetInvoices(Guid contactId)
        {
            var invoices = await _invoicesHandler.ExecuteAsync(new GetInvoicesQuery(contactId));

            if (invoices != null && invoices.Any())
            {
                var externalInvoices = await _externalInvoicesService.GetInvoices(invoices.Select(i => Guid.Parse(i.ExternalInvoiceId)));
                if (externalInvoices.Any())
                {
                    //get invoice amount from external invoice service - this may have changed since it was generated.
                    foreach (var i in invoices)
                        i.TotalAmount = externalInvoices.FirstOrDefault(e => e.ExternalInvoiceNumber == i.ExternalInvoiceNumber)?.TotalAmount;

                    return invoices.Where(i => i.TotalAmount != null).ToList();
                }
            }
            return null;
        }

        public async Task<Invoice> GetInvoice(Guid Id)
        {
            var invoice = await _invoicesHandler.ExecuteAsync(new GetByIdQuery<Invoice>(Id));

            return invoice;
        }

        public async Task<Guid> CreateInvoice(Invoice invoice)
        {
            var id = await _invoicesHandler.ExecuteAsync(new CreateCommand<Invoice>(invoice));
            return id;
        }

        public async Task<bool> UpdateInvoice(Invoice invoice)
        {
            await _invoicesHandler.ExecuteAsync(new UpdateCommand<Invoice>(invoice.Id, invoice));
            return true;
        }

        public async Task<byte[]> GetInvoicePdf(Guid id)
        {
            var invoice = await _invoicesHandler.ExecuteAsync(new GetByIdQuery<Invoice>(id));
            var bytes = await _externalInvoicesService.GetExternalInvoicePdf(invoice.ExternalInvoiceId);
            return bytes;
        }
    }
}
