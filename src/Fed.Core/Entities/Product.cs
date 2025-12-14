using Fed.Core.Enums;
using System;
using System.Collections.Generic;

namespace Fed.Core.Entities
{
    public class Product
    {
        public string Id { get; set; }
        public string ProductCode { get; set; }
        public string ProductGroup { get; set; }
        public string ProductName { get; set; }
        public string SupplierId { get; set; }
        public string SupplierSKU { get; set; }
        public decimal Price { get; set; }
        public decimal? SalePrice { get; set; }
        public int FedUnits { get; set; }
        public int SupplierUnits { get; set; }
        public string IconCategory { get; set; }
        public bool IsTaxable { get; set; }
        public bool IsShippable { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedDate { get; set; }

        public bool IsSuppliedByAbelAndCole() => Suppliers.AbelAndCole.MatchesSupplierId(SupplierId);
        public bool IsSuppliedBySevenSeeded() => Suppliers.SevenSeeded.MatchesSupplierId(SupplierId);

        public Guid? ProductCategoryId { get; set; }
        public ProductCategory ProductCategory { get; set; }

        public decimal ActualPrice => SalePrice ?? Price;

        public IList<Product> ChildProducts { get; set; }
    }
}