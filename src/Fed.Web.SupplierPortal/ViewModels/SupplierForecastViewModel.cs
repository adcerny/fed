using Fed.Core.Entities;
using Fed.Core.Models;
using Fed.Core.ValueTypes;
using System;
using System.Collections.Generic;

namespace Fed.Web.SupplierPortal.ViewModels
{
    public class SupplierForecastViewModel
    {
        public SupplierForecastViewModel(
            string supplier,
            Date toDate,
            IList<Product> products,
            IDictionary<DateTime, IList<SupplierProductQuantity>> forecast)
        {
            Supplier = supplier;
            ToDate = toDate;
            Products = products;
            Forecast = forecast;
        }

        public string Supplier { get; set; }
        public Date ToDate { get; set; }
        public IList<Product> Products { get; set; }
        public IDictionary<DateTime, IList<SupplierProductQuantity>> Forecast { get; set; }
    }
}