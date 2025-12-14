using System;
using System.Collections.Generic;
using System.Text;

namespace Fed.Core.Entities
{
    public class DiscountProduct
    {
        public DiscountProduct(Guid productId,
                            Guid discountId,
                            decimal price,
                            int quantity)
        {
            ProductId = productId;
            DiscountId = discountId;
            Price = price;
            Quantity = quantity;
        }

        public Guid ProductId { get; }
        public Guid DiscountId { get; }
        public decimal Price { get; }
        public int Quantity { get; set; }
    }
}
