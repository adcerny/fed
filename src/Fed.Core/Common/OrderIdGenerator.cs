using Fed.Core.Common.Interfaces;
using System;

namespace Fed.Core.Common
{
    public class OrderIdGenerator : IOrderIdGenerator
    {
        private readonly DateTime InceptionDay = new DateTime(2019, 1, 1);

        private int RoundToInt(double x) => Convert.ToInt32(Math.Floor(x));

        public string GenerateId(DateTime deliveryDay, int orderNumber)
        {
            var delta = deliveryDay.Date - InceptionDay;
            var deltaDays = RoundToInt(delta.TotalDays);

            return $"Q{Base36.FromDec(deltaDays)}-{orderNumber}";
        }
    }
}