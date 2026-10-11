using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.Dynamics.Commerce.Runtime;
using Microsoft.Dynamics.Commerce.Runtime.DataModel;
using Microsoft.Dynamics.Commerce.Runtime.Messages;
using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
using Microsoft.Dynamics.Commerce.Runtime.Hosting.Contracts;

using RR.Commerce.Runtime.Utilities;
using RR.Commerce.Runtime.Mobility;
//using RR.MobilityAuth.Helper;

namespace RR.Retail.RetailServerLibrary
{
    public class ExtendedNonBindableOperationController : IController
    {
        public ExtendedNonBindableOperationController()
        {
        }

        [HttpPost]
        [Authorization(CommerceRoles.Anonymous)]
        public async Task<string> MobilityLogOn(IEndpointContext context, string deviceId, string staffId, string password)
        {
            //ThrowIf.Null<ODataActionParameters>(parameters, "parameters");
            //return this.systemManager.LogOn((string)parameters["deviceId"], (string)parameters["staffId"], (string)parameters["password"], MobilityAuthentication.GetCertificateLocation());
            ThrowIf.NullOrWhiteSpace(deviceId, "deviceId");
            ThrowIf.NullOrWhiteSpace(staffId, "staffId");
            return (await context.ExecuteAsync<SingleEntityDataServiceResponse<string>>(new MobilityConnectionRequest(deviceId, staffId, password, null)).ConfigureAwait(false)).Entity;
        }

        [HttpPost]
        [Authorization(CommerceRoles.Customer, CommerceRoles.Employee, CommerceRoles.Anonymous)]
        public async Task<PagedResult<StoreSection>> GetStoreSections(IEndpointContext context)
        {
            //return this.ProcessPagedResults<StoreSection>(this.utilitiesManager.GetStoreSections(), 0);
            return (await context.ExecuteAsync<EntityDataServiceResponse<StoreSection>>(new GetStoreSectionDataRequest()).ConfigureAwait(false)).PagedEntityCollection;
        }

        [HttpPost]
        [Authorization(CommerceRoles.Customer, CommerceRoles.Employee, CommerceRoles.Anonymous)]
        public async Task<PagedResult<StoreShelf>> GetStoreShelves(IEndpointContext context, string sectionId)
        {
            //ThrowIf.Null<ODataActionParameters>(parameters, "parameters");
            //return this.ProcessPagedResults<StoreShelf>(this.utilitiesManager.GetStoreShelves((string)parameters["sectionId"]), 0);
            return (await context.ExecuteAsync<EntityDataServiceResponse<StoreShelf>>(new GetStoreShelfDataRequest(sectionId)).ConfigureAwait(false)).PagedEntityCollection;
        }

        [HttpPost]
        [Authorization(CommerceRoles.Customer, CommerceRoles.Employee, CommerceRoles.Anonymous)]
        public async Task<PagedResult<StoreSection>> FetchStoreSections(IEndpointContext context, string mToken)
        {
            //return this.ProcessPagedResults<StoreSection>(this.utilitiesManager.GetStoreSections(), 0);
            await MobilityAuthentication(context, mToken).ConfigureAwait(false);
            return await GetStoreSections(context).ConfigureAwait(false);
        }

        [HttpPost]
        [Authorization(CommerceRoles.Customer, CommerceRoles.Employee, CommerceRoles.Anonymous)]
        public async Task<PagedResult<StoreShelf>> FetchStoreShelves(IEndpointContext context, string mToken, string sectionId)
        {
            //ThrowIf.Null<ODataActionParameters>(parameters, "parameters");
            //return this.ProcessPagedResults<StoreShelf>(this.utilitiesManager.GetStoreShelves((string)parameters["sectionId"]), 0);
            await MobilityAuthentication(context, mToken).ConfigureAwait(false);
            return await GetStoreShelves(context, sectionId).ConfigureAwait(false);
        }

        [HttpPost]
        [Authorization(CommerceRoles.Customer, CommerceRoles.Employee, CommerceRoles.Anonymous)]
        public async Task<UtilitiesConfiguration> GetUtilitiesConfiguration(IEndpointContext context)
        {
            //return this.utilitiesManager.GetUtilitiesConfiguration();
            return (await context.ExecuteAsync<SingleEntityDataServiceResponse<UtilitiesConfiguration>>(new GetUtilitiesConfigurationDataRequest()).ConfigureAwait(false)).Entity;
        }

        [HttpPost]
        [Authorization(CommerceRoles.Anonymous)]
        public async Task<CreateHardwareStationTokenResult> CreateMobilityHardwareStationToken(IEndpointContext context, string mToken)
        {
            await MobilityAuthentication(context, mToken).ConfigureAwait(false);
            Uri certificateLocation = null;  // MobilityAuthentication.GetCertificateLocation();
            return (await context.ExecuteAsync<Microsoft.Dynamics.Commerce.Runtime.Services.Messages.CreateHardwareStationTokenServiceResponse>(new CreateMobilityHardwareStationTokenServiceRequest(certificateLocation)).ConfigureAwait(false)).Result;
        }

        [HttpPost]
        [Authorization(CommerceRoles.Anonymous)]
        public async Task<MobilityParameters> GetMobilityParameters(IEndpointContext context, string mToken)
        {
            //return this.systemManager.GetParameters();
            await MobilityAuthentication(context, mToken).ConfigureAwait(false);
            return (await context.ExecuteAsync<SingleEntityDataServiceResponse<MobilityParameters>>(new MobilityParametersRequest()).ConfigureAwait(false)).Entity;
        }

        #region "ILKP"
        // Inventory LookUp >>
        [HttpPost]
        [Authorization(CommerceRoles.Anonymous, CommerceRoles.Device, CommerceRoles.Customer, CommerceRoles.Employee)]
        public async Task<PagedResult<Product>> ProductSearchByText(IEndpointContext context, ProductSearchCriteria productSearchCriteria, long topRows,long skipRows)
        {
            //ThrowIf.Null<ODataActionParameters>(parameters, "parameters");
            //ProductSearchCriteria criteria= (ProductSearchCriteria)parameters["productSearchCriteria"];
            //long top = (long)parameters["topRows"];
            //long skip = (long)parameters["skipRows"];
            QueryResultSettings setting = new QueryResultSettings(new PagingInfo(topRows, skipRows));
            return (await context.ExecuteAsync<ProductSearchByTextResponse>(new ProductSearchByTextRequest(productSearchCriteria, setting)).ConfigureAwait(false)).QueryResults;
        }

        [HttpPost]
        [Authorization(CommerceRoles.Application, CommerceRoles.Customer, CommerceRoles.Device, CommerceRoles.Employee, CommerceRoles.Anonymous)]
        public async Task<decimal> ProductAvailableQuantity(IEndpointContext context,long productId,string itemID,string inventDimId)
        {
            //ThrowIf.Null<ODataActionParameters>(parameters, "parameters");
            //long productID = (long)parameters["productId"];
            //string itemID = (string)parameters["itemID"];
            //string inventDimId = (string)parameters["inventDimId"];
            return (await context.ExecuteAsync<SingleEntityDataServiceResponse<decimal>>(new GetItemVariantAvailableQtyDataRequest(itemID, inventDimId, productId)).ConfigureAwait(false)).Entity;
        }
        // Inventory LookUp <<
        #endregion

        // Offline StockTake >>
        [HttpPost]
        [Authorization(CommerceRoles.Customer, CommerceRoles.Employee, CommerceRoles.Anonymous)]
        public async Task<IEnumerable<BarcodeItem>> GetItemsBarcode(IEndpointContext context, IEnumerable<BarcodeItem> barcodeItems)
        {
            //ThrowIf.Null<ODataActionParameters>(parameters, "parameters");
            //GetBarcodeForItemRequest request = new GetBarcodeForItemRequest((IEnumerable<BarcodeItem>)parameters["barcodeItems"]);
            //return this.CommerceRuntime.Execute<GetBarcodeForItemResponse>(request, request.RequestContext).Barcodes;
            ThrowIf.Null<IEnumerable<BarcodeItem>>(barcodeItems, "barcodeItems");
            return (await context.ExecuteAsync<GetBarcodeForItemResponse>(new GetBarcodeForItemRequest(barcodeItems)).ConfigureAwait(false)).Barcodes;
        }
        // Offline StockTake <<

        //protected override void Initialize(HttpControllerContext controllerContext)
        //{
        //    base.Initialize(controllerContext);
        //    MobilityAuthentication.Initialize(controllerContext, this.CommerceRuntime);            
        //    this.utilitiesManager = UtilitiesManager.Create(this.CommerceRuntime);
        //    this.systemManager = SystemManager.Create(this.CommerceRuntime);
        //}

        private async Task MobilityAuthentication(IEndpointContext context, string mToken)
        {
            var mobilityAuthRequest = new MobilityAuthenticationRequest(mToken, null);
            await context.ExecuteAsync<SingleEntityDataServiceResponse<MobilityToken>>(mobilityAuthRequest).ConfigureAwait(false);
        }

    }
}
