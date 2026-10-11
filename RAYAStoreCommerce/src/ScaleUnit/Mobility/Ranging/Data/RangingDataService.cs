using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Dynamics.Commerce.Runtime;
using Microsoft.Dynamics.Commerce.Runtime.Messages;
using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
using Microsoft.Dynamics.Commerce.Runtime.Data;
using Microsoft.Dynamics.Commerce.Runtime.DataModel;

namespace RR.Commerce.Runtime.Mobility
{
    public class RangingDataService : IRequestHandlerAsync
    {
        private const string DEVICELOCATIONCLAUSE = " SHELFID = @SHELFID AND SECTIONID = @SECTIONID AND SERIALNUMBER = @SERIALNUMBER ";
        public IEnumerable<Type> SupportedRequestTypes
        {
            get
            {
                return new[]
                {
                    typeof(RangingCreateDataRequest),
                    typeof(RangingSelectDataRequest),
                    typeof(RangingItemGetDataRequest),
                    typeof(RangingItemAddDataRequest),
                    typeof(RangingItemUndoDataRequest),
                    typeof(RangingItemDeleteDataRequest),
                    typeof(RangingStatDataRequest),
                    typeof(RangingResetDataRequest)
                };
            }
        }

        public async Task<Response> Execute(Request request)
        {
            if (request == null)
                throw new ArgumentNullException("request");

            Type requestType = request.GetType();
            Response response;

            if (requestType == typeof(RangingCreateDataRequest))
                response = await this.RangingCreate((RangingCreateDataRequest)request).ConfigureAwait(false);
            else if (requestType == typeof(RangingSelectDataRequest))
                response = await this.RangingSelect((RangingSelectDataRequest)request).ConfigureAwait(false);
            else if (requestType == typeof(RangingItemGetDataRequest))
                response = await this.RangingItemGet((RangingItemGetDataRequest)request).ConfigureAwait(false);
            else if (requestType == typeof(RangingItemAddDataRequest))
                response = await this.RangingItemAdd((RangingItemAddDataRequest)request).ConfigureAwait(false);
            else if (requestType == typeof(RangingItemUndoDataRequest))
                response = await this.RangingItemUndo((RangingItemUndoDataRequest)request).ConfigureAwait(false);
            else if (requestType == typeof(RangingItemDeleteDataRequest))
                response = await this.RangingItemDelete((RangingItemDeleteDataRequest)request).ConfigureAwait(false);
            else if (requestType == typeof(RangingStatDataRequest))
                response = await this.RangingStat((RangingStatDataRequest)request).ConfigureAwait(false);
            else if (requestType == typeof(RangingResetDataRequest))
                response = await this.RangingReset((RangingResetDataRequest)request).ConfigureAwait(false);
            else
                throw new NotSupportedException(String.Format("Request '{0}' is not supported.", request.GetType()));

            return response;
        }

        private async Task<SingleEntityDataServiceResponse<RangingStat>> RangingCreate(RangingCreateDataRequest request)
        {
            ThrowIf.Null(request, "request");
            
            RangingStat stat = await GetStat(request.RequestContext, request.SectionId, request.ShelfId).ConfigureAwait(false);
            return new SingleEntityDataServiceResponse<RangingStat>(stat);
        }

        private async Task<SingleEntityDataServiceResponse<PagedResult<RangingItem>>> RangingSelect(RangingSelectDataRequest request)
        {
            ThrowIf.Null(request, "request");

            PagedResult<RangingItem> list = null;
            SqlPagedQuery selectQuery = new SqlPagedQuery(QueryResultSettings.AllRecords);
            selectQuery.DatabaseSchema = "ext";
            selectQuery.From = "RUMOBILEITEMRANGING";
            selectQuery.Where = DEVICELOCATIONCLAUSE;
            selectQuery.Parameters["@SECTIONID"] = request.SectionId;
            selectQuery.Parameters["@SHELFID"] = request.ShelfId;
            selectQuery.Parameters["@SERIALNUMBER"] = request.RequestContext.GetPrincipal().DeviceNumber;

            using (DatabaseQuery databaseContext = new DatabaseQuery(request.RequestContext))
            {
                list = await databaseContext.ReadEntityAsync<RangingItem>(selectQuery).ConfigureAwait(false);
            }
            return new SingleEntityDataServiceResponse<PagedResult<RangingItem>>(list);
        }

        private async Task<SingleEntityDataServiceResponse<RangingItem>> RangingItemGet(RangingItemGetDataRequest request)
        {
            ThrowIf.Null(request, "request");
            
            RangingItem item = new RangingItem();
            SqlPagedQuery selectQuery = new SqlPagedQuery(QueryResultSettings.SingleRecord);
            selectQuery.DatabaseSchema = "ext";
            selectQuery.From = "RUMOBILEITEMRANGING";
            selectQuery.Where = "ITEMID = @ITEMID AND" + DEVICELOCATIONCLAUSE;
            selectQuery.Parameters["@ITEMID"] = request.ItemId;
            selectQuery.Parameters["@SECTIONID"] = request.SectionId;
            selectQuery.Parameters["@SHELFID"] = request.ShelfId;
            selectQuery.Parameters["@SERIALNUMBER"] = request.RequestContext.GetPrincipal().DeviceNumber;

            using (DatabaseQuery databaseContext = new DatabaseQuery(request.RequestContext))
            {
                item = (await databaseContext.ReadEntityAsync<RangingItem>(selectQuery).ConfigureAwait(false)).FirstOrDefault();
            }
            return new SingleEntityDataServiceResponse<RangingItem>(item);
        }

        private async Task<SingleEntityDataServiceResponse<RangingStat>> RangingItemAdd(RangingItemAddDataRequest request)
        {
            ThrowIf.Null(request, "request");
            ThrowIf.Null(request.Item, "request.Item");
            
            SqlQuery query = new SqlQuery();
            query.QueryString = "DELETE FROM ext.RUMOBILEITEMRANGING WHERE ITEMID = @ITEMID AND" + DEVICELOCATIONCLAUSE + "; " +
                "INSERT INTO ext.RUMOBILEITEMRANGING ( " +
                    "[CHANNELID], [SECTIONID], [SHELFID], [ITEMID], [ITEMNAME], [FACES], [SERIALNUMBER], [ID] " +
                ") VALUES ( " +
                    "@CHANNELID, @SECTIONID, @SHELFID, @ITEMID, @ITEMNAME, @FACES, @SERIALNUMBER, " +
                    "(SELECT ISNULL(MAX(ID),0) + 1 FROM ext.RUMOBILEITEMRANGING WHERE" + DEVICELOCATIONCLAUSE + ") "+
                "); ";

            query.Parameters["@CHANNELID"] = request.RequestContext.GetPrincipal().ChannelId;
            query.Parameters["@SECTIONID"] = request.Item.SectionId ?? String.Empty;
            query.Parameters["@SHELFID"] = request.Item.ShelfId ?? String.Empty;
            query.Parameters["@ITEMID"] = request.Item.ItemId;
            query.Parameters["@ITEMNAME"] = request.Item.ItemName ?? String.Empty;
            query.Parameters["@FACES"] = request.Item.Faces;
            query.Parameters["@SERIALNUMBER"] = request.RequestContext.GetPrincipal().DeviceNumber ?? String.Empty;

            using (DatabaseContext databaseContext = new DatabaseContext(request.RequestContext))
            {
                await databaseContext.ExecuteNonQueryAsync(query).ConfigureAwait(false);
            }
            RangingStat stat = await GetStat(request.RequestContext, request.Item.SectionId, request.Item.ShelfId).ConfigureAwait(false);
            return new SingleEntityDataServiceResponse<RangingStat>(stat);
            
        }

        private async Task<SingleEntityDataServiceResponse<RangingStat>> RangingItemUndo(RangingItemUndoDataRequest request)
        {
            ThrowIf.Null(request, "request");
            
            SqlQuery query = new SqlQuery();
            query.QueryString = "DELETE FROM ext.RUMOBILEITEMRANGING WHERE ID = (SELECT MAX(ID) FROM ext.RUMOBILEITEMRANGING WHERE" + DEVICELOCATIONCLAUSE + ")";

            query.Parameters["@SECTIONID"] = request.SectionId;
            query.Parameters["@SHELFID"] = request.ShelfId;
            query.Parameters["@SERIALNUMBER"] = request.RequestContext.GetPrincipal().DeviceNumber;

            using (DatabaseContext databaseContext = new DatabaseContext(request.RequestContext))
            {
                await databaseContext.ExecuteNonQueryAsync(query).ConfigureAwait(false);
            }
            RangingStat stat = await GetStat(request.RequestContext, request.SectionId, request.ShelfId).ConfigureAwait(false);
            return new SingleEntityDataServiceResponse<RangingStat>(stat);
        }

        private async Task<SingleEntityDataServiceResponse<RangingStat>> RangingItemDelete(RangingItemDeleteDataRequest request)
        {
            ThrowIf.Null(request, "request");
            
            SqlQuery query = new SqlQuery();
            query.QueryString = "DELETE FROM ext.RUMOBILEITEMRANGING WHERE ID = @ID AND" + DEVICELOCATIONCLAUSE;

            query.Parameters["@ID"] = request.Item.Id;
            query.Parameters["@SECTIONID"] = request.Item.SectionId;
            query.Parameters["@SHELFID"] = request.Item.ShelfId;
            query.Parameters["@SERIALNUMBER"] = request.RequestContext.GetPrincipal().DeviceNumber;

            using (DatabaseContext databaseContext = new DatabaseContext(request.RequestContext))
            {
                await databaseContext.ExecuteNonQueryAsync(query).ConfigureAwait(false);
            }
            RangingStat stat = await GetStat(request.RequestContext, request.Item.SectionId, request.Item.ShelfId).ConfigureAwait(false);
            return new SingleEntityDataServiceResponse<RangingStat>(stat);
        }

        private async Task<SingleEntityDataServiceResponse<RangingStat>> RangingStat(RangingStatDataRequest request)
        {
            ThrowIf.Null(request, "request");
            
            RangingStat stat = await GetStat(request.RequestContext, request.SectionId, request.ShelfId).ConfigureAwait(false);
            return new SingleEntityDataServiceResponse<RangingStat>(stat);
        }

        private async Task<SingleEntityDataServiceResponse<RangingStat>> RangingReset(RangingResetDataRequest request)
        {
            ThrowIf.Null(request, "request");
            
            SqlQuery query = new SqlQuery();
            query.QueryString = "DELETE FROM ext.RUMOBILEITEMRANGING WHERE" + DEVICELOCATIONCLAUSE;

            query.Parameters["@SECTIONID"] = request.SectionId;
            query.Parameters["@SHELFID"] = request.ShelfId;
            query.Parameters["@SERIALNUMBER"] = request.RequestContext.GetPrincipal().DeviceNumber;

            using (DatabaseContext databaseContext = new DatabaseContext(request.RequestContext))
            {
                await databaseContext.ExecuteNonQueryAsync(query).ConfigureAwait(false);
            }
            RangingStat stat = await GetStat(request.RequestContext, request.SectionId, request.ShelfId).ConfigureAwait(false);
            return new SingleEntityDataServiceResponse<RangingStat>(stat);
        }

        private async Task<RangingStat> GetStat(RequestContext context, string sectionId, string shelfId)
        {
            RangingStat stat = new RangingStat();
            stat.SectionId = sectionId;
            stat.ShelfId = shelfId;

            SqlQuery selectQuery = new SqlQuery();
            selectQuery.QueryString = "SELECT COUNT(ID) as cnt FROM ext.RUMOBILEITEMRANGING WHERE" + DEVICELOCATIONCLAUSE;
            selectQuery.Parameters["@SECTIONID"] = sectionId;
            selectQuery.Parameters["@SHELFID"] = shelfId;
            selectQuery.Parameters["@SERIALNUMBER"] = context.GetPrincipal().DeviceNumber;

            using (DatabaseQuery databaseContext = new DatabaseQuery(context))
            {
                stat.Count = await databaseContext.ExecuteScalarAsync<int>(selectQuery).ConfigureAwait(false);
            }
            return stat;
        }

    }
}