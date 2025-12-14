using System.Collections.Generic;

namespace Fed.Core.Data.Queries
{
    public class GetByIdsQuery<T> : IDataOperation<T>
    {
        public GetByIdsQuery(IList<string> ids)
        {
            Ids = ids;
        }

        public IList<string> Ids { get; }
    }
}
