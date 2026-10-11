
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.Dynamics.Commerce.Runtime;
using Microsoft.Dynamics.Commerce.Runtime.DataModel;
using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
using Microsoft.Dynamics.Commerce.Runtime.Hosting.Contracts;

using RR.Commerce.Runtime.Mobility;
//using RR.MobilityAuth.Helper;

namespace RR.Retail.RetailServerLibrary
{
    [RoutePrefix("StockTake")]
    [BindEntity(typeof(StockTakeJournal))]
    public class StockTakeController : IController
    {
        //private StockTakeManager stockTakeManager;

        //public override string ControllerName
        //{
        //    get { return "StockTake"; }
        //}

        //protected override void Initialize(HttpControllerContext controllerContext)
        //{
        //    base.Initialize(controllerContext);
        //    MobilityAuthentication.Initialize(controllerContext, this.CommerceRuntime);
        //    this.stockTakeManager = StockTakeManager.Create(this.CommerceRuntime);
        //}

        //protected override int GetKey(StockTakeJournal entity)
        //{
        //    ThrowIf.Null<StockTakeJournal>(entity, "entity");
        //    return entity.Id;
        //}

        public StockTakeController()
        {
        }

        //[Authorization(CommerceRoles.Employee)]
        //public override PagedResult<StockTakeJournal> Get()
        //{
        //    return this.ProcessPagedResults<StockTakeJournal>(this.stockTakeManager.GetJournals(this.GetQueryResultSettings()));
        //}

        [HttpPost]
        [Authorization(CommerceRoles.Employee, CommerceRoles.Anonymous)]
        public async Task<PagedResult<StockTakeJournal>> GetJournals(IEndpointContext context, string mToken, QueryResultSettings queryResultSettings)  // override Get
        {
            //return this.ProcessPagedResults<StockTakeJournal>(this.stockTakeManager.GetJournals(this.GetQueryResultSettings()));
            //QueryResultSettings settings = QueryResultSettings.AllRecords;
            //settings.Paging = new PagingInfo(50);
            if (queryResultSettings.Sorting == null || !queryResultSettings.Sorting.IsSpecified)
                queryResultSettings.Sorting = new SortingInfo(new SortColumn("JOURNALID", true));
            await MobilityAuthentication(context, mToken).ConfigureAwait(false);
            return (await context.ExecuteAsync<EntityDataServiceResponse<StockTakeJournal>>(new GetStockTakeJournalsServiceRequest(queryResultSettings)).ConfigureAwait(false)).PagedEntityCollection;
        }

        [HttpPost]
        [Authorization(CommerceRoles.Employee, CommerceRoles.Anonymous)]
        public async Task<StockTakeItem> GetItem(IEndpointContext context, string mToken, StockTakeRequest request)
        {
            //ThrowIf.Null<ODataActionParameters>(parameters, "parameters");
            //return this.stockTakeManager.GetItem((StockTakeRequest)parameters["request"]);
            ThrowIf.Null<StockTakeRequest>(request, "request");
            ThrowIf.NullOrWhiteSpace(request.BarcodeId, "request.BarcodeId");
            await MobilityAuthentication(context, mToken).ConfigureAwait(false);
            return (await context.ExecuteAsync<SingleEntityDataServiceResponse<StockTakeItem>>(new GetStockTakeItemServiceRequest(request)).ConfigureAwait(false)).Entity;
        }

        [HttpPost]
        [Authorization(CommerceRoles.Employee, CommerceRoles.Anonymous)]
        public async Task<decimal> CountItem(IEndpointContext context, string mToken, StockTakeItem item, bool addMode)
        {
            //ThrowIf.Null<ODataActionParameters>(parameters, "parameters");
            //return this.stockTakeManager.CountItem((StockTakeItem)parameters["item"], (bool)parameters["addMode"]);
            ThrowIf.Null<StockTakeItem>(item, "item");
            await MobilityAuthentication(context, mToken).ConfigureAwait(false);
            return (await context.ExecuteAsync<SingleEntityDataServiceResponse<decimal>>(new CountStockTakeItemServiceRequest(item, addMode)).ConfigureAwait(false)).Entity;
        }

        // Offline StockTake >>
        [HttpPost]
        [Authorization(CommerceRoles.Employee, CommerceRoles.Anonymous)]
        public async Task<decimal> CountOfflineItems(IEndpointContext context, string mToken, IEnumerable<StockTakeItem> stockTakeItems)
        {
            //ThrowIf.Null<ODataActionParameters>(parameters, "parameters");
            await MobilityAuthentication(context, mToken).ConfigureAwait(false);
            return (await context.ExecuteAsync<CountOfflineStockTakeItemsResponse>(new CountOfflineStockTakeItemsRequest(stockTakeItems)).ConfigureAwait(false)).Quantity;
        }
        // Offline StockTake <<

        private async Task MobilityAuthentication(IEndpointContext context, string mToken)
        {
            var mobilityAuthRequest = new MobilityAuthenticationRequest(mToken, null);
            await context.ExecuteAsync<SingleEntityDataServiceResponse<MobilityToken>>(mobilityAuthRequest).ConfigureAwait(false);
        }
    }
}
