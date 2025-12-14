using Fed.Core.Entities;
using Fed.Core.Extensions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xero.Api.Core.Model;

namespace Fed.Api.External.XeroService
{
    public class XeroProductsSyncService : XeroService
    {
        public XeroProductsSyncService(XeroSettings settings, ILogger logger) :
            base(settings, logger)
        {

        }
        public async Task<bool> SyncProducts(IList<Product> products)
        {
            var items = await _api.Items.FindAsync();

            foreach (Product product in products)
            {
                var itemCode = product.ProductCode.Trim().Truncate(XeroConstants.MaxItemCodeLength);
                var itemName = product.ProductName.Trim().Truncate(XeroConstants.MaxItemNameLength);

                var xeroItem = items.Where(i => i.Code == itemCode).FirstOrDefault();
                if (xeroItem == null)
                    await CreateNewItem(itemCode, itemName, product.ProductGroup.NormaliseSpace());
                else if (xeroItem.Name != itemName.NormaliseSpace() ||
                        xeroItem.PurchaseDescription != product.ProductGroup)
                {
                    await UpdateExistingItem(xeroItem, itemName, product.ProductGroup);
                }
            }
            return true;
        }

        private async Task UpdateExistingItem(Item xeroItem, string itemName, string productGroup)
        {
            _logger.LogInformation($"Updating product {xeroItem.Code}");

            xeroItem.Name = itemName;
            xeroItem.PurchaseDescription = productGroup;
            try
            {
                await _api.Items.UpdateAsync(xeroItem);
                Thread.Sleep(1000);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating product {xeroItem.Code}. Error message was {ex.ToString()}");
            }

        }

        private async Task CreateNewItem(string itemCode, string itemName, string productGroup)
        {
            _logger.LogInformation($"Adding product {itemCode}");

            try
            {
                await _api.Items.CreateAsync(new Item
                {
                    Code = itemCode,
                    Name = itemName,
                    PurchaseDescription = productGroup,
                    PurchaseDetails = new PurchaseDetails { AccountCode = "200" },
                    SalesDetails = new SalesDetails { AccountCode = "100" }
                });
                Thread.Sleep(1000);
            }

            catch (Exception ex)
            {
                _logger.LogError($"Error creating product {itemCode}. Error message was {ex.ToString()}");
            }


        }
    }
}
