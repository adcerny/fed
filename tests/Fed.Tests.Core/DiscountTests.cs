using Fed.Core.Entities;
using Fed.Core.Enums;
using Fed.Core.Models;
using Fed.Core.Services.Factories;
using Fed.Tests.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace Fed.Tests.Core
{
    public class DiscountTests
    {
        [Theory]
        [InlineData(20, 20, 200, 20)]
        [InlineData(20, 21, 200, 20)]
        [InlineData(20, 30, 200, 20)]
        public void TestOrderValueQualification(decimal percentage, decimal minOrder, decimal maxOrder, decimal orderValue)
        {
            var discount = TestDataBuilder.BuildDiscount(DiscountRewardType.Percentage,
                                        DiscountQualificationType.OrderValue,
                                        DiscountEligibleProductsType.AllProducts,
                                        percentage,
                                        null,
                                        minOrder,
                                        maxOrder);            

            var items = new List<LineItem> { new LineItem { Price = orderValue, ProductCode = "testSku", Quantity = 1 } };

            var result = new DiscountStrategyFactory().GetCalculator(discount).CalculateDiscount(items);

            Assert.Equal(orderValue >= minOrder, result.DiscountQualification.IsQualified);

        }

        //[Theory]
        //[InlineData(20, 20, 200, 10, "FV")]
        //[InlineData(20, 21, 200, 20, "DL")]
        //[InlineData(20, 30, 200, 20, "DY")]
        //public void TestCategorySpendQualification(decimal percentage, decimal minOrder, decimal maxOrder, decimal orderValue, string qualifyingCategory)
        //{
        //    var discount = TestDataBuilder.BuildDiscount(DiscountRewardType.Percentage,
        //                                DiscountQualificationType.CategorySpend,
        //                                DiscountEligibleProductsType.AllProducts,
        //                                percentage,
        //                                null,
        //                                minOrder,
        //                                maxOrder,
        //                                null,
        //                                new List<string> { new qualifyingCategory });

        //    var items = new List<LineItem> { new LineItem { Price = orderValue, ProductCode = $"{qualifyingCategory}_TestProduct", Quantity = 1 },
        //                                   { new LineItem { Price = orderValue, ProductCode = $"######", Quantity = 1 } } };

        //    var result = new DiscountStrategyFactory().GetCalculator(discount).CalculateDiscount(items);

        //    Assert.Equal(orderValue > minOrder, result.DiscountQualification.IsQualified);

        //}

        [Theory]
        [InlineData(1, 1, true)]
        [InlineData(1, 2, true)]
        [InlineData(2, 1, false)]
        public void TestProductPurchaseQualification(int minQuantity, int actualQuantity, bool expectedQualification)
        {

            var qualifingItems = new List<LineItem> { new LineItem { Price = 20, ProductCode = "testSku", Quantity = minQuantity } };

            var discount = TestDataBuilder.BuildDiscount(DiscountRewardType.Percentage,
                                        DiscountQualificationType.ProductPurchase,
                                        DiscountEligibleProductsType.AllProducts,
                                        20,
                                        null,
                                        null,
                                        null,
                                        null,
                                        null,
                                        null,
                                        qualifingItems
                                        );

            var orderItems = new List<LineItem> { new LineItem { Price = 20, ProductCode = "testSku", Quantity = actualQuantity } };
                                           

            var result = new DiscountStrategyFactory().GetCalculator(discount).CalculateDiscount(orderItems);

            Assert.Equal(expectedQualification, result.DiscountQualification.IsQualified);

        }

        [Theory]
        [InlineData(20, 20, 200, 20, 4)]
        [InlineData(25, 20, 200, 30, 7.5)]
        [InlineData(20, 20, 200, 19.99, 0)]
        [InlineData(20, 20, 200, 1000, 40)]
        public void TestPercentageCalculation(decimal percentage, decimal minOrder, decimal maxOrder, decimal orderValue, decimal expected)
        {
            var discount = TestDataBuilder.BuildDiscount(DiscountRewardType.Percentage,
                                        DiscountQualificationType.OrderValue,
                                        DiscountEligibleProductsType.AllProducts,
                                        percentage,
                                        null,
                                        minOrder,
                                        maxOrder);

            var items = new List<LineItem> { new LineItem { Price = orderValue, ProductCode = "testSku", Quantity = 1 } };

            var result = new DiscountStrategyFactory().GetCalculator(discount).CalculateDiscount(items);

            Assert.Equal(expected, result.DiscountAmount);

        }


        [Theory]
        [InlineData(5, 20, 200, 20, 5)]
        [InlineData(3, 20, 200, 30, 3)]
        [InlineData(7, 20, 200, 19.99, 0)]
        public void TestValueCalculation(decimal value, decimal minOrder, decimal maxOrder, decimal orderValue, decimal expected)
        {
            var discount = TestDataBuilder.BuildDiscount(DiscountRewardType.Value,
                                        DiscountQualificationType.OrderValue,
                                        DiscountEligibleProductsType.AllProducts,
                                        null,
                                        value,
                                        minOrder,
                                        maxOrder);

            var items = new List<LineItem> { new LineItem { Price = orderValue, ProductCode = "testSku", Quantity = 1 } };

            var result = new DiscountStrategyFactory().GetCalculator(discount).CalculateDiscount(items);

            Assert.Equal(expected, result.DiscountAmount);

        }

        [Fact]
        public void TestProductCalculation()
        {

            var discountProduct = new LineItem { Price = 0, ProductCode = "testSku", Quantity = 1 };

            var discount = TestDataBuilder.BuildDiscount(DiscountRewardType.Product,
                                        DiscountQualificationType.OrderValue,
                                        DiscountEligibleProductsType.AllProducts,
                                        null,
                                        null,
                                        20,
                                        200,
                                        null,
                                        null,
                                        new List<LineItem> { discountProduct });

            var items = new List<LineItem> { new LineItem { Price = 200, ProductCode = "testSku", Quantity = 1 } };

            var result = new DiscountStrategyFactory().GetCalculator(discount).CalculateDiscount(items);

            Assert.Single(result.DiscountedProducts);
            var discountedProduct = result.DiscountedProducts.Single();

            Assert.Equal(discountProduct.Price, discountedProduct.Price);
            Assert.Equal(discountProduct.ProductCode, discountedProduct.ProductCode);
            Assert.Equal(discountProduct.Quantity, discountedProduct.Quantity);
        }

        [Theory]
        [InlineData(20, 20, 1)]
        [InlineData(20, 21, 1)]
        [InlineData(22, 23, 1)]
        [InlineData(20, 19, 0)]
        [InlineData(19, 18, 0)]
        public void TestMinOrderProductCalculation(decimal minOrder, decimal actualOrder, int expectedNoOfDiscountedProducts)
        {

            var discountProduct = new LineItem { Price = 0, ProductCode = "testSku", Quantity = 1 };

            var discount = TestDataBuilder.BuildDiscount(DiscountRewardType.Product,
                                        DiscountQualificationType.OrderValue,
                                        DiscountEligibleProductsType.AllProducts,
                                        null,
                                        null,
                                        minOrder,
                                        null,
                                        null,
                                        null,
                                        new List<LineItem> { discountProduct });

            var items = new List<LineItem> { new LineItem { Price = actualOrder, ProductCode = "testSku", Quantity = 1 } };

            var result = new DiscountStrategyFactory().GetCalculator(discount).CalculateDiscount(items);

            Assert.Equal(expectedNoOfDiscountedProducts, result.DiscountedProducts?.Count() ?? 0);
        }


        [Theory]
        [InlineData(20, 20, 200, new string[] { "FV", "PK" })]
        public void TestCategoryEligibility(decimal percentage, decimal minOrder, decimal maxOrder, IEnumerable<string> categories)
        {
            var discount = TestDataBuilder.BuildDiscount(DiscountRewardType.Percentage,
                                        DiscountQualificationType.OrderValue,
                                        DiscountEligibleProductsType.Category,
                                        percentage,
                                        null,
                                        minOrder,
                                        maxOrder,
                                        categories.ToList());

            var items = new List<LineItem>();
            for (int i = 0; i < 10; i ++)
            {
                items.Add(new LineItem { Price = 10, Quantity = 1, ProductCode = i.ToString() + TestDataBuilder.GetRandomShortId() });
            }
            foreach(var sku in categories)
            {
                items.Add(new LineItem { Price = 10, Quantity = 1, ProductCode = sku });
            }

            var eligiableItems = new DiscountStrategyFactory().GetCalculator(discount).GetEligableProducts(items);

            Assert.Equal(eligiableItems.Count(), categories.Count());
        }

        [Theory]
        [InlineData(40, 40, new string[] { "FV-AbelC-Apples-5pcs", "FV-AbelC-Bananas-5pcs" }, new string[] { "FV-AbelC-Apples-5pcs", "FV-AbelC-Bananas-5pcs" }, 1)]
        [InlineData(40, 40, new string[] { "FV-AbelC-Apples-5pcs", "FV-AbelC-Bananas-5pcs" }, new string[] { "PK-PlanO-DarkChocAlm-30g", "PK-Evviv-ClassicPanet-500g" }, 0)]
        [InlineData(40, 80, new string[] { "FV-AbelC-Apples-5pcs", "FV-AbelC-Bananas-5pcs" }, new string[] { "FV-AbelC-Apples-5pcs", "FV-AbelC-Bananas-5pcs" }, 1)]
        [InlineData(40, 20, new string[] { "FV-AbelC-Apples-5pcs", "FV-AbelC-Bananas-5pcs" }, new string[] { "FV-AbelC-Apples-5pcs", "FV-AbelC-Bananas-5pcs" }, 0)]
        public void TestCategoryMinSpendFreeProduct(decimal minOrder, decimal orderVal, IEnumerable<string> productCateogrySkus, IEnumerable<string> orderSkus, int expectedFreeProducts)
        {


            var disountedProducts = new List<LineItem> {
                new LineItem {
                    Price = 0,
                    ProductCode = Guid.NewGuid().ToString(),
                    Quantity = 1
                }
            };

            var qualifyingCategories = new List<DiscountQualifyingCategory>
            {
                new DiscountQualifyingCategory
                {
                    CategoryProductSkus =  productCateogrySkus.ToList(),
                    ProductCategoryId = Guid.NewGuid(),
                    ProductQuantity = 0
                }
            };
            


            var discount = TestDataBuilder.BuildDiscount(DiscountRewardType.Product,
                                        DiscountQualificationType.CategorySpend,
                                        DiscountEligibleProductsType.AllProducts,
                                        null,
                                        null,
                                        minOrder,
                                        null,
                                        null,
                                        qualifyingCategories,
                                        disountedProducts);

            var items = new List<LineItem>();
            for (int i = 0; i < 10; i++)
            {
                items.Add(new LineItem { Price = 10, Quantity = 1, ProductCode = i.ToString() + TestDataBuilder.GetRandomShortId() });
            }
            foreach (var sku in orderSkus)
            {
                items.Add(new LineItem { Price = (orderVal / orderSkus.Count()), Quantity = 1, ProductCode = sku });
            }

            var calculator = new DiscountStrategyFactory().GetCalculator(discount);
            var calculation = calculator.CalculateDiscount(items);
            Assert.Equal(expectedFreeProducts, (calculation.DiscountedProducts?.Count() ?? 0));
        }

        

        [Theory]
        [InlineData(40, 40, new string[] { "FV-AbelC-Apples-5pcs", "FV-AbelC-Bananas-5pcs" }, new string[] { "FV-AbelC-Apples-5pcs", "FV-AbelC-Bananas-5pcs" }, 4)]
        [InlineData(40, 40, new string[] { "FV-AbelC-Apples-5pcs", "FV-AbelC-Bananas-5pcs" }, new string[] { "PK-PlanO-DarkChocAlm-30g", "PK-Evviv-ClassicPanet-500g" }, 0)]
        [InlineData(40, 80, new string[] { "FV-AbelC-Apples-5pcs", "FV-AbelC-Bananas-5pcs" }, new string[] { "FV-AbelC-Apples-5pcs", "FV-AbelC-Bananas-5pcs" }, 4)]
        [InlineData(40, 20, new string[] { "FV-AbelC-Apples-5pcs", "FV-AbelC-Bananas-5pcs" }, new string[] { "FV-AbelC-Apples-5pcs", "FV-AbelC-Bananas-5pcs" }, 0)]
        public void TestCategoryMinSpendFreeProducts(decimal minOrder, decimal orderVal, IEnumerable<string> productCateogrySkus, IEnumerable<string> orderSkus, int expectedFreeProducts)
        {


            var disountedProducts = new List<LineItem> {
                new LineItem {
                    Price = 0,
                    ProductCode = Guid.NewGuid().ToString(),
                    Quantity = 1
                },
                new LineItem {
                    Price = 0,
                    ProductCode = Guid.NewGuid().ToString(),
                    Quantity = 1
                },
                new LineItem {
                    Price = 0,
                    ProductCode = Guid.NewGuid().ToString(),
                    Quantity = 1
                },
                new LineItem {
                    Price = 0,
                    ProductCode = Guid.NewGuid().ToString(),
                    Quantity = 1
                }
            };

            var qualifyingCategories = new List<DiscountQualifyingCategory>
            {
                new DiscountQualifyingCategory
                {
                    CategoryProductSkus =  productCateogrySkus.ToList(),
                    ProductCategoryId = Guid.NewGuid(),
                    ProductQuantity = 0
                }
            };



            var discount = TestDataBuilder.BuildDiscount(DiscountRewardType.Product,
                                        DiscountQualificationType.CategorySpend,
                                        DiscountEligibleProductsType.AllProducts,
                                        null,
                                        null,
                                        minOrder,
                                        null,
                                        null,
                                        qualifyingCategories,
                                        disountedProducts);

            var items = new List<LineItem>();
            for (int i = 0; i < 10; i++)
            {
                items.Add(new LineItem { Price = 10, Quantity = 1, ProductCode = i.ToString() + TestDataBuilder.GetRandomShortId() });
            }
            foreach (var sku in orderSkus)
            {
                items.Add(new LineItem { Price = (orderVal / orderSkus.Count()), Quantity = 1, ProductCode = sku });
            }

            var calculator = new DiscountStrategyFactory().GetCalculator(discount);
            var calculation = calculator.CalculateDiscount(items);
            Assert.Equal(expectedFreeProducts, (calculation.DiscountedProducts?.Count() ?? 0));
        }

        [Theory]
        [InlineData(5, 5, new string[] { "FV-AbelC-Apples-5pcs", "FV-AbelC-Bananas-5pcs" }, new string[] { "FV-AbelC-Apples-5pcs", "FV-AbelC-Bananas-5pcs" }, 1)]
        [InlineData(5, 5, new string[] { "FV-AbelC-Apples-5pcs", "FV-AbelC-Bananas-5pcs" }, new string[] { "PK-PlanO-DarkChocAlm-30g", "PK-Evviv-ClassicPanet-500g" }, 0)]
        [InlineData(5, 4, new string[] { "FV-AbelC-Apples-5pcs", "FV-AbelC-Bananas-5pcs" }, new string[] { "FV-AbelC-Apples-5pcs", "FV-AbelC-Bananas-5pcs" }, 0)]
        [InlineData(5, 6, new string[] { "FV-AbelC-Apples-5pcs", "FV-AbelC-Bananas-5pcs" }, new string[] { "FV-AbelC-Apples-5pcs", "FV-AbelC-Bananas-5pcs" }, 1)]
        public void TestCategoryMinQuanitityFreeProduct(int quantityNeeded, int quantityPurcased, IEnumerable<string> productCateogrySkus, IEnumerable<string> orderSkus, int expectedFreeProducts)
        {


            var disountedProducts = new List<LineItem> {
                new LineItem {
                    Price = 0,
                    ProductCode = Guid.NewGuid().ToString(),
                    Quantity = 1
                }
            };

            var qualifyingCategories = new List<DiscountQualifyingCategory>
            {
                new DiscountQualifyingCategory
                {
                    CategoryProductSkus =  productCateogrySkus.ToList(),
                    ProductCategoryId = Guid.NewGuid(),
                    ProductQuantity = quantityNeeded
                }
            };

            var discount = TestDataBuilder.BuildDiscount(DiscountRewardType.Product,
                                        DiscountQualificationType.CategorySpend,
                                        DiscountEligibleProductsType.AllProducts,
                                        null,
                                        null,
                                        null,
                                        null,
                                        null,
                                        qualifyingCategories,
                                        disountedProducts);

            var items = new List<LineItem>();
            for (int i = 0; i < 10; i++)
            {
                items.Add(new LineItem { Price = 10, Quantity = 1, ProductCode = i.ToString() + TestDataBuilder.GetRandomShortId() });
            }
            for (int i = 0; i < quantityPurcased; i++)
            {
                items.Add(new LineItem { Price = 10, Quantity = 1, ProductCode = orderSkus.ToArray()[i % orderSkus.Count()] });
            }

            var calculator = new DiscountStrategyFactory().GetCalculator(discount);
            var calculation = calculator.CalculateDiscount(items);
            Assert.Equal(expectedFreeProducts, (calculation.DiscountedProducts?.Count() ?? 0));
        }

        [Theory]
        [InlineData(20, 20, 100, new string[] { "FV-AbelC-Apples-5pcs", "FV-AbelC-Bananas-5pcs" }, new string[] { "FV-AbelC-Apples-5pcs", "FV-AbelC-Bananas-5pcs" }, 20)]
        [InlineData(20, 20, 99, new string[] { "FV-AbelC-Apples-5pcs", "FV-AbelC-Bananas-5pcs" }, new string[] { "PK-PlanO-DarkChocAlm-30g", "PK-Evviv-ClassicPanet-500g" }, 0)]
        [InlineData(20, 20, 19, new string[] { "FV-AbelC-Apples-5pcs", "FV-AbelC-Bananas-5pcs" }, new string[] { "FV-AbelC-Apples-5pcs", "FV-AbelC-Bananas-5pcs" }, 0)]
        [InlineData(20, 20, 20, new string[] { "FV-AbelC-Apples-5pcs", "FV-AbelC-Bananas-5pcs" }, new string[] { "FV-AbelC-Apples-5pcs", "FV-AbelC-Bananas-5pcs" }, 4)]
        public void TestCategoryMinSpendPercentageOffCategory(decimal percentage, decimal minOrder, decimal eligibleOrderValue, IEnumerable<string> productCateogrySkus, IEnumerable<string> orderSkus, decimal expectedPercentage)
        {

            var qualifyingCategories = new List<DiscountQualifyingCategory>
            {
                new DiscountQualifyingCategory
                {
                    CategoryProductSkus =  productCateogrySkus.ToList(),
                    ProductCategoryId = Guid.NewGuid(),
                    ProductQuantity = 0
                }
            };

            var discount = TestDataBuilder.BuildDiscount(DiscountRewardType.Percentage,
                                        DiscountQualificationType.CategorySpend,
                                        DiscountEligibleProductsType.Category,
                                        percentage,
                                        null,
                                        minOrder,
                                        null,
                                        productCateogrySkus.ToList(),
                                        qualifyingCategories,
                                        null);

            var items = new List<LineItem>();
            for (int i = 0; i < 10; i++)
            {
                items.Add(new LineItem { Price = 10, Quantity = 1, ProductCode = i.ToString() + TestDataBuilder.GetRandomShortId() });
            }
            var eligiableItems = new List<LineItem>();
            int lineItemPrice = 1;
            int x = 0;
            do
            {
                var currentTotal = eligiableItems.Sum(i => i.Quantity * i.Price);

                eligiableItems.Add(new LineItem
                {
                    Quantity = 1,
                    Price = Math.Min(lineItemPrice, eligibleOrderValue - currentTotal),
                    ProductCode = orderSkus.ToArray()[x % orderSkus.Count()]
                });
                x++;
            } while (eligiableItems.Sum(i => i.Quantity * i.Price) < eligibleOrderValue);

            items.AddRange(eligiableItems);

            var calculator = new DiscountStrategyFactory().GetCalculator(discount);
            var calculation = calculator.CalculateDiscount(items);
            Assert.Equal(expectedPercentage, (calculation.DiscountAmount));
        }


        [Theory]
        [InlineData(20, 5, 5, 100, new string[] { "FV-AbelC-Apples-5pcs", "FV-AbelC-Bananas-5pcs" }, "FV-AbelC-Apples-5pcs", 20)]
        [InlineData(20, 5, 5, 100, new string[] { "FV-AbelC-Apples-5pcs", "FV-AbelC-Bananas-5pcs" }, "PK-PlanO-DarkChocAlm-30g", 0)]
        [InlineData(20, 5, 4, 100, new string[] { "FV-AbelC-Apples-5pcs", "FV-AbelC-Bananas-5pcs" }, "FV-AbelC-Apples-5pcs", 0)]
        [InlineData(20, 5, 6, 100, new string[] { "FV-AbelC-Apples-5pcs", "FV-AbelC-Bananas-5pcs" }, "FV-AbelC-Apples-5pcs", 20)]
        public void TestCategoryQuantitySpendPercentageOffCategory(decimal percentage, 
                                                                   int quantityNeeded, 
                                                                   int quantityActual, 
                                                                   decimal orderValue, 
                                                                   IEnumerable<string> productCateogrySkus, 
                                                                   string orderSku, decimal expectedPercentage)
        {

            var qualifyingCategories = new List<DiscountQualifyingCategory>
            {
                new DiscountQualifyingCategory
                {
                    CategoryProductSkus =  productCateogrySkus.ToList(),
                    ProductCategoryId = Guid.NewGuid(),
                    ProductQuantity = quantityNeeded
                }
            };

            var discount = TestDataBuilder.BuildDiscount(DiscountRewardType.Percentage,
                                        DiscountQualificationType.CategorySpend,
                                        DiscountEligibleProductsType.Category,
                                        percentage,
                                        null,
                                        null,
                                        null,
                                        productCateogrySkus.ToList(),
                                        qualifyingCategories,
                                        null);

            var items = new List<LineItem>();
            for (int i = 0; i < 10; i++)
            {
                items.Add(new LineItem { Price = 10, Quantity = 1, ProductCode = i.ToString() + TestDataBuilder.GetRandomShortId() });
            }

            items.Add(new LineItem { Price = orderValue / (decimal)quantityActual, Quantity = quantityActual, ProductCode = orderSku });


            var calculator = new DiscountStrategyFactory().GetCalculator(discount);
            var calculation = calculator.CalculateDiscount(items);
            Assert.Equal(expectedPercentage, (calculation.DiscountAmount));
        }
    }
}
