using System;

namespace Fed.Core.Entities
{
    public class DiscountCode
    {
        public DiscountCode()
        { }


        public DiscountCode(Guid id,
                            Guid discountId,
                            string code,
                            string description,
                            bool isInactive)
        {
            Id = id;
            DiscountId = discountId;
            Code = code;
            Description = description;
            IsInactive = isInactive;
        }

        public Guid Id { get; }
        public Guid DiscountId { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public bool IsInactive { get; set; }
    }
}