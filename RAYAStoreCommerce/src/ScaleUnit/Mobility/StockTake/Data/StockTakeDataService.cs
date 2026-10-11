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
    public class StockTakeDataService : IRequestHandlerAsync
    {
        public IEnumerable<Type> SupportedRequestTypes
        {
            get
            {
                return new[]
                {
                    typeof(CalculateStockTakeItemDataRequest),
                    typeof(CheckStockTakeItemDataRequest),
                    typeof(CalculateStockTakeTrackingDataRequest),
                    typeof(CalculateStockTakeDeviceDataRequest),
                    typeof(UpdateStockTakeItemDataRequest),
                    typeof(CreateStockTakeItemDataRequest)
                };
            }
        }

        public async Task<Response> Execute(Request request)
        {
            if (request == null)
                throw new ArgumentNullException("request");

            Type requestType = request.GetType();
            Response response;

            if (requestType == typeof(CalculateStockTakeItemDataRequest))
                response = await this.CalculateStockTakeItem((CalculateStockTakeItemDataRequest)request).ConfigureAwait(false);
            else if (requestType == typeof(CheckStockTakeItemDataRequest))
                response = await this.CheckStockTakeItem((CheckStockTakeItemDataRequest)request).ConfigureAwait(false);
            else if (requestType == typeof(CalculateStockTakeTrackingDataRequest))
                response = await this.CalculateStockTakeTracking((CalculateStockTakeTrackingDataRequest)request).ConfigureAwait(false);
            else if (requestType == typeof(CalculateStockTakeDeviceDataRequest))
                response = await this.CalculateStockTakeDevice((CalculateStockTakeDeviceDataRequest)request).ConfigureAwait(false);
            else if (requestType == typeof(UpdateStockTakeItemDataRequest))
                response = await this.UpdateStockTakeItem((UpdateStockTakeItemDataRequest)request).ConfigureAwait(false);
            else if (requestType == typeof(CreateStockTakeItemDataRequest))
                response = await this.CreateStockTakeItem((CreateStockTakeItemDataRequest)request).ConfigureAwait(false);
            else
                throw new NotSupportedException(String.Format("Request '{0}' is not supported.", request.GetType()));

            return response;
        }

        private async Task<SingleEntityDataServiceResponse<decimal>> CalculateStockTakeItem(CalculateStockTakeItemDataRequest request)
        {
            ThrowIf.Null(request, "request");
            ThrowIf.Null(request.Item, "request.Item");

            StockTakeItem item = request.Item;

            SqlQuery query = new SqlQuery();

            query.QueryString = "SELECT ISNULL(SUM(QTYCOUNT),0) FROM ext.SCJOURNALENTRY " +
                "WHERE JOURNALID = @JOURNALID AND ITEMID = @ITEMID AND SECTIONID = ISNULL(@SECTIONID, SECTIONID) AND SHELFID = ISNULL(@SHELFID, SHELFID) " +
                "AND CONFIGID = @CONFIGID AND INVENTSIZEID = @INVENTSIZEID AND INVENTCOLORID = @INVENTCOLORID AND INVENTSTYLEID = @INVENTSTYLEID";

            query.Parameters["@JOURNALID"] = item.JournalId;
            query.Parameters["@ITEMID"] = item.ItemId ?? String.Empty;
            query.Parameters["@CONFIGID"] = item.ConfigId ?? String.Empty;
            query.Parameters["@INVENTSIZEID"] = item.InventSizeId ?? String.Empty;
            query.Parameters["@INVENTCOLORID"] = item.InventColorId ?? String.Empty;
            query.Parameters["@INVENTSTYLEID"] = item.InventStyleId ?? String.Empty;
            query.Parameters["@SECTIONID"] = String.IsNullOrEmpty(item.SectionId) ? null : item.SectionId;
            query.Parameters["@SHELFID"] = String.IsNullOrEmpty(item.ShelfId) ? null : item.ShelfId;

            decimal qtyCount = 0;

            using (DatabaseContext databaseContext = new DatabaseContext(request.RequestContext))
            {
                qtyCount = await databaseContext.ExecuteScalarAsync<decimal>(query).ConfigureAwait(false);
            }

            return new SingleEntityDataServiceResponse<decimal>(qtyCount);
        }

        private async Task<SingleEntityDataServiceResponse<bool>> CheckStockTakeItem(CheckStockTakeItemDataRequest request)
        {
            ThrowIf.Null(request, "request");
            ThrowIf.Null(request.Item, "request.Item");

            StockTakeItem item = request.Item;

            SqlQuery query = new SqlQuery();

            query.QueryString = "SELECT ID FROM ext.SCJOURNALTRANS " +
                "WHERE JOURNALID = @JOURNALID AND ITEMID = @ITEMID " +
                "AND CONFIGID = @CONFIGID AND INVENTSIZEID = @INVENTSIZEID AND INVENTCOLORID = @INVENTCOLORID AND INVENTSTYLEID = @INVENTSTYLEID";

            query.Parameters["@JOURNALID"] = item.JournalId;
            query.Parameters["@ITEMID"] = item.ItemId ?? String.Empty;
            query.Parameters["@CONFIGID"] = item.ConfigId ?? String.Empty;
            query.Parameters["@INVENTSIZEID"] = item.InventSizeId ?? String.Empty;
            query.Parameters["@INVENTCOLORID"] = item.InventColorId ?? String.Empty;
            query.Parameters["@INVENTSTYLEID"] = item.InventStyleId ?? String.Empty;

            bool itemFound = false;

            using (DatabaseQuery databaseContext = new DatabaseQuery(request.RequestContext))
            {
                using (IDatabaseResult reader = await databaseContext.Execute(query).ConfigureAwait(false))
                {
                    if (await reader.ReadAsync().ConfigureAwait(false))
                        itemFound = true;
                }
            }

            return new SingleEntityDataServiceResponse<bool>(itemFound);
        }

        private async Task<SingleEntityDataServiceResponse<decimal>> CalculateStockTakeTracking(CalculateStockTakeTrackingDataRequest request)
        {
            ThrowIf.Null(request, "request");
            ThrowIf.Null(request.Item, "request.Item");

            StockTakeItem item = request.Item;

            SqlQuery query = new SqlQuery();

            query.QueryString = "SELECT ISNULL(SUM(QTYCOUNT),0) FROM ext.SCJOURNALENTRY " +
                "WHERE JOURNALID = @JOURNALID AND ITEMID = @ITEMID AND SECTIONID = ISNULL(@SECTIONID, SECTIONID) AND SHELFID = ISNULL(@SHELFID, SHELFID) " +
                "AND CONFIGID = @CONFIGID AND INVENTSIZEID = @INVENTSIZEID AND INVENTCOLORID = @INVENTCOLORID AND INVENTSTYLEID = @INVENTSTYLEID " +
                "AND INVENTBATCHID = @INVENTBATCHID AND INVENTSERIALID = @INVENTSERIALID";

            query.Parameters["@JOURNALID"] = item.JournalId;
            query.Parameters["@ITEMID"] = item.ItemId ?? String.Empty;
            query.Parameters["@CONFIGID"] = item.ConfigId ?? String.Empty;
            query.Parameters["@INVENTSIZEID"] = item.InventSizeId ?? String.Empty;
            query.Parameters["@INVENTCOLORID"] = item.InventColorId ?? String.Empty;
            query.Parameters["@INVENTSTYLEID"] = item.InventStyleId ?? String.Empty;
            query.Parameters["@INVENTBATCHID"] = item.InventBatchId ?? String.Empty;
            query.Parameters["@INVENTSERIALID"] = item.InventSerialId ?? String.Empty;
            query.Parameters["@SECTIONID"] = String.IsNullOrEmpty(item.SectionId) ? null : item.SectionId;
            query.Parameters["@SHELFID"] = String.IsNullOrEmpty(item.ShelfId) ? null : item.ShelfId;

            decimal qtyCount = 0;

            using (DatabaseContext databaseContext = new DatabaseContext(request.RequestContext))
            {
                qtyCount = await databaseContext.ExecuteScalarAsync<decimal>(query).ConfigureAwait(false);
            }

            return new SingleEntityDataServiceResponse<decimal>(qtyCount);
        }

        private async Task<SingleEntityDataServiceResponse<Tuple<int, decimal>>> CalculateStockTakeDevice(CalculateStockTakeDeviceDataRequest request)
        {
            ThrowIf.Null(request, "request");
            ThrowIf.Null(request.Item, "request.Item");
            string deviceNumber = request.RequestContext.GetPrincipal().DeviceNumber;
            if (string.IsNullOrEmpty(deviceNumber))
            {
                throw new NotFoundException("Please try again.");
            }
            StockTakeItem item = request.Item;

            SqlQuery query = new SqlQuery();

            query.QueryString = "SELECT TOP 1 ID, QTYCOUNT FROM ext.SCJOURNALENTRY " +
                "WHERE JOURNALID = @JOURNALID AND ITEMID = @ITEMID AND SECTIONID = ISNULL(@SECTIONID, SECTIONID) AND SHELFID = ISNULL(@SHELFID, SHELFID) " +
                "AND CONFIGID = @CONFIGID AND INVENTSIZEID = @INVENTSIZEID AND INVENTCOLORID = @INVENTCOLORID AND INVENTSTYLEID = @INVENTSTYLEID " +
                "AND INVENTBATCHID = @INVENTBATCHID AND INVENTSERIALID = @INVENTSERIALID " +
                "AND DEVICEID = @DEVICEID AND [STATUS] = 0 ORDER BY ID DESC";

            query.Parameters["@JOURNALID"] = item.JournalId;
            query.Parameters["@ITEMID"] = item.ItemId ?? String.Empty;
            query.Parameters["@CONFIGID"] = item.ConfigId ?? String.Empty;
            query.Parameters["@INVENTSIZEID"] = item.InventSizeId ?? String.Empty;
            query.Parameters["@INVENTCOLORID"] = item.InventColorId ?? String.Empty;
            query.Parameters["@INVENTSTYLEID"] = item.InventStyleId ?? String.Empty;
            query.Parameters["@INVENTBATCHID"] = item.InventBatchId ?? String.Empty;
            query.Parameters["@INVENTSERIALID"] = item.InventSerialId ?? String.Empty;
            query.Parameters["@SECTIONID"] = String.IsNullOrEmpty(item.SectionId) ? null : item.SectionId;
            query.Parameters["@SHELFID"] = String.IsNullOrEmpty(item.ShelfId) ? null : item.ShelfId;
            query.Parameters["@DEVICEID"] = deviceNumber;

            int entryId = 0;
            decimal entryQty = 0;

            using (DatabaseQuery databaseContext = new DatabaseQuery(request.RequestContext))
            {
                using (IDatabaseResult reader = await databaseContext.Execute(query).ConfigureAwait(false))
                {
                    if (await reader.ReadAsync().ConfigureAwait(false))
                    {
                        entryId = reader.GetValue<int>("ID");
                        entryQty = reader.GetValue<decimal>("QTYCOUNT");
                    }
                }
            }

            return new SingleEntityDataServiceResponse<Tuple<int, decimal>>(new Tuple<int, decimal>(entryId, entryQty));
        }

        private async Task<NullResponse> UpdateStockTakeItem(UpdateStockTakeItemDataRequest request)
        {
            ThrowIf.Null(request, "request");

            SqlQuery query = new SqlQuery();

            query.QueryString = "UPDATE ext.SCJOURNALENTRY SET QTYCOUNT = @QTYCOUNT, COUNTDATE = @COUNTDATE WHERE ID = @ID";

            query.Parameters["@ID"] = request.Id;
            query.Parameters["@QTYCOUNT"] = request.Quantity;
            query.Parameters["@COUNTDATE"] = request.RequestContext.GetNowInChannelTimeZone();  // DateTime.Now;

            using (DatabaseContext databaseContext = new DatabaseContext(request.RequestContext))
            {
                await databaseContext.ExecuteNonQueryAsync(query).ConfigureAwait(false);
            }

            return NullResponse.Instance; //   return new NullResponse();
        }

        private async Task<NullResponse> CreateStockTakeItem(CreateStockTakeItemDataRequest request)
        {
            ThrowIf.Null(request, "request");
            ThrowIf.Null(request.Item, "request.Item");

            string deviceNumber = request.RequestContext.GetPrincipal().DeviceNumber;
            if (string.IsNullOrEmpty(deviceNumber))
            {
                throw new NotFoundException("Please try again.");
            }

            StockTakeItem item = request.Item;

            SqlQuery query = new SqlQuery();

            query.QueryString = 
                "INSERT INTO ext.SCJOURNALENTRY ( " +
                    "JOURNALID, ITEMID, UNIT, INVENTBATCHID, INVENTSERIALID, INVENTSIZEID, INVENTCOLORID, INVENTSTYLEID, CONFIGID, DEVICEID, SECTIONID, SHELFID, QTYCOUNT, COUNTDATE, STATUS, TRANSID" +
                ") VALUES (" +
                    "@JOURNALID, @ITEMID, @UNIT, @INVENTBATCHID, @INVENTSERIALID, @INVENTSIZEID, @INVENTCOLORID, @INVENTSTYLEID, @CONFIGID, @DEVICEID, @SECTIONID, @SHELFID, @QTYCOUNT, @COUNTDATE, 0, 0" +
                ")";

            query.Parameters["@JOURNALID"] = item.JournalId;
            query.Parameters["@ITEMID"] = item.ItemId ?? String.Empty;
            query.Parameters["@UNIT"] = item.Unit ?? String.Empty;
            query.Parameters["@INVENTBATCHID"] = item.InventBatchId ?? String.Empty;
            query.Parameters["@INVENTSERIALID"] = item.InventSerialId ?? String.Empty;
            query.Parameters["@INVENTSIZEID"] = item.InventSizeId ?? String.Empty;
            query.Parameters["@INVENTCOLORID"] = item.InventColorId ?? String.Empty;
            query.Parameters["@INVENTSTYLEID"] = item.InventStyleId ?? String.Empty;
            query.Parameters["@CONFIGID"] = item.ConfigId ?? String.Empty;
            query.Parameters["@SECTIONID"] = item.SectionId ?? String.Empty;
            query.Parameters["@SHELFID"] = item.ShelfId ?? String.Empty;
            query.Parameters["@DEVICEID"] = deviceNumber;
            query.Parameters["@QTYCOUNT"] = request.Quantity;
            query.Parameters["@COUNTDATE"] = request.RequestContext.GetNowInChannelTimeZone();  // DateTime.Now;

            using (DatabaseContext databaseContext = new DatabaseContext(request.RequestContext))
            {
                await databaseContext.ExecuteNonQueryAsync(query).ConfigureAwait(false);
            }

            return NullResponse.Instance; //   return new NullResponse();
        }
    }
}