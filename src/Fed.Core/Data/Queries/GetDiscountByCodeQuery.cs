using Fed.Core.Entities;

namespace Fed.Core.Data.Queries
{
    public class GetDiscountByCodeQuery : IDataOperation<Discount>
    {
        public GetDiscountByCodeQuery(string discountCode)
        {
            DiscountCode = discountCode;
        }

        public string DiscountCode { get; }
    }
}