
using Microsoft.Dynamics.Commerce.Runtime;
using Microsoft.Dynamics.Commerce.Runtime.Data;
using Microsoft.Dynamics.Commerce.Runtime.DataModel;
using Microsoft.Dynamics.Commerce.Runtime.Hosting.Contracts;

using RR.Commerce.Runtime.Utilities;
//using RR.MobilityAuth.Helper;

using System.Threading.Tasks;
using Microsoft.Dynamics.Commerce.Runtime.Messages;
using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;

namespace RR.Retail.RetailServerLibrary
{
    /*
    public class PlacementController : CommerceController<StoreLocationItem, int>
    {
        private PlacementManager placementManager;

        public override string ControllerName
        {
            get { return "Placement"; }
        }

        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            MobilityAuthentication.Initialize(controllerContext, this.CommerceRuntime);
            this.placementManager = PlacementManager.Create(this.CommerceRuntime);

            // Test
            // CommercePrincipal principal = this.CommerceRuntime.CurrentPrincipal as CommercePrincipal;
            // System.Diagnostics.Trace.WriteLine(principal.DeviceNumber);
            // System.Diagnostics.Trace.WriteLine(principal.UserId);
        }

        protected override int GetKey(StoreLocationItem entity)
        {
            ThrowIf.Null<StoreLocationItem>(entity, "entity");
            return entity.Location;
        }

        [HttpGet]
        [CommerceAuthorization(CommerceRoles.Employee)]
        public PageResult<StoreLocationItem> Select([FromODataUri] string sectionId, [FromODataUri] string shelfId)
        {
            return this.ProcessPagedResults<StoreLocationItem>(this.placementManager.Select(sectionId, shelfId, this.GetQueryResultSettings()));
        }

        [HttpPost]
        [CommerceAuthorization(CommerceRoles.Employee)]
        public void Clear(ODataActionParameters parameters)
        {
            ThrowIf.Null<ODataActionParameters>(parameters, "parameters");
            this.placementManager.Clear((string)parameters["sectionId"], (string)parameters["shelfId"]);
        }

        [HttpPost]
        [CommerceAuthorization(CommerceRoles.Employee)]
        public void Commit(ODataActionParameters parameters)
        {
            ThrowIf.Null<ODataActionParameters>(parameters, "parameters");
            this.placementManager.Commit((string)parameters["sectionId"], (string)parameters["shelfId"]);
        }

        [HttpPost]
        [CommerceAuthorization(CommerceRoles.Employee)]
        public void Place(ODataActionParameters parameters)
        {
            ThrowIf.Null<ODataActionParameters>(parameters, "parameters");
            this.placementManager.Place((string)parameters["sectionId"], (string)parameters["shelfId"], (int)parameters["location"], (string)parameters["itemId"], (int)parameters["facingCount"]);
        }

        [HttpPost]
        [CommerceAuthorization(CommerceRoles.Employee)]
        public void Remove(ODataActionParameters parameters)
        {
            ThrowIf.Null<ODataActionParameters>(parameters, "parameters");
            this.placementManager.Remove((string)parameters["sectionId"], (string)parameters["shelfId"], (int)parameters["location"]);
        }

        [HttpPost]
        [CommerceAuthorization(CommerceRoles.Employee)]
        public void Move(ODataActionParameters parameters)
        {
            ThrowIf.Null<ODataActionParameters>(parameters, "parameters");
            this.placementManager.Move((string)parameters["sectionId"], (string)parameters["shelfId"], (int)parameters["oldLocation"], (int)parameters["newLocation"]);
        }
    }
    */


    /// <summary>
    /// The controller to retrieve a new entity.
    /// </summary>
    [RoutePrefix("Placement")]
    [BindEntity(typeof(StoreLocationItem))]
    public class PlacementController : IController
    {

        public PlacementController()
        {
        }

        [HttpGet]
        [Authorization(CommerceRoles.Employee, CommerceRoles.Anonymous)]
        public async Task<PagedResult<StoreLocationItem>> Select(IEndpointContext context, string sectionId, string shelfId, QueryResultSettings queryResultSettings)
        {
            ThrowIf.NullOrWhiteSpace(sectionId, "sectionId");
            ThrowIf.NullOrWhiteSpace(shelfId, "shelfId");
            //QueryResultSettings settings = QueryResultSettings.AllRecords;
            //settings.Paging = new PagingInfo(50);
            return (await context.ExecuteAsync<EntityDataServiceResponse<StoreLocationItem>>(new GetStoreLocationItemsServiceRequest(sectionId, shelfId, queryResultSettings)).ConfigureAwait(false)).PagedEntityCollection;
        }

        [HttpPost]
        [Authorization(CommerceRoles.Employee, CommerceRoles.Anonymous)]
        public async Task<NullResponse> Clear(IEndpointContext context, string sectionId, string shelfId)
        {
            return await context.ExecuteAsync<NullResponse>(new ClearStoreLocationItemsDataRequest(sectionId, shelfId)).ConfigureAwait(false);
        }

        [HttpPost]
        [Authorization(CommerceRoles.Employee, CommerceRoles.Anonymous)]
        public async Task<int> Commit(IEndpointContext context, string sectionId, string shelfId)
        {
            return (await context.ExecuteAsync<SingleEntityDataServiceResponse<int>>(new CommitStoreLocationItemsServiceRequest(sectionId, shelfId)).ConfigureAwait(false)).Entity;
        }

        [HttpPost]
        [Authorization(CommerceRoles.Employee, CommerceRoles.Anonymous)]
        public async Task<NullResponse> Place(IEndpointContext context, string sectionId, string shelfId, int location, string itemId, int facingCount)
        {
            ThrowIf.NullOrWhiteSpace(sectionId, "sectionId");
            ThrowIf.NullOrWhiteSpace(shelfId, "shelfId");
            ThrowIf.NullOrWhiteSpace(itemId, "itemId");
            return await context.ExecuteAsync<NullResponse>(new PlaceStoreLocationItemDataRequest(sectionId, shelfId, location, itemId, facingCount)).ConfigureAwait(false);
        }

        [HttpPost]
        [Authorization(CommerceRoles.Employee, CommerceRoles.Anonymous)]
        public async Task<NullResponse> Remove(IEndpointContext context, string sectionId, string shelfId, int location)
        {
            ThrowIf.NullOrWhiteSpace(sectionId, "sectionId");
            ThrowIf.NullOrWhiteSpace(shelfId, "shelfId");
            return await context.ExecuteAsync<NullResponse>(new RemoveStoreLocationItemDataRequest(sectionId, shelfId, location)).ConfigureAwait(false);
        }

        [HttpPost]
        [Authorization(CommerceRoles.Employee, CommerceRoles.Anonymous)]
        public async Task<bool> Move(IEndpointContext context, string sectionId, string shelfId, int oldLocation, int newLocation)
        {
            ThrowIf.NullOrWhiteSpace(sectionId, "sectionId");
            ThrowIf.NullOrWhiteSpace(shelfId, "shelfId");
            return (await context.ExecuteAsync<SingleEntityDataServiceResponse<bool>>(new MoveStoreLocationItemServiceRequest(sectionId, shelfId, oldLocation, newLocation)).ConfigureAwait(false)).Entity;
        }
    }
}
