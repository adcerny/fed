using Fed.Core.Common;
using Fed.Core.Entities;
using Fed.Core.Enums;
using FizzWare.NBuilder;
using System;
using System.Collections.Generic;
using System.Linq;
using Fed.Core.Models;

namespace Fed.Tests.Common
{
    public class TestDataBuilder
    {
        private static RandomGenerator _randomGenerator = new RandomGenerator();
        private static RandomIdGenerator _randomIdGenerator = new RandomIdGenerator();
        private static Random rng = new Random();

        private static List<string> _postcodes =
            new List<string>()
            {
                "EC1M 4AA",
                "N1 6PE",
                "EC1V 1NY",
                "EC2M 1JD",
                "EC1V 9AZ",
                "EC1V 3QU",
                "E8 3DL",
                "N1 6NU",
                "E8 3RL",
                "EC2A 4DX",
                "EC2M 7EA",
                "N1 3QP",
                "EC2A 2BB",
                "EC1V 9BP",
                "EC1V 0NB",
                "N1 6LA",
                "EC3A 8BF",
                "N1 7ED",
                "N1 8DW"
            };

        public static string GetRandomPostcode() => _postcodes[GetRandomInt(0, _postcodes.Count - 1)];

        public static string GetRandomString(int length = 10) => _randomGenerator.Phrase(length);

        public static string GetRandomEmail() => Faker.Internet.Email();

        public static string GetRandomPhone() => _randomGenerator.Long().ToString();

        public static string GetRandomName() => Faker.Name.FullName();

        public static string GetRandomCompanyName() => Faker.Company.Name();

        public static int GetRandomInt(int min, int max) => new Random().Next(min, max + 1);

        public static string GetRandomShortId() => _randomIdGenerator.GenerateId();

        private static decimal GetRandomDecimal(decimal minValue, decimal maxValue) =>
            Math.Round(minValue + ((decimal)new Random().NextDouble() * (maxValue - minValue)), 2);

        public static BillingAddress BuildBillingAddress(Guid contactId, bool isPrimary) =>
            new BillingAddress(
                Guid.NewGuid(),
                contactId,
                isPrimary,
                Faker.Name.FullName(),
                Faker.Company.Name(),
                Faker.Address.StreetAddress(),
                Faker.Address.SecondaryAddress(),
                Faker.Address.City(),
                GetRandomPostcode(),
                Faker.Internet.Email(),
                Faker.Phone.Number(),
                Faker.Lorem.GetFirstWord());

        public static DeliveryAddress BuildDeliveryAddress(Guid contactId, bool isPrimary, Guid hubId) =>
            new DeliveryAddress(
                Guid.NewGuid(),
                contactId,
                isPrimary,
                Faker.Name.FullName(),
                Faker.Company.Name(),
                Faker.Address.StreetAddress(),
                Faker.Address.SecondaryAddress(),
                Faker.Address.City(),
                GetRandomPostcode(),
                Faker.Lorem.Paragraph(),
                Faker.Phone.Number(),
                _randomGenerator.Boolean(),
                hubId);

        public static CardToken BuildCardToken(Guid contactId, bool isPrimary) =>
            new CardToken(
                Guid.NewGuid(),
                contactId,
                GetRandomInt(1, 12),
                GetRandomInt(DateTime.Now.Year, DateTime.Now.AddYears(5).Year),
                $"{GetRandomInt(100000, 999999)}******{GetRandomInt(1000, 9999)}",
                Faker.Name.FullName(),
                Faker.Address.StreetAddress(),
                GetRandomPostcode(),
                isPrimary);

        public static Customer BuildCustomer() =>
            new Customer(
                Guid.NewGuid(),
                GetRandomShortId(),
                Faker.Company.Name(),
                Faker.Internet.DomainName(),
                null,
                true,
                10,
                100,
                false,
                false,
                false,
                AccountType.Standard,
                null,
                null,
                false,
                null);

        public static Contact BuildContact(Guid customerId, Guid hubId)
        {
            var id = Guid.NewGuid();

            return
                new Contact(
                    id,
                    GetRandomShortId(),
                    customerId,
                    "",
                    Faker.Name.First(),
                    Faker.Name.Last(),
                    Faker.Internet.Email(),
                    Faker.Phone.Number(),
                    _randomGenerator.Boolean(),
                    false,
                    (int)_randomGenerator.Enumeration<PaymentMethod>(),
                    DateTime.UtcNow,
                    new List<DeliveryAddress> { BuildDeliveryAddress(id, true, hubId) },
                    new List<BillingAddress> { BuildBillingAddress(id, true) },
                    new List<CardToken> { BuildCardToken(id, true) });
        }


        public static Customer BuildCustomerWithContact(Guid? hubId = null)
        {
            var customer = BuildCustomer();
            customer.Contacts = new List<Contact> { BuildContact(customer.Id, hubId ?? Guid.NewGuid()) };
            return customer;
        }

        public static RecurringOrder BuildRecurringOrder(
            Guid contactId,
            Guid deliveryAddresId,
            Guid billingAddressId,
            Guid cardTokenId,
            Guid timeslotId,
            int? weeklyRecurrance = null,
            bool isFree = false) =>
            new RecurringOrder(
                Guid.NewGuid(),
                "My test order",
                contactId,
                deliveryAddresId,
                billingAddressId,
                DateTime.Today.Date,
                DateTime.MaxValue.Date,
                (WeeklyRecurrence)(weeklyRecurrance ?? GetRandomInt(0, 1)),
                timeslotId,
                null, null, false, null, null, null, null, null, null,
                isFree);

        public static RecurringOrder BuildRecurringOrder(Contact contact,
            Guid timeslotId,
            int? weeklyRecurrance = null,
            bool isFree = false) =>
            BuildRecurringOrder(contact.Id,
                                contact.DeliveryAddresses.First().Id,
                                contact.BillingAddresses.First().Id,
                                contact.CardTokens.First().Id,
                                timeslotId,
                                weeklyRecurrance,
                                isFree);

        public static Discount BuildDiscount(
           DiscountRewardType rewardType,
           DiscountQualificationType qualificationType,
           DiscountEligibleProductsType eligibleProductsType,
           decimal? percentage,
           decimal? value,
           decimal? minOrderValue,
           decimal? maxOrderValue,
           IList<string> eligibleProductCategorySkus = null,
           IList<DiscountQualifyingCategory> qualifyingProductCategories = null,
           IList<LineItem> discountProducts = null,
           IList<LineItem> qualifyingProducts = null) =>
            new Discount(Guid.NewGuid(),
                        $"Test {rewardType} discount",
                        $"{rewardType} discount with {qualificationType} qualification applicable to {eligibleProductsType} ",
                        rewardType,
                        qualificationType,
                        eligibleProductsType,
                        percentage,
                        value,
                        minOrderValue,
                        maxOrderValue,
                        false,
                        true,
                        DiscountEvent.Manual,
                        DateTime.Now,
                        null,
                        DiscountEvent.Manual,
                        null,
                        null,
                        null,
                        null,
                        null,
                        discountProducts,
                        qualifyingProducts,
                        eligibleProductCategorySkus,
                        qualifyingProductCategories);

        public static RecurringOrderItem BuildRecurringOrderItem(Product product, int? quantity = null)
        {
            RecurringOrderItem item = new RecurringOrderItem
            {
                ProductId = product.Id,
                Price = product.Price,
                SalePrice = product.SalePrice,
                Quantity = quantity ?? GetRandomInt(1, 10)
            };
            return item;
        }

        public static List<RecurringOrderItem> BuildRecurringOrderItems(IList<Product> products, int? length = null)
        {
            if (length.HasValue && length > products.Count)
                throw new ArgumentException("Length cannot be longer than number of products");

            length = length ?? GetRandomInt(3, Math.Min(products.Count, 15));

            var items = new List<RecurringOrderItem>();
            products.Shuffle();

            for (int i = 0; i <= length; i++)
            {
                items.Add(BuildRecurringOrderItem(products[i]));
            }
            return items;
        }

        public static Product BuildProduct(IList<Product> children = null)
        {
            return new Product()
            {
                Id = Guid.NewGuid().ToString(),
                SupplierSKU = TestDataBuilder.GetRandomString(),
                ProductName = TestDataBuilder.GetRandomString(),
                Price = GetRandomDecimal(1, 15),
                IsTaxable = false,
                ProductCode = TestDataBuilder.GetRandomName(),
                SupplierId = "8",
                ChildProducts = children,
                IconCategory = "/media/1360/cat-13.png",
                IsShippable = true
            };
        }
    }
}
