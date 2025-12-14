using Fed.Core.Enums;
using Fed.Core.Extensions;
using Fed.Core.ValueTypes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fed.Core.Entities
{
    public class Delivery
    {
        public Guid Id { get; set; }
        public string ShortId { get; set; }

        public Guid ContactId { get; set; }
        public Guid DeliveryAddressId { get; set; }
        public Date DeliveryDate { get; set; }
        public Guid TimeslotId { get; set; }

        public TimeSpan EarliestTime { get; set; }
        public TimeSpan LatestTime { get; set; }

        public string DeliveryCompanyName { get; set; }
        public string DeliveryFullName { get; set; }
        public string DeliveryAddressLine1 { get; set; }
        public string DeliveryAddressLine2 { get; set; }
        public string DeliveryTown { get; set; }
        public string DeliveryPostcode { get; set; }
        public string DeliveryInstructions { get; set; }
        public bool LeaveDeliveryOutside { get; set; }
        public decimal DeliveryCharge { get; set; }

        public int PackingStatusId { get; set; }
        public int BagCount { get; set; }
        public PackingStatus PackingStatus
        {
            get { return (PackingStatus)PackingStatusId; }
            set { PackingStatusId = (int)value; }
        }

        public IList<Order> Orders { get; set; }
        public IList<DeliveryShortage> DeliveryShortages { get; set; }
        public IList<DeliveryAddition> DeliveryAdditions { get; set; }

        public int SortIndex
        {
            get
            {
                var hour = int.Parse(ShortId.Split('-')[1]);
                var index = int.Parse(ShortId.Split('-')[2]);
                return hour * 100 + index;
            }
        }

        public IList<OrderItem> GetAggregatedOrderItems(bool splitByPrice = false)
        {
            var orderItems = new List<OrderItem>();

            if (Orders is null || Orders.Count == 0)
                return orderItems;

            foreach (var order in Orders)
            {
                if (order.OrderItems == null || order.OrderItems.Count == 0)
                    continue;

                foreach (var item in order.OrderItems)
                {
                    //create copy of item to avoid updating quantity of original reference
                    var newItem = new OrderItem
                    {
                        OrderId = item.OrderId,
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        ProductCode = item.ProductCode,
                        ProductGroup = item.ProductGroup,
                        ProductName = item.ProductName,
                        SupplierId = item.SupplierId,
                        SupplierSKU = item.SupplierSKU,
                        Price = item.Price,
                        SalePrice = item.SalePrice,
                        IsTaxable = item.IsTaxable
                    };

                    var existingItem = orderItems.SingleOrDefault(i => i.ProductId == item.ProductId && 
                                                                       (!splitByPrice || i.ActualPrice == item.ActualPrice));

                    if (existingItem == default(OrderItem))
                        orderItems.Add(newItem);
                    else
                        existingItem.Quantity += item.Quantity;
                }
            }

            return orderItems;
        }

        public decimal GetOrderItemsTotal()
        {
            var orderItems = Orders.SelectMany(o => o.OrderItems);
            return orderItems.Sum(o => o.Quantity * o.ActualPrice);
        }

        public decimal GetDeliveryTotal()
        {
            decimal itemsTotal = GetOrderItemsTotal();
            decimal discountTotal = Orders.SelectMany(o => o.OrderDiscounts.OrEmptyIfNull())?.Sum(d => d.OrderTotalDeduction) ?? 0;
            return itemsTotal + DeliveryCharge - discountTotal;
        }

        public bool IsFirstDelivery() =>
            Orders != null && Orders.Count > 0 && Orders.Any(o => o.IsFirstOrder);

        public bool HasShortages() =>
            DeliveryShortages != null
            && DeliveryShortages.Count > 0;

        public bool HasAdditions() =>
            DeliveryAdditions != null
            && DeliveryAdditions.Count > 0;

        public bool HasShortagesSince(TimeSpan time) =>
            DeliveryShortages != null
            && DeliveryShortages.Count > 0
            && DeliveryShortages.Any(s => s.TimeRecorded >= time);

        public bool HasAdditionsSince(TimeSpan time) =>
            DeliveryAdditions != null
            && DeliveryAdditions.Count > 0
            && DeliveryAdditions.Any(a => a.TimeRecorded >= time);
    }
}