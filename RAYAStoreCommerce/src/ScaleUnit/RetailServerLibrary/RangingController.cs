
using System.Threading.Tasks;

using Microsoft.Dynamics.Commerce.Runtime;
using Microsoft.Dynamics.Commerce.Runtime.DataModel;
using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
using Microsoft.Dynamics.Commerce.Runtime.Hosting.Contracts;

using RR.Commerce.Runtime.Mobility;
//using RR.MobilityAuth.Helper;

namespace RR.Retail.RetailServerLibrary
{
    [RoutePrefix("Ranging")]
    [BindEntity(typeof(RangingItem))]
    public class RangingController : IController
    {
        //private RangingManager rangingManager;

        //public override string ControllerName
        //{
        //    get { return "Ranging"; }
        //}

        //protected override void Initialize(HttpControllerContext controllerContext)
        //{
        //    base.Initialize(controllerContext);
        //    MobilityAuthentication.Initialize(controllerContext, this.CommerceRuntime);
        //    this.rangingManager = RangingManager.Create(this.CommerceRuntime);
        //}

        //protected override int GetKey(RangingItem entity)
        //{
        //    ThrowIf.Null<RangingItem>(entity, "entity");
        //    return entity.Id;
        //}

        //[CommerceAuthorization(CommerceRoles.Employee)]
        //public PageResult<RangingItem> Get(ODataActionParameters parameters)
        //{
        //   return this.ProcessPagedResults<RangingItem>(this.rangingManager.Select(this.GetQueryResultSettings()));
        //}

        //[CommerceAuthorization(CommerceRoles.Employee)]
        //protected override void DeleteEntity(int key)
        //{
        //    this.rangingManager.Delete(key);
        //}

        public RangingController()
        {
        }

        [HttpPost]
        [Authorization(CommerceRoles.Employee, CommerceRoles.Anonymous)]
        public async Task<PagedResult<RangingItem>> GetItemList(IEndpointContext context, string mToken, string sectionId, string shelfId, QueryResultSettings queryResultSettings)
        {
            //ThrowIf.Null<ODataActionParameters>(parameters, "parameters");
            //string sectionId = (string)parameters["sectionId"];
            //string shelfId = (string)parameters["shelfId"];
            //return this.ProcessPagedResults<RangingItem>(this.rangingManager.Select(sectionId, shelfId, this.GetQueryResultSettings()));
            await MobilityAuthentication(context, mToken).ConfigureAwait(false);
            return (await context.ExecuteAsync<SingleEntityDataServiceResponse<PagedResult<RangingItem>>>(new RangingSelectDataRequest(sectionId, shelfId)).ConfigureAwait(false)).Entity;
        }

        [HttpPost]
        [Authorization(CommerceRoles.Employee, CommerceRoles.Anonymous)]
        public async Task<RangingStat> DeleteItem(IEndpointContext context, string mToken, RangingItem item)
        {
            //ThrowIf.Null<ODataActionParameters>(parameters, "parameters");
            //return this.rangingManager.Delete((RangingItem)parameters["item"]);
            ThrowIf.Null<RangingItem>(item, "item");
            await MobilityAuthentication(context, mToken).ConfigureAwait(false);
            return (await context.ExecuteAsync<SingleEntityDataServiceResponse<RangingStat>>(new RangingItemDeleteDataRequest(item)).ConfigureAwait(false)).Entity;
        }

        [HttpPost]
        [Authorization(CommerceRoles.Employee, CommerceRoles.Anonymous)]
        public async Task<RangingItem> GetItem(IEndpointContext context, string mToken, string barcodeId, string sectionId, string shelfId)
        {
            //ThrowIf.Null<ODataActionParameters>(parameters, "parameters");
            //string barcodeId = (string)parameters["barcodeId"];
            //string sectionId = (string)parameters["sectionId"];
            //string shelfId = (string)parameters["shelfId"];
            //return this.rangingManager.GetItem(barcodeId, sectionId, shelfId);
            ThrowIf.NullOrWhiteSpace(barcodeId, barcodeId);
            await MobilityAuthentication(context, mToken).ConfigureAwait(false);
            return (await context.ExecuteAsync<SingleEntityDataServiceResponse<RangingItem>>(new GetRangingItemServiceRequest(barcodeId, sectionId, shelfId)).ConfigureAwait(false)).Entity;
        }

        [HttpPost]
        [Authorization(CommerceRoles.Employee, CommerceRoles.Anonymous)]
        public async Task<RangingStat> AddItem(IEndpointContext context, string mToken, RangingItem item)
        {
            //ThrowIf.Null<ODataActionParameters>(parameters, "parameters");
            //return this.rangingManager.Add((RangingItem)parameters["item"]);
            ThrowIf.Null<RangingItem>(item, "item");
            await MobilityAuthentication(context, mToken).ConfigureAwait(false);
            return (await context.ExecuteAsync<SingleEntityDataServiceResponse<RangingStat>>(new RangingItemAddDataRequest(item)).ConfigureAwait(false)).Entity;
        }

        [HttpPost]
        [Authorization(CommerceRoles.Employee, CommerceRoles.Anonymous)]
        public async Task<RangingStat> UndoItem(IEndpointContext context, string mToken, string sectionId, string shelfId)
        {
            //ThrowIf.Null<ODataActionParameters>(parameters, "parameters");
            //return this.rangingManager.UndoItem((string)parameters["sectionId"], (string)parameters["shelfId"]);
            await MobilityAuthentication(context, mToken).ConfigureAwait(false);
            return (await context.ExecuteAsync<SingleEntityDataServiceResponse<RangingStat>>(new RangingItemUndoDataRequest(sectionId, shelfId)).ConfigureAwait(false)).Entity;
        }

        [HttpPost]
        [Authorization(CommerceRoles.Employee, CommerceRoles.Anonymous)]
        public async Task<RangingStat> Create(IEndpointContext context, string mToken, string sectionId, string shelfId)
        {
            //ThrowIf.Null<ODataActionParameters>(parameters, "parameters");
            //return this.rangingManager.Create((string)parameters["sectionId"], (string)parameters["shelfId"]);
            ThrowIf.NullOrWhiteSpace(sectionId, "sectionId");
            ThrowIf.NullOrWhiteSpace(shelfId, "shelfId");
            await MobilityAuthentication(context, mToken).ConfigureAwait(false);
            return (await context.ExecuteAsync<SingleEntityDataServiceResponse<RangingStat>>(new RangingCreateDataRequest(sectionId, shelfId)).ConfigureAwait(false)).Entity;
        }

        [HttpPost]
        [Authorization(CommerceRoles.Employee, CommerceRoles.Anonymous)]
        public async Task<RangingStat> Reset(IEndpointContext context, string mToken, string sectionId, string shelfId)
        {
            //ThrowIf.Null<ODataActionParameters>(parameters, "parameters");
            //return this.rangingManager.Reset((string)parameters["sectionId"], (string)parameters["shelfId"]);
            await MobilityAuthentication(context, mToken).ConfigureAwait(false);
            return (await context.ExecuteAsync<SingleEntityDataServiceResponse<RangingStat>>(new RangingResetDataRequest(sectionId, shelfId)).ConfigureAwait(false)).Entity;
        }

        [HttpPost]
        [Authorization(CommerceRoles.Employee, CommerceRoles.Anonymous)]
        public async Task<RangingStat> GetStat(IEndpointContext context, string mToken, string sectionId, string shelfId)
        {
            //ThrowIf.Null<ODataActionParameters>(parameters, "parameters");
            //return this.rangingManager.GetStat((string)parameters["sectionId"], (string)parameters["shelfId"]);
            await MobilityAuthentication(context, mToken).ConfigureAwait(false);
            return (await context.ExecuteAsync<SingleEntityDataServiceResponse<RangingStat>>(new RangingStatDataRequest(sectionId, shelfId)).ConfigureAwait(false)).Entity;
        }

        [HttpPost]
        [Authorization(CommerceRoles.Employee, CommerceRoles.Anonymous)]
        public async Task<RangingStat> Commit(IEndpointContext context, string mToken, string sectionId, string shelfId)
        {
            //ThrowIf.Null<ODataActionParameters>(parameters, "parameters");
            //this.rangingManager.Commit((string)parameters["sectionId"], (string)parameters["shelfId"]);
            await MobilityAuthentication(context, mToken).ConfigureAwait(false);
            return (await context.ExecuteAsync<SingleEntityDataServiceResponse<RangingStat>>(new CommitRangingServiceRequest(sectionId, shelfId)).ConfigureAwait(false)).Entity;
        }

        private async Task MobilityAuthentication(IEndpointContext context, string mToken)
        {
            var mobilityAuthRequest = new MobilityAuthenticationRequest(mToken, null);
            await context.ExecuteAsync<SingleEntityDataServiceResponse<MobilityToken>>(mobilityAuthRequest).ConfigureAwait(false);
        }
    }
}
