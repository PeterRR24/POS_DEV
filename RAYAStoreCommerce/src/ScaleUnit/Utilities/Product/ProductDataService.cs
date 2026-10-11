using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

using Microsoft.Dynamics.Commerce.Runtime;
using Microsoft.Dynamics.Commerce.Runtime.Messages;
using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
using Microsoft.Dynamics.Commerce.Runtime.DataModel;
using Microsoft.Dynamics.Commerce.Runtime.Data;
using System.Threading.Tasks;

namespace RR.Commerce.Runtime.Utilities
{
    public class ProductDataService : IRequestHandlerAsync
    {
        public IEnumerable<Type> SupportedRequestTypes
        {
            get
            {
                return new[]
                {
                    typeof(GetProductVariantsByItemDataRequest),
                    typeof(GetBarcodeForProductDataRequest),
                    typeof(GetProductCategoryHierarchyDataRequest),
                    typeof(GetCategoryHierarchyProductsDataRequest),
                    typeof(GetVariantIdForItemDataRequest),
                    typeof(GetItemTrackingInfoDataRequest),
                    typeof(GetBarcodeForItemRequest),
                    typeof(ProductSearchByTextRequest)
                };
            }
        }

        public async Task<Response> Execute(Request request)
        {
            if (request == null)
                throw new ArgumentNullException("request");

            Type requestType = request.GetType();
            Response response;

            if (requestType == typeof(GetProductVariantsByItemDataRequest))
                response = await this.GetProductVariants((GetProductVariantsByItemDataRequest)request).ConfigureAwait(false);
            else if (requestType == typeof(GetBarcodeForProductDataRequest))
                response = await this.GetProductBarcode((GetBarcodeForProductDataRequest)request).ConfigureAwait(false);
            else if (requestType == typeof(GetProductCategoryHierarchyDataRequest))
                response = await this.GetProductCategoryHierarchy((GetProductCategoryHierarchyDataRequest)request).ConfigureAwait(false);
            else if (requestType == typeof(GetCategoryHierarchyProductsDataRequest))
                response = await this.GetCategoryHierarchyProducts((GetCategoryHierarchyProductsDataRequest)request).ConfigureAwait(false);
            else if (requestType == typeof(GetVariantIdForItemDataRequest))
                response = await this.GetVariantIdForItem((GetVariantIdForItemDataRequest)request).ConfigureAwait(false);
            else if (requestType == typeof(GetItemTrackingInfoDataRequest))
                response = await this.GetItemTrackingInfo((GetItemTrackingInfoDataRequest)request).ConfigureAwait(false);
            else if (requestType == typeof(GetBarcodeForItemRequest))
                response = await this.GetBarcodeForItems((GetBarcodeForItemRequest)request).ConfigureAwait(false);
            //else if (requestType == typeof(ProductSearchByTextRequest))
            //    response = this.ProductSearchByText((ProductSearchByTextRequest)request);
            else if (requestType == typeof(ProductSearchByTextRequest))
                response = await this.SearchProducts((ProductSearchByTextRequest)request).ConfigureAwait(false);
            else
                throw new NotSupportedException(String.Format("Request '{0}' is not supported.", request.GetType()));

            return response;
        }

        private async Task<EntityDataServiceResponse<ProductVariant>> GetProductVariants(GetProductVariantsByItemDataRequest request)
        {
            ThrowIf.Null(request, "request");
            ThrowIf.Null(request.ItemId, "request.ItemId");

            List<ItemVariantInventoryDimension> itemDimensions = new List<ItemVariantInventoryDimension>();
            ReadOnlyCollection<string> InvDimIds;
            string dataAreaId = request.RequestContext.GetChannelConfiguration().InventLocationDataAreaId;

            //SqlQuery query = new SqlQuery();
            //query.QueryString = "SELECT DISTINCTPRODUCTVARIANT, INVENTDIMID FROM ax.INVENTDIMCOMBINATION WHERE (ITEMID = @ItemId) AND (DATAAREAID = @DataAreaId)";
            //query.Parameters["@DataAreaId"] = dataAreaId;
            //query.Parameters["@ItemId"] = request.ItemId;

            SqlPagedQuery query = new SqlPagedQuery(QueryResultSettings.AllRecords);
            query.DatabaseSchema = "ax";
            query.Select = new ColumnSet("INVENTDIMID");
            query.From = "INVENTDIMCOMBINATION";
            query.Where = "(ITEMID = @ItemId) AND (DATAAREAID = @DataAreaId)";
            query.Parameters["@DataAreaId"] = dataAreaId;
            query.Parameters["@ItemId"] = request.ItemId;

            using (DatabaseQuery databaseContext = new DatabaseQuery(request.RequestContext))
            {
                InvDimIds = await databaseContext.ExecuteScalarCollectionAsync<string>(query).ConfigureAwait(false);
            }
            itemDimensions = InvDimIds.Where(dim => !String.IsNullOrEmpty(dim)).Distinct().
                                Select(dim => new ItemVariantInventoryDimension() { ItemId = request.ItemId, VariantInventoryDimensionId = dim }).ToList();
                                
            //using (DatabaseQuery databaseContext = new DatabaseQuery(request.RequestContext))
            //{
            //    using (IDatabaseResult reader = databaseContext.Execute(query))
            //    {
            //        while (reader.Read())
            //        {
            //            itemDimensions.Add(new ItemVariantInventoryDimension(request.ItemId, reader.GetValue<string>(1)));
            //        }
            //    }
            //}

            if (itemDimensions.Any())
                return await request.RequestContext.ExecuteAsync<EntityDataServiceResponse<ProductVariant>>(new GetProductVariantsDataRequest(itemDimensions)).ConfigureAwait(false);
            else
                return new EntityDataServiceResponse<ProductVariant>();

            // Superseded
            // ProductDataManager productDataManager = ProductDataManager.Instantiate(request.RequestContext);
            // return new EntityDataServiceResponse<ProductVariant>(productDataManager.GetProductVariants(request.ItemIds));
        }

        private async Task<SingleEntityDataServiceResponse<ItemBarcode>> GetProductBarcode(GetBarcodeForProductDataRequest request)
        {
            ThrowIf.Null(request, "request");

            //ProductDataManager productDataManager = ProductDataManager.Instantiate(request.RequestContext);
            //return new SingleEntityDataServiceResponse<ItemBarcode>(productDataManager.GetProductBarcode(request.ItemId, request.InventDimId, request.UnitId));

            IEnumerable<ItemBarcode> itemBarcodes = null;
            SqlPagedQuery query = new SqlPagedQuery(QueryResultSettings.AllRecords);
            //query.DatabaseSchema = "ax";
            //query.Select = new ColumnSet("INVENTDIMID");
            query.From = "INVENTITEMBARCODESVIEW";
            query.Where = "ITEMID = @ITEMID AND (INVENTDIMID = @INVENTDIMID OR INVENTDIMID IS NULL) AND DATAAREAID = @DATAAREAID";
            query.Parameters["@DATAAREAID"] = request.RequestContext.GetChannelConfiguration().InventLocationDataAreaId;
            query.Parameters["ITEMID"] = request.ItemId;
            query.Parameters["@INVENTDIMID"] = request.InventDimId ?? String.Empty;

            using (DatabaseQuery databaseContext = new DatabaseQuery(request.RequestContext))
            {
                itemBarcodes = await databaseContext.ReadEntityAsync<ItemBarcode>(query).ConfigureAwait(false);
            }
            //IEnumerable<ItemBarcode> itemBarcodes = this.ExecuteSelect<ItemBarcode>(query).Results;
            ItemBarcode itemBarcode = null;

            foreach (ItemBarcode ib in itemBarcodes)
            {
                if (itemBarcode != null)
                {
                    if (String.Compare(itemBarcode.InventoryDimensionId, request.InventDimId, StringComparison.OrdinalIgnoreCase) != 0)
                    {
                        if (String.Compare(ib.InventoryDimensionId, request.InventDimId, StringComparison.OrdinalIgnoreCase) == 0)
                            itemBarcode = ib;
                    }
                    else
                    {
                        if (String.Compare(itemBarcode.UnitId, request.UnitId, StringComparison.OrdinalIgnoreCase) != 0 && String.Compare(ib.UnitId, request.UnitId, StringComparison.OrdinalIgnoreCase) == 0)
                            itemBarcode = ib;
                    }
                }
                else
                    itemBarcode = ib;
            }
            return new SingleEntityDataServiceResponse<ItemBarcode>(itemBarcode);
        }

        private async Task<EntityDataServiceResponse<ProductCategoryHierarchy>> GetProductCategoryHierarchy(GetProductCategoryHierarchyDataRequest request)
        {
            ThrowIf.Null(request, "request");

            //ProductDataManager productDataManager = ProductDataManager.Instantiate(request.RequestContext);
            //return new EntityDataServiceResponse<ProductCategoryHierarchy>(productDataManager.GetProductCategoryHierarchy(request.ProductId, request.HierarchyId));

            SqlPagedQuery query = new SqlPagedQuery(QueryResultSettings.AllRecords);
            query.DatabaseSchema = "ext";
            query.From = "GETPRODUCTCATEGORYHIERARCHY(@ProductId, @HierarchyId)";
            query.Parameters["@ProductId"] = request.ProductId;
            query.Parameters["@HierarchyId"] = request.HierarchyId;
            query.OrderBy = "LEVEL";

            PagedResult<ProductCategoryHierarchy> pch;
            using (DatabaseQuery databaseContext = new DatabaseQuery(request.RequestContext))
            {
                pch = await databaseContext.ReadEntityAsync<ProductCategoryHierarchy>(query).ConfigureAwait(false);
            }
            return new EntityDataServiceResponse<ProductCategoryHierarchy>(pch);
        }

        private async Task<GetProductIdsDataResponse> GetCategoryHierarchyProducts(GetCategoryHierarchyProductsDataRequest request)
        {
            ThrowIf.Null(request, "request");

            ReadOnlyCollection<long> list;

            //SqlQuery query = new SqlQuery();
            //query.QueryString = "SELECT * FROM ext.GETCATEGORYHIERARCHYPRODUCTS(@CategoryId, @HierarchyId)";
            //query.Parameters["@CategoryId"] = request.CategoryId;
            //query.Parameters["@HierarchyId"] = request.HierarchyId;

            SqlPagedQuery query = new SqlPagedQuery(request.QueryResultSettings);
            query.DatabaseSchema = "ext";
            query.Select = new ColumnSet("PRODUCT");
            query.From = "GETCATEGORYHIERARCHYPRODUCTS(@CategoryId, @HierarchyId)";
            query.OrderBy = "PRODUCT";
            query.Distinct = true;
            query.Parameters["@CategoryId"] = request.CategoryId;
            query.Parameters["@HierarchyId"] = request.HierarchyId;

            using (DatabaseContext databaseContext = new DatabaseContext(request.RequestContext))
            {
                list = await databaseContext.ExecuteScalarCollectionAsync<long>(query).ConfigureAwait(false);
                /*
                using (IDatabaseResult reader = databaseContext.ExecuteQuery(query))
                {
                    while (reader.Read())
                    {
                        list.Add(reader.GetValue<long>(0));
                    }
                }
                */
            }

            return new GetProductIdsDataResponse(list);
        }

        private async Task<SingleEntityDataServiceResponse<string>> GetVariantIdForItem(GetVariantIdForItemDataRequest request)
        {
            ThrowIf.Null(request, "request");

            if (String.IsNullOrEmpty(request.ConfigId) && String.IsNullOrEmpty(request.InventSizeId) && String.IsNullOrEmpty(request.InventColorId) && String.IsNullOrEmpty(request.InventStyleId))
                return null;

            string dataAreaId = request.RequestContext.GetChannelConfiguration().InventLocationDataAreaId;

            SqlQuery query = new SqlQuery();

            query.QueryString = "SELECT RETAILVARIANTID FROM ax.INVENTDIMCOMBINATION IDC INNER JOIN ax.INVENTDIM ID ON IDC.INVENTDIMID = ID.INVENTDIMID " +
                "WHERE IDC.ITEMID = @ITEMID AND IDC.DATAAREAID = @DATAAREAID " +
                "AND ID.CONFIGID IN (@CONFIGID,'') " +
                "AND ID.INVENTSIZEID IN(@SIZEID,'') " +
                "AND ID.INVENTSTYLEID IN (@STYLEID,'') " +
                "AND ID.INVENTCOLORID IN (@COLORID,'')";                

            query.Parameters["@DATAAREAID"] = dataAreaId;
            query.Parameters["@ITEMID"] = request.ItemId;
            query.Parameters["@CONFIGID"] = request.ConfigId;
            query.Parameters["@SIZEID"] = request.InventSizeId;
            query.Parameters["@STYLEID"] = request.InventStyleId;
            query.Parameters["@COLORID"] = request.InventColorId;

            string variantId = null;

            using (DatabaseContext databaseContext = new DatabaseContext(request.RequestContext))
            {
                variantId = await databaseContext.ExecuteScalarAsync<string>(query).ConfigureAwait(false);
            }

            return new SingleEntityDataServiceResponse<string>(variantId);
        }

        private async Task<SingleEntityDataServiceResponse<int>> GetItemTrackingInfo(GetItemTrackingInfoDataRequest request)
        {
            ThrowIf.Null(request, "request");

            string dataAreaId = request.RequestContext.GetChannelConfiguration().InventLocationDataAreaId;

            SqlQuery query = new SqlQuery();

            query.QueryString = "SELECT DGP.TRACKINGDIMENSIONGROUP, DGF.DIMENSIONFIELDID AS [DIMENSIONFIELDID] " +
                "FROM ax.ECORESTRACKINGDIMENSIONGROUPITEM DGP INNER JOIN ax.ECORESTRACKINGDIMENSIONGROUPFLDSETUP DGF " +
                  "ON DGP.TRACKINGDIMENSIONGROUP = DGF.TRACKINGDIMENSIONGROUP AND DGF.ISACTIVE = 1 " +
                "WHERE DGP.ITEMID = @ITEMID AND DGP.ITEMDATAAREAID = @DATAAREAID";

            query.Parameters["@DATAAREAID"] = dataAreaId;
            query.Parameters["@ITEMID"] = request.ItemId;

            int trackingInfo = 0;

            using (DatabaseQuery databaseContext = new DatabaseQuery(request.RequestContext))
            {
                using (IDatabaseResult reader = await databaseContext.Execute(query).ConfigureAwait(false))
                {
                    while (await reader.ReadAsync().ConfigureAwait(false))
                    {
                        int dimensionFieldId = reader.GetValue<int>("DIMENSIONFIELDID");
                        switch (dimensionFieldId)
                        {
                            case 2: //batch
                                trackingInfo = BitEncoder.Set(trackingInfo, 2, true);
                                break;
                            case 5: //serial
                                trackingInfo = BitEncoder.Set(trackingInfo, 1, true);
                                break;
                        }
                    }
                }
            }

            return new SingleEntityDataServiceResponse<int>(trackingInfo);
        }

        private async Task<ProductSearchByTextResponse> ProductSearchByText(ProductSearchByTextRequest request)
        {
            string AvailableQty = "AvailableQty";

            if (!request.QueryResultSettings.Sorting.IsSpecified)
            {
                request.QueryResultSettings.Sorting = new SortingInfo("RANKING", true);
            }
            //long channelId = request.RequestContext.GetPrincipal().ChannelId;
            //ProductSearchCriteria productSearchCriterium = new ProductSearchCriteria(channelId, 0)
            //{
            //    SearchCondition = request.SearchText,
            //    SkipVariantExpansion = true
            //};
            SearchProductsRequest searchProductsRequest = new SearchProductsRequest(request.Criteria, true, request.QueryResultSettings);
            EntityDataServiceResponse<ProductSearchResult> searchResults = await request.RequestContext.ExecuteAsync<EntityDataServiceResponse<ProductSearchResult>>(searchProductsRequest).ConfigureAwait(false);

            foreach (ProductSearchResult psr in searchResults)
            {
                int qty = decimal.ToInt32((await request.RequestContext.ExecuteAsync<SingleEntityDataServiceResponse
                                <decimal>>(new GetItemVariantAvailableQtyDataRequest(psr.ItemId, string.Empty, psr.RecordId)).ConfigureAwait(false)).Entity);
                psr.SetProperty(AvailableQty, qty);
            }

            //return new ProductSearchByTextResponse(searchResults.AsPagedResult());
            return new ProductSearchByTextResponse(null);
        }

        private async Task<ProductSearchByTextResponse> SearchProducts(ProductSearchByTextRequest request)
        {
#if !DEBUG
            try
            {
                await LabelLicense.Check(request.RequestContext).ConfigureAwait(false);
            }
            catch (Exception)
            {
                //throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_RequiredValueNotFound, "MAX Essentials inventory lookup is not licensed.");
                throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_RequiredValueNotFound, null, null, "MAX Essentials inventory lookup is not licensed.")
                {
                    LocalizedMessage = "MAX Essentials inventory lookup is not licensed.",
                    LocalizedMessageParameters = new object[] { }
                };
            }
#endif
            List<ProductLookupClause> list = new List<ProductLookupClause>();
            foreach (ProductLookupClause itemId in request.Criteria.ItemIds)
            {
                if (itemId.ItemId.Length > 20)
                {
                    Microsoft.Dynamics.Retail.Diagnostics.RetailLogger.Log.ProductManagerSearchProductsItemIdNotValid();
                }
                else
                {
                    list.Add(itemId);
                }
            }
            request.Criteria.ItemIds = list;
            ProductSearchRequest productSearchRequest = new ProductSearchRequest()
            {
                QueryCriteria = request.Criteria,
                QueryResultSettings = request.QueryResultSettings
            };
            ProductSearchResultContainer psr = (await request.RequestContext.ExecuteAsync<ProductSearchResponse>(productSearchRequest).ConfigureAwait(false)).QueryResults;
            psr = await CalculateAvailability(request.RequestContext, psr).ConfigureAwait(false);
            
            PagedResult<Product> result = psr.Results.AsPagedResult();
            return new ProductSearchByTextResponse(result);
        }

        private async Task<ProductSearchResultContainer> CalculateAvailability(RequestContext context, ProductSearchResultContainer psr)
        {
            string AvailableQty = "AvailableQty";
            HashSet<string> itemVariantMap = new HashSet<string>();
            Dictionary<string, decimal> itemAvailableMap = new Dictionary<string, decimal>();
            List<ItemVariantInventoryDimension> itemVariantList = new List<ItemVariantInventoryDimension>();
            string itemVariantKey;
            decimal itemVariantQty;

            IEnumerable<long> productIds = null;
            ReadOnlyCollection<Item> items = null;
            if (psr.Results.Count > 0)
            {
                productIds = psr.Results.Select(p => p.RecordId).Distinct();
                items = (await context.ExecuteAsync<GetItemsDataResponse>(new GetItemsDataRequest(productIds)).ConfigureAwait(false)).Items;
            }
            // Prepare
            foreach (Product product in psr.Results)
            {
                itemVariantKey = CreateItemVariantKey(product.ItemId, Convert.ToString(product.RecordId));
                if (!itemVariantMap.Contains(itemVariantKey))
                {
                    itemVariantMap.Add(itemVariantKey);
                    itemVariantList.Add(new ItemVariantInventoryDimension(product.ItemId, Convert.ToString(product.RecordId)));

                    if (product.IsMasterProduct)
                    {
                        foreach (ProductVariant productVariant in product.GetVariants())
                        {
                            itemVariantKey = CreateItemVariantKey(product.ItemId, Convert.ToString(productVariant.DistinctProductVariantId));
                            if (!itemVariantMap.Contains(itemVariantKey))
                            {
                                itemVariantMap.Add(itemVariantKey);
                                itemVariantList.Add(new ItemVariantInventoryDimension(product.ItemId, Convert.ToString(productVariant.DistinctProductVariantId)));
                            }
                        }
                    }
                }
                if (items != null && string.IsNullOrEmpty(product.Name))
                {
                    product.Name = items.Where(itm => itm.Product == product.RecordId).Select(i => { return i.Name ?? i.Description; }).FirstOrDefault();
                }
            }
            foreach (ItemVariantInventoryDimension varItem in itemVariantList)
            {
                decimal qty = (await context.ExecuteAsync<SingleEntityDataServiceResponse
                            <decimal>>(new GetItemVariantAvailableQtyDataRequest(varItem.ItemId, string.Empty, Convert.ToInt64(varItem.VariantInventoryDimensionId))).ConfigureAwait(false)).Entity;

                itemVariantKey = CreateItemVariantKey(varItem.ItemId, varItem.VariantInventoryDimensionId);
                itemAvailableMap[itemVariantKey] = qty;
            }
            // Update
            foreach (Product product in psr.Results)
            {
                itemVariantKey = CreateItemVariantKey(product.ItemId, Convert.ToString(product.RecordId));
                itemAvailableMap.TryGetValue(itemVariantKey, out itemVariantQty);
                product.SetProperty(AvailableQty, itemVariantQty);

                if (product.IsMasterProduct)
                {
                    foreach (ProductVariant productVariant in product.GetVariants())
                    {
                        itemVariantKey = CreateItemVariantKey(product.ItemId, Convert.ToString(productVariant.DistinctProductVariantId));
                        itemAvailableMap.TryGetValue(itemVariantKey, out itemVariantQty);
                        productVariant.SetProperty(AvailableQty, itemVariantQty);
                    }
                }
            }
            return psr;
        }

        private static string CreateItemVariantKey(string itemId, string inventoryDimensionId)
        {
            if (String.IsNullOrEmpty(inventoryDimensionId))
                return itemId;
            else
                return itemId + "-" + inventoryDimensionId;
        }

        // Offline StockTake >>
        private async Task<GetBarcodeForItemResponse> GetBarcodeForItems(GetBarcodeForItemRequest request)
        {
            ThrowIf.Null(request, "request");

            //ProductDataManager productDataManager = ProductDataManager.Instantiate(request.RequestContext);
            ProductDataManager productDataManager = new ProductDataManager(request.RequestContext);
            IList<BarcodeItem> items = new List<BarcodeItem>();
            ItemBarcode IB = null;
            foreach (BarcodeItem item in request.Items)
            {
                IB = new SingleEntityDataServiceResponse<ItemBarcode>(await productDataManager.GetProductBarcode(item.ItemId, item.InventDimId, item.UnitId).ConfigureAwait(false)).Entity;
                if (IB != null)
                    item.Barcode = IB.ItemBarcodeValue;
                items.Add(item);
            }
            return new GetBarcodeForItemResponse(items);
        }
        // Offline StockTake <<
    }
}
