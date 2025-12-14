using Fed.Core.Enums;
using Fed.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fed.Core.Entities
{
    public class FullCustomerInfo : Customer
    {
        public FullCustomerInfo(
            Guid id,
            string shortId,
            string companyName,
            string website,
            int? acAccountNumber,
            bool? isInvoiceable,
            int? officeSizeMin,
            int? officeSizeMax,
            bool? isDeliveryChargeExempt,
            bool? splitDeliveriesByOrder,
            bool isTestAccount,
            AccountType accountTypeId,
            string source,
            string notes,
            bool isFriend,
            string cancellationReason,
            DateTime? registerDate = null,
            DateTime? firstDeliveryDate = null,
            DateTime? lastDeliveryDate = null)
            : base(
                  id,
                  shortId,
                  companyName,
                  website,
                  acAccountNumber,
                  isInvoiceable,
                  officeSizeMin,
                  officeSizeMax,
                  isDeliveryChargeExempt,
                  splitDeliveriesByOrder,
                  isTestAccount,
                  accountTypeId,
                  source,
                  notes,
                  isFriend,
                  cancellationReason,
                  registerDate,
                  firstDeliveryDate,
                  lastDeliveryDate)
        {
        }

        public IList<Delivery> Deliveries { get; set; }

        public IList<Order> PastOrders => Deliveries?.SelectMany(d => d.Orders).ToList();

        public decimal TotalSalesValue
        {
            get
            {
                if (Deliveries == null || Deliveries.Count == 0)
                    return 0;

                return
                    Deliveries.Sum(d =>
                        d.Orders != null
                            ? d.Orders.Sum(o =>
                                o.OrderItems != null
                                    ? o.OrderItems.Sum(i => i.Quantity * (i.SalePrice ?? i.Price))
                                    : 0)
                            : 0);
            }
        }

        public decimal TotalSalesLastWeek
        {
            get
            {
                if (Deliveries == null || Deliveries.Count == 0)
                    return 0;

                var lastWeekDeliveries =
                    Deliveries
                        .Where(d =>
                            d.DeliveryDate.Value >= DateTime.Today.MondayOfPreviousWeek()
                            && d.DeliveryDate.Value <= DateTime.Today.MondayOfPreviousWeek().AddDays(5))
                        .ToList();

                if (lastWeekDeliveries == null || lastWeekDeliveries.Count == 0)
                    return 0;

                return
                    lastWeekDeliveries.Sum(d =>
                        d.Orders != null
                            ? d.Orders.Sum(o =>
                                o.OrderItems != null
                                    ? o.OrderItems.Sum(i => i.Quantity * (i.SalePrice ?? i.Price))
                                    : 0)
                            : 0);
            }
        }
    }
}