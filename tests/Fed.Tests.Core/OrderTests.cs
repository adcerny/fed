using Fed.Core.Entities;
using Fed.Core.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Fed.Core.Tests
{
    public class OrderTests
    {
        [Fact]
        public void OrderCannotHaveZeroQuantityItems()
        {
            var order = new Order();

            var orderItems = new List<OrderItem>();

            orderItems.Add(new OrderItem
            {
                Quantity = 1
            });

            orderItems.Add(new OrderItem
            {
                Quantity = 0
            });

            order.OrderItems = orderItems;

            Assert.Single(order.OrderItems);

            order.OrderItems.Add(new OrderItem
            {
                Quantity = 0
            });

            Assert.Single(order.OrderItems);
        }
    }
}
