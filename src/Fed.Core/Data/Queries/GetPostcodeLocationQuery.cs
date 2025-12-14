using Fed.Core.Entities;

namespace Fed.Core.Data.Queries
{
    public class GetPostcodeLocationQuery : IDataOperation<PostcodeLocation>
    {
        public GetPostcodeLocationQuery(string postcode)
        {
            Postcode = postcode;
        }

        public string Postcode { get; }
    }
}