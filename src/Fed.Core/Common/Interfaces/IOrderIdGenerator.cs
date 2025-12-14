using System;

namespace Fed.Core.Common.Interfaces
{ 
    public interface IOrderIdGenerator
    {
        string GenerateId(DateTime deliveryDay, int orderNumber);
    }
}
