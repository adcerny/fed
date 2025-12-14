using System;

namespace Fed.Core.Common.Interfaces
{
    public interface IDeliveryIdGenerator
    {
        string GenerateId(DateTime deliveryDay, int latestHour, int dorderNumber);
    }
}