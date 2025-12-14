using System;

namespace Fed.Core.Enums
{
    public enum Suppliers
    {
        AbelAndCole = 8,
        SevenSeeded = 7,
        PlanetOrganic = 209,
        YummyTummy = 291
    }

    public static class SupplierExtensions
    {
        public static bool MatchesSupplierId(this Suppliers supplier, string supplierId) =>
            Convert.ToInt32(supplier).ToString().Equals(supplierId);
    }
}