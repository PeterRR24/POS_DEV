using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Dynamics.Commerce.Runtime;
using Microsoft.Dynamics.Commerce.Runtime.DataModel;
using Microsoft.Dynamics.Commerce.Runtime.Data;
using Microsoft.Dynamics.Commerce.Runtime.Data.Types;
using System.Threading.Tasks;

namespace RR.Commerce.Runtime.Utilities
{
    public sealed class ProductDataManager //: DatabaseAccessor
    {
        //public ProductDataManager(RequestContext context): base(DataStoreManager.DataStores[DataStoreType.Database], context)
        //{
        //}

        //public static ProductDataManager Instantiate(RequestContext context)
        //{
        //    return new ProductDataManager(context);
        //}
        readonly RequestContext Context;
        public ProductDataManager(RequestContext context) { Context = context; }

        /* Superseded
        public PagedResult<ProductVariant> GetProductVariants(IEnumerable<ProductLookupClause> itemIds)
        {
            ThrowIf.Null(itemIds, "itemIds");

            using (var itemIdTable = new ItemIdSearchTableType(itemIds))
            {
                ParameterSet parameters = new ParameterSet();
                parameters["@bi_ChannelId"] = this.ChannelId;
                parameters["@dt_ChannelDate"] = this.Context.GetNowInChannelTimeZone().DateTime;
                parameters["@nvc_DataAreaId"] = this.Context.GetChannelConfiguration().InventLocationDataAreaId;
                parameters["@tvp_ItemIds"] = itemIdTable.DataTable;

                return this.ExecuteStoredProcedure<ProductVariant>("GETPRODUCTVARIANTSBYITEMID", parameters, QueryResultSettings.AllRecords);
            }
        }
        */

        public async Task<ItemBarcode> GetProductBarcode(string itemId, string inventDimId, string unitId)
        {
            ThrowIf.NullOrWhiteSpace(itemId, "itemId");

            SqlPagedQuery query = new SqlPagedQuery(QueryResultSettings.AllRecords)
            {
                From = "INVENTITEMBARCODESVIEW",
                Where = "ITEMID = @ITEMID AND (INVENTDIMID = @INVENTDIMID OR INVENTDIMID IS NULL) AND DATAAREAID = @DATAAREAID"
            };

            query.Parameters["@DATAAREAID"] = this.Context.GetChannelConfiguration().InventLocationDataAreaId;
            query.Parameters["ITEMID"] = itemId;
            query.Parameters["@INVENTDIMID"] = inventDimId ?? String.Empty;

            IEnumerable<ItemBarcode> itemBarcodes;  // = this.ExecuteSelect<ItemBarcode>(query).Results;
            using (DatabaseContext databaseContext = new DatabaseContext(Context))
            {
                itemBarcodes= await databaseContext.ReadEntityAsync<ItemBarcode>(query).ConfigureAwait(false);
            }
            ItemBarcode itemBarcode = null;

            foreach (ItemBarcode ib in itemBarcodes)
            {
                if (itemBarcode != null)
                {
                    if (String.Compare(itemBarcode.InventoryDimensionId, inventDimId, StringComparison.OrdinalIgnoreCase) != 0)
                    {
                        if (String.Compare(ib.InventoryDimensionId, inventDimId, StringComparison.OrdinalIgnoreCase) == 0)
                            itemBarcode = ib;
                    }
                    else
                    {
                        if (String.Compare(itemBarcode.UnitId, unitId, StringComparison.OrdinalIgnoreCase) != 0 && String.Compare(ib.UnitId, unitId, StringComparison.OrdinalIgnoreCase) == 0)
                            itemBarcode = ib;
                    }
                }
                else
                    itemBarcode = ib;
            }

            return itemBarcode;
        }

        public async Task<PagedResult<ProductCategoryHierarchy>> GetProductCategoryHierarchy(long productId, long hierarchyId)
        {
            SqlPagedQuery query = new SqlPagedQuery(QueryResultSettings.AllRecords)
            {
                DatabaseSchema = "ext",
                From = "GETPRODUCTCATEGORYHIERARCHY(@ProductId, @HierarchyId)",
                OrderBy = "LEVEL"
            };

            query.Parameters["@ProductId"] = productId;
            query.Parameters["@HierarchyId"] = hierarchyId;

            //return this.ExecuteSelect<ProductCategoryHierarchy>(query);
            using(DatabaseContext databaseContext = new DatabaseContext(Context))
            {
                return await databaseContext.ReadEntityAsync<ProductCategoryHierarchy>(query).ConfigureAwait(false);
            }
        }
    }
}
