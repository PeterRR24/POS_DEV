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
using Microsoft.Dynamics.Commerce.Runtime.Services.Messages;

using RR.Commerce.Runtime.Utilities;

namespace RR.Commerce.Runtime.Printing
{
    public class LabelItemDataService : IRequestHandlerAsync
    {
        public IEnumerable<Type> SupportedRequestTypes
        {
            get
            {
                return new[]
                {
                    typeof(LabelItemSearchDataRequest),
                    typeof(LabelItemFormatDataRequest),
                    typeof(LabelItemGetDataRequest),
                    typeof(LabelItemFindDataRequest),
                    typeof(LabelItemLoadDataRequest),
                    typeof(LabelItemSaveDataRequest),
                    typeof(LabelItemUpdateDataRequest),
                    typeof(LabelItemDeleteDataRequest),
                    typeof(LabelItemClearDataRequest),
                    typeof(LabelItemStatisticsDataRequest),
                    typeof(LabelItemCountDataRequest),
                    typeof(LabelHistoryGetDataRequest),
                    typeof(LabelHistorySaveDataRequest),
                    typeof(GetLabelProfileDataRequest),
                    typeof(GetLabelProfileLineDataRequest),
                    typeof(GetLabelFormatDataRequest),
                    typeof(GetLabelLayoutDataRequest),
                    typeof(GetLabelDataDataRequest),
                    typeof(GetLabelCodeDataRequest),
                };
            }
        }

        public async Task<Response> Execute(Request request)
        {
            if (request == null)
                throw new ArgumentNullException("request");

            Type requestType = request.GetType();
            Response response;

            if (requestType == typeof(LabelItemSearchDataRequest))
                response = await this.SearchLabelItems((LabelItemSearchDataRequest)request).ConfigureAwait(false);
            else if (requestType == typeof(LabelItemFormatDataRequest))
                response = await this.FormatLabelItems((LabelItemFormatDataRequest)request).ConfigureAwait(false);
            else if (requestType == typeof(LabelItemGetDataRequest))
                response = await this.GetLabelItem((LabelItemGetDataRequest)request).ConfigureAwait(false);
            else if (requestType == typeof(LabelItemFindDataRequest))
                response = await this.FindLabelItem((LabelItemFindDataRequest)request).ConfigureAwait(false);
            else if (requestType == typeof(LabelItemLoadDataRequest))
                response = await this.LoadLabelItem((LabelItemLoadDataRequest)request).ConfigureAwait(false);
            else if (requestType == typeof(LabelItemSaveDataRequest))
                response = await this.SaveLabelItem((LabelItemSaveDataRequest)request).ConfigureAwait(false);
            else if (requestType == typeof(LabelItemUpdateDataRequest))
                response = await this.UpdateLabelItem((LabelItemUpdateDataRequest)request).ConfigureAwait(false);
            else if (requestType == typeof(LabelItemDeleteDataRequest))
                response = await this.DeleteLabelItem((LabelItemDeleteDataRequest)request).ConfigureAwait(false);
            else if (requestType == typeof(LabelItemClearDataRequest))
                response = await this.ClearLabelItems((LabelItemClearDataRequest)request).ConfigureAwait(false);
            else if (requestType == typeof(LabelItemStatisticsDataRequest))
                response = await this.CalcLabelStats((LabelItemStatisticsDataRequest)request).ConfigureAwait(false);
            else if (requestType == typeof(LabelItemCountDataRequest))
                response = await this.CountLabelItem((LabelItemCountDataRequest)request).ConfigureAwait(false);
            else if (requestType == typeof(LabelHistoryGetDataRequest))
                response = await this.GetLabelHistory((LabelHistoryGetDataRequest)request).ConfigureAwait(false);
            else if (requestType == typeof(LabelHistorySaveDataRequest))
                response = await this.SaveLabelHistory((LabelHistorySaveDataRequest)request).ConfigureAwait(false);
            else if (requestType == typeof(GetLabelProfileDataRequest))
                response = await this.GetLabelProfileId((GetLabelProfileDataRequest)request).ConfigureAwait(false);
            else if (requestType == typeof(GetLabelProfileLineDataRequest))
                response = await this.GetLabelProfileLine((GetLabelProfileLineDataRequest)request).ConfigureAwait(false);
            else if (requestType == typeof(GetLabelFormatDataRequest))
                response = await this.GetLabelFormat((GetLabelFormatDataRequest)request).ConfigureAwait(false);
            else if (requestType == typeof(GetLabelLayoutDataRequest))
                response = await this.GetLabelLayout((GetLabelLayoutDataRequest)request).ConfigureAwait(false);
            else if (requestType == typeof(GetLabelDataDataRequest))
                response = await this.GetLabelData((GetLabelDataDataRequest)request).ConfigureAwait(false);
            else if (requestType == typeof(GetLabelCodeDataRequest))
                response = await this.GetLabelCode((GetLabelCodeDataRequest)request).ConfigureAwait(false);
            else
                throw new NotSupportedException(String.Format("Request '{0}' is not supported.", request.GetType()));

            return response;
        }

        private async Task<EntityDataServiceResponse<LabelItem>> SearchLabelItems(LabelItemSearchDataRequest request)
        {
            ThrowIf.Null(request, "request");
            ThrowIf.Null(request.QueryResultSettings, "request.QueryResultSettings");
            string strCondition = string.Empty;
            bool isLabelPrintDeviceItems = await IsLabelPrintDeviceItems(request.RequestContext).ConfigureAwait(false);
            if (request.Personal || !isLabelPrintDeviceItems) strCondition = " AND (DEVICEID = @DeviceId)";
#pragma warning disable PreventSqlInjectionAnalyzer // Expression assgined to SqlPagedQuery.Where should use only literals and constants to prevent SQL injection.
            SqlPagedQuery query = new SqlPagedQuery(request.QueryResultSettings)
            {
                DatabaseSchema = "ext",
                From = "PRLABELITEMVIEW",
                Where = "(CHANNELID = @ChannelId) AND (LABELTYPE = @LabelType)" + strCondition
            };
#pragma warning restore PreventSqlInjectionAnalyzer // Expression assgined to SqlPagedQuery.Where should use only literals and constants to prevent SQL injection.

            query.Parameters["@ChannelId"] = request.RequestContext.GetPrincipal().ChannelId;
            query.Parameters["@LabelType"] = (int)request.LabelType;
            if (request.Personal || !isLabelPrintDeviceItems)
                query.Parameters["@DeviceId"] = request.Personal ? request.RequestContext.GetPrincipal().DeviceNumber : String.Empty;

            if (!String.IsNullOrEmpty(request.Keyword))
            {
                query.Where += " AND ((ITEMID LIKE @SearchText) OR (NAME LIKE @SearchText) OR (DESCRIPTION LIKE @SearchText))";
                if (request.Keyword.Contains("%"))
                    query.Parameters["@SearchText"] = request.Keyword;
                else
                    query.Parameters["@SearchText"] = '%' + request.Keyword + '%';
            }

            using (DatabaseContext databaseContext = new DatabaseContext(request.RequestContext))
            {
                return new EntityDataServiceResponse<LabelItem>(await databaseContext.ReadEntityAsync<LabelItem>(query).ConfigureAwait(false));
            }
        }

        private async Task<EntityDataServiceResponse<LabelItem>> FormatLabelItems(LabelItemFormatDataRequest request)
        {
            ThrowIf.Null(request, "request");
            ThrowIf.Null(request.Item, "request.Item");

            LabelItemStatistic item = request.Item;
            List<long> Ids = new List<long>();
            if (item.LabelRecIds != null && item.LabelRecIds.Length > 1)
            {
                string[] strIdArr = item.LabelRecIds.Split(',');
                for (int i = 0; i < strIdArr.Length; i++)
                {
                    if (!string.IsNullOrEmpty(strIdArr[i]))
                        Ids.Add(Convert.ToInt64(strIdArr[i]));
                }
            }
            string strCondition = string.Empty;
            bool isLabelPrintDeviceItems = await IsLabelPrintDeviceItems(request.RequestContext).ConfigureAwait(false);
            if (item.Personal || !isLabelPrintDeviceItems) strCondition = " AND (DEVICEID = @DeviceId)";
            if (Ids.Any()) {
                string[] paramNames = Ids.Select((s, i) => "@RecId" + i.ToString()).ToArray();
                string inClause = string.Join(", ", paramNames);
                strCondition += " AND RECID IN (" + inClause + ")";
            }
#pragma warning disable PreventSqlInjectionAnalyzer // Expression assgined to SqlPagedQuery.Where should use only literals and constants to prevent SQL injection.
            SqlPagedQuery query = new SqlPagedQuery(QueryResultSettings.AllRecords)
            {
                DatabaseSchema = "ext",
                From = "PRLABELITEMVIEW",
                Where = "(CHANNELID = @ChannelId) AND (LABELTYPE = @LabelType) AND (LAYOUTRECID = @LayoutId) AND (PRINTERRECID = @PrinterId)" + strCondition
            };
#pragma warning restore PreventSqlInjectionAnalyzer // Expression assgined to SqlPagedQuery.Where should use only literals and constants to prevent SQL injection.

            if (item.LabelType == LabelLayoutType.Shelf)
                query.OrderBy = "SECTIONID, SHELFID, LOCATION, ITEMID, INVENTDIMID";
            else
                query.OrderBy = "ITEMID, INVENTDIMID";

            query.Parameters["@ChannelId"] = request.RequestContext.GetPrincipal().ChannelId;
            query.Parameters["@LabelType"] = (int)item.LabelType;
            query.Parameters["@LayoutId"] = item.LayoutRecId;
            query.Parameters["@PrinterId"] = item.PrinterRecId;
            if (item.Personal || !isLabelPrintDeviceItems)
                query.Parameters["@DeviceId"] = item.Personal ? request.RequestContext.GetPrincipal().DeviceNumber : String.Empty;
            if (Ids.Any()) {
                for (int i = 0; i < Ids.Count; i++)
                {
                    query.Parameters["@RecId" + i] = Ids[i];
                }
            }
            using (DatabaseContext databaseContext = new DatabaseContext(request.RequestContext))
            {
                return new EntityDataServiceResponse<LabelItem>(await databaseContext.ReadEntityAsync<LabelItem>(query).ConfigureAwait(false));
            }
        }

        private async Task<SingleEntityDataServiceResponse<LabelItem>> GetLabelItem(LabelItemGetDataRequest request)
        {
            ThrowIf.Null(request, "request");

            SqlPagedQuery query = new SqlPagedQuery(QueryResultSettings.SingleRecord)
            {
                DatabaseSchema = "ext",
                From = "PRLABELITEMVIEW",
                Where = "(RECID = @RecId)"
            };

            query.Parameters["@RecId"] = request.RecordId;

            using (DatabaseContext databaseContext = new DatabaseContext(request.RequestContext))
            {
                return new SingleEntityDataServiceResponse<LabelItem>((await databaseContext.ReadEntityAsync<LabelItem>(query).ConfigureAwait(false)).FirstOrDefault());
            }
        }

        private async Task<SingleEntityDataServiceResponse<LabelItem>> FindLabelItem(LabelItemFindDataRequest request)
        {
            ThrowIf.Null(request, "request");

            SqlPagedQuery query = new SqlPagedQuery(QueryResultSettings.SingleRecord)
            {
                DatabaseSchema = "ext",
                From = "PRLABELITEMVIEW",
                Where = "(CHANNELID = @ChannelId) AND (DEVICEID = @DeviceId) AND (ITEMID = @ItemId) AND (INVENTDIMID = @InventDimId) AND (LABELTYPE = @LabelType) AND (CODERECID = @CodeRecId)"
            };

            query.Parameters["@ChannelId"] = request.RequestContext.GetPrincipal().ChannelId;
            query.Parameters["@DeviceId"] = request.Personal ? request.RequestContext.GetPrincipal().DeviceNumber : String.Empty;
            query.Parameters["@ItemId"] = request.ItemId ?? String.Empty;
            query.Parameters["@InventDimId"] = request.InventDimId ?? String.Empty;
            query.Parameters["@LabelType"] = (int)request.LabelType;
            query.Parameters["@CodeRecId"] = request.CodeRecId;

            using (DatabaseContext databaseContext = new DatabaseContext(request.RequestContext))
            {
                return new SingleEntityDataServiceResponse<LabelItem>((await databaseContext.ReadEntityAsync<LabelItem>(query).ConfigureAwait(false)).FirstOrDefault());
            }
        }

        private async Task<SingleEntityDataServiceResponse<LabelItem>> LoadLabelItem(LabelItemLoadDataRequest request)
        {
            ThrowIf.Null(request, "request");

            SqlPagedQuery query = new SqlPagedQuery(QueryResultSettings.SingleRecord)
            {
                DatabaseSchema = "ext",
                From = "GETPRLABELITEMVIEW(@tvp_LabelItems)"
            };

            using (LabelItemTableType type = new LabelItemTableType(request.Item))
            {
                query.Parameters["@tvp_LabelItems"] = type.DataTable;

                using (DatabaseContext databaseContext = new DatabaseContext(request.RequestContext))
                {
                    return new SingleEntityDataServiceResponse<LabelItem>((await databaseContext.ReadEntityAsync<LabelItem>(query).ConfigureAwait(false)).FirstOrDefault());
                }
            }
        }

        private async Task<SingleEntityDataServiceResponse<long>> SaveLabelItem(LabelItemSaveDataRequest request)
        {
            ThrowIf.Null(request, "request");
            ThrowIf.Null(request.Item, "request.Item");

            SqlQuery query = new SqlQuery();

            query.QueryString =
            "MERGE INTO ext.PRLABELITEM T " +
            "USING (SELECT @RecordId, @ChannelId, @DeviceId, @ItemId, @InventDimId, @LabelType, " +
                "@DataAreaId, @DateCreated, @LayoutId, @FormatId, @PrinterId, @NoOfLabels, " + 
                "@Barcode, @Name, @Description, " +
                "@UnitId, @BaseUnitId, @BaseUnitQty, @EquivUnitId, @EquivUnitQty, " +
                "@UnitPrice, @UnitPriceOld, @UnitPriceCur, @SalePrice, " +
                "@SectionId, @ShelfId, @SeasonId, @Department, @Category, @DataRecId, @CodeRecId, @PriceDate, @Location, @PrintBatchNo" +
            ") AS S ( " +
                "[RECID], [CHANNELID], [DEVICEID], [ITEMID], [INVENTDIMID], [LABELTYPE], " +
                "[DATAAREAID], [DATECREATED], [LAYOUTRECID], [FORMATRECID], [PRINTERRECID], [NOOFLABELS], " +
                "[BARCODE], [NAME], [DESCRIPTION], " +
                "[UNITID], [BASEUNITID], [BASEUNITQTY], [EQUIVUNITID], [EQUIVUNITQTY], " +
                "[UNITPRICE], [UNITPRICEOLD], [UNITPRICECUR], [SALEPRICE], " +
                "[SECTIONID], [SHELFID], [SEASONID], [DEPARTMENT], [CATEGORY], [DATARECID], [CODERECID], [PRICEDATE], [LOCATION], [PRINTBATCHNO] " +
            ") " +
            "ON " +
            "(S.[RECID] != 0 AND T.[RECID] = S.[RECID]) " +
            "OR " +
            "(S.[RECID] = 0 AND T.[CHANNELID] = S.[CHANNELID] AND T.[DEVICEID] = S.[DEVICEID] AND T.[ITEMID] = S.[ITEMID] AND T.[INVENTDIMID] = S.[INVENTDIMID] AND T.[LABELTYPE] = S.[LABELTYPE] AND T.[CODERECID] = S.[CODERECID]) " +
            "WHEN MATCHED THEN " +
            "UPDATE SET " +
                "[CHANNELID] = S.[CHANNELID], [DEVICEID] = S.[DEVICEID], [ITEMID] = S.[ITEMID], [INVENTDIMID] = S.[INVENTDIMID], [LABELTYPE] = S.[LABELTYPE], " +
                "[DATAAREAID] = S.[DATAAREAID], [DATECREATED] = S.[DATECREATED], [LAYOUTRECID] = S.[LAYOUTRECID], [FORMATRECID] = S.[FORMATRECID], [PRINTERRECID] = S.[PRINTERRECID], [NOOFLABELS] = S.[NOOFLABELS], " +
                "[BARCODE] = S.[BARCODE], [NAME] = S.[NAME], [DESCRIPTION] = S.[DESCRIPTION], " +
                "[UNITID] = S.[UNITID], [BASEUNITID] = S.[BASEUNITID], [BASEUNITQTY] = S.[BASEUNITQTY], [EQUIVUNITID] = S.[EQUIVUNITID], [EQUIVUNITQTY] = S.[EQUIVUNITQTY], " +
                "[UNITPRICE] = S.[UNITPRICE], [UNITPRICEOLD] = S.[UNITPRICEOLD], [UNITPRICECUR] = S.[UNITPRICECUR], [SALEPRICE] = S.[SALEPRICE], " +
                "[SECTIONID] = S.[SECTIONID], [SHELFID] = S.[SHELFID], [SEASONID] = S.[SEASONID], [DEPARTMENT] = S.[DEPARTMENT], [CATEGORY] = S.[CATEGORY], [DATARECID] = S.[DATARECID], " +
                "[CODERECID] = S.[CODERECID], [PRICEDATE] = S.[PRICEDATE], [LOCATION] = S.[LOCATION], [PRINTBATCHNO] = S.[PRINTBATCHNO] " +
            "WHEN NOT MATCHED THEN " +
            "INSERT " +
            "( " +
                "[CHANNELID], [DEVICEID], [ITEMID], [INVENTDIMID], [LABELTYPE], " +
                "[DATAAREAID], [DATECREATED], [LAYOUTRECID], [FORMATRECID], [PRINTERRECID], [NOOFLABELS], " +
                "[BARCODE], [NAME], [DESCRIPTION], " +
                "[UNITID], [BASEUNITID], [BASEUNITQTY], [EQUIVUNITID], [EQUIVUNITQTY], " +
                "[UNITPRICE], [UNITPRICEOLD], [UNITPRICECUR], [SALEPRICE], " +
                "[SECTIONID], [SHELFID], [SEASONID], [DEPARTMENT], [CATEGORY], [DATARECID], [CODERECID], [PRICEDATE], [LOCATION], [PRINTBATCHNO], " +
                "[RECID] " +
            ") " +
            "VALUES " +
            "( " +
                "S.[CHANNELID], S.[DEVICEID], S.[ITEMID], S.[INVENTDIMID], S.[LABELTYPE], " +
                "S.[DATAAREAID], S.[DATECREATED], S.[LAYOUTRECID], S.[FORMATRECID], S.[PRINTERRECID], S.[NOOFLABELS], " +
                "S.[BARCODE], S.[NAME], S.[DESCRIPTION], " +
                "S.[UNITID], S.[BASEUNITID], S.[BASEUNITQTY], S.[EQUIVUNITID], S.[EQUIVUNITQTY], " +
                "S.[UNITPRICE], S.[UNITPRICEOLD], S.[UNITPRICECUR], S.[SALEPRICE], " +
                "S.[SECTIONID], S.[SHELFID], S.[SEASONID], S.[DEPARTMENT], S.[CATEGORY], S.[DATARECID], S.[CODERECID], S.[PRICEDATE], S.[LOCATION], S.[PRINTBATCHNO], " +
                "(SELECT ISNULL(MAX(RECID),0) + 1 from ext.PRLABELITEM) " +
            "); " +
            "SELECT RECID FROM ext.PRLABELITEM WHERE [CHANNELID] = @ChannelId AND [DEVICEID] = @DeviceId AND [ITEMID] = @ItemId AND [INVENTDIMID] = @InventDimId AND [LABELTYPE] = @LabelType AND [CODERECID] = @CodeRecId";

            query.Parameters["@RecordId"] = request.Item.Id;
            query.Parameters["@ChannelId"] = request.Item.ChannelId;
            query.Parameters["@DeviceId"] = request.Item.DeviceId ?? String.Empty;
            query.Parameters["@ItemId"] = request.Item.ItemId ?? String.Empty;
            query.Parameters["@InventDimId"] = request.Item.InventDimId ?? String.Empty;
            query.Parameters["@LabelType"] = (int)request.Item.LabelType;
            query.Parameters["@DataAreaId"] = request.Item.DataAreaId ?? String.Empty;
            query.Parameters["@DateCreated"] = request.Item.DateCreated == DateTimeOffset.MinValue ? DateTimeOffset.Now : request.Item.DateCreated;
            query.Parameters["@LayoutId"] = request.Item.LayoutlId;
            query.Parameters["@FormatId"] = request.Item.FormatId;
            query.Parameters["@PrinterId"] = request.Item.PrinterId;
            query.Parameters["@NoOfLabels"] = request.Item.NoOfLabels;
            query.Parameters["@Barcode"] = request.Item.Barcode ?? String.Empty;
            query.Parameters["@Name"] = request.Item.Name ?? String.Empty;
            query.Parameters["@Description"] = request.Item.Description ?? String.Empty;
            query.Parameters["@UnitId"] = request.Item.UnitId ?? String.Empty;
            query.Parameters["@BaseUnitId"] = request.Item.BaseUnitId ?? String.Empty;
            query.Parameters["@BaseUnitQty"] = request.Item.BaseUnitQty;
            query.Parameters["@EquivUnitId"] = request.Item.EquivUnitId ?? String.Empty;
            query.Parameters["@EquivUnitQty"] = request.Item.EquivUnitQty;
            query.Parameters["@UnitPrice"] = request.Item.UnitPrice;
            if (request.Item.UnitPriceOld.HasValue)
                query.Parameters["@UnitPriceOld"] = request.Item.UnitPriceOld.Value;
            else
                query.Parameters["@UnitPriceOld"] = null;
            if (request.Item.UnitPriceCurrency.HasValue)
                query.Parameters["@UnitPriceCur"] = request.Item.UnitPriceCurrency.Value;
            else
                query.Parameters["@UnitPriceCur"] = null;
            if (request.Item.SalePrice.HasValue)
                query.Parameters["@SalePrice"] = request.Item.SalePrice.Value;
            else
                query.Parameters["@SalePrice"] = null;
            query.Parameters["@SectionId"] = request.Item.SectionId ?? String.Empty;
            query.Parameters["@ShelfId"] = request.Item.ShelfId ?? String.Empty;
            query.Parameters["@SeasonId"] = request.Item.SeasonId ?? String.Empty;
            query.Parameters["@Department"] = request.Item.Department ?? String.Empty;
            query.Parameters["@Category"] = request.Item.Category ?? String.Empty;
            query.Parameters["@DataRecId"] = request.Item.DataRecId;
            query.Parameters["@CodeRecId"] = request.Item.CodeRecId;
            query.Parameters["@PriceDate"] = request.Item.PriceDate == DateTime.MinValue ? DateTime.Today : request.Item.PriceDate;
            query.Parameters["@Location"] = request.Item.Location;
            query.Parameters["@PrintBatchNo"] = request.Item.PrintBatchNo;

            long recordId = 0;

            using (DatabaseContext databaseContext = new DatabaseContext(request.RequestContext))
            {
                recordId = await databaseContext.ExecuteScalarAsync<long>(query).ConfigureAwait(false);
                /*
                using (IDatabaseResult databaseResult = databaseContext.ExecuteQuery(query))
                {
                    if (databaseResult.Read())
                        recordId = databaseResult.GetValue<long>(0);
                }
                */
            }

            return new SingleEntityDataServiceResponse<long>(recordId);
        }

        private async Task<SingleEntityDataServiceResponse<LabelItem>> UpdateLabelItem(LabelItemUpdateDataRequest request)
        {
            ThrowIf.Null(request, "request");
            ThrowIf.Null(request.Item, "request.Item");

            SqlQuery query = new SqlQuery();

            query.QueryString =
                "UPDATE ext.PRLABELITEM SET NOOFLABELS = @NoOfLabels WHERE RECID = @RecordId; " +
                "SELECT NOOFLABELS FROM ext.PRLABELITEM WHERE RECID = @RecordId";

            query.Parameters["@RecordId"] = request.Item.Id;
            query.Parameters["@NoOfLabels"] = request.Item.NoOfLabels;

            using (DatabaseQuery databaseContext = new DatabaseQuery(request.RequestContext))
            {
                using (IDatabaseResult databaseResult = await databaseContext.Execute(query).ConfigureAwait(false))
                {
                    if (await databaseResult.ReadAsync().ConfigureAwait(false))
                        request.Item.NoOfLabels = databaseResult.GetValue<int>(0);
                }
            }

            return new SingleEntityDataServiceResponse<LabelItem>(request.Item);
        }

        private async Task<NullResponse> DeleteLabelItem(LabelItemDeleteDataRequest request)
        {
            ThrowIf.Null(request, "request");

            SqlQuery query = new SqlQuery();

            query.QueryString = "DELETE FROM ext.PRLABELITEM WHERE RECID = @RecordId";
            query.Parameters["@RecordId"] = request.RecordId;

            using (DatabaseContext databaseContext = new DatabaseContext(request.RequestContext))
            {
                await databaseContext.ExecuteNonQueryAsync(query).ConfigureAwait(false);
            }

            //return new NullResponse();
            return NullResponse.Instance;
        }

        private async Task<NullResponse> ClearLabelItems(LabelItemClearDataRequest request)
        {
            ThrowIf.Null(request, "request");

            SqlQuery query = new SqlQuery();
            string strCondition = string.Empty;
            bool isLabelPrintDeviceItems = await IsLabelPrintDeviceItems(request.RequestContext).ConfigureAwait(false);
            if (request.Personal || !isLabelPrintDeviceItems) strCondition = " AND (DEVICEID = @DeviceId)";

            if (request.RecordId != 0)
            {
                query.QueryString = "DELETE FROM ext.PRLABELITEM WHERE ([RECID] = @RecId)";
                query.Parameters["@RecId"] = request.RecordId;
            }
            else
            {
                query.QueryString = "DELETE FROM ext.PRLABELITEM WHERE (CHANNELID = @ChannelId) AND (LABELTYPE = @LabelType)" + strCondition;
                query.Parameters["@ChannelId"] = request.RequestContext.GetPrincipal().ChannelId;                
                query.Parameters["@LabelType"] = (int)request.LabelType;
                if (request.Personal || !isLabelPrintDeviceItems)
                    query.Parameters["@DeviceId"] = request.Personal ? request.RequestContext.GetPrincipal().DeviceNumber : String.Empty;

                if (request.LayoutRecId != 0)
                {
                    query.QueryString += " AND (LAYOUTRECID = @LayoutId) AND (PRINTERRECID = @PrinterId)";
                    query.Parameters["@LayoutId"] = request.LayoutRecId;
                    query.Parameters["@PrinterId"] = request.PrinterRecId;
                }
            }
            
            using (DatabaseContext databaseContext = new DatabaseContext(request.RequestContext))
            {
                await databaseContext.ExecuteNonQueryAsync(query).ConfigureAwait(false);
            }

            //return new NullResponse();
            return NullResponse.Instance;
        }

        private async Task<CalcLabelStatisticsResponse> CalcLabelStats(LabelItemStatisticsDataRequest request)
        {
            ThrowIf.Null(request, "request");

            List<LabelItemStatistic> list = new List<LabelItemStatistic>();

            SqlQuery query = new SqlQuery();
            string strCondition = string.Empty;
            bool isLabelPrintDeviceItems = await IsLabelPrintDeviceItems(request.RequestContext).ConfigureAwait(false);
            if (request.Personal || !isLabelPrintDeviceItems) strCondition = " AND (DEVICEID = @DeviceId)";
            query.QueryString = "SELECT LAYOUTRECID, PRINTERRECID, SUM(NOOFLABELS) AS NOOFLABELS " +
                "FROM ext.PRLABELITEM " +
                "WHERE CHANNELID = @ChannelId AND LABELTYPE = @LabelType " + strCondition +
                "GROUP BY LAYOUTRECID, PRINTERRECID";
            query.Parameters["@ChannelId"] = request.RequestContext.GetPrincipal().ChannelId;
            query.Parameters["@LabelType"] = (int)request.LabelType;
            if (request.Personal || !isLabelPrintDeviceItems)
                query.Parameters["@DeviceId"] = request.Personal ? request.RequestContext.GetPrincipal().DeviceNumber : String.Empty;

            using (DatabaseQuery databaseContext = new DatabaseQuery(request.RequestContext))
            {
                using (IDatabaseResult reader = await databaseContext.Execute(query).ConfigureAwait(false))
                {
                    while (await reader.ReadAsync().ConfigureAwait(false))
                    {
                        LabelItemStatistic entry = new LabelItemStatistic();
                        entry.LabelType = request.LabelType;
                        entry.Personal = request.Personal;
                        entry.LayoutRecId = reader.GetValue<long>(0);
                        entry.PrinterRecId = reader.GetValue<long>(1);
                        entry.NoOfLabels = reader.GetValue<int>(2);
                        list.Add(entry);
                    }
                }
            }

            return new CalcLabelStatisticsResponse(list);
        }

        private async Task<SingleEntityDataServiceResponse<int[]>>  CountLabelItem(LabelItemCountDataRequest request)
        {
            ThrowIf.Null(request, "request");

            List<LabelItemStatistic> list = new List<LabelItemStatistic>();

            SqlQuery query = new SqlQuery();
            query.QueryString = "SELECT COUNT(*), SUM(NOOFLABELS) " +
                "FROM ext.PRLABELITEM " +
                "WHERE CHANNELID = @ChannelId AND DEVICEID = @DeviceId AND LABELTYPE = @LabelType ";
            query.Parameters["@ChannelId"] = request.RequestContext.GetPrincipal().ChannelId;
            query.Parameters["@DeviceId"] = request.Personal ? request.RequestContext.GetPrincipal().DeviceNumber : String.Empty;
            query.Parameters["@LabelType"] = (int)request.LabelType;

            int[] result = new int[2];

            using (DatabaseQuery databaseContext = new DatabaseQuery(request.RequestContext))
            {
                using (IDatabaseResult reader = await databaseContext.Execute(query).ConfigureAwait(false))
                {
                    if (await reader.ReadAsync().ConfigureAwait(false))
                    {
                        result[0] = reader.GetValue<int>(0);
                        object value = reader.GetValue<object>(1);
                        if (value != null)
                            result[1] = Convert.ToInt16(value);
                    }
                }
            }

            return new SingleEntityDataServiceResponse<int[]>(result);
        }

        private async Task<SingleEntityDataServiceResponse<LabelHistory>> GetLabelHistory(LabelHistoryGetDataRequest request)
        {
            ThrowIf.Null(request, "request");
            ThrowIf.Null(request.Item, "request.Item");
            
            SqlPagedQuery query = new SqlPagedQuery(QueryResultSettings.SingleRecord)
            {
                DatabaseSchema = "ext",
                From = "PRLABELHISTORY",
                Where = "(CHANNELID = @ChannelId) AND (ITEMID = @ItemId) AND (INVENTDIMID = @InventDimId) AND (UNITID = @UnitId) AND (LABELTYPE = @LabelType) AND (CODERECID = @CodeRecId) AND (DATAAREAID = @DataAreaId)"
            };

            query.Parameters["@ChannelId"] = request.Item.ChannelId;
            query.Parameters["@ItemId"] = request.Item.ItemId ?? String.Empty;
            query.Parameters["@InventDimId"] = request.Item.InventDimId ?? String.Empty;
            query.Parameters["@UnitId"] = request.Item.UnitId ?? String.Empty;
            query.Parameters["@LabelType"] = (int)request.Item.LabelType;
            query.Parameters["@CodeRecId"] = request.Item.CodeRecId;
            query.Parameters["@DataAreaId"] = request.Item.DataAreaId ?? String.Empty;

            using (DatabaseContext databaseContext = new DatabaseContext(request.RequestContext))
            {
                return new SingleEntityDataServiceResponse<LabelHistory>((await databaseContext.ReadEntityAsync<LabelHistory>(query).ConfigureAwait(false)).FirstOrDefault());
            }
        }

        private async Task<NullResponse> SaveLabelHistory(LabelHistorySaveDataRequest request)
        {
            ThrowIf.Null(request, "request");
            if (request.RecordId != 0)
                return await SaveLabelItemHistory(request).ConfigureAwait(false);
            else
                return await SaveLayoutItemsHistory(request).ConfigureAwait(false);
        }

        private async Task<NullResponse> SaveLabelItemHistory(LabelHistorySaveDataRequest request)
        {
            SqlQuery query = new SqlQuery();
            query.QueryString =
            "MERGE INTO ext.PRLABELHISTORY T " +
            "USING ( " +
                "SELECT [CHANNELID], [ITEMID], [INVENTDIMID], [UNITID], [LABELTYPE], [CODERECID], [DATAAREAID], " +
                "[DATECREATED], [LAYOUTRECID], [FORMATRECID], [BARCODE], [NAME], [DESCRIPTION], [UNITPRICE], [UNITPRICEOLD], [UNITPRICECUR] " +
                "FROM ext.PRLABELITEM " +
                "WHERE ([RECID] = @RecId) " +
            ") AS S ( " +
                "[CHANNELID], [ITEMID], [INVENTDIMID], [UNITID], [LABELTYPE], [CODERECID], [DATAAREAID], " +
                "[LASTUPDATED], [LAYOUTRECID], [FORMATRECID], [BARCODE], [NAME], [DESCRIPTION], [UNITPRICE], [UNITPRICEOLD], [UNITPRICECUR] " +
            ") " +
            "ON T.[CHANNELID] = S.[CHANNELID] AND T.[ITEMID] = S.[ITEMID] AND T.[INVENTDIMID] = S.[INVENTDIMID] AND T.[UNITID] = S.[UNITID] AND T.[LABELTYPE] = S.[LABELTYPE] AND T.[CODERECID] = S.[CODERECID] AND T.[DATAAREAID] = S.[DATAAREAID] " +
            "WHEN MATCHED THEN " +
                "UPDATE SET " +
                    "[LASTUPDATED] = S.[LASTUPDATED], [LAYOUTRECID] = S.[LAYOUTRECID], [FORMATRECID] = S.[FORMATRECID], " +
                    "[BARCODE] = S.[BARCODE], [NAME] = S.[NAME], [DESCRIPTION] = S.[DESCRIPTION], " +
                    "[UNITPRICE] = S.[UNITPRICE], [UNITPRICEOLD] = S.[UNITPRICEOLD], [UNITPRICECUR] = S.[UNITPRICECUR] " +
            "WHEN NOT MATCHED THEN " +
                "INSERT " +
                "( " +
                    "[CHANNELID], [ITEMID], [INVENTDIMID], [UNITID], [LABELTYPE], [CODERECID], [DATAAREAID],  " +
                    "[LASTUPDATED], [LAYOUTRECID], [FORMATRECID], [BARCODE], [NAME], [DESCRIPTION], [UNITPRICE], [UNITPRICEOLD], [UNITPRICECUR] " +
                ") " +
                "VALUES " +
                "( " +
                    "S.[CHANNELID], S.[ITEMID], S.[INVENTDIMID], S.[UNITID], S.[LABELTYPE], S.[CODERECID], S.[DATAAREAID], " +
                    "S.[LASTUPDATED], S.[LAYOUTRECID], S.[FORMATRECID], S.[BARCODE], S.[NAME], S.[DESCRIPTION], S.[UNITPRICE], S.[UNITPRICEOLD], S.[UNITPRICECUR] " +
                "); ";

            query.Parameters["@RecId"] = request.RecordId;

            using (DatabaseContext databaseContext = new DatabaseContext(request.RequestContext))
            {
                await databaseContext.ExecuteNonQueryAsync(query).ConfigureAwait(false);
            }
            //return new NullResponse();
            return NullResponse.Instance;
        }

        private async Task<NullResponse> SaveLayoutItemsHistory(LabelHistorySaveDataRequest request)
        {
            SqlQuery query = new SqlQuery();
            string strCondition = string.Empty;
            bool isLabelPrintDeviceItems = await IsLabelPrintDeviceItems(request.RequestContext).ConfigureAwait(false);
            if (request.Personal || !isLabelPrintDeviceItems) strCondition = " AND ([DEVICEID] = @DeviceId)";
            query.QueryString =
            "MERGE INTO ext.PRLABELHISTORY T " +
            "USING ( " +
                "SELECT [CHANNELID], [ITEMID], [INVENTDIMID], [UNITID], [LABELTYPE], [CODERECID], [DATAAREAID], " +
                "[DATECREATED], [LAYOUTRECID], [FORMATRECID], [BARCODE], [NAME], [DESCRIPTION], [UNITPRICE], [UNITPRICEOLD], [UNITPRICECUR] " +
                "FROM ext.PRLABELITEM " +
                "WHERE ([CHANNELID] = @ChannelId) AND ([LABELTYPE] = @LabelType) AND ([LAYOUTRECID] = @LayoutId) AND ([PRINTERRECID] = @PrinterId) " + strCondition +
            ") AS S ( " +
                "[CHANNELID], [ITEMID], [INVENTDIMID], [UNITID], [LABELTYPE], [CODERECID], [DATAAREAID], " +
                "[LASTUPDATED], [LAYOUTRECID], [FORMATRECID], [BARCODE], [NAME], [DESCRIPTION], [UNITPRICE], [UNITPRICEOLD], [UNITPRICECUR] " +
            ") " +
            "ON T.[CHANNELID] = S.[CHANNELID] AND T.[ITEMID] = S.[ITEMID] AND T.[INVENTDIMID] = S.[INVENTDIMID] AND T.[UNITID] = S.[UNITID] AND T.[LABELTYPE] = S.[LABELTYPE] AND T.[CODERECID] = S.[CODERECID] AND T.[DATAAREAID] = S.[DATAAREAID] " +
            "WHEN MATCHED THEN " +
                "UPDATE SET " +
                    "[LASTUPDATED] = S.[LASTUPDATED], [LAYOUTRECID] = S.[LAYOUTRECID], [FORMATRECID] = S.[FORMATRECID], " +
                    "[BARCODE] = S.[BARCODE], [NAME] = S.[NAME], [DESCRIPTION] = S.[DESCRIPTION], " +
                    "[UNITPRICE] = S.[UNITPRICE], [UNITPRICEOLD] = S.[UNITPRICEOLD], [UNITPRICECUR] = S.[UNITPRICECUR] " +
            "WHEN NOT MATCHED THEN " +
                "INSERT " +
                "( " +
                    "[CHANNELID], [ITEMID], [INVENTDIMID], [UNITID], [LABELTYPE], [CODERECID], [DATAAREAID],  " +
                    "[LASTUPDATED], [LAYOUTRECID], [FORMATRECID], [BARCODE], [NAME], [DESCRIPTION], [UNITPRICE], [UNITPRICEOLD], [UNITPRICECUR] " +
                ") " +
                "VALUES " +
                "( " +
                    "S.[CHANNELID], S.[ITEMID], S.[INVENTDIMID], S.[UNITID], S.[LABELTYPE], S.[CODERECID], S.[DATAAREAID], " +
                    "S.[LASTUPDATED], S.[LAYOUTRECID], S.[FORMATRECID], S.[BARCODE], S.[NAME], S.[DESCRIPTION], S.[UNITPRICE], S.[UNITPRICEOLD], S.[UNITPRICECUR] " +
                "); ";
            
            query.Parameters["@ChannelId"] = request.RequestContext.GetPrincipal().ChannelId;
            query.Parameters["@LabelType"] = (int)request.LabelType;
            query.Parameters["@LayoutId"] = request.LayoutRecId;
            query.Parameters["@PrinterId"] = request.PrinterRecId;
            if(request.Personal || !isLabelPrintDeviceItems)
                query.Parameters["@DeviceId"] = request.Personal ? request.RequestContext.GetPrincipal().DeviceNumber : String.Empty;

            using (DatabaseContext databaseContext = new DatabaseContext(request.RequestContext))
            {
                await databaseContext.ExecuteNonQueryAsync(query).ConfigureAwait(false);
            }
            //return new NullResponse();
            return NullResponse.Instance;
        }

        private async Task<bool> IsLabelPrintDeviceItems(RequestContext context)
        {
            UtilitiesConfiguration config = (await context.ExecuteAsync<SingleEntityDataServiceResponse<UtilitiesConfiguration>>(new GetUtilitiesConfigurationDataRequest()).ConfigureAwait(false)).Entity;
            return config.LabelPrintDeviceItems == 1;
        }

        // Calculate

        private async Task<SingleEntityDataServiceResponse<LabelProfile>> GetLabelProfileId(GetLabelProfileDataRequest request)
        {
            ThrowIf.Null(request, "request");

            UtilitiesConfiguration utilitiesConfiguration = (UtilitiesConfiguration)request.RequestContext.GetProperty("UtilitiesConfiguration");
            if (utilitiesConfiguration == null)
                throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_RequiredValueNotFound, "Utilities Configuration on the query context must be specified.");

            LabelDataManager dataManager = LabelDataManager.Instantiate(request.RequestContext);

            //long profileId = dataManager.GetLabelProfileForItem(request.ProductId, utilitiesConfiguration.LabelHierarchyRecId);
            long profileId = await GetLabelProfileForItem(request, utilitiesConfiguration.LabelHierarchyRecId).ConfigureAwait(false);
            if (profileId == 0)
                profileId = utilitiesConfiguration.DefaultLabelProfileRecId;

            LabelProfile profile = await dataManager.GetLabelProfile(profileId).ConfigureAwait(false);

            return new SingleEntityDataServiceResponse<LabelProfile>(profile);
        }

        private async Task<long> GetLabelProfileForItem(GetLabelProfileDataRequest request, long hierarchyRecId)
        {
            SqlQuery query = new SqlQuery();
            query.QueryString = "SELECT ext.GETPRODUCTLABELPROFILEID(@ProductId, @HierarchyId)";
            query.Parameters["@ProductId"] = request.ProductId;
            query.Parameters["@HierarchyId"] = hierarchyRecId;
            using (DatabaseContext databaseContext = new DatabaseContext(request.RequestContext))
            {
                return await databaseContext.ExecuteScalarAsync<long>(query).ConfigureAwait(false);
            }
        }

        private async Task<SingleEntityDataServiceResponse<LabelProfileLine>> GetLabelProfileLine(GetLabelProfileLineDataRequest request)
        {
            ThrowIf.Null(request, "request");

            LabelDataManager dataManager = LabelDataManager.Instantiate(request.RequestContext);
            LabelProfileLine result = await dataManager.GetLabelProfileLine(request.ProfileId, request.LayoutType, request.CodeRecId).ConfigureAwait(false);

            return new SingleEntityDataServiceResponse<LabelProfileLine>(result);
        }

        private async Task<SingleEntityDataServiceResponse<LabelFormat>> GetLabelFormat(GetLabelFormatDataRequest request)
        {
            ThrowIf.Null(request, "request");

            LabelDataManager dataManager = LabelDataManager.Instantiate(request.RequestContext);
            LabelFormat result = await dataManager.GetLabelFormat(request.FormatId).ConfigureAwait(false);

            return new SingleEntityDataServiceResponse<LabelFormat>(result);
        }

        private async Task<SingleEntityDataServiceResponse<LabelLayout>> GetLabelLayout(GetLabelLayoutDataRequest request)
        {
            ThrowIf.Null(request, "request");

            LabelDataManager dataManager = LabelDataManager.Instantiate(request.RequestContext);
            LabelLayout result = await dataManager.GetLabelLayout(request.LayoutId).ConfigureAwait(false);

            return new SingleEntityDataServiceResponse<LabelLayout>(result);
        }

        private async Task<SingleEntityDataServiceResponse<LabelData>> GetLabelData(GetLabelDataDataRequest request)
        {
            ThrowIf.Null(request, "request");

            //LabelDatabaseAccessor dataAccessor = LabelDatabaseAccessor.Instantiate(request.RequestContext);
            //if (request.RecId != 0)
            //    return new SingleEntityDataServiceResponse<LabelData>(dataAccessor.GetLabelData(request.RecId));
            //else
            //    return new SingleEntityDataServiceResponse<LabelData>(dataAccessor.GetLabelData(request.ItemId, request.VariantId));

            LabelData labelData;
            SqlPagedQuery query = new SqlPagedQuery(QueryResultSettings.SingleRecord);
            query.DatabaseSchema = "ext";
            query.From = "PRLABELDATA";

            if (request.RecId != 0)
            {
                query.Where = "RECID = @RECID";
                query.Parameters["@RECID"] = request.RecId;
            }
            else
            {
                query.Where = "ITEMID = @ITEMID AND (VARIANTID = @VARIANTID OR VARIANTID = '') AND DATAAREAID = @DATAAREAID";
                query.Parameters["@DATAAREAID"] = request.RequestContext.GetChannelConfiguration().InventLocationDataAreaId;
                query.Parameters["@ITEMID"] = request.ItemId;
                query.Parameters["@VARIANTID"] = request.VariantId ?? String.Empty;
                query.OrderBy = "ITEMID, VARIANTID DESC";
            }

            using (DatabaseQuery databaseContext = new DatabaseQuery(request.RequestContext))
            {
                labelData = (await databaseContext.ReadEntityAsync<LabelData>(query).ConfigureAwait(false)).FirstOrDefault();
            }
            return new SingleEntityDataServiceResponse<LabelData>(labelData);
        }

        private async Task<EntityDataServiceResponse<LabelCode>> GetLabelCode(GetLabelCodeDataRequest request)
        {
            ThrowIf.Null(request, "request");

            LabelDataManager dataManager = LabelDataManager.Instantiate(request.RequestContext);
            PagedResult<LabelCode> labelCodes = await dataManager.GetLabelCodes().ConfigureAwait(false);

            return new EntityDataServiceResponse<LabelCode>(labelCodes);
        }

    }
}
