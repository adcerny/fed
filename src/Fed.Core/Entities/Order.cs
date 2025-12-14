using Fed.Core.Extensions;
using Fed.Core.ValueTypes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fed.Core.Entities
{
    public class Order
    {
        public Guid Id { get; set; }
        public string ShortId { get; set; }
        public Guid RecurringOrderId { get; set; }
        public int WeeklyRecurrence { get; set; }
        public string OrderName { get; set; }
        public Date DeliveryDate { get; set; }
        public DateTime OrderGeneratedDate { get; set; }

        public Guid TimeslotId { get; set; }
        public TimeSpan EarliestTime { get; set; }
        public TimeSpan LatestTime { get; set; }


        public Guid HubId { get; set; }
        public string HubName { get; set; }
        public string HubPostCode { get; set; }
        public string HubAddressLine1 { get; set; }
        public string HubAddressLine2 { get; set; }

        public Guid ContactId { get; set; }
        public string ContactShortId { get; set; }
        public string ContactFirstName { get; set; }
        public string ContactLastName { get; set; }
        public string ContactEmail { get; set; }
        public string ContactPhone { get; set; }

        public Guid CustomerId { get; set; }
        public string CustomerShortId { get; set; }
        public string CompanyName { get; set; }

        public Guid DeliveryAddressId { get; set; }
        public string DeliveryFullName { get; set; }
        public string DeliveryCompanyName { get; set; }
        public string DeliveryAddressLine1 { get; set; }
        public string DeliveryAddressLine2 { get; set; }
        public string DeliveryTown { get; set; }
        public string DeliveryPostcode { get; set; }
        public string DeliveryInstructions { get; set; }
        public bool LeaveDeliveryOutside { get; set; }

        public int PaymentMethodId { get; set; }

        public Guid BillingAddressId { get; set; }
        public string BillingFullName { get; set; }
        public string BillingCompanyName { get; set; }
        public string BillingAddressLine1 { get; set; }
        public string BillingAddressLine2 { get; set; }
        public string BillingTown { get; set; }
        public string BillingPostcode { get; set; }
        public string BillingEmail { get; set; }
        public bool SplitDeliveriesByOrder { get; set; }

        private IList<OrderItem> _orderItems;
        public IList<OrderItem> OrderItems
        {
            get
            {
                _orderItems?.RemoveAll(o => o.Quantity < 1);
                return _orderItems;
            }
            set
            {
                value?.RemoveAll(o => o.Quantity < 1);
                _orderItems = value;
            }
        }

        public IList<OrderDiscount> OrderDiscounts { get; set; }

        public bool IsFirstOrder { get; set; }
        public bool IsFree { get; set; }

        public int SortIndex
        {
            get { return ShortId != null && ShortId.Contains('-') ? int.TryParse(ShortId.Split('-')[1], out int i) ? i : -1 : -1; }
        }

        public decimal OrderItemsTotal
        {
            get { return OrderItems != null ? OrderItems.Sum(o => o.Quantity * o.ActualPrice) : 0; }
        }

        public decimal OrderTotal
        {
            get { return OrderItemsTotal; }
        }

        public bool HasOrderItems() =>
            OrderItems != null
            && OrderItems.Count > 0;
    }
}