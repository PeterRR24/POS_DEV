using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

using Microsoft.Dynamics.Commerce.Runtime;
using Microsoft.Dynamics.Commerce.Runtime.DataModel;
using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
using Microsoft.Dynamics.Commerce.Runtime.Hosting.Contracts;
using Microsoft.Dynamics.Commerce.Runtime.Messages;

using RR.Commerce.Runtime.Mobility;

namespace RR.Retail.RetailServerLibrary
{
    [RoutePrefix("StockCount")]
    [BindEntity(typeof(SCJournal))]
    public class StockCountController : IController
    {
        //private StockCountManager stockCountManager;

        //public override string ControllerName
        //{
        //    get { return "StockCount"; }
        //}

        //[Authorization(CommerceRoles.Anonymous, CommerceRoles.Employee)]
        //protected override void DeleteEntity(IEndpointContext context, [EntityKey]int key)
        //{
        //    this.stockCountManager.Delete(key);
        //}

        [HttpPost]
        [Authorization(CommerceRoles.Anonymous, CommerceRoles.Employee)]
        public async Task<NullResponse> DeleteJournal(IEndpointContext context, [EntityKey]int key) // override DeleteEntity
        {
            //this.stockCountManager.Delete(key);
            return await context.ExecuteAsync<NullResponse>(new SCJournalDeleteDataRequest(key)).ConfigureAwait(false);
        }

        [HttpPost]
        [Authorization(CommerceRoles.Anonymous, CommerceRoles.Employee)]
        public async Task<PagedResult<StockCountJournal>> GetNewJournals(IEndpointContext context)
        {
            //return this.stockCountManager.GetNewJournals();
            ReadOnlyCollection<StockCountJournal> Journals = (await context.ExecuteAsync<GetNewStockCountJournalsServiceResponse>(new GetNewStockCountJournalsServiceRequest()).ConfigureAwait(false)).Journals;
            return Journals.AsPagedResult();
        }

        [HttpPost]
        [Authorization(CommerceRoles.Anonymous, CommerceRoles.Employee)]
        public async Task<SCJournal> Import(IEndpointContext context, SCJournalRequest journal)
        {
            //ThrowIf.Null<ODataActionParameters>(parameters, "parameters");
            //SCJournalRequest journal = (SCJournalRequest)parameters["journal"];
            //ThrowIf.Null<SCJournalRequest>(journal, "parameters.journal");
            //return this.stockCountManager.Create(journal);
            ThrowIf.Null(journal, "journal");
            return (await context.ExecuteAsync<CreateStockCountJournalServiceResponse>(new CreateStockCountJournalServiceRequest(journal)).ConfigureAwait(false)).Journal;
        }

        [HttpGet]
        [Authorization(CommerceRoles.Anonymous, CommerceRoles.Employee)]
        public async Task<PagedResult<SCJournal>> Search(IEndpointContext context, int status, string searchText, QueryResultSettings queryResultSettings)
        {
            //QueryResultSettings queryResultSettings = this.GetQueryResultSettings();
            //return this.ProcessPagedResults<SCJournal>(this.stockCountManager.GetJournals((SCJournalStatus)status, searchText, queryResultSettings));
            //QueryResultSettings settings = QueryResultSettings.AllRecords;
            //settings.Paging = new PagingInfo(50);
            if (queryResultSettings.Sorting == null || !queryResultSettings.Sorting.IsSpecified)
                queryResultSettings.Sorting = new SortingInfo(new SortColumn("JOURNALID", true));
            return (await context.ExecuteAsync<EntityDataServiceResponse<SCJournal>>(new SCJournalSearchDataRequest((SCJournalStatus)status, searchText, queryResultSettings)).ConfigureAwait(false)).PagedEntityCollection;
        }

        [HttpPost]
        [Authorization(CommerceRoles.Anonymous, CommerceRoles.Employee)]
        public async Task<int> Calculate(IEndpointContext context, int recordId, bool addLines)
        {
            //ThrowIf.Null<ODataActionParameters>(parameters, "parameters");
            //return this.stockCountManager.Calculate((int)parameters["recordId"], (bool)parameters["addLines"]);
            return (await context.ExecuteAsync<SingleEntityDataServiceResponse<int>>(new CalculateStockCountJournalServiceRequest(recordId, addLines)).ConfigureAwait(false)).Entity;
        }

        [HttpPost]
        [Authorization(CommerceRoles.Anonymous, CommerceRoles.Employee)]
        public async Task<int> Adjust(IEndpointContext context, int recordId)
        {
            //ThrowIf.Null<ODataActionParameters>(parameters, "parameters");
            //return this.stockCountManager.Adjust((int)parameters["recordId"]);
            return (await context.ExecuteAsync<SingleEntityDataServiceResponse<int>>(new AdjustStockCountJournalServiceRequest(recordId)).ConfigureAwait(false)).Entity;
        }

        [HttpPost]
        [Authorization(CommerceRoles.Anonymous, CommerceRoles.Employee)]
        public async Task<SCJournal> Apply(IEndpointContext context, int recordId)
        {
            //ThrowIf.Null<ODataActionParameters>(parameters, "parameters");
            //return this.stockCountManager.Commit((int)parameters["recordId"]);
            return (await context.ExecuteAsync<SingleEntityDataServiceResponse<SCJournal>>(new CommitStockCountJournalServiceRequest(recordId)).ConfigureAwait(false)).Entity;
        }

        //

        [HttpGet]
        [Authorization(CommerceRoles.Anonymous, CommerceRoles.Employee)]
        public async Task<PagedResult<SCJournalTransaction>> SearchTransactions(IEndpointContext context, [EntityKey]int key, string searchText, QueryResultSettings queryResultSettings)
        {
            //QueryResultSettings queryResultSettings = this.GetQueryResultSettings();
            //return this.ProcessPagedResults<SCJournalTransaction>(this.stockCountManager.GetJournalTransactions(key, searchText, queryResultSettings));
            //QueryResultSettings settings = QueryResultSettings.AllRecords;
            //settings.Paging = new PagingInfo(50);
            if (queryResultSettings.Sorting == null || !queryResultSettings.Sorting.IsSpecified)
                queryResultSettings.Sorting = new SortingInfo(new SortColumn("ITEMID", true), new SortColumn("INVENTDIMID", true));
            return (await context.ExecuteAsync<EntityDataServiceResponse<SCJournalTransaction>>(new SCJournalTransactionSearchDataRequest(key, searchText, queryResultSettings)).ConfigureAwait(false)).PagedEntityCollection;
        }

        [HttpPost]
        [Authorization(CommerceRoles.Anonymous, CommerceRoles.Employee)]
        public async Task<SCJournalTransaction> CountTransaction(IEndpointContext context, int recordId, string comment, decimal qtyCount)
        {
            //ThrowIf.Null<ODataActionParameters>(parameters, "parameters");
            //return this.stockCountManager.UpdateJournalTransaction((int)parameters["recordId"], (decimal)parameters["qtyCount"], (string)parameters["comment"]);
            SCJournalTransaction tran = (await context.ExecuteAsync<SingleEntityDataServiceResponse<SCJournalTransaction>>(new SCJournalTransactionGetDataRequest(recordId)).ConfigureAwait(false)).Entity;
            if (tran == null)
                throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_ValueOutOfRange, String.Format("Invalid Journal Transaction Id {0}", recordId));

            tran.QtyCount = qtyCount;
            tran.CountedDate = DateTime.Now;
            tran.Comment = comment;

            return (await context.ExecuteAsync<SingleEntityDataServiceResponse<SCJournalTransaction>>(new SCJournalTransactionSaveDataRequest(tran)).ConfigureAwait(false)).Entity;
        }

        [HttpPost]
        [Authorization(CommerceRoles.Anonymous, CommerceRoles.Employee)]
        public async Task<SCJournalTransaction> ClearTransaction(IEndpointContext context, int recordId, string comment)
        {
            //ThrowIf.Null<ODataActionParameters>(parameters, "parameters");
            //return this.stockCountManager.UpdateJournalTransaction((int)parameters["recordId"], null, (string)parameters["comment"]);
            SCJournalTransaction tran = (await context.ExecuteAsync<SingleEntityDataServiceResponse<SCJournalTransaction>>(new SCJournalTransactionGetDataRequest(recordId)).ConfigureAwait(false)).Entity;
            if (tran == null)
                throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_ValueOutOfRange, String.Format("Invalid Journal Transaction Id {0}", recordId));

            tran.QtyCount = null;
            tran.CountedDate = null;
            tran.QtyAdjust = null;
            tran.Comment = comment;

            return (await context.ExecuteAsync<SingleEntityDataServiceResponse<SCJournalTransaction>>(new SCJournalTransactionSaveDataRequest(tran)).ConfigureAwait(false)).Entity;
        }

        //

        [HttpGet]
        [Authorization(CommerceRoles.Anonymous, CommerceRoles.Employee)]
        public async Task<PagedResult<SCJournalEntry>> SearchEntries(IEndpointContext context, [EntityKey]int key, string searchText, QueryResultSettings queryResultSettings)
        {
            //QueryResultSettings queryResultSettings = this.GetQueryResultSettings();
            //return this.ProcessPagedResults<SCJournalEntry>(this.stockCountManager.GetJournalEntries(key, searchText, queryResultSettings));
            //QueryResultSettings settings = QueryResultSettings.AllRecords;
            //settings.Paging = new PagingInfo(50);
            if (queryResultSettings.Sorting == null || !queryResultSettings.Sorting.IsSpecified)
                queryResultSettings.Sorting = new SortingInfo(new SortColumn("ITEMID", true),
                    new SortColumn("INVENTSIZEID", true),
                    new SortColumn("INVENTCOLORID", true),
                    new SortColumn("INVENTSTYLEID", true),
                    new SortColumn("CONFIGID", true),
                    new SortColumn("INVENTBATCHID", true),
                    new SortColumn("INVENTSERIALID", true),
                    new SortColumn("SECTIONID", true),
                    new SortColumn("SHELFID", true),
                    new SortColumn("DEVICEID", true));
            return (await context.ExecuteAsync<EntityDataServiceResponse<SCJournalEntry>>(new SCJournalEntrySearchDataRequest(key, searchText, queryResultSettings)).ConfigureAwait(false)).PagedEntityCollection;
        }

        [HttpGet]
        [Authorization(CommerceRoles.Anonymous, CommerceRoles.Employee)]
        public async Task<PagedResult<SCJournalEntry>> GetEntries(IEndpointContext context, int transId, QueryResultSettings queryResultSettings)
        {
            //QueryResultSettings queryResultSettings = this.GetQueryResultSettings();
            //return this.ProcessPagedResults<SCJournalEntry>(this.stockCountManager.GetJournalEntries(transId, queryResultSettings));
            //QueryResultSettings settings = QueryResultSettings.AllRecords;
            //settings.Paging = new PagingInfo(50);
            if (queryResultSettings.Sorting == null || !queryResultSettings.Sorting.IsSpecified)
                queryResultSettings.Sorting = new SortingInfo(new SortColumn("SECTIONID", true), new SortColumn("SHELFID", true), new SortColumn("DEVICEID", true));
            return (await context.ExecuteAsync<EntityDataServiceResponse<SCJournalEntry>>(new SCJournalEntrySearchDataRequest(transId, queryResultSettings)).ConfigureAwait(false)).PagedEntityCollection;
        }

        [HttpPost]
        [Authorization(CommerceRoles.Anonymous, CommerceRoles.Employee)]
        public async Task<NullResponse> DeleteEntries(IEndpointContext context, IEnumerable<int> recordIds)
        {
            //ThrowIf.Null<ODataActionParameters>(parameters, "parameters");
            //this.stockCountManager.DeleteJournalEntries((IEnumerable<int>)parameters["recordIds"]);
            ThrowIf.Null(recordIds, "recordIds");
            foreach (int recordId in recordIds)
            {
                SCJournalEntry entry = (await context.ExecuteAsync<SingleEntityDataServiceResponse<SCJournalEntry>>(new SCJournalEntryGetDataRequest(recordId)).ConfigureAwait(false)).Entity;
                if (entry != null)
                {
                    if (entry.Status == SCJournalEntryStatus.Counted)
                        throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_ValueOutOfRange, String.Format("Journal Entry Id {0} Status must not be Counted", recordId));
                    await context.ExecuteAsync<NullResponse>(new SCJournalEntryDeleteDataRequest(recordId)).ConfigureAwait(false);
                }
            }
            return NullResponse.Instance; //   return new NullResponse();
        }

        //

        //protected override void Initialize(HttpControllerContext controllerContext)
        //{
        //    base.Initialize(controllerContext);
        //    this.stockCountManager = StockCountManager.Create(this.CommerceRuntime);
        //}

        //protected override int GetKey(SCJournal entity)
        //{
        //    ThrowIf.Null<SCJournal>(entity, "entity");
        //    return entity.Id;
        //}
    }
}
