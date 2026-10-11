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
    public sealed class PlacementDataManager //: DatabaseAccessor
    {
        //public PlacementDataManager(RequestContext context): base(DataStoreManager.DataStores[DataStoreType.Database], context)
        //{
        //}

        //public static PlacementDataManager Instantiate(RequestContext context)
        //{
        //    return new PlacementDataManager(context);
        //}
        readonly RequestContext Context;
        public PlacementDataManager(RequestContext context) { Context = context; }

        /* NU
        public ItemStoreLocation GetItemStoreLocation(string itemId)
        {
            ThrowIf.NullOrWhiteSpace(itemId, "itemId");

            SqlPagedQuery query = new SqlPagedQuery(QueryResultSettings.SingleRecord)
            {
                DatabaseSchema = "ext",
                From = "RETAILINVENTITEMSECTIONLOCATIONVIEW",
                Where = "ITEMID = @ITEMID AND CHANNELID = @CHANNELID AND DATAAREAID = @DATAAREAID"
            };

            query.Parameters["@DATAAREAID"] = this.Context.GetChannelConfiguration().InventLocationDataAreaId;
            query.Parameters["@CHANNELID"] = this.ChannelId;
            query.Parameters["@ITEMID"] = itemId;

            return this.ExecuteSelect<ItemStoreLocation>(query).FirstOrDefault();
        }
        */

        public async Task<PagedResult<StoreLocationItem>> GetStoreLocationItems(string sectionId, string shelfId, QueryResultSettings settings)
        {
            SqlPagedQuery query = new SqlPagedQuery(settings)
            {
                DatabaseSchema = "ext",
                Select = new ColumnSet("LOCATION", "ITEMID", "INVENTDIMID", "FACINGCOUNT", "SECTIONID", "SHELFID"),
                From = "RUITEMSTORELOCATION",
                Where = "CHANNELID = @CHANNELID AND SECTIONID = @SECTIONID AND SHELFID = @SHELFID",
                OrderBy = "LOCATION"
            };

            query.Parameters["@CHANNELID"] = Context.GetPrincipal().ChannelId;
            query.Parameters["@SECTIONID"] = sectionId ?? String.Empty;
            query.Parameters["@SHELFID"] = shelfId ?? String.Empty;

            //return this.ExecuteSelect<StoreLocationItem>(query);
            using (DatabaseContext databaseContext = new DatabaseContext(Context))
            {
                return await databaseContext.ReadEntityAsync<StoreLocationItem>(query).ConfigureAwait(false);
            }
        }

        public async Task<StoreLocationItem> GetStoreLocationItem(string sectionId, string shelfId, int location)
        {
            SqlPagedQuery query = new SqlPagedQuery(QueryResultSettings.SingleRecord)
            {
                DatabaseSchema = "ext",
                Select = new ColumnSet("LOCATION", "ITEMID", "INVENTDIMID", "FACINGCOUNT"),
                From = "RUITEMSTORELOCATION",
                Where = "CHANNELID = @CHANNELID AND SECTIONID = @SECTIONID AND SHELFID = @SHELFID AND LOCATION = @LOCATION"
            };

            query.Parameters["@CHANNELID"] = Context.GetPrincipal().ChannelId;
            query.Parameters["@SECTIONID"] = sectionId ?? String.Empty;
            query.Parameters["@SHELFID"] = shelfId ?? String.Empty;
            query.Parameters["@LOCATION"] = location;

            //return this.ExecuteSelect<StoreLocationItem>(query).FirstOrDefault();
            using (DatabaseContext databaseContext = new DatabaseContext(Context))
            {
                return (await databaseContext.ReadEntityAsync<StoreLocationItem>(query).ConfigureAwait(false)).FirstOrDefault();
            }
        }

    }
}
