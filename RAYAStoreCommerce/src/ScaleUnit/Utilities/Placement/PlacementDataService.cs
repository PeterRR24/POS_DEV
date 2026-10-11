using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Threading.Tasks;

using Microsoft.Dynamics.Commerce.Runtime;
using Microsoft.Dynamics.Commerce.Runtime.Messages;
using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
using Microsoft.Dynamics.Commerce.Runtime.DataModel;
using Microsoft.Dynamics.Commerce.Runtime.Data;

namespace RR.Commerce.Runtime.Utilities
{
    public class PlacementDataService : IRequestHandlerAsync
    {
        public IEnumerable<Type> SupportedRequestTypes
        {
            get
            {
                return new[]
                {
                    typeof(GetStoreLocationTypeDataRequest),
                    // typeof(GetItemStoreLocationDataRequest),
                    typeof(GetItemStoreLocationsDataRequest),
                    typeof(GetStoreLocationItemIdsDataRequest),
                    typeof(GetStoreLocationItemsDataRequest),
                    typeof(GetStoreLocationItemDataRequest),
                    typeof(ClearStoreLocationItemsDataRequest),
                    typeof(PlaceStoreLocationItemDataRequest),
                    typeof(PlaceStoreLocationItemsDataRequest),
                    typeof(RemoveStoreLocationItemDataRequest),
                    typeof(GetItemStoreLocationFacesDataRequest)
                };
            }
        }

        public async Task<Response> Execute(Request request)
        {
            if (request == null)
                throw new ArgumentNullException("request");

            Type requestType = request.GetType();
            Response response;

            if (requestType == typeof(GetStoreLocationTypeDataRequest))
                response = await this.GetStoreLocationType((GetStoreLocationTypeDataRequest)request).ConfigureAwait(false);
            // else if (requestType == typeof(GetItemStoreLocationDataRequest))
            //     response = this.GetItemStoreLocation((GetItemStoreLocationDataRequest)request);
            else if (requestType == typeof(GetItemStoreLocationsDataRequest))
                response = await this.GetItemStoreLocations((GetItemStoreLocationsDataRequest)request).ConfigureAwait(false);
            else if (requestType == typeof(GetStoreLocationItemIdsDataRequest))
                response = await this.GetStoreLocationItemIds((GetStoreLocationItemIdsDataRequest)request).ConfigureAwait(false);
            else if (requestType == typeof(GetStoreLocationItemsDataRequest))
                response = await this.GetStoreLocationItems((GetStoreLocationItemsDataRequest)request).ConfigureAwait(false);
            else if (requestType == typeof(GetStoreLocationItemDataRequest))
                response = await this.GetStoreLocationItem((GetStoreLocationItemDataRequest)request).ConfigureAwait(false);
            else if (requestType == typeof(ClearStoreLocationItemsDataRequest))
                response = await this.ClearStoreLocationItems((ClearStoreLocationItemsDataRequest)request).ConfigureAwait(false);
            else if (requestType == typeof(PlaceStoreLocationItemsDataRequest))
                response = await this.PlaceStoreLocationItems((PlaceStoreLocationItemsDataRequest)request).ConfigureAwait(false);
            else if (requestType == typeof(PlaceStoreLocationItemDataRequest))
                response = await this.PlaceStoreLocationItem((PlaceStoreLocationItemDataRequest)request).ConfigureAwait(false);
            else if (requestType == typeof(RemoveStoreLocationItemDataRequest))
                response = await this.RemoveStoreLocationItem((RemoveStoreLocationItemDataRequest)request).ConfigureAwait(false);
            else if (requestType == typeof(GetItemStoreLocationFacesDataRequest))
                response = await this.GetItemStoreLocationFaces((GetItemStoreLocationFacesDataRequest)request).ConfigureAwait(false);
            else
                throw new NotSupportedException(String.Format("Request '{0}' is not supported.", request.GetType()));

            return response;
        }

        private async Task<SingleEntityDataServiceResponse<bool>> GetStoreLocationType(GetStoreLocationTypeDataRequest request)
        {
            ThrowIf.Null(request, "request");

            //UtilitiesDataManager manager = UtilitiesDataManager.Instantiate(request.RequestContext);
            //bool result = manager.GetStoreLocationType(request.SectionId, request.ShelfId);
            UtilitiesDatabaseAccessor databaseAccessor = new UtilitiesDatabaseAccessor(request.RequestContext);
            bool result = await databaseAccessor.GetStoreLocationType(request.SectionId, request.ShelfId).ConfigureAwait(false);
            return new SingleEntityDataServiceResponse<bool>(result);
        }

        //NU
        /*
        private SingleEntityDataServiceResponse<ItemStoreLocation> GetItemStoreLocation(GetItemStoreLocationDataRequest request)
        {
            ThrowIf.Null(request, "request");

            PlacementDataManager placementDataManager = PlacementDataManager.Instantiate(request.RequestContext);

            return new SingleEntityDataServiceResponse<ItemStoreLocation>(placementDataManager.GetItemStoreLocation(request.ItemId));
        }
        */

        private async Task<GetItemStoreLocationsResponse> GetItemStoreLocations(GetItemStoreLocationsDataRequest request)
        {
            ThrowIf.Null(request, "request");
            ThrowIf.NullOrWhiteSpace(request.ItemId, "request.ItemId");

            List<ItemStoreLocation> list = new List<ItemStoreLocation>();

            SqlQuery query = new SqlQuery();

            if (request.Local)
            {
                query.QueryString = "SELECT DISTINCT SECTIONID, SHELFID, LOCATION FROM ext.RUITEMSTORELOCATION WHERE (CHANNELID = @ChannelId) AND (ITEMID = @ItemId)";
                query.Parameters["@ChannelId"] = request.RequestContext.GetPrincipal().ChannelId;
            }
            else
            {
                query.QueryString = "SELECT DISTINCT SECTIONID, SHELFID FROM ext.RRRETAILINVENTITEMSECTIONLOCATION WHERE (DATAAREAID = @DataAreaId) AND (STOREID = @StoreId) AND (ITEMID = @ItemId)";
                query.Parameters["@DataAreaId"] = request.RequestContext.GetChannelConfiguration().InventLocationDataAreaId;
                query.Parameters["@StoreId"] = request.RequestContext.GetOrgUnit().OrgUnitNumber;
            }

            query.Parameters["@ItemId"] = request.ItemId;

            if (!String.IsNullOrEmpty(request.SectionId))
            {
                query.QueryString += " AND (SECTIONID = @SectionId)";
                query.Parameters["@SectionId"] = request.SectionId;

                if (!String.IsNullOrEmpty(request.ShelfId))
                {
                    query.QueryString += " AND (SHELFID = @ShelfId)";
                    query.Parameters["@ShelfId"] = request.ShelfId;
                }
            }

            using (DatabaseQuery databaseContext = new DatabaseQuery(request.RequestContext))
            {
                using (IDatabaseResult reader = await databaseContext.Execute(query).ConfigureAwait(false))
                {
                    while (await reader.ReadAsync().ConfigureAwait(false))
                    {
                        ItemStoreLocation item = new ItemStoreLocation();
                        item.ItemId = request.ItemId;
                        item.SectionId = reader.GetValue<string>(0);
                        item.ShelfId = reader.GetValue<string>(1);
                        if (request.Local)
                            item.Location = reader.GetValue<int>(2);
                        list.Add(item);
                    }
                }
            }

            return new GetItemStoreLocationsResponse(list);
        }

        private async Task<GetItemIdsDataResponse> GetStoreLocationItemIds(GetStoreLocationItemIdsDataRequest request)
        {
            ThrowIf.Null(request, "request");
            ThrowIf.NullOrWhiteSpace(request.SectionId, "request.SectionId");

            SqlPagedQuery query = new SqlPagedQuery(request.QueryResultSettings);
            query.DatabaseSchema = "ext";
            query.Select = new ColumnSet("ITEMID");
            query.OrderBy = "ITEMID";
            query.Distinct = true;
            if (request.Local)
            {
                query.From = "RUITEMSTORELOCATION";
                query.Where = "(CHANNELID = @ChannelId) AND (SECTIONID = @SectionId)";

                query.Parameters["@ChannelId"] = request.RequestContext.GetPrincipal().ChannelId;
            }
            else
            {
                query.From = "RRRETAILINVENTITEMSECTIONLOCATION";
                query.Where = "(DATAAREAID = @DataAreaId) AND (STOREID = @StoreId) AND (SECTIONID = @SectionId)";

                query.Parameters["@DataAreaId"] = request.RequestContext.GetChannelConfiguration().InventLocationDataAreaId;
                query.Parameters["@StoreId"] = request.RequestContext.GetOrgUnit().OrgUnitNumber;
            }

            query.Parameters["@SectionId"] = request.SectionId;

            if (!String.IsNullOrEmpty(request.ShelfId))
            {
                query.Where += " AND (SHELFID = @ShelfId)";
                query.Parameters["@ShelfId"] = request.ShelfId;
            }

            using (DatabaseQuery databaseContext = new DatabaseQuery(request.RequestContext))
            {
                return new GetItemIdsDataResponse(await databaseContext.ExecuteScalarCollectionAsync<string>(query).ConfigureAwait(false));
            }
        }
        /*
        private GetItemIdsDataResponse GetStoreLocationItemIds(GetStoreLocationItemIdsDataRequest request)
        {
            ThrowIf.Null(request, "request");
            ThrowIf.NullOrWhiteSpace(request.SectionId, "request.SectionId");

            SqlQuery query = new SqlQuery();

            if (request.Local)
            {
                query.QueryString = "SELECT DISTINCT ITEMID FROM ext.RUITEMSTORELOCATION WHERE (CHANNELID = @ChannelId) AND (SECTIONID = @SectionId)";
                query.Parameters["@ChannelId"] = request.RequestContext.GetPrincipal().ChannelId;
            }
            else
            {
                query.QueryString = "SELECT DISTINCT ITEMID FROM ext.RRRETAILINVENTITEMSECTIONLOCATION WHERE (DATAAREAID = @DataAreaId) AND (STOREID = @StoreId) AND (SECTIONID = @SectionId)";
                query.Parameters["@DataAreaId"] = request.RequestContext.GetChannelConfiguration().InventLocationDataAreaId;
                query.Parameters["@StoreId"] = request.RequestContext.GetOrgUnit().OrgUnitNumber;
            }
            query.Parameters["@SectionId"] = request.SectionId;

            if (!String.IsNullOrEmpty(request.ShelfId))
            {
                query.QueryString += " AND (SHELFID = @ShelfId)";
                query.Parameters["@ShelfId"] = request.ShelfId;
            }

            using (DatabaseQuery databaseContext = new DatabaseQuery(request.RequestContext))
            {
                return new GetItemIdsDataResponse(databaseContext.ExecuteScalarCollection<string>(query));
            }
        }
        */

        private async Task<EntityDataServiceResponse<StoreLocationItem>> GetStoreLocationItems(GetStoreLocationItemsDataRequest request)
        {
            ThrowIf.Null(request, "request");

            //PlacementDataManager placementDataManager = PlacementDataManager.Instantiate(request.RequestContext);
            PlacementDataManager placementDataManager = new PlacementDataManager(request.RequestContext);
            return new EntityDataServiceResponse<StoreLocationItem>(await placementDataManager.GetStoreLocationItems(request.SectionId, request.ShelfId, request.QueryResultSettings).ConfigureAwait(false));
        }

        private async Task<SingleEntityDataServiceResponse<StoreLocationItem>> GetStoreLocationItem(GetStoreLocationItemDataRequest request)
        {
            ThrowIf.Null(request, "request");

            //PlacementDataManager placementDataManager = PlacementDataManager.Instantiate(request.RequestContext);
            PlacementDataManager placementDataManager = new PlacementDataManager(request.RequestContext);
            return new SingleEntityDataServiceResponse<StoreLocationItem>(await placementDataManager.GetStoreLocationItem(request.SectionId, request.ShelfId, request.Location).ConfigureAwait(false));
        }

        private async Task<NullResponse> ClearStoreLocationItems(ClearStoreLocationItemsDataRequest request)
        {
            ThrowIf.Null(request, "request");

            SqlQuery query = new SqlQuery();

            query.QueryString = "DELETE FROM ext.RUITEMSTORELOCATION WHERE (CHANNELID = @ChannelId)";

            query.Parameters["@ChannelId"] = request.RequestContext.GetPrincipal().ChannelId;

            if (!String.IsNullOrEmpty(request.SectionId))
            {
                query.QueryString += " AND (SECTIONID = @SectionId)";
                query.Parameters["@SectionId"] = request.SectionId;

                if (!String.IsNullOrEmpty(request.ShelfId))
                {
                    query.QueryString += " AND (SHELFID = @ShelfId)";
                    query.Parameters["@ShelfId"] = request.ShelfId;
                }
            }

            using (DatabaseContext databaseContext = new DatabaseContext(request.RequestContext))
            {
                await databaseContext.ExecuteNonQueryAsync(query).ConfigureAwait(false);
            }

            //return new NullResponse();
            return NullResponse.Instance;
        }

        private async Task<NullResponse> PlaceStoreLocationItems(PlaceStoreLocationItemsDataRequest request)
        {
            ThrowIf.Null(request, "request");

            SqlQuery queryClear = new SqlQuery();
            queryClear.QueryString = "DELETE FROM ext.RUITEMSTORELOCATION WHERE (CHANNELID = @ChannelId) AND (SECTIONID = @SectionId) AND (SHELFID = @ShelfId)";
            queryClear.Parameters["@ChannelId"] = request.RequestContext.GetPrincipal().ChannelId;
            queryClear.Parameters["@SectionId"] = request.SectionId ?? String.Empty;
            queryClear.Parameters["@ShelfId"] = request.ShelfId ?? String.Empty;

            SqlQuery queryPlace = CreatePlaceQuery(request.RequestContext.GetPrincipal().ChannelId, request.SectionId, request.ShelfId);

            using (DatabaseContext databaseContext = new DatabaseContext(request.RequestContext))
            {
                await databaseContext.ExecuteNonQueryAsync(queryClear).ConfigureAwait(false);

                foreach (StoreLocationItem item in request.Items)
                {
                    queryPlace.Parameters["@Location"] = item.Location;
                    queryPlace.Parameters["@ItemId"] = item.ItemId ?? String.Empty;
                    queryPlace.Parameters["@InventDimId"] = item.InventDimId ?? String.Empty;
                    queryPlace.Parameters["@FacingCount"] = item.FacingCount;

                    await databaseContext.ExecuteNonQueryAsync(queryPlace).ConfigureAwait(false);
                }
            }

            //return new NullResponse();
            return NullResponse.Instance;
        }

        private async Task<NullResponse> PlaceStoreLocationItem(PlaceStoreLocationItemDataRequest request)
        {
            ThrowIf.Null(request, "request");

            SqlQuery query = CreatePlaceQuery(request.RequestContext.GetPrincipal().ChannelId, request.SectionId, request.ShelfId);

            query.Parameters["@Location"] = request.Location;
            query.Parameters["@ItemId"] = request.ItemId ?? String.Empty;
            query.Parameters["@InventDimId"] = request.InventDimId ?? String.Empty;
            query.Parameters["@FacingCount"] = request.FacingCount;

            using (DatabaseContext databaseContext = new DatabaseContext(request.RequestContext))
            {
                await databaseContext.ExecuteNonQueryAsync(query).ConfigureAwait(false);
            }

            //return new NullResponse();
            return NullResponse.Instance;
        }

        private SqlQuery CreatePlaceQuery(long channelId, string sectionId, string shelfId)
        {
            SqlQuery query = new SqlQuery();

            query.QueryString =
            "MERGE INTO ext.RUITEMSTORELOCATION T " +
            "USING ( " +
                "SELECT @ChannelId, @SectionId, @ShelfId, @Location, @ItemId, @InventDimId, @FacingCount " +
            ") AS S ( " +
                "[CHANNELID], [SECTIONID], [SHELFID], [LOCATION], [ITEMID], [INVENTDIMID], [FACINGCOUNT] " +
            ") " +
            "ON T.[CHANNELID] = S.[CHANNELID] AND T.[SECTIONID] = S.[SECTIONID] AND T.[SHELFID] = S.[SHELFID] AND T.[LOCATION] = S.[LOCATION] " +
            "WHEN MATCHED THEN " +
                "UPDATE SET " +
                    "[ITEMID] = S.[ITEMID], [INVENTDIMID] = S.[INVENTDIMID], [FACINGCOUNT] = S.[FACINGCOUNT] " +
            "WHEN NOT MATCHED THEN " +
                "INSERT " +
                "( " +
                    "[CHANNELID], [SECTIONID], [SHELFID], [LOCATION], [ITEMID], [INVENTDIMID], [FACINGCOUNT] " +
                ") " +
                "VALUES " +
                "( " +
                    "S.[CHANNELID], S.[SECTIONID], S.[SHELFID], S.[LOCATION], S.[ITEMID], S.[INVENTDIMID], S.[FACINGCOUNT] " +
                "); ";

            query.Parameters["@ChannelId"] = channelId;
            query.Parameters["@SectionId"] = sectionId ?? String.Empty;
            query.Parameters["@ShelfId"] = shelfId ?? String.Empty;

            return query;
        }

        private async Task<NullResponse> RemoveStoreLocationItem(RemoveStoreLocationItemDataRequest request)
        {
            ThrowIf.Null(request, "request");

            SqlQuery query = new SqlQuery();

            query.QueryString = "DELETE FROM ext.RUITEMSTORELOCATION WHERE (CHANNELID = @ChannelId) AND (SECTIONID = @SectionId) AND (SHELFID = @ShelfId) AND (LOCATION = @Location)";
            query.Parameters["@ChannelId"] = request.RequestContext.GetPrincipal().ChannelId;
            query.Parameters["@SectionId"] = request.SectionId ?? String.Empty;
            query.Parameters["@ShelfId"] = request.ShelfId ?? String.Empty;
            query.Parameters["@Location"] = request.Location;

            using (DatabaseContext databaseContext = new DatabaseContext(request.RequestContext))
            {
                await databaseContext.ExecuteNonQueryAsync(query).ConfigureAwait(false);
            }

            //return new NullResponse();
            return NullResponse.Instance;
        }

        private async Task<GetItemStoreLocationFacesResponse> GetItemStoreLocationFaces(GetItemStoreLocationFacesDataRequest request)
        {
            ThrowIf.Null(request, "request");

            SqlQuery query = new SqlQuery();

            string strQuery = "SELECT ISNULL(SUM(FACINGCOUNT), 0)AS FACES FROM EXT.RUITEMSTORELOCATION "
                + "WHERE (CHANNELID = @ChannelId) AND ITEMID = @ItemId ";
                
            if(!string.IsNullOrEmpty(request.InventDimId))
            {
                strQuery = strQuery + "AND (INVENTDIMID = @InventDimId) ";
                query.Parameters["@InventDimId"] = request.InventDimId;
            }
            if (!string.IsNullOrEmpty(request.SectionId))
            {
                strQuery = strQuery + "AND (SECTIONID = @SectionId) ";
                query.Parameters["@SectionId"] = request.SectionId;
            }
            if (!string.IsNullOrEmpty(request.ShelfId))
            {
                strQuery = strQuery + "AND (SHELFID = @ShelfId) ";
                query.Parameters["@ShelfId"] = request.ShelfId;
            }
            query.QueryString = strQuery;
            query.Parameters["@ChannelId"] = request.RequestContext.GetPrincipal().ChannelId;
            query.Parameters["@ItemId"] = request.ItemId;

            int result = 0;
            using (DatabaseContext databaseContext = new DatabaseContext(request.RequestContext))
            {
                result = await databaseContext.ExecuteScalarAsync<int>(query).ConfigureAwait(false);
            }

            return new GetItemStoreLocationFacesResponse(result);
        }

    }
}
