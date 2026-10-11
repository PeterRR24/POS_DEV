using System;
using System.Xml;
using System.Text;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml.Linq;
using System.Threading.Tasks;

using Microsoft.Dynamics.Commerce.Runtime;
using Microsoft.Dynamics.Commerce.Runtime.Data;
using Microsoft.Dynamics.Commerce.Runtime.DataModel;
using Microsoft.Dynamics.Commerce.Runtime.Messages;
using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
using Microsoft.Dynamics.Commerce.Runtime.RealtimeServices.Messages;
//using Microsoft.Dynamics.Commerce.Runtime.TransactionService;

using RR.Commerce.Runtime.Utilities;

namespace RR.Commerce.Runtime.Mobility
{
    public class MobilityRealtimeService : IRequestHandlerAsync
    {
        public IEnumerable<Type> SupportedRequestTypes
        {
            get
            {
                return new[]
                {
                    typeof(CommitRangingRealtimeRequest),
                    typeof(CommitStockCountRealtimeRequest)
                };
            }
        }

        public async Task<Response> Execute(Request request)
        {
            ThrowIf.Null(request, "request");

            Type requestType = request.GetType();
            Response response;

            if (requestType == typeof(CommitRangingRealtimeRequest))
                response = await this.CommitRanging((CommitRangingRealtimeRequest)request).ConfigureAwait(false);
            else if (requestType == typeof(CommitStockCountRealtimeRequest))
                response = await this.CommitStockCount((CommitStockCountRealtimeRequest)request).ConfigureAwait(false);
            else
                throw new NotSupportedException(String.Format("Request '{0}' is not supported.", request.GetType()));

            return response;
        }

        private async Task<NullResponse> CommitRanging(CommitRangingRealtimeRequest request)
        {
            ThrowIf.Null(request, "request");
            ThrowIf.NullOrWhiteSpace(request.SectionId, "request.SectionId");
            ThrowIf.NullOrWhiteSpace(request.ShelfId, "request.ShelfId");

            int position = 0;

            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("<Ranging");
            stringBuilder.Append(" StoreId=\"" + request.StoreId + "\"");
            stringBuilder.Append(" SectionId=\"" + request.SectionId + "\"");
            stringBuilder.Append(" ShelfId=\"" + request.ShelfId + "\"");
            stringBuilder.Append(">");

            foreach (RangingItem item in request.Items)
            {
                position++;

                stringBuilder.Append("<Item");
                stringBuilder.Append(" Id=\"" + item.ItemId + "\"");
                stringBuilder.Append(" Order=\"" + position + "\"");
                stringBuilder.Append(">");
                stringBuilder.Append("</Item>");
            }

            stringBuilder.Append("</Ranging>");

            //TransactionServiceClient transactionClient = new TransactionServiceClient(request.RequestContext);
            //transactionClient.InvokeExtensionMethod("createItemSectionLocation", stringBuilder.ToString());

            InvokeExtensionMethodRealtimeRequest extensionRequest = new InvokeExtensionMethodRealtimeRequest("createItemSectionLocation", stringBuilder.ToString());
            await request.RequestContext.ExecuteAsync<InvokeExtensionMethodRealtimeResponse>(extensionRequest).ConfigureAwait(false);

            return NullResponse.Instance; //   return new NullResponse();
        }

        private async Task<SingleEntityDataServiceResponse<StockCountJournal>> CommitStockCount(CommitStockCountRealtimeRequest request)
        {
            ThrowIf.Null(request, "request");
            ThrowIf.Null(request.Journal, "request.Journal");

            StockCountJournal journal = request.Journal;

            string xmlJournal = ToXml(journal);

            //TransactionServiceClient transactionClient = new TransactionServiceClient(request.RequestContext);
            //transactionClient.InvokeExtensionMethod("commitStockCount", xmlJournal);

            InvokeExtensionMethodRealtimeRequest extensionRequest = new InvokeExtensionMethodRealtimeRequest("commitStockCount", xmlJournal);
            await request.RequestContext.ExecuteAsync<InvokeExtensionMethodRealtimeResponse>(extensionRequest).ConfigureAwait(false);

            journal.StockCountTransactionLines.Clear();

            return new SingleEntityDataServiceResponse<StockCountJournal>(journal);
        }

        private static string ToXml(StockCountJournal journal)
        {
            ThrowIf.Null(journal, "journal");

            StringBuilder strOutput = new StringBuilder();
            strOutput.Append("<InventJournalTable");
            strOutput.Append(" JournalId=\"" + journal.JournalId + "\"");
            strOutput.Append(" RecId=\"" + journal.RecordId + "\"");
            strOutput.Append(" Description=\"" + journal.Description + "\"");
            strOutput.Append(" PersonnelNumber=\"" + journal.Worker + "\"");
            strOutput.Append(" InventLocationId=\"" + journal.InventLocationId  + "\"");
            strOutput.Append(" DataAreaId=\"" + journal.DataAreaId + "\"");
            strOutput.Append(">");

            foreach (StockCountJournalTransaction line in journal.StockCountTransactionLines)
            {
                strOutput.Append(ToXml(line));
            }

            strOutput.Append("</InventJournalTable>");

            return strOutput.ToString();
        }

        private static string ToXml(StockCountJournalTransaction journalTransaction)
        {
            ThrowIf.Null(journalTransaction, "journalTransaction");

            bool deleted = (bool)journalTransaction.GetProperty("Deleted");

            XElement inventJournalTrans = new XElement(
                    "InventJournalTrans",
                    new XAttribute("RecId", journalTransaction.RecordId),
                    new XAttribute("ItemId", journalTransaction.ItemId ?? string.Empty),
                    new XAttribute("EcoResProductName", journalTransaction.ItemName ?? string.Empty),
                    new XAttribute("InventDimId", journalTransaction.InventDimId ?? string.Empty),
                    new XAttribute("Counted", journalTransaction.Counted),
                    new XAttribute("Deleted", deleted ? 1 : 0),
                    new XAttribute("ConfigId", journalTransaction.ConfigId ?? string.Empty),
                    new XAttribute("InventSizeId", journalTransaction.InventSizeId ?? string.Empty),
                    new XAttribute("InventColorId", journalTransaction.InventColorId ?? string.Empty),
                    new XAttribute("InventStyleId", journalTransaction.InventStyleId ?? string.Empty),
                    new XAttribute("InventBatchId", journalTransaction.InventBatchId ?? string.Empty),
                    new XAttribute("InventSerialId", journalTransaction.InventSerialId ?? string.Empty));

            // new XAttribute("WmsLocationId", journalTransaction.WarehouseLocationId ?? string.Empty),
            // new XAttribute("WmsPalletId", journalTransaction.WarehousePalletId ?? string.Empty),
            // new XAttribute("InventSiteId", journalTransaction.InventSiteId ?? string.Empty),
            // new XAttribute("InventLocationId", journalTransaction.InventLocationId ?? string.Empty),
            // new XAttribute("Guid", journalTransaction.TrackingGuid)

            return inventJournalTrans.ToString();
        }
    }
}
