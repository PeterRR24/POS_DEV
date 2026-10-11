
using System.Threading.Tasks;

using Microsoft.Dynamics.Commerce.Runtime;
using Microsoft.Dynamics.Commerce.Runtime.DataModel;
using Microsoft.Dynamics.Commerce.Runtime.Messages;
using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
using Microsoft.Dynamics.Commerce.Runtime.Hosting.Contracts;

using RR.Commerce.Runtime.Printing;
using RR.Commerce.Runtime.Mobility;
//using RR.MobilityAuth.Helper;

namespace RR.Retail.RetailServerLibrary
{
    [RoutePrefix("ProductSales")]
    [BindEntity(typeof(ProductSale))]
    public class ProductSalesController : IController
    {
        //private ProductSaleManager productSaleManager;

        //public override string ControllerName
        //{
        //    get { return "ProductSales"; }
        //}

        //protected override void Initialize(HttpControllerContext controllerContext)
        //{
        //    base.Initialize(controllerContext);
        //    MobilityAuthentication.Initialize(controllerContext, this.CommerceRuntime);
        //    this.productSaleManager = ProductSaleManager.Create(this.CommerceRuntime);
        //}

        //protected override string GetKey(ProductSale entity)
        //{
        //    ThrowIf.Null<ProductSale>(entity, "entity");
        //    return entity.BarcodeId;
        //}

        public ProductSalesController()
        {
        }

        //[Authorization(CommerceRoles.Employee)]
        //protected override ProductSale GetEntityByKey([EntityKey]string key)
        //{
        //    return this.productSaleManager.Get(key);
        //}

        [HttpPost]
        [Authorization(CommerceRoles.Employee, CommerceRoles.Anonymous)]
        public async Task<ProductSale> GetItem(IEndpointContext context, string mToken, [EntityKey]string key)    // override GetEntityByKey
        {
            //return this.productSaleManager.Get(key);
            ThrowIf.NullOrWhiteSpace(key, "key");
            await MobilityAuthentication(context, mToken).ConfigureAwait(false);
            return (await context.ExecuteAsync<SingleEntityDataServiceResponse<ProductSale>>(new GetProductSaleServiceRequest(key)).ConfigureAwait(false)).Entity;
        }

        //

        [HttpPost]
        [Authorization(CommerceRoles.Employee, CommerceRoles.Anonymous)]
        public async Task<ProductSale> Sale(IEndpointContext context, string mToken, [EntityKey] string key, string reasonCode)
        {
            //ThrowIf.NullOrWhiteSpace(key, "key");
            //ThrowIf.Null<ODataActionParameters>(parameters, "parameters");
            //return this.productSaleManager.Sale(key, (string)parameters["reasonCode"]);
            ThrowIf.NullOrWhiteSpace(key, "key");
            await MobilityAuthentication(context, mToken).ConfigureAwait(false);
            return (await context.ExecuteAsync<SingleEntityDataServiceResponse<ProductSale>>(new CalcProductSaleServiceRequest(key, reasonCode)).ConfigureAwait(false)).Entity;
        }

        [HttpPost]
        [Authorization(CommerceRoles.Employee, CommerceRoles.Anonymous)]
        public async Task<string> Format(IEndpointContext context, string mToken, ProductSaleRequest request)
        {
            //ThrowIf.Null<ODataActionParameters>(parameters, "parameters");
            //return this.productSaleManager.Format((ProductSaleRequest)parameters["request"]);
            ThrowIf.Null<ProductSaleRequest>(request, "request");
            ThrowIf.NullOrWhiteSpace(request.BarcodeId, "request.BarcodeId");
            ThrowIf.NullOrWhiteSpace(request.ReasonCode, "request.ReasonCode");
            await MobilityAuthentication(context, mToken).ConfigureAwait(false);
            return (await context.ExecuteAsync<SingleEntityDataServiceResponse<string>>(new FormatProductSaleServiceRequest(request)).ConfigureAwait(false)).Entity;
        }

        [HttpPost]
        [Authorization(CommerceRoles.Employee, CommerceRoles.Anonymous)]
        public async Task<LabelPrinterTask> Label(IEndpointContext context, string mToken, ProductSaleRequest request, int templateNo, int noOfLabels)
        {
            //ThrowIf.Null<ODataActionParameters>(parameters, "parameters");
            //return this.productSaleManager.Label((ProductSaleRequest)parameters["request"], (int)parameters["templateNo"],(int)parameters["noOfLabels"]);
            ThrowIf.Null<ProductSaleRequest>(request, "request");
            ThrowIf.NullOrWhiteSpace(request.BarcodeId, "request.BarcodeId");
            ThrowIf.NullOrWhiteSpace(request.ItemId, "request.ItemId");
            ThrowIf.NullOrWhiteSpace(request.ReasonCode, "request.ReasonCode");
            await MobilityAuthentication(context, mToken).ConfigureAwait(false);
            return (await context.ExecuteAsync<SingleEntityDataServiceResponse<LabelPrinterTask>>(new LabelProductSaleServiceRequest(request, templateNo, noOfLabels)).ConfigureAwait(false)).Entity;
        }

        [HttpPost]
        [Authorization(CommerceRoles.Employee, CommerceRoles.Anonymous)]
        public async Task<PagedResult<ProductSaleReason>> GetReasons(IEndpointContext context, string mToken)
        {
            //return this.ProcessPagedResults<ProductSaleReason>(this.productSaleManager.GetSaleReasons(), 0);
            await MobilityAuthentication(context, mToken).ConfigureAwait(false);
            return (await context.ExecuteAsync<EntityDataServiceResponse<ProductSaleReason>>(new ProductSaleReasonSearchDataRequest()).ConfigureAwait(false)).PagedEntityCollection;
        }

        private async Task MobilityAuthentication(IEndpointContext context, string mToken)
        {
            var mobilityAuthRequest = new MobilityAuthenticationRequest(mToken, null);
            await context.ExecuteAsync<SingleEntityDataServiceResponse<MobilityToken>>(mobilityAuthRequest).ConfigureAwait(false);
        }
    }
}
