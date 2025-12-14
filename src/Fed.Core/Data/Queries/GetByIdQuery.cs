using System;

namespace Fed.Core.Data.Queries
{
    public class GetByIdQuery<T> : IDataOperation<T>
    {
        public GetByIdQuery(Guid id)
        {
            Id = id.ToString();
        }

        public GetByIdQuery(string id)
        {
            Id = id;
        }

        public string Id { get; }
    }
}
