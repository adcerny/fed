using Fed.Core.Enums;

namespace Fed.Core.Entities
{
    public class DeliveryPickOrder
    {
        public bool UseSortBy2 { get; set; }
        public bool UseSortBy3 { get; set; }
        public bool UseSortBy4 { get; set; }
        public bool UseSortBy5 { get; set; }

        public DeliverySortOrder SortBy1 { get; set; }
        public DeliverySortOrder SortBy2 { get; set; }
        public DeliverySortOrder SortBy3 { get; set; }
        public DeliverySortOrder SortBy4 { get; set; }
        public DeliverySortOrder SortBy5 { get; set; }

        public static DeliveryPickOrder Default =>
            new DeliveryPickOrder
            {
                SortBy1 = DeliverySortOrder.Slot,
                SortBy2 = DeliverySortOrder.DeliveryId,
                SortBy3 = DeliverySortOrder.FirstDelivery,
                SortBy4 = DeliverySortOrder.ItemCount,
                SortBy5 = DeliverySortOrder.CompanyName,
                UseSortBy2 = true,
                UseSortBy3 = true,
                UseSortBy4 = true,
                UseSortBy5 = true
            };
    }
}