using Fed.Core.Entities;
using Fed.Core.Enums;
using Fed.Core.Extensions;
using System;
using Xunit;

namespace Fed.Tests.Core
{
    public class DateTimeExtensionTests
    {
        [Theory]
        [InlineData(2019, 3, 25, 2019, 4, 1, 1)]
        [InlineData(2019, 3, 25, 2019, 4, 8, 2)]
        [InlineData(2019, 1, 7, 2019, 3, 25, 11)]
        [InlineData(2019, 9, 19, 2019, 8, 15, -5)]
        [InlineData(2019, 12, 1, 2018, 12, 2, -52)]
        public void AddWeeksTests(
            int year,
            int month,
            int day,
            int expectedYear,
            int expectedMonth,
            int expectedDay,
            int weeks)
        {
            var date = new DateTime(year, month, day);
            var expectedDate = new DateTime(expectedYear, expectedMonth, expectedDay);

            var result = date.AddWeeks(weeks);

            Assert.Equal(expectedDate, result);
        }

        [Theory]
        [InlineData("2020/01/17 11:50:00", "2020/01/20", "2020/01/20")]
        [InlineData("2020/01/17 16:01:00", "2020/01/20", "2020/01/27")]
        [InlineData("2020/01/18 15:01:00", "2020/01/20", "2020/01/27")]
        [InlineData("2020/01/18 16:01:00", "2020/01/20", "2020/01/27")]
        [InlineData("2020/01/19 15:01:00", "2020/01/20", "2020/01/27")]
        [InlineData("2020/01/19 16:01:00", "2020/01/20", "2020/01/27")]
        [InlineData("2020/01/20 09:00:00", "2020/01/20", "2020/01/27")]
        [InlineData("2020/01/20 16:00:00", "2020/01/20", "2020/01/27")]
        [InlineData("2020/01/20 15:00:00", "2020/01/13", "2020/01/27")]
        [InlineData("2020/01/20 16:01:00", "2020/01/13", "2020/01/27")]
        [InlineData("2020/01/17 11:50:00", "2020/01/27", "2020/01/27")]
        [InlineData("2020/01/17 16:01:00", "2020/01/27", "2020/01/27")]
        [InlineData("2020/01/20 15:01:00", "2020/01/21", "2020/01/21")]
        [InlineData("2020/01/20 16:01:00", "2020/01/21", "2020/01/28")]
        [InlineData("2020/01/17 15:01:00", "2020/01/18", "2020/01/20")]
        [InlineData("2020/01/17 16:01:00", "2020/01/18", "2020/01/27")]
        [InlineData("2020/01/17 15:01:00", "2020/02/03", "2020/02/03")]
        [InlineData("2020/01/17 16:01:00", "2020/02/03", "2020/02/03")]
        [InlineData("2020/01/17 15:01:00", "2020/02/04", "2020/02/04")]
        [InlineData("2020/01/17 16:01:00", "2020/02/04", "2020/02/04")]
        [InlineData("2020/01/21 16:48:00", "2020/01/23", "2020/01/23")]
        public void GetNextAvailableDeliveryDatesTest(string currentDateTimeString, string orderDateString, string expectedDateString)
        {

            var now = DateTime.Parse(currentDateTimeString);
            var orderDate = DateTime.Parse(orderDateString);
            var expectedDate = DateTime.Parse(expectedDateString);

            //override current time
            DateTimeExtensions.Now = () => now;

            var deliveryDate = orderDate.GetNextAvailableDeliveryDate();

            Assert.Equal(expectedDate, deliveryDate);
        }


        [Theory]
        [InlineData("2020/02/14 11:50:00", "2020/02/17")]
        [InlineData("2020/02/14 16:01:00", "2020/02/18")]
        [InlineData("2020/02/15 11:50:00", "2020/02/18")]
        [InlineData("2020/02/15 16:01:00", "2020/02/18")]
        [InlineData("2020/02/16 11:50:00", "2020/02/18")]
        [InlineData("2020/02/16 16:01:00", "2020/02/18")]
        [InlineData("2020/02/17 11:50:00", "2020/02/18")]
        [InlineData("2020/02/17 16:01:00", "2020/02/19")]
        [InlineData("2020/02/18 11:50:00", "2020/02/19")]
        [InlineData("2020/02/18 16:01:00", "2020/02/20")]
        [InlineData("2020/02/19 11:50:00", "2020/02/20")]
        [InlineData("2020/02/19 16:01:00", "2020/02/21")]
        [InlineData("2020/02/20 11:50:00", "2020/02/21")]
        [InlineData("2020/02/20 16:01:00", "2020/02/24")]
        [InlineData("2020/02/21 11:50:00", "2020/02/24")]
        [InlineData("2020/02/21 16:01:00", "2020/02/25")]
        public void GetNextAvailableWorkingDay(string currentDateTimeString, string expectedDateString)
        {

            var now = DateTime.Parse(currentDateTimeString);
            var expectedDate = DateTime.Parse(expectedDateString);

            //override current time
            DateTimeExtensions.Now = () => now;

            var deliveryDate = DateTimeExtensions.GetNextAvailableWorkingDay();

            Assert.Equal(expectedDate, deliveryDate);
        }


        [Theory]
        [InlineData("2020/01/20", DayOfWeek.Monday, "2020/01/20", "2020/01/20")]
        [InlineData("2020/01/13", DayOfWeek.Monday, "2020/01/20", "2020/01/20")]
        [InlineData("2020/01/20", DayOfWeek.Monday, "2020/01/13", "2020/01/20")]
        [InlineData("2020/01/20", DayOfWeek.Tuesday, "2020/01/13", "2020/01/21")]
        [InlineData("2020/01/20", DayOfWeek.Monday, "2020/01/27", "2020/01/27")]
        public void GetNextWeekDayTest(string currentDateTimeString, DayOfWeek dayOfWeek, string testDateString, string expectedDateString)
        {

            var now = DateTime.Parse(currentDateTimeString);
            var orderDate = DateTime.Parse(testDateString);
            var expectedDate = DateTime.Parse(expectedDateString);

            //override current time
            DateTimeExtensions.Now = () => now;

            var nextDate = orderDate.NextWeekday(dayOfWeek);

            Assert.Equal(expectedDate, nextDate);
        }

        [Theory]
        [InlineData("2020/01/17", "2020/01/20", "2020/01/20")]
        [InlineData("2020/01/19", "2020/01/20", "2020/01/20")]
        [InlineData("2020/01/20", "2020/01/20", "2020/01/27")]
        [InlineData("2020/01/21", "2020/01/20", "2020/01/27")]
        [InlineData("2020/02/09", "2020/01/20", "2020/02/10")]
        [InlineData("2020/02/10", "2020/02/10", "2020/02/17")]
        public void RecurringOrderNextDeliveryDateTests(string currentDateTimeString, string startDateString, string expectedDateString)
        {
            var now = DateTime.Parse(currentDateTimeString);
            var orderDate = DateTime.Parse(startDateString);
            var expectedDate = DateTime.Parse(expectedDateString);

            //override current time
            DateTimeExtensions.Now = () => now;

            var recurringOrder = new RecurringOrder(
                Guid.NewGuid(),
                "My test order",
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                orderDate,
                DateTime.Today.AddYears(100),
                WeeklyRecurrence.EveryWeek,
                Guid.NewGuid(),
                null, null, false, null, null, null, null, null, null,
                false);

            Assert.Equal(expectedDate, recurringOrder.NextDeliveryDate.Value);
        }
    }
}
