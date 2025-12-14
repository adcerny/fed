using Fed.Core.Entities;
using Fed.Core.Enums;
using System.Collections.Generic;
using System.Linq;

namespace Fed.Web.Portal.Extensions
{
    public static class DeliveryExtensions
    {
        private static IOrderedEnumerable<Delivery> SortDeliveries(
            IOrderedEnumerable<Delivery> orderedDeliveries,
            DeliverySortOrder sortOrder)
        {
            switch (sortOrder)
            {
                case DeliverySortOrder.Slot:
                    return orderedDeliveries.ThenBy(d => d.EarliestTime);
                case DeliverySortOrder.DeliveryId:
                    return orderedDeliveries.ThenBy(d => d.SortIndex);
                case DeliverySortOrder.CompanyName:
                    return orderedDeliveries.ThenBy(d => d.DeliveryCompanyName);
                case DeliverySortOrder.FirstDelivery:
                    return orderedDeliveries.ThenBy(d => d.IsFirstDelivery());
                case DeliverySortOrder.ItemCount:
                    return orderedDeliveries.ThenBy(d => d.GetAggregatedOrderItems().Count);
                default:
                    return orderedDeliveries;
            }
        }

        public static IList<Delivery> SortDeliveries(
            this IList<Delivery> deliveries,
            DeliveryPickOrder pickOrder)
        {
            IOrderedEnumerable<Delivery> orderedDeliveries = null;

            switch (pickOrder.SortBy1)
            {
                case DeliverySortOrder.Slot:
                    orderedDeliveries = deliveries.OrderBy(d => d.EarliestTime); break;
                case DeliverySortOrder.DeliveryId:
                    orderedDeliveries = deliveries.OrderBy(d => d.SortIndex); break;
                case DeliverySortOrder.CompanyName:
                    orderedDeliveries = deliveries.OrderBy(d => d.DeliveryCompanyName); break;
                case DeliverySortOrder.FirstDelivery:
                    orderedDeliveries = deliveries.OrderBy(d => d.IsFirstDelivery()); break;
                case DeliverySortOrder.ItemCount:
                    orderedDeliveries = deliveries.OrderBy(d => d.GetAggregatedOrderItems().Count); break;
                default:
                    orderedDeliveries = deliveries.OrderBy(d => d.EarliestTime); break;
            }

            if (!pickOrder.UseSortBy2)
                return orderedDeliveries.ToList();

            orderedDeliveries = SortDeliveries(orderedDeliveries, pickOrder.SortBy2);

            if (!pickOrder.UseSortBy3)
                return orderedDeliveries.ToList();

            orderedDeliveries = SortDeliveries(orderedDeliveries, pickOrder.SortBy3);

            if (!pickOrder.UseSortBy4)
                return orderedDeliveries.ToList();

            orderedDeliveries = SortDeliveries(orderedDeliveries, pickOrder.SortBy4);

            if (!pickOrder.UseSortBy5)
                return orderedDeliveries.ToList();

            orderedDeliveries = SortDeliveries(orderedDeliveries, pickOrder.SortBy5);

            return orderedDeliveries.ToList();
        }
    }
}