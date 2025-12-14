using Fed.Core.Common.Interfaces;
using System;

namespace Fed.Core.Common
{
    public class DeliveryIdGenerator : IDeliveryIdGenerator
    {
        private readonly DateTime InceptionDay = new DateTime(2019, 1, 1);

        private int RoundToInt(double x) => Convert.ToInt32(Math.Floor(x));

        public string GenerateId(DateTime deliveryDay, int latestHour, int orderNumber)
        {
            var delta = deliveryDay.Date - InceptionDay;
            var deltaDays = RoundToInt(delta.TotalDays);


            return $"D{Base36.FromDec(deltaDays)}-{latestHour}-{orderNumber}";
        }
    }
}