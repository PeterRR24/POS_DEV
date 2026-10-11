using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Dynamics.Commerce.Runtime;
//using Microsoft.Dynamics.Commerce.Runtime.Client;
using Microsoft.Dynamics.Commerce.Runtime.DataModel;
using Microsoft.Dynamics.Retail.Diagnostics;

namespace RR.Commerce.Runtime.Utilities
{/*
    public sealed class ProductManagerEx
    {
        private const string AvailableQty = "AvailableQty";

        private readonly ProductManager productManager;
        private readonly ICommerceRuntime runtime;

        private ProductManagerEx(ICommerceRuntime runtime)
        {
            this.productManager = ProductManager.Create(runtime);
            this.runtime = runtime;
        }

        public static ProductManagerEx Create(ICommerceRuntime runtime)
        {
            if (runtime == null)
                throw new ArgumentNullException("runtime");
            return new ProductManagerEx(runtime);
        }

        public ProductSearchResultContainer SearchProducts(ProductSearchCriteria criteria, int searchMode, QueryResultSettings settings)
        {
            ProductSearchResultContainer psr = productManager.SearchProducts(criteria, settings);

            if ((searchMode & 1) != 0)
                CalculateAvailability(psr);

            return psr;
        }

        private void CalculateAvailability(ProductSearchResultContainer psr)
        {
            HashSet<string> itemVariantMap = new HashSet<string>();
            Dictionary<string, decimal> itemAvailableMap = new Dictionary<string, decimal>();
            List<ItemVariantInventoryDimension> itemVariantList = new List<ItemVariantInventoryDimension>();
            string itemVariantKey;
            decimal itemVariantQty;

            // Prepare
            foreach (Product product in psr.Results)
            {
                itemVariantKey = CreateItemVariantKey(product.ItemId, String.Empty);
                if (!itemVariantMap.Contains(itemVariantKey))
                {
                    itemVariantMap.Add(itemVariantKey);
                    itemVariantList.Add(new ItemVariantInventoryDimension(product.ItemId, String.Empty));

                    if (product.IsMasterProduct)
                    {
                        foreach (ProductVariant productVariant in product.GetVariants())
                        {
                            itemVariantKey = CreateItemVariantKey(product.ItemId, productVariant.InventoryDimensionId);
                            if (!itemVariantMap.Contains(itemVariantKey))
                            {
                                itemVariantMap.Add(itemVariantKey);
                                itemVariantList.Add(new ItemVariantInventoryDimension(product.ItemId, productVariant.InventoryDimensionId));
                            }
                        }
                    }
                }
            }

            // Result
            IEnumerable<ItemAvailableQuantity> itemAvailableQuantities = this.runtime.Execute<GetItemVariantAvailabilityResponse>(new GetItemVariantAvailabilityRequest(itemVariantList), null).ItemAvailableQuantities.Results;
            foreach (ItemAvailableQuantity itemAvailableQuantity in itemAvailableQuantities)
            {
                itemVariantKey = CreateItemVariantKey(itemAvailableQuantity.ItemId, itemAvailableQuantity.VariantInventoryDimensionId);
                itemAvailableMap[itemVariantKey] = itemAvailableQuantity.AvailableQuantity;
            }

            // Update
            foreach (Product product in psr.Results)
            {
                itemVariantKey = CreateItemVariantKey(product.ItemId, String.Empty);
                itemAvailableMap.TryGetValue(itemVariantKey, out itemVariantQty);

                //TODO - Review
                product.SetProperty(AvailableQty, itemVariantQty);

                if (product.IsMasterProduct)
                {
                    foreach (ProductVariant productVariant in product.GetVariants())
                    {
                        itemVariantKey = CreateItemVariantKey(product.ItemId, productVariant.InventoryDimensionId);
                        itemAvailableMap.TryGetValue(itemVariantKey, out itemVariantQty);

                        //TODO - Review
                        productVariant.SetProperty(AvailableQty, itemVariantQty);
                    }
                }
            }
        }

        private static string CreateItemVariantKey(string itemId, string inventoryDimensionId)
        {
            if (String.IsNullOrEmpty(inventoryDimensionId))
                return itemId;
            else
                return itemId + "-" + inventoryDimensionId;
        }
    }*/
}
