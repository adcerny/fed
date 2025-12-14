using Fed.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Fed.Core.Models
{
    public class LineItem
    {
        public LineItem()
        {

        }

        public LineItem(OrderItem orderItem)
        {
            ProductCode = orderItem.ProductCode;
            Quantity = orderItem.Quantity;
            Price = orderItem.Price;
        }

        public LineItem(RecurringOrderItem orderItem)
        {
            ProductCode = orderItem.ProductCode;
            Quantity = orderItem.Quantity;
            Price = orderItem.Price.GetValueOrDefault();
        }

        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}