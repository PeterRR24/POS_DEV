using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Dynamics.Commerce.Runtime;
using Microsoft.Dynamics.Commerce.Runtime.Data;
using Microsoft.Dynamics.Commerce.Runtime.DataModel;
using Microsoft.Dynamics.Commerce.Runtime.Messages;
using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
using Microsoft.Dynamics.Commerce.Runtime.RealtimeServices.Messages;

using RR.Commerce.Runtime.Utilities;

namespace RR.Commerce.Runtime.Mobility
{
    public class StockCountService : IRequestHandlerAsync
    {
        public IEnumerable<Type> SupportedRequestTypes
        {
            get
            {
                return new[]
                {
                    typeof(GetNewStockCountJournalsServiceRequest),
                    typeof(CreateStockCountJournalServiceRequest),
                    typeof(CalculateStockCountJournalServiceRequest),
                    typeof(AdjustStockCountJournalServiceRequest),
                    typeof(CommitStockCountJournalServiceRequest)
                };
            }
        }

        public async Task<Response> Execute(Request request)
        {
            ThrowIf.Null(request, "request");

            Type requestType = request.GetType();
            Response response;

            if (requestType == typeof(GetNewStockCountJournalsServiceRequest))
                response = await this.GetNewStockCountJournals((GetNewStockCountJournalsServiceRequest)request).ConfigureAwait(false);
            else if (requestType == typeof(CreateStockCountJournalServiceRequest))
                response = await this.CreateStockCountJournal((CreateStockCountJournalServiceRequest)request).ConfigureAwait(false);
            else if (requestType == typeof(CalculateStockCountJournalServiceRequest))
                response = await this.CalculateStockCountJournal((CalculateStockCountJournalServiceRequest)request).ConfigureAwait(false);
            else if (requestType == typeof(AdjustStockCountJournalServiceRequest))
                response = await this.AdjustStockCountJournal((AdjustStockCountJournalServiceRequest)request).ConfigureAwait(false);
            else if (requestType == typeof(CommitStockCountJournalServiceRequest))
                response = await this.CommitStockCountJournal((CommitStockCountJournalServiceRequest)request).ConfigureAwait(false);
            else
                throw new NotSupportedException(String.Format("Request '{0}' is not supported.", request.GetType()));

            return response;
        }

        private async Task<GetNewStockCountJournalsServiceResponse> GetNewStockCountJournals(GetNewStockCountJournalsServiceRequest request)
        {
            ThrowIf.Null(request, "request");

            ChannelConfiguration channelConfiguration = request.RequestContext.GetChannelConfiguration();

            GetStockCountJournalsRealtimeRequest stockCountJournalsRequest = new GetStockCountJournalsRealtimeRequest(channelConfiguration.InventLocation);
            ReadOnlyCollection<StockCountJournal> axJournals = (await request.RequestContext.ExecuteAsync<EntityDataServiceResponse<StockCountJournal>>(stockCountJournalsRequest).ConfigureAwait(false)).PagedEntityCollection.Results;
            //TODO - may we need to use this in future 
            //var syncStockCountJournalsFromAxServiceRequest = new Microsoft.Dynamics.Commerce.Runtime.Services.Messages.SyncStockCountJournalsFromAxServiceRequest();
            //var syncStockCountJournalsFromAxServiceResponse = request.RequestContext.Execute<Microsoft.Dynamics.Commerce.Runtime.Services.Messages.SyncStockCountJournalsFromAxServiceResponse>(syncStockCountJournalsFromAxServiceRequest);
            //var syncStockCountResponse = syncStockCountJournalsFromAxServiceResponse.StockCountJournals;

            List<StockCountJournal> journals = new List<StockCountJournal>();

            foreach (StockCountJournal axJournal in axJournals)
            {
                //TODO - review
                SCJournal journal = (await request.RequestContext.ExecuteAsync<SingleEntityDataServiceResponse<SCJournal>>(new SCJournalGetDataRequest(axJournal.JournalId)).ConfigureAwait(false)).Entity;
                if (journal == null)
                    journals.Add(axJournal);
            }

            return new GetNewStockCountJournalsServiceResponse(journals);
        }

        private async Task<CreateStockCountJournalServiceResponse> CreateStockCountJournal(CreateStockCountJournalServiceRequest request)
        {
            ThrowIf.Null(request, "request");
            ThrowIf.Null(request.Journal, "request.Journal");

            ChannelConfiguration channelConfiguration = request.RequestContext.GetChannelConfiguration();
            OrgUnit orgUnit = request.RequestContext.GetOrgUnit();

            SCJournal journal = (await request.RequestContext.ExecuteAsync<SingleEntityDataServiceResponse<SCJournal>>(new SCJournalGetDataRequest(request.Journal.JournalId)).ConfigureAwait(false)).Entity;
            if (journal != null)
                throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_ValueOutOfRange, String.Format("Duplicate Journal Id {0}", request.Journal.JournalId));

            ReadOnlyCollection<StockCountJournalTransaction> axJournalTrans = await GetStockCountJournalTransFromAx(request.RequestContext, request.Journal.JournalId, channelConfiguration.InventLocation).ConfigureAwait(false);
            if (axJournalTrans == null || axJournalTrans.Count == 0)
                throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidRequest, String.Format("Journal {0} is empty", request.Journal.JournalId));

            journal = new SCJournal(request.Journal, orgUnit.OrgUnitNumber, channelConfiguration.InventLocation);

            journal = (await request.RequestContext.ExecuteAsync<SingleEntityDataServiceResponse<SCJournal>>(new SCJournalCreateDataRequest(journal, axJournalTrans)).ConfigureAwait(false)).Entity;

            return new CreateStockCountJournalServiceResponse(journal);
        }

        private static async Task<ReadOnlyCollection<StockCountJournalTransaction>> GetStockCountJournalTransFromAx(RequestContext context, string journalId, string inventLocation)
        {
            //TODO - may we need to use this in future 
            //var syncStockCountTransactionsFromAxServiceRequest = new Microsoft.Dynamics.Commerce.Runtime.Services.Messages.SyncStockCountTransactionsFromAxServiceRequest()
            //{
            //    JournalId = journalId
            //};
            //var syncStockCountTransactionsFromAxServiceResponse = context.Execute<Microsoft.Dynamics.Commerce.Runtime.Services.Messages.SyncStockCountTransactionsFromAxServiceResponse>(syncStockCountTransactionsFromAxServiceRequest);
            //var syncStockCountResponse = syncStockCountTransactionsFromAxServiceResponse.StockCountJournalTransactions;

            GetStockCountJournalTransactionsRealtimeRequest journalTransactionsRequest = new GetStockCountJournalTransactionsRealtimeRequest(journalId, inventLocation);
            ReadOnlyCollection<StockCountJournalTransaction> journalTransactions = (await context.ExecuteAsync<EntityDataServiceResponse<StockCountJournalTransaction>>(journalTransactionsRequest).ConfigureAwait(false)).PagedEntityCollection.Results;
            return journalTransactions;
        }

        private async Task<SingleEntityDataServiceResponse<int>> CalculateStockCountJournal(CalculateStockCountJournalServiceRequest request)
        {
            ThrowIf.Null(request, "request");

            int calcCount = 0;

            ReadOnlyCollection<StockCountJournalBuffer> entries = (await request.RequestContext.ExecuteAsync<SCJournalBufferCalcDataResponse>(new SCJournalBufferCalcDataRequest(request.JournalId)).ConfigureAwait(false)).Entries;
            foreach (StockCountJournalBuffer entry in entries)
            {
                SCJournalTransaction tran = (await request.RequestContext.ExecuteAsync<SingleEntityDataServiceResponse<SCJournalTransaction>>(new SCJournalTransactionFindDataRequest(request.JournalId, entry)).ConfigureAwait(false)).Entity;
                if (tran == null)
                {
                    if (request.AddLines)
                    {
                        Item item = (await request.RequestContext.ExecuteAsync<GetItemsDataResponse>(new GetItemsDataRequest(new string[] { entry.ItemId })).ConfigureAwait(false)).Items.FirstOrDefault();
                        if (item != null)
                        {
                            entry.ItemName = item.Name;
                            entry.Status = SCJournalEntryStatus.Counted;

                            calcCount++;
                        }
                        else
                            entry.Status = SCJournalEntryStatus.Missing;
                    }
                    else
                        entry.Status = SCJournalEntryStatus.Skipped;
                }
                else
                {
                    entry.TransactionId = tran.Id;
                    entry.Status = SCJournalEntryStatus.Counted;

                    calcCount++;
                }
            }
           await request.RequestContext.ExecuteAsync<NullResponse>(new SCJournalBufferCommitDataRequest(request.JournalId, entries)).ConfigureAwait(false);

            return new SingleEntityDataServiceResponse<int>(calcCount);
        }

        private async Task<SingleEntityDataServiceResponse<int>> AdjustStockCountJournal(AdjustStockCountJournalServiceRequest request)
        {
            ThrowIf.Null(request, "request");

            ChannelConfiguration channelConfiguration = request.RequestContext.GetChannelConfiguration();
            int ajustCount = 0;
            decimal? newQtyAdjust;

            ReadOnlyCollection<SCJournalTransaction> trans = (await request.RequestContext.ExecuteAsync<EntityDataServiceResponse<SCJournalTransaction>>(new SCJournalTransactionSearchDataRequest(request.JournalId, true)).ConfigureAwait(false)).PagedEntityCollection.Results;
            foreach (SCJournalTransaction tran in trans)
            {
                string variantId = null;
                if (!String.IsNullOrEmpty(tran.ConfigId) || !String.IsNullOrEmpty(tran.InventSizeId) || !String.IsNullOrEmpty(tran.InventColorId) || !String.IsNullOrEmpty(tran.InventStyleId))
                {
                    variantId = (await request.RequestContext.ExecuteAsync<SingleEntityDataServiceResponse<string>>(
                        new GetVariantIdForItemDataRequest(tran.ItemId, tran.InventSizeId, tran.InventColorId, tran.InventStyleId, tran.ConfigId)).ConfigureAwait(false)).Entity;
                }

                DateTime tranDate = tran.CountedDate.Value.Date;
                int tranTime = tran.CountedDate.Value.Hour * 60 * 60 + tran.CountedDate.Value.Minute * 60 + tran.CountedDate.Value.Second;

                decimal sales = (await request.RequestContext.ExecuteAsync<SingleEntityDataServiceResponse<decimal>>(
                        new SCJournalTransactionSalesDataRequest(channelConfiguration.InventLocation, tran.ItemId, (variantId ?? ""), tran.InventBatchId, tran.InventSerialId, tranDate, tranTime)).ConfigureAwait(false)).Entity;

                if (sales == 0)
                    newQtyAdjust = null;
                else
                    newQtyAdjust = sales;

                if (tran.QtyAdjust != newQtyAdjust)
                {
                    tran.QtyAdjust = newQtyAdjust;

                    await request.RequestContext.ExecuteAsync<SingleEntityDataServiceResponse<SCJournalTransaction>>(new SCJournalTransactionSaveDataRequest(tran)).ConfigureAwait(false);

                    ajustCount++;
                }
            }

            return new SingleEntityDataServiceResponse<int>(ajustCount);
        }

        private async Task<SingleEntityDataServiceResponse<SCJournal>> CommitStockCountJournal(CommitStockCountJournalServiceRequest request)
        {
            ThrowIf.Null(request, "request");

            SCJournal journal = (await request.RequestContext.ExecuteAsync<SingleEntityDataServiceResponse<SCJournal>>(new SCJournalGetDataRequest(request.JournalId)).ConfigureAwait(false)).Entity;
            if (journal == null)
                throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_ValueOutOfRange, String.Format("Invalid Journal Id {0}", request.JournalId));
            if (journal.Status != SCJournalStatus.Open)
                throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_ValueOutOfRange, String.Format("Journal {0} Status must be open", request.JournalId));

#if !DEBUG
            await Mobilityicense.Check(request.RequestContext).ConfigureAwait(false);
#endif

            StockCountJournal axJournal = journal.ToModel(request.RequestContext.GetChannelConfiguration().InventLocationDataAreaId, request.RequestContext.GetPrincipal().UserId);

            ReadOnlyCollection<SCJournalTransaction> trans = (await request.RequestContext.ExecuteAsync<EntityDataServiceResponse<SCJournalTransaction>>(new SCJournalTransactionSearchDataRequest(journal.Id, false)).ConfigureAwait(false)).PagedEntityCollection.Results;
            foreach (SCJournalTransaction tran in trans)
            {
                StockCountJournalTransaction axTran = tran.ToModel();

                bool deleted = false;
                if (tran.QtyCount.HasValue)
                {
                    if (tran.QtyAdjust.HasValue)
                        axTran.Counted = tran.QtyCount.Value - tran.QtyAdjust.Value;
                    else
                        axTran.Counted = tran.QtyCount.Value;
                }
                else
                {
                    axTran.Counted = 0M;
                    deleted = !((journal.CalcMode == SCJournalCalculation.EmptyAsDiscrepant) || (journal.CalcMode == SCJournalCalculation.NegativeAsDiscrepant || tran.Quantity < 0));
                }
                axTran.SetProperty("Deleted", deleted);

                axJournal.StockCountTransactionLines.Add(axTran);
            }

            StockCountJournal commitedStockCounts = (await request.RequestContext.ExecuteAsync<SingleEntityDataServiceResponse<StockCountJournal>>(new CommitStockCountRealtimeRequest(axJournal)).ConfigureAwait(false)).Entity;

            journal.Status = SCJournalStatus.Committed;
            journal.CommitDate = DateTime.Now;
            journal.StaffId = axJournal.Worker;

            return await request.RequestContext.ExecuteAsync<SingleEntityDataServiceResponse<SCJournal>>(new SCJournalSaveDataRequest(journal)).ConfigureAwait(false);
        }
    }
}
