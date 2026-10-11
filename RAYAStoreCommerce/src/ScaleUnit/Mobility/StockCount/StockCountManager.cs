using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;

using Microsoft.Dynamics.Commerce.Runtime;
//using Microsoft.Dynamics.Commerce.Runtime.Client;
using Microsoft.Dynamics.Commerce.Runtime.DataModel;
using Microsoft.Dynamics.Commerce.Runtime.Messages;
using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;

namespace RR.Commerce.Runtime.Mobility
{
    public sealed class StockCountManager
    {
        private readonly ICommerceRuntime runtime;

        private StockCountManager(ICommerceRuntime runtime)
        {
            this.runtime = runtime;
        }

        public static StockCountManager Create(ICommerceRuntime runtime)
        {
            if (runtime == null)
                throw new ArgumentNullException("runtime");
            return new StockCountManager(runtime);
        }

        internal TResponse Execute<TResponse>(Request request) where TResponse : Response
        {
#pragma warning disable CS0618 // Type or member is obsolete
#pragma warning disable AvoidBlockingCallsAnalyzer // Avoid executing requests synchronously.
            return this.runtime.Execute<TResponse>(request, null);
#pragma warning restore AvoidBlockingCallsAnalyzer // Avoid executing requests synchronously.
#pragma warning restore CS0618 // Type or member is obsolete
        }

        //

        public ReadOnlyCollection<StockCountJournal> GetNewJournals()
        {
            return Execute<GetNewStockCountJournalsServiceResponse>(new GetNewStockCountJournalsServiceRequest()).Journals;
        }

        public SCJournal Create(SCJournalRequest journal)
        {
            ThrowIf.Null(journal, "journal");
            return Execute<CreateStockCountJournalServiceResponse>(new CreateStockCountJournalServiceRequest(journal)).Journal;
        }

        public PagedResult<SCJournal> GetJournals(SCJournalStatus status, string keyword, QueryResultSettings settings)
        {
            ThrowIf.Null<QueryResultSettings>(settings, "settings");
            if (settings.Sorting == null || !settings.Sorting.IsSpecified)
                settings.Sorting = new SortingInfo(new SortColumn("JOURNALID", true));
            return Execute<EntityDataServiceResponse<SCJournal>>(new SCJournalSearchDataRequest(status, keyword, settings)).PagedEntityCollection;
        }

        public void Delete(int recordId)
        {
            Execute<NullResponse>(new SCJournalDeleteDataRequest(recordId));
        }

        public int Calculate(int recordId, bool addLines)
        {
            return Execute<SingleEntityDataServiceResponse<int>>(new CalculateStockCountJournalServiceRequest(recordId, addLines)).Entity;
        }

        public int Adjust(int recordId)
        {
            return Execute<SingleEntityDataServiceResponse<int>>(new AdjustStockCountJournalServiceRequest(recordId)).Entity;
        }

        public SCJournal Commit(int recordId)
        {
            return Execute<SingleEntityDataServiceResponse<SCJournal>>(new CommitStockCountJournalServiceRequest(recordId)).Entity;
        }

        public PagedResult<SCJournalTransaction> GetJournalTransactions(int journalId, string keyword, QueryResultSettings settings)
        {
            if (settings == null)
                throw new ArgumentNullException("settings");
            if (settings.Sorting == null || !settings.Sorting.IsSpecified)
                settings.Sorting = new SortingInfo(new SortColumn("ITEMID", true), new SortColumn("INVENTDIMID", true));
            return Execute<EntityDataServiceResponse<SCJournalTransaction>>(new SCJournalTransactionSearchDataRequest(journalId, keyword, settings)).PagedEntityCollection;
        }

        public SCJournalTransaction UpdateJournalTransaction(int recordId, decimal? qtyCount, string comment)
        {
            SCJournalTransaction tran = Execute<SingleEntityDataServiceResponse<SCJournalTransaction>>(new SCJournalTransactionGetDataRequest(recordId)).Entity;
            if (tran == null)
                throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_ValueOutOfRange, String.Format("Invalid Journal Transaction Id {0}", recordId));

            tran.QtyCount = qtyCount;

            if (qtyCount.HasValue)
            {
                tran.CountedDate = DateTime.Now;
            }
            else
            {
                tran.CountedDate = null;
                tran.QtyAdjust = null;
            }

            tran.Comment = comment;

            return Execute<SingleEntityDataServiceResponse<SCJournalTransaction>>(new SCJournalTransactionSaveDataRequest(tran)).Entity;
        }

        public PagedResult<SCJournalEntry> GetJournalEntries(int journalId, string keyword, QueryResultSettings settings)
        {
            if (settings == null)
                throw new ArgumentNullException("settings");
            if (settings.Sorting == null || !settings.Sorting.IsSpecified)
                settings.Sorting = new SortingInfo(new SortColumn("ITEMID", true),
                    new SortColumn("INVENTSIZEID", true),
                    new SortColumn("INVENTCOLORID", true),
                    new SortColumn("INVENTSTYLEID", true),
                    new SortColumn("CONFIGID", true),
                    new SortColumn("INVENTBATCHID", true),
                    new SortColumn("INVENTSERIALID", true),
                    new SortColumn("SECTIONID", true),
                    new SortColumn("SHELFID", true),
                    new SortColumn("DEVICEID", true));
            return Execute<EntityDataServiceResponse<SCJournalEntry>>(new SCJournalEntrySearchDataRequest(journalId, keyword, settings)).PagedEntityCollection;
        }

        public PagedResult<SCJournalEntry> GetJournalEntries(int transId, QueryResultSettings settings)
        {
            if (settings == null)
                throw new ArgumentNullException("settings");
            if (settings.Sorting == null || !settings.Sorting.IsSpecified)
                settings.Sorting = new SortingInfo(new SortColumn("SECTIONID", true), new SortColumn("SHELFID", true), new SortColumn("DEVICEID", true));
            return Execute<EntityDataServiceResponse<SCJournalEntry>>(new SCJournalEntrySearchDataRequest(transId, settings)).PagedEntityCollection;
        }

        public void DeleteJournalEntries(IEnumerable<int> recordIds)
        {
            ThrowIf.Null(recordIds, "recordIds");
            foreach (int recordId in recordIds)
            {
                SCJournalEntry entry = Execute<SingleEntityDataServiceResponse<SCJournalEntry>>(new SCJournalEntryGetDataRequest(recordId)).Entity;
                if (entry != null)
                {
                    if (entry.Status == SCJournalEntryStatus.Counted)
                        throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_ValueOutOfRange, String.Format("Journal Entry Id {0} Status must not be Counted", recordId));
                    Execute<NullResponse>(new SCJournalEntryDeleteDataRequest(recordId));
                }
            }
        }
    }
}
