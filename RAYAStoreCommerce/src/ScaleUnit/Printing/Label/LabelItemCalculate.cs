using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Dynamics.Commerce.Runtime;
using Microsoft.Dynamics.Commerce.Runtime.DataModel;
using Microsoft.Dynamics.Commerce.Runtime.Messages;
using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
using Microsoft.Dynamics.Commerce.Runtime.Services.Messages;
using Microsoft.Dynamics.Commerce.Runtime.Data;

using RR.Commerce.Runtime.Utilities;

namespace RR.Commerce.Runtime.Printing
{
    public sealed class LabelItemCalculateRequestHandler : SingleAsyncRequestHandler<LabelItemCalculateRequest>
    {
        private const int PageSize = 50;
        private RequestContext Context;
        private LabelItemCalculator calculator;

        protected override async Task<Response> Process(LabelItemCalculateRequest request)
        {
            ThrowIf.Null(request, "request");
            ThrowIf.Null(request.Parameters, "request.Parameters");

            int labelCount = 0;
            bool hasNext = false;
            Tuple<int, bool> tuple;
            Context = request.RequestContext;

            calculator = new LabelItemCalculator(request.RequestContext);

            if (request.Criteria != null)
            {
                //labelCount = ProcessByCriteria(request.LabelType, request.Parameters, request.Personal, request.Criteria, request.PageNumber, ref hasNext);
                tuple = await ProcessByCriteria(request.LabelType, request.Parameters, request.Personal, request.Criteria, request.PageNumber).ConfigureAwait(false);
                labelCount = tuple.Item1;
                hasNext = tuple.Item2;
            }
            else if (request.Items != null)
            {
                labelCount = await ProcessByItems(request.LabelType, request.Parameters, request.Personal, request.Items).ConfigureAwait(false);
            }
            /*
            if (request.Criteria != null)
                labelCount += ProcessByCriteria(request.LabelType, request.Parameters, request.Personal, request.Criteria);
            if (request.Items != null)
                labelCount += ProcessByItems(request.LabelType, request.Parameters, request.Personal, request.Items);
            */
            DbConnectionPool.ReSetSize(request.RequestContext);
            return new LabelItemCalculateResponse(labelCount, hasNext);
        }

        //private int ProcessByCriteria(LabelLayoutType labelType, LabelCalculateParams parameters, bool personal, LabelItemCalculateCriteria criteria, int pageNumber, ref bool hasNext)
        private async Task<Tuple<int, bool>> ProcessByCriteria(LabelLayoutType labelType, LabelCalculateParams parameters, bool personal, LabelItemCalculateCriteria criteria, int pageNumber)
        {
            //ReadOnlyCollection<Item> items = null;
            Tuple<ReadOnlyCollection<Item>, bool> tuple = new Tuple<ReadOnlyCollection<Item>, bool>(null, false);
            DbConnectionPool.SetSize(this.Context);
            
            switch (criteria.SearchOption)
            {
                case LabelItemSearchBy.CategoryHierarchy:
                    //items = GetItemsByCategory(criteria.CategoryId, criteria.HierarchyId == 0 ? calculator.Configuration.LabelHierarchyRecId : criteria.HierarchyId, pageNumber, ref hasNext); // Workaround
                    tuple = await GetItemsByCategory(criteria.CategoryId, criteria.HierarchyId == 0 ? calculator.Configuration.LabelHierarchyRecId : criteria.HierarchyId, pageNumber).ConfigureAwait(false); // Workaround
                    break;
                case LabelItemSearchBy.LocationHierarchy:
                    //items = GetItemsByLocation(criteria.SectionId, criteria.ShelfId, pageNumber, ref hasNext);
                    tuple = await GetItemsByLocation(criteria.SectionId, criteria.ShelfId, pageNumber).ConfigureAwait(false);
                    break;
                case LabelItemSearchBy.ItemFilter:
                    //items = GetItemsByFilter(criteria.ItemFilter, pageNumber, ref hasNext);
                    tuple = await GetItemsByFilter(criteria.ItemFilter, pageNumber).ConfigureAwait(false);
                    break;
            }
            int itemCount = await ProcessItems(labelType, parameters, personal, criteria, tuple.Item1).ConfigureAwait(false);
            return Tuple.Create(itemCount, tuple.Item2);
            //return ProcessItems(labelType, parameters, personal, criteria, items).Result;
        }

        //private ReadOnlyCollection<Item> GetItemsByFilter(string searchText, int pageNumber, ref bool hasNext)
        private async Task<Tuple<ReadOnlyCollection<Item>, bool>> GetItemsByFilter(string searchText, int pageNumber)
        {
            ThrowIf.NullOrWhiteSpace(searchText, "searchText");
            bool hasNext = false;
            ReadOnlyCollection<Item> items = null;
            long channelId = this.Context.GetPrincipal().ChannelId;

            if (pageNumber == 0)
            {
                GetProductBarcodeDataResponse productBarcodeResponse = await this.Context.ExecuteAsync<GetProductBarcodeDataResponse>(new GetProductBarcodeDataRequest(searchText)).ConfigureAwait(false);
                if (productBarcodeResponse != null && productBarcodeResponse.Barcode != null && !string.IsNullOrWhiteSpace(productBarcodeResponse.Barcode.ItemId))
                {
                    items = (await this.Context.ExecuteAsync<GetItemsDataResponse>(new GetItemsDataRequest(new string[] { productBarcodeResponse.Barcode.ItemId })).ConfigureAwait(false)).Items;
                    return new Tuple<ReadOnlyCollection<Item>, bool>(items, hasNext);
                }
            }

            QueryResultSettings settings = new QueryResultSettings(new PagingInfo(PageSize, pageNumber * PageSize));
            GetProductSearchResultsDataRequest productRequest = new GetProductSearchResultsDataRequest(channelId, 0L, searchText, settings);
            PagedResult<ProductSearchResult> psr = (await this.Context.ExecuteAsync<EntityDataServiceResponse<ProductSearchResult>>(productRequest).ConfigureAwait(false)).PagedEntityCollection;

            IEnumerable<string> itemIds = psr.Results.Select(p => p.ItemId).Distinct();
            if (itemIds == null || !itemIds.Any())
                return new Tuple<ReadOnlyCollection<Item>, bool>(null, false);

            hasNext = psr.HasNextPage;

            items = (await this.Context.ExecuteAsync<GetItemsDataResponse>(new GetItemsDataRequest(itemIds)).ConfigureAwait(false)).Items;
            return Tuple.Create(items, hasNext);
            // return items;
            /*
            if (productBarcodeResponse != null && productBarcodeResponse.Barcode != null && !string.IsNullOrWhiteSpace(productBarcodeResponse.Barcode.ItemId))
            {
                productRequest = new GetProductSearchResultsDataRequest(channelId, 0L, productBarcodeResponse.Barcode.ItemId, settings);
                productRequest.UseFuzzySearch = false;
            }
            else
            {
                productRequest = new GetProductSearchResultsDataRequest(channelId, 0L, searchText, settings);
            }

            PagedResult<ProductSearchResult> psr = this.Context.Execute<EntityDataServiceResponse<ProductSearchResult>>(productRequest).PagedEntityCollection;
            if (psr.HasNextPage)
                throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_ValueOutOfRange, "Search returned too many products. Please apply filter.");
            */
        }

        //private ReadOnlyCollection<Item> GetItemsByLocation(string sectionId, string shelfId, int pageNumber, ref bool hasNext)
        private async Task<Tuple<ReadOnlyCollection<Item>, bool>> GetItemsByLocation(string sectionId, string shelfId, int pageNumber)
        {
            ThrowIf.NullOrWhiteSpace(sectionId, "sectionId");
            Tuple<ReadOnlyCollection<Item>, bool> tuple;
            bool hasNext = false;
            QueryResultSettings settings = new QueryResultSettings(new PagingInfo(PageSize, pageNumber * PageSize));

            GetStoreLocationItemIdsServiceRequest request = new GetStoreLocationItemIdsServiceRequest(sectionId, shelfId, settings);
            IEnumerable<string> itemIds = (await this.Context.ExecuteAsync<GetItemIdsDataResponse>(request).ConfigureAwait(false)).ItemIds;
            if (itemIds == null || !itemIds.Any())
                return new Tuple<ReadOnlyCollection<Item>, bool>(null, false);  // return null;

            if (itemIds.Count() > PageSize)
            {
                itemIds = itemIds.Take(PageSize);
                hasNext = true;
            }

            ReadOnlyCollection<Item> items = (await this.Context.ExecuteAsync<GetItemsDataResponse>(new GetItemsDataRequest(itemIds)).ConfigureAwait(false)).Items;
            tuple = new Tuple<ReadOnlyCollection<Item>, bool>(items, hasNext);
            return tuple;
            //return items;
        }

        //private ReadOnlyCollection<Item>, bool> GetItemsByCategory(long categoryId, long hierarchyId, int pageNumber, ref bool hasNext)
        private async Task<Tuple<ReadOnlyCollection<Item>, bool>> GetItemsByCategory(long categoryId, long hierarchyId, int pageNumber)
        {
            Tuple<ReadOnlyCollection<Item>, bool> tuple;
            bool hasNext = false;
            QueryResultSettings settings = new QueryResultSettings(new PagingInfo(PageSize, pageNumber * PageSize));

            GetCategoryHierarchyProductsDataRequest request = new GetCategoryHierarchyProductsDataRequest(categoryId, hierarchyId, settings);
            IEnumerable<long> productIds = (await this.Context.ExecuteAsync<GetProductIdsDataResponse>(request).ConfigureAwait(false)).ProductIds;
            if (productIds == null || !productIds.Any())
                return null;

            if (productIds.Count() > PageSize)
            {
                productIds = productIds.Take(PageSize);
                hasNext = true;
            }

            ReadOnlyCollection<Item> items = (await this.Context.ExecuteAsync<GetItemsDataResponse>(new GetItemsDataRequest(productIds)).ConfigureAwait(false)).Items;
            tuple = new Tuple<ReadOnlyCollection<Item>, bool>(items, hasNext);
            return tuple;
            //return items;
        }

        private async Task ProcessItemAsync(LabelLayoutType labelType, LabelCalculateParams parameters, bool personal, LabelItemCalculateCriteria criteria, ReadOnlyCollection<Item> items)
        {
            await ProcessItems(labelType, parameters, personal, criteria, items).ConfigureAwait(false);
        }
        
        private async Task<int> ProcessItems(LabelLayoutType labelType, LabelCalculateParams parameters, bool personal, LabelItemCalculateCriteria criteria, ReadOnlyCollection<Item> items)
        {
            if (items == null || !items.Any())
                return 0;

            int numberOfItems = 0;

            LabelItemCalculateQuantity qtyCalc = new LabelItemCalculateQuantity(this.Context, criteria, labelType);
            foreach (Item item in items)
            {
                // System.Diagnostics.Debug.WriteLine(item.ItemId);

                GetLabelProfileServiceResponse lbsr = await this.Context.ExecuteAsync<GetLabelProfileServiceResponse>(new GetLabelProfileServiceRequest(labelType, parameters.CodeRecId, item.Product)).ConfigureAwait(false);

                //TEST
                if (parameters.CodeRecId < 0)
                {
                    lbsr = new GetLabelProfileServiceResponse();
                    lbsr.Profile = new LabelProfileLine();
                    lbsr.Profile.LayoutId = -1;
                    lbsr.Profile.PrinterId = -1;
                    lbsr.Profile.FormatId = -1;
                    lbsr.Format = LabelFormat.Default();
                }
                //

                if (lbsr.Profile != null && lbsr.Format != null && lbsr.Profile.Active)
                {
                    if (labelType == LabelLayoutType.Product)
                    {
                        IEnumerable<ProductVariant> productVariants = await GetItemVariants(item.ItemId).ConfigureAwait(false);
                        if (productVariants != null)
                        {
                            qtyCalc.Calculate(productVariants);

                            foreach (ProductVariant variant in productVariants)
                            {
                                LabelItem litem = await calculator.CreateLabelItem(labelType, parameters, personal, item, variant, lbsr.Profile).ConfigureAwait(false);
                                litem.NoOfLabels = await qtyCalc.GetNoOfLabels(variant.ItemId, variant.InventoryDimensionId, variant.DistinctProductVariantId).ConfigureAwait(false);
                                if (litem.NoOfLabels > 0)
                                {
                                    if (await calculator.CalculateLabelItem(litem, lbsr.Format).ConfigureAwait(false))
                                    {
                                        if (await CheckChangeTracking(litem, lbsr.Format, criteria.ChangeTracking).ConfigureAwait(false))
                                        {
                                            numberOfItems++;
                                            await this.Context.ExecuteAsync<SingleEntityDataServiceResponse<long>>(new LabelItemSaveDataRequest(litem)).ConfigureAwait(false);
                                        }
                                    }
                                }
                            }

                        }
                        else
                        {
                            LabelItem litem = await calculator.CreateLabelItem(labelType, parameters, personal, item, null, lbsr.Profile).ConfigureAwait(false);

                            litem.NoOfLabels = await qtyCalc.Calculate(item.ItemId, item.Product).ConfigureAwait(false);
                            if (litem.NoOfLabels > 0)
                            {
                                if (await calculator.CalculateLabelItem(litem, lbsr.Format).ConfigureAwait(false))
                                {
                                    if (await CheckChangeTracking(litem, lbsr.Format, criteria.ChangeTracking).ConfigureAwait(false))
                                    {
                                        numberOfItems++;
                                        await this.Context.ExecuteAsync<SingleEntityDataServiceResponse<long>>(new LabelItemSaveDataRequest(litem)).ConfigureAwait(false);
                                    }
                                }
                            }
                        }
                    }
                    else if (labelType == LabelLayoutType.Shelf)
                    {
                        LabelItem litem = await calculator.CreateLabelItem(labelType, parameters, personal, item, null, lbsr.Profile).ConfigureAwait(false);

                        litem.NoOfLabels = await qtyCalc.Calculate(item.ItemId, item.Product).ConfigureAwait(false);
                        if (litem.NoOfLabels > 0)
                        {
                            if (await calculator.CalculateLabelItem(litem, lbsr.Format).ConfigureAwait(false))
                            {
                                if (await CheckChangeTracking(litem, lbsr.Format, criteria.ChangeTracking).ConfigureAwait(false))
                                {
                                    IEnumerable<ItemStoreLocation> locations = (await this.Context.ExecuteAsync<GetItemStoreLocationsResponse>(new GetItemStoreLocationsServiceRequest(item.ItemId, criteria.SectionId, criteria.ShelfId)).ConfigureAwait(false)).Locations;
                                    foreach (ItemStoreLocation location in locations)
                                    {
                                        litem.SectionId = location.SectionId;
                                        litem.ShelfId = location.ShelfId;
                                        litem.Location = location.Location;

                                        numberOfItems++;
                                        await this.Context.ExecuteAsync<SingleEntityDataServiceResponse<long>>(new LabelItemSaveDataRequest(litem)).ConfigureAwait(false);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            //return 0;
            return numberOfItems;
        }

        private async Task<IEnumerable<ProductVariant>> GetItemVariants(string itemId)
        {
            ThrowIf.NullOrWhiteSpace(itemId, "itemId");

            PagedResult<ProductVariant> productVariants = (await this.Context.ExecuteAsync<EntityDataServiceResponse<ProductVariant>>(new GetProductVariantsByItemDataRequest(itemId)).ConfigureAwait(false)).PagedEntityCollection;

            if (productVariants != null && productVariants.Results.Any())
                return productVariants.Results;
            else
                return null;
        }

        //

        private async Task<IList<LabelItemCalculateItem>> TransformItems(IList<LabelItemCalculateItem> licItems)
        {
            List<LabelItemCalculateItem> licOutItems = new List<LabelItemCalculateItem>();

            // Items
            licOutItems.AddRange(licItems.Where(i => !String.IsNullOrEmpty(i.ItemId)));

            // Barcodes
            var barcodes = licItems.Where(i => String.IsNullOrEmpty(i.ItemId) && !String.IsNullOrEmpty(i.BarCode)).GroupBy(i => i.BarCode).Select(g => new { BarCode = g.Key, Quantity = g.Sum(e => e.Quantity) });
            foreach (var barcode in barcodes)
            {
                ItemBarcode itemBarcode = (await this.Context.ExecuteAsync<GetProductBarcodeDataResponse>(new GetProductBarcodeDataRequest(barcode.BarCode)).ConfigureAwait(false)).Barcode;
                if (itemBarcode != null)
                    licOutItems.Add(new LabelItemCalculateItem(itemBarcode.ItemId, itemBarcode.InventoryDimensionId, barcode.Quantity));
            }

            /*
            Dictionary<string, int> barcodes = new Dictionary<string, int>();
            string key = string.Empty;
            int value = 0;
            for (int i = 0; i < list_licItem.Count; i++)
            {
                key = list_licItem[i].BarCode;
                if (!(string.IsNullOrEmpty(key) || string.IsNullOrWhiteSpace(key)))
                {
                    key = list_licItem[i].BarCode.Trim();
                    value = list_licItem[i].Quantity;
                    if (barcodes.ContainsKey(key))
                    {
                        int tempVal = barcodes[key];
                        barcodes.Remove(key);
                        barcodes.Add(key, value + tempVal);
                    }
                    else
                        barcodes.Add(key, value);
                }
            }

            if (barcodes.Count > 0)
            {
                licItems = new List<LabelItemCalculateItem>();
                GetProductBarcodeDataRequest getProductBarcodeDataRequest;
                ItemBarcode IB;
                foreach (KeyValuePair<string, int> kvp in barcodes)
                {
                    getProductBarcodeDataRequest = new GetProductBarcodeDataRequest(kvp.Key);
                    IB = this.Context.Execute<GetProductBarcodeDataResponse>(getProductBarcodeDataRequest).Barcode;
                    if (IB != null)
                        licItems.Add(new LabelItemCalculateItem(IB.ItemId, IB.InventoryDimensionId, kvp.Value));
                }
            }
            else
                licItems = list_licItem.ToList();
            */

            return licOutItems;
        }

        private async Task<int> ProcessByItems(LabelLayoutType labelType, LabelCalculateParams parameters, bool personal, IList<LabelItemCalculateItem> licInItems)
        {
            IList<LabelItemCalculateItem> licItems = await TransformItems(licInItems).ConfigureAwait(false);

            IEnumerable<string> itemIds = licItems.Select(i => i.ItemId).Distinct();
            if (!itemIds.Any())
                return 0;

            int numberOfItems = 0;

            ReadOnlyCollection<Item> items = (await this.Context.ExecuteAsync<GetItemsDataResponse>(new GetItemsDataRequest(itemIds)).ConfigureAwait(false)).Items;
            foreach (Item item in items)
            {
                GetLabelProfileServiceResponse lbsr = await this.Context.ExecuteAsync<GetLabelProfileServiceResponse>(new GetLabelProfileServiceRequest(labelType, parameters.CodeRecId, item.Product)).ConfigureAwait(false);

                //TEST
                if (parameters.CodeRecId < 0)
                {
                    lbsr = new GetLabelProfileServiceResponse();
                    lbsr.Profile = new LabelProfileLine();
                    lbsr.Profile.LayoutId = -1;
                    lbsr.Profile.PrinterId = -1;
                    lbsr.Profile.FormatId = -1;
                    lbsr.Format = LabelFormat.Default();
                }
                //

                if (lbsr.Profile != null && lbsr.Format != null && lbsr.Profile.Active)
                {
                    if (labelType == LabelLayoutType.Product)
                    {
                        IEnumerable<ItemVariantInventoryDimension> inventDimensionIds =
                            licItems.Where(i => i.ItemId == item.ItemId && !String.IsNullOrEmpty(i.InventDimensionId)).
                                Select(i => i.InventDimensionId).Distinct().
                                Select(i => new ItemVariantInventoryDimension() { ItemId = item.ItemId, VariantInventoryDimensionId = i });

                        if (inventDimensionIds.Any())
                        {
                            PagedResult<ProductVariant> variants = (await this.Context.ExecuteAsync<EntityDataServiceResponse<ProductVariant>>(new GetProductVariantsDataRequest(inventDimensionIds)).ConfigureAwait(false)).PagedEntityCollection;
                            foreach (ProductVariant variant in variants)
                            {
                                // System.Diagnostics.Debug.WriteLine(variant.VariantId);

                                /*  TEST
                                GetProductsDataRequest pdr = new GetProductsDataRequest(new long[] { variant.DistinctProductVariantId }, QueryResultSettings.AllRecords);
                                ReadOnlyCollection<SimpleProduct> spl = this.Context.Execute<EntityDataServiceResponse<SimpleProduct>>(pdr).PagedEntityCollection.Results;
                                SimpleProduct sp = spl.FirstOrDefault();
                                if (sp != null) ProductVariant variant2 = ProductVariant.ConvertFrom(sp);

                                GetProductsServiceRequest psr = new GetProductsServiceRequest(this.Context.GetPrincipal().ChannelId, new long[] { variant.DistinctProductVariantId }, false, QueryResultSettings.AllRecords);
                                psr.SearchLocation = SearchLocation.Local;
                                GetProductsServiceResponse gpsr = this.Context.Execute<GetProductsServiceResponse>(psr);
                                sp = gpsr.Products.FirstOrDefault();
                                if (sp != null) ProductVariant variant3 = ProductVariant.ConvertFrom(sp);
                                */

                                LabelItem litem = await calculator.CreateLabelItem(labelType, parameters, personal, item, variant, lbsr.Profile).ConfigureAwait(false);

                                litem.UnitId = licItems.FirstOrDefault(i => i.ItemId == variant.ItemId && i.InventDimensionId == variant.InventoryDimensionId).UnitOfMeasure ?? litem.UnitId;
                                litem.NoOfLabels = licItems.Where(i => i.ItemId == variant.ItemId && i.InventDimensionId == variant.InventoryDimensionId).Sum(i => i.Quantity);

                                if (litem.NoOfLabels > 0 && await calculator.CalculateLabelItem(litem, lbsr.Format).ConfigureAwait(false))
                                {
                                    numberOfItems++;
                                    await this.Context.ExecuteAsync<SingleEntityDataServiceResponse<long>>(new LabelItemSaveDataRequest(litem)).ConfigureAwait(false);
                                }
                            }
                        }

                        IEnumerable<long> productIds =
                            licItems.Where(i => i.ItemId == item.ItemId && i.ProductId != 0 && String.IsNullOrEmpty(i.InventDimensionId)).
                                Select(i => i.ProductId).Distinct();

                        if (productIds.Any())
                        {
                            GetProductsServiceRequest productsServiceRequest = new GetProductsServiceRequest(this.Context.GetPrincipal().ChannelId, productIds, false, QueryResultSettings.AllRecords);
                            productsServiceRequest.SearchLocation = SearchLocation.Local;
                            PagedResult<SimpleProduct> products = (await this.Context.ExecuteAsync<GetProductsServiceResponse>(productsServiceRequest).ConfigureAwait(false)).Products;
                            foreach (SimpleProduct product in products)
                            {
                                if (product.IsDistinct)
                                {
                                    ProductVariant variant = null;
                                    if (product.ProductType == ProductType.Variant || product.ProductType == ProductType.KitVariant)
                                        variant = ProductVariant.ConvertFrom(product);

                                    LabelItem litem = await calculator.CreateLabelItem(labelType, parameters, personal, item, variant, lbsr.Profile).ConfigureAwait(false);

                                    litem.UnitId = licItems.FirstOrDefault(i => i.ItemId == variant.ItemId && i.ProductId == product.RecordId && String.IsNullOrEmpty(i.InventDimensionId)).UnitOfMeasure ?? litem.UnitId;
                                    litem.NoOfLabels = licItems.Where(i => i.ItemId == variant.ItemId && i.ProductId == product.RecordId && String.IsNullOrEmpty(i.InventDimensionId)).Sum(i => i.Quantity);

                                    if (litem.NoOfLabels > 0 && await calculator.CalculateLabelItem(litem, lbsr.Format).ConfigureAwait(false))
                                    {
                                        numberOfItems++;
                                        await this.Context.ExecuteAsync<SingleEntityDataServiceResponse<long>>(new LabelItemSaveDataRequest(litem)).ConfigureAwait(false);
                                    }
                                }
                            }
                        }

                        int NoOfLabels = licItems.Where(i => i.ItemId == item.ItemId && i.ProductId == 0 && String.IsNullOrEmpty(i.InventDimensionId)).Sum(i => i.Quantity);
                        if (NoOfLabels > 0)
                        {
                            LabelItem litem = await calculator.CreateLabelItem(labelType, parameters, personal, item, null, lbsr.Profile).ConfigureAwait(false);

                            litem.UnitId = licItems.FirstOrDefault(i => i.ItemId == item.ItemId && i.ProductId == 0 && String.IsNullOrEmpty(i.InventDimensionId)).UnitOfMeasure ?? litem.UnitId;
                            litem.NoOfLabels = NoOfLabels;

                            if (await calculator.CalculateLabelItem(litem, lbsr.Format).ConfigureAwait(false))
                            {
                                numberOfItems++;
                                await this.Context.ExecuteAsync<SingleEntityDataServiceResponse<long>>(new LabelItemSaveDataRequest(litem)).ConfigureAwait(false);
                            }
                        }
                    }
                    else if (labelType == LabelLayoutType.Shelf)
                    {
                        LabelItem litem = await calculator.CreateLabelItem(labelType, parameters, personal, item, null, lbsr.Profile).ConfigureAwait(false);

                        litem.UnitId = licItems.FirstOrDefault(i => i.ItemId == item.ItemId).UnitOfMeasure ?? litem.UnitId;

                        litem.NoOfLabels = licItems.Where(i => i.ItemId == item.ItemId).Sum(i => i.Quantity);
                        if (litem.NoOfLabels <= 0)
                            litem.NoOfLabels = 1;

                        if (await calculator.CalculateLabelItem(litem, lbsr.Format).ConfigureAwait(false))
                        {
                            IEnumerable<ItemStoreLocation> locations = (await this.Context.ExecuteAsync<GetItemStoreLocationsResponse>(new GetItemStoreLocationsServiceRequest(item.ItemId)).ConfigureAwait(false)).Locations;
                            foreach (ItemStoreLocation location in locations)
                            {
                                litem.SectionId = location.SectionId;
                                litem.ShelfId = location.ShelfId;
                                litem.Location = location.Location;

                                numberOfItems++;
                                await this.Context.ExecuteAsync<SingleEntityDataServiceResponse<long>>(new LabelItemSaveDataRequest(litem)).ConfigureAwait(false);
                            }
                        }
                    }
                }
            }
         
            return numberOfItems;
        }

        private async Task<bool> CheckChangeTracking(LabelItem litem, LabelFormat format, LabelItemChangeTracking changeTracking)
        {
            if (changeTracking == LabelItemChangeTracking.None)
                return true;

            TrackLabelItemServiceRequest request = new TrackLabelItemServiceRequest(litem, format, changeTracking);
            SingleEntityDataServiceResponse<bool> response = await this.Context.ExecuteAsync<SingleEntityDataServiceResponse<bool>>(request).ConfigureAwait(false);

            return response.Entity;
        }

        // Test

        // Sample
        private void ProductSearch(LabelItemCalculateRequest request)
        {
            ThrowIf.Null(request, "request");

            /*
            long currentChannelId = this.Context.GetPrincipal().ChannelId;

            var searchRequest = new ProductSearchServiceRequest(request.QueryCriteria, request.QueryResultSettings);
            ProductSearchServiceResponse searchResponse = this.Context.Runtime.Execute<ProductSearchServiceResponse>(searchRequest, this.Context);

            ProductSearchResultContainer results = searchResponse.ProductSearchResult;
            if (results.Results.Any() && !request.QueryCriteria.Context.IsRemoteLookup(currentChannelId))
            {
                // retrieve and update prices
                var priceRequest = new GetProductPricesServiceRequest(results.Results);
                var priceResponse = this.Context.Execute<GetProductPricesServiceResponse>(priceRequest);
                var productMap = new Dictionary<long, Product>(results.Results.Count);

                foreach (var product in results.Results)
                {
                    productMap[product.RecordId] = product;
                }

                // update prices on the products
                SearchProductsRequestHandler.SetProductPrices(priceResponse.ProductPrices.Results, productMap, results.ProductIdLookupMap);
            }
            */
        }

        private void GetProducts(IList<LabelItemCalculateItem> licItems)
        {
            /* Item
            List<ProductLookupClause> itemAndInventDimIdCombinations = new List<ProductLookupClause>();
            ProductLookupClause productLookupClause;

            foreach (LabelItemCalculateItem item in items)
            {
                productLookupClause = new ProductLookupClause();
                productLookupClause.ItemId = item.ItemId;
                productLookupClause.InventDimensionId = item.InventDimensionId;
                productLookupClause.Barcode = item.Barcode;
                itemAndInventDimIdCombinations.Add(productLookupClause);
            }

            if (itemAndInventDimIdCombinations.Count <= 0)
                return;

            // GetProductsServiceRequest
            GetProductsServiceRequest request = new GetProductsServiceRequest(this.Context.GetPrincipal().ChannelId, itemAndInventDimIdCombinations, QueryResultSettings.AllRecords);
            request.SearchLocation = SearchLocation.Local;
            PagedResult<SimpleProduct> products = this.Context.Execute<GetProductsServiceResponse>(request).Products;

            // GetProductsDataRequest
            GetProductsDataRequest productsDataRequest = new GetProductsDataRequest(request.ItemAndInventDimIdCombinations, request.QueryResultSettings, false);
            products = this.Context.Execute<EntityDataServiceResponse<SimpleProduct>>(productsDataRequest).PagedEntityCollection;

            // GetItemsDataRequest
            List<string> itemIds = new List<string>();
            itemIds.Add("0001");
            itemIds.Add("0140");
            GetItemsDataRequest itemRequest = new GetItemsDataRequest(itemIds);
            GetItemsDataResponse gidr = this.Context.Execute<GetItemsDataResponse>(itemRequest);

            // GetProductVariantsDataRequest
            List<ItemVariantInventoryDimension> variants = new List<ItemVariantInventoryDimension>();
            variants.Add(new ItemVariantInventoryDimension() { ItemId = "0140", VariantInventoryDimensionId = "000001" });
            variants.Add(new ItemVariantInventoryDimension() { ItemId = "0140", VariantInventoryDimensionId = "000003" });
            GetProductVariantsDataRequest variantRequest = new GetProductVariantsDataRequest(variants);
            PagedResult<ProductVariant> pv = this.Context.Execute<EntityDataServiceResponse<ProductVariant>>(variantRequest).PagedEntityCollection;
            */

            //GETVARIANTSBYITEMIDANDINVENTDIMIDS
        }
    }

    internal class LabelItemCalculateQuantity
    {
        private RequestContext Context;
        private IEnumerable<ItemAvailableQuantity> qtyBuffer;
        private LabelItemQtySource qtySource;
        private DateTime? fromDate;
        private DateTime? toDate;
        private int quantity;

        public LabelItemCalculateQuantity(RequestContext context, LabelItemCalculateCriteria criteria, LabelLayoutType labelType)
        {
            ThrowIf.Null(context, "context");
            ThrowIf.Null(criteria, "criteria");

            this.Context = context;
            this.qtyBuffer = null;

            if (labelType == LabelLayoutType.Product)
            {
                qtySource = criteria.QtySource;
                fromDate = criteria.FromDate;
                toDate = criteria.ToDate;
                quantity = criteria.Quantity;
            }
            else
            {
                qtySource = LabelItemQtySource.Fixed;
                quantity = criteria.Quantity > 0 ? criteria.Quantity : 1;
            }
        }

        public async Task<int> Calculate(string itemId, long productId)
        {
            switch (qtySource)
            {
                case LabelItemQtySource.Available:
                    return await CalculateAvailableQty(itemId, String.Empty, productId).ConfigureAwait(false); // GetNoOfLabelsFromBuffer(CalcQtyAvailable(new ItemVariantInventoryDimension[] { new ItemVariantInventoryDimension() { ItemId = itemId } }));
                case LabelItemQtySource.OrderReceived:
                    return 0;
                default:
                    return quantity;
            }
        }

        public void Calculate(IEnumerable<ProductVariant> productVariants)
        {
            switch (qtySource)
            {
                case LabelItemQtySource.Available:
                    // this.qtyBuffer = CalcQtyAvailable(productVariants.Select(i => new ItemVariantInventoryDimension() { ItemId = i.ItemId, VariantInventoryDimensionId = i.InventoryDimensionId }));
                    break;
            }
        }

        public async Task<int> GetNoOfLabels(string itemId, string inventDimId, long productId)
        {
            switch (qtySource)
            {
                case LabelItemQtySource.Available:
                    return await CalculateAvailableQty(itemId, inventDimId, productId).ConfigureAwait(false); // GetNoOfLabelsFromBuffer(itemId, inventDimId);
                case LabelItemQtySource.OrderReceived:
                    return 0;
                default:
                    return quantity;
            }
        }

        private int GetNoOfLabelsFromBuffer(string itemId, string inventDimId)
        {
            if (this.qtyBuffer == null)
                return 0;
            ItemAvailableQuantity iaq = this.qtyBuffer.FirstOrDefault(i => i.ItemId == itemId && i.VariantInventoryDimensionId == inventDimId);
            return iaq == null ? 0 : Decimal.ToInt32(iaq.AvailableQuantity);
        }

        private int GetNoOfLabelsFromBuffer(ReadOnlyCollection<ItemAvailableQuantity> qtyBuffer)
        {
            if (qtyBuffer == null || qtyBuffer.Count == 0)
                return 0;
            return Decimal.ToInt32(qtyBuffer[0].AvailableQuantity);
        }

        private async Task<ReadOnlyCollection<ItemAvailableQuantity>> CalcQtyAvailable(IEnumerable<ItemVariantInventoryDimension> itemVariants)
        {
            GetItemVariantAvailabilityRequest request = new GetItemVariantAvailabilityRequest(itemVariants);
            GetItemVariantAvailabilityResponse response = await this.Context.ExecuteAsync<GetItemVariantAvailabilityResponse>(request).ConfigureAwait(false);
            return response.ItemAvailableQuantities.Results;
        }

        private async Task<int> CalculateAvailableQty(string itemId, string inventDimId, long productId)
        {
            return Decimal.ToInt32((await this.Context.ExecuteAsync<SingleEntityDataServiceResponse<decimal>>(new GetItemVariantAvailableQtyDataRequest(itemId, inventDimId, productId)).ConfigureAwait(false)).Entity);
        }
    }
}
