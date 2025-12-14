using System;
using System.Collections.Generic;

namespace Fed.Api.External.MerchelloService
{
    public class Product
    {
        public string Description { get; set; }
        public string Supplier { get; set; }
        public string Name { get; set; }
        public string Key { get; set; }
        public string Sku { get; set; }
        public bool Taxable { get; set; }
        public bool Shippable { get; set; }
        public Guid? ProductCategoryId { get; set; }
        public string CategoryIcon { get; set; }

        public IList<ProductVariant> Variants { get; set; }
    }
}