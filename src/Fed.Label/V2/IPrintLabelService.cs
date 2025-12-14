using Fed.Core.ValueTypes;
using System.Collections.Generic;

namespace Fed.Label.V2
{
    public interface IPrintLabelService
    {
        void PrintLabels(string deliveryId, string companyName, Date deliveryDate, int bagCount);
        List<byte[]> GetLabels(string deliveryId, string companyName, Date deliveryDate, int bagCount);
    }
}
