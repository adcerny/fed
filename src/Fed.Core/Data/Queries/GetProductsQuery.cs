using Fed.Core.Entities;
using System;
using System.Collections.Generic;

namespace Fed.Core.Data.Queries
{
    public class GetProductsQuery : IDataOperation<IList<Product>>
    {
        public GetProductsQuery(string productGroup = null, Guid? productCategoryId = null, bool includeDeleted = false)
        {
            ProductGroup = productGroup;
            ProductCategoryId = productCategoryId;
            IncludeDeleted = includeDeleted;
        }

        public string ProductGroup { get; }
        public Guid? ProductCategoryId { get; }
        public bool IncludeDeleted { get; }
    }
}