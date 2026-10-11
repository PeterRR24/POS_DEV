using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Dynamics.Commerce.Runtime;
using Microsoft.Dynamics.Commerce.Runtime.Data;
using Microsoft.Dynamics.Commerce.Runtime.Data.Types;
using Microsoft.Dynamics.Commerce.Runtime.DataModel;

namespace RR.Commerce.Runtime.Utilities
{
    public class ItemVariantAvailabilityDataManager //: DatabaseAccessor
    {
        //public ItemVariantAvailabilityDataManager(RequestContext context) : base(DataStoreManager.DataStores[DataStoreType.Database], context)
        //{
        //}
        readonly RequestContext Context;
        public ItemVariantAvailabilityDataManager(RequestContext context) { Context = context; }

        public async Task<PagedResult<ItemAvailableQuantity>> GetItemAvailableQuantities(IEnumerable<ItemVariantInventoryDimension> items, QueryResultSettings settings)
        {
            ThrowIf.Null<IEnumerable<ItemVariantInventoryDimension>>(items, "items");
            SqlPagedQuery query = new SqlPagedQuery(settings)
            {
                DatabaseSchema = "ext",
                From = "GETITEMVARIANTAVAILABILITY(@bi_ChannelId, @tvp_ids)",
                OrderBy = "ITEMID, VARIANTINVENTDIMID"
            };
            using (DataTable table = new DataTable())
            {
                FillDataTable(table, items);
                query.Parameters["@bi_ChannelId"] = Context.GetPrincipal().ChannelId;
                query.Parameters["@tvp_ids"] = table;
                //return this.ExecuteSelect<ItemAvailableQuantity>(query);
                using (DatabaseContext databaseContext = new DatabaseContext(Context))
                {
                    return await databaseContext.ReadEntityAsync<ItemAvailableQuantity>(query).ConfigureAwait(false);
                }
            }
        }

        private static void FillDataTable(DataTable table, IEnumerable<ItemVariantInventoryDimension> items)
        {
            table.TableName = "ext.RRITEMINVENTDIMTABLETYPE";
            table.Columns.Add("ITEMID", typeof(string));
            table.Columns.Add("INVENTDIMID", typeof(string));
            foreach (ItemVariantInventoryDimension inventoryDimension in items)
            {
                DataRow row = table.NewRow();
                row["ITEMID"] = inventoryDimension.ItemId ?? String.Empty;
                row["INVENTDIMID"] = inventoryDimension.VariantInventoryDimensionId ?? String.Empty;
                table.Rows.Add(row);
            }
        }
    }
}
