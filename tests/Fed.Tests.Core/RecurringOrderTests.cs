using Fed.Core.Entities;
using Fed.Core.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Fed.Core.Tests
{
    public class RecurringOrderTests
    {
        [Fact]
        public void RecurringOrderCannotHaveZeroQuantityItems()
        {
            var recurringOrder =
                        new RecurringOrder(
                        Guid.NewGuid(),
                        "Test",
                        Guid.NewGuid(),
                        Guid.NewGuid(),
                        Guid.NewGuid(),
                        DateTime.Today,
                        DateTime.Today,
                        WeeklyRecurrence.EveryWeek,
                        Guid.NewGuid());


            var orderItems = new List<RecurringOrderItem>();

            orderItems.Add(new RecurringOrderItem
            {
                Quantity = 1
            });

            orderItems.Add(new RecurringOrderItem
            {
                Quantity = 0
            });

            recurringOrder.OrderItems = orderItems;

            Assert.Single(recurringOrder.OrderItems);

            recurringOrder.OrderItems.Add(new RecurringOrderItem
            {
                Quantity = 0
            });

            Assert.Single(recurringOrder.OrderItems);

            var orderItems2 = new List<RecurringOrderItem>();

            orderItems2.Add(new RecurringOrderItem
            {
                Quantity = 1
            });

            orderItems2.Add(new RecurringOrderItem
            {
                Quantity = 0
            });

            var recurringOrder2 =
            new RecurringOrder(
            Guid.NewGuid(),
            "Test",
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTime.Today,
            DateTime.Today,
            WeeklyRecurrence.EveryWeek,
            Guid.NewGuid(),
            null,
            null,
            false,
            null,
            null,
            null,
            orderItems2);

            Assert.Single(recurringOrder2.OrderItems);

            recurringOrder2.OrderItems.Add(new RecurringOrderItem
            {
                Quantity = 0
            });

            Assert.Single(recurringOrder2.OrderItems);
        }
    }
}
