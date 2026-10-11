
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.Dynamics.Commerce.Runtime;
using Microsoft.Dynamics.Commerce.Runtime.DataModel;
using Microsoft.Dynamics.Commerce.Runtime.Hosting.Contracts;
using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
using Microsoft.Dynamics.Commerce.Runtime.Messages;

using RR.Commerce.Runtime.Printing;
using RR.Commerce.Runtime.Utilities;

// DEBUG - attach to process w3wp.exe

namespace RR.Retail.RetailServerLibrary
{
    /*
    [ComVisible(false)]
    public class LabelItemsController : CommerceController<LabelItem,long>
    {
        private LabelItemManager labelItemManager;

        public override string ControllerName
        {
            get { return "LabelItems"; }
        }

        [HttpPatch]
        [CommerceAuthorization(CommerceRoles.Anonymous, CommerceRoles.Customer, CommerceRoles.Employee)]
        protected override LabelItem PatchEntity(long key, Delta<LabelItem> patch)
        {
            ThrowIf.Null<Delta<LabelItem>>(patch, "patch");
            LabelItem item = this.labelItemManager.GetLabelItem(key);
            ThrowIf.Null<LabelItem>(item, "existingEntity");
            patch.Patch(item);
            return this.labelItemManager.UpdateLabelItem(item);
        }

        [CommerceAuthorization(CommerceRoles.Anonymous, CommerceRoles.Customer, CommerceRoles.Employee)]
        protected override void DeleteEntity(long key)
        {
            this.labelItemManager.DeleteLabelItem(key);
        }

        [HttpPost]
        [CommerceAuthorization(CommerceRoles.Anonymous, CommerceRoles.Customer, CommerceRoles.Employee)]
        public int Calculate(ODataActionParameters parameters)
        {
            ThrowIf.Null<ODataActionParameters>(parameters, "parameters");
            LabelLayoutType labelType = (LabelLayoutType)parameters["labelType"];
            if (parameters["items"] != null)
                return this.labelItemManager.CalculateLabels(labelType, (LabelItemCalculateParams)parameters["param"], (IEnumerable<LabelItemCalculateItem>)parameters["items"]);
            else
                return this.labelItemManager.CalculateLabels(labelType, (LabelItemCalculateParams)parameters["param"], (LabelItemCalculateCriteria)parameters["criteria"], (int)parameters["pageNumber"]);
        }

        [HttpPost]
        [CommerceAuthorization(CommerceRoles.Anonymous, CommerceRoles.Customer, CommerceRoles.Employee)]
        public PageResult<LabelItem> Search(ODataActionParameters parameters)
        {
            ThrowIf.Null<ODataActionParameters>(parameters, "parameters");
            return this.ProcessPagedResults<LabelItem>(this.labelItemManager.SearchLabelItems((LabelLayoutType)parameters["labelType"], (string)parameters["keyword"], this.GetQueryResultSettings()));
        }

        [HttpPost]
        [CommerceAuthorization(CommerceRoles.Anonymous, CommerceRoles.Customer, CommerceRoles.Employee)]
        public ReadOnlyCollection<LabelItemStatistic> CalcStat(ODataActionParameters parameters)
        {
            ThrowIf.Null<ODataActionParameters>(parameters, "parameters");
            return this.labelItemManager.CalcLabelStats((LabelLayoutType)parameters["labelType"]);
        }

        [HttpPost]
        [CommerceAuthorization(CommerceRoles.Anonymous, CommerceRoles.Customer, CommerceRoles.Employee)]
        public PrinterJob Format(ODataActionParameters parameters)
        {
            ThrowIf.Null<ODataActionParameters>(parameters, "parameters");
            LabelItemStatistic stat = (LabelItemStatistic)parameters["stat"];
            ThrowIf.Null<LabelItemStatistic>(stat, "parameters.stat");
            return this.labelItemManager.FormatLabels(stat);
        }

        [HttpPost]
        [CommerceAuthorization(CommerceRoles.Anonymous, CommerceRoles.Customer, CommerceRoles.Employee)]
        public PrinterJob FormatLabelItem(ODataActionParameters parameters)
        {
            ThrowIf.Null<ODataActionParameters>(parameters, "parameters");
            return this.labelItemManager.FormatLabelItem((long)parameters["recordId"], (int)parameters["quantity"]);
        }

        [HttpPost]
        [CommerceAuthorization(CommerceRoles.Anonymous, CommerceRoles.Customer, CommerceRoles.Employee)]
        public LabelPrinterTask PrintLabelItem(ODataActionParameters parameters)
        {
            ThrowIf.Null<ODataActionParameters>(parameters, "parameters");
            return this.labelItemManager.PrintLabelItem((long)parameters["recordId"], (int)parameters["quantity"], (int)parameters["templateNo"]);
        }

        [HttpPost]
        [CommerceAuthorization(CommerceRoles.Anonymous, CommerceRoles.Customer, CommerceRoles.Employee)]
        public void CommitLabelItem(ODataActionParameters parameters)
        {
            ThrowIf.Null<ODataActionParameters>(parameters, "parameters");
            this.labelItemManager.ConfirmLabelItem((long)parameters["recordId"]);
        }

        [HttpPost]
        [CommerceAuthorization(CommerceRoles.Anonymous, CommerceRoles.Customer, CommerceRoles.Employee)]
        public void Submit(ODataActionParameters parameters)
        {
            ThrowIf.Null<ODataActionParameters>(parameters, "parameters");
            LabelItemStatistic stat = (LabelItemStatistic)parameters["stat"];
            ThrowIf.Null<LabelItemStatistic>(stat, "parameters.stat");
            this.labelItemManager.CommitLabels(stat);
        }

        [HttpPost]
        [CommerceAuthorization(CommerceRoles.Anonymous, CommerceRoles.Customer, CommerceRoles.Employee)]
        public void Clear(ODataActionParameters parameters)
        {
            ThrowIf.Null<ODataActionParameters>(parameters, "parameters");
            this.labelItemManager.ClearLabelItems((LabelLayoutType)parameters["labelType"]);
        }

        [HttpGet]
        [CommerceAuthorization(CommerceRoles.Anonymous, CommerceRoles.Customer, CommerceRoles.Employee)]
        public PageResult<Category> GetCategoryHierarchy()
        {
            return this.ProcessPagedResults<Category>(this.labelItemManager.GetCategoryHierarchy(), 0);
        }

        [HttpGet]
        [CommerceAuthorization(CommerceRoles.Anonymous, CommerceRoles.Customer, CommerceRoles.Employee)]
        public PageResult<LabelCode> GetCodes()
        {
            return this.ProcessPagedResults<LabelCode>(this.labelItemManager.GetLabelCodes(), 0);
        }

        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            this.labelItemManager = LabelItemManager.Create(this.CommerceRuntime);
        }

        protected override long GetKey(LabelItem entity)
        {
            ThrowIf.Null<LabelItem>(entity, "entity");
            return entity.Id;
        }
    }
    */


    /// <summary>
    /// The controller to retrieve a new entity.
    /// </summary>
    [RoutePrefix("LabelItems")]
    [BindEntity(typeof(LabelItem))]
    public class LabelItemsController : IController
    {
        public LabelItemsController()
        {
        }

        //[HttpPatch]
        //[Authorization(CommerceRoles.Anonymous, CommerceRoles.Customer, CommerceRoles.Employee)]
        //protected override LabelItem PatchEntity(long key, Delta<LabelItem> patch)
        //{
        //    ThrowIf.Null<Delta<LabelItem>>(patch, "patch");
        //    LabelItem item = this.labelItemManager.GetLabelItem(key);
        //    ThrowIf.Null<LabelItem>(item, "existingEntity");
        //    patch.Patch(item);
        //    return this.labelItemManager.UpdateLabelItem(item);
        //}

        //[Authorization(CommerceRoles.Anonymous, CommerceRoles.Customer, CommerceRoles.Employee)]
        //protected override void DeleteEntity(long key)
        //{
        //    this.labelItemManager.DeleteLabelItem(key);
        //}

        [HttpPost]
        [Authorization(CommerceRoles.Anonymous, CommerceRoles.Customer, CommerceRoles.Employee)]
        public async Task<LabelItem> UpdateLabelItem(IEndpointContext context, [EntityKey]long key, LabelItem patch)    // override PatchEntity
        {
            ThrowIf.Null<LabelItem>(patch, "patch");
            LabelItem item = (await context.ExecuteAsync<SingleEntityDataServiceResponse<LabelItem>>(new LabelItemGetDataRequest(key)).ConfigureAwait(false)).Entity;
            ThrowIf.Null<LabelItem>(item, "existingEntity");
            item = patch;
            return (await context.ExecuteAsync<SingleEntityDataServiceResponse<LabelItem>>(new LabelItemUpdateDataRequest(item)).ConfigureAwait(false)).Entity;
        }

        [HttpPost]
        [Authorization(CommerceRoles.Anonymous, CommerceRoles.Customer, CommerceRoles.Employee)]
        public async Task<NullResponse> DeleteLabelItem(IEndpointContext context, [EntityKey]long key) // override DeleteEntity
        {
            return await context.ExecuteAsync<NullResponse>(new LabelItemDeleteDataRequest(key)).ConfigureAwait(false);
        }

        internal async Task<LabelCalculateParams> CreateParams(IEndpointContext context, LabelItemCalculateParams parameters)
        {
            if (parameters == null)
                return LabelCalculateParams.Default();

            GetLabelParamsServiceRequest request = new GetLabelParamsServiceRequest();
            request.CodeRecId = parameters.CodeRecId;
            request.LabelCode = parameters.LabelCode;
            request.PriceDate = parameters.PriceDate;

            return (await context.ExecuteAsync<SingleEntityDataServiceResponse<LabelCalculateParams>>(request).ConfigureAwait(false)).Entity;
        }

        [HttpPost]
        [Authorization(CommerceRoles.Anonymous, CommerceRoles.Customer, CommerceRoles.Employee)]
        public async Task<int> Calculate(IEndpointContext context, int labelType, LabelItemCalculateParams param, LabelItemCalculateCriteria criteria, IEnumerable<LabelItemCalculateItem> items, int pageNumber)
        {
            LabelItemCalculateRequest request = new LabelItemCalculateRequest((LabelLayoutType)labelType);
            request.Parameters = await CreateParams(context, param).ConfigureAwait(false);
            if (items != null)
            {
                request.Items = IEnumerableExtensions.AsReadOnly<LabelItemCalculateItem>(items);
                return (await context.ExecuteAsync<LabelItemCalculateResponse>(request).ConfigureAwait(false)).Count;
            }
            else
            {
                request.Criteria = criteria;
                request.PageNumber = pageNumber;
                LabelItemCalculateResponse licr = await context.ExecuteAsync<LabelItemCalculateResponse>(request).ConfigureAwait(false);
                if (licr.HasNext)
                    return -1;
                else
                    return licr.Count;
            }
        }

        [HttpPost]
        [Authorization(CommerceRoles.Anonymous, CommerceRoles.Customer, CommerceRoles.Employee)]
        public async Task<PagedResult<LabelItem>> Search(IEndpointContext context, int labelType, string keyword, QueryResultSettings queryResultSettings)
        {
            //QueryResultSettings settings = QueryResultSettings.AllRecords;
            //settings.Paging = new PagingInfo(50);
            if (queryResultSettings.Sorting == null || !queryResultSettings.Sorting.IsSpecified)
                queryResultSettings.Sorting = new SortingInfo(new SortColumn("ITEMID", true), new SortColumn("INVENTDIMID", true));

            LabelItemSearchDataRequest request = new LabelItemSearchDataRequest((LabelLayoutType)labelType, keyword, queryResultSettings);
            return (await context.ExecuteAsync<EntityDataServiceResponse<LabelItem>>(request).ConfigureAwait(false)).PagedEntityCollection;
        }

        [HttpPost]
        [Authorization(CommerceRoles.Anonymous, CommerceRoles.Customer, CommerceRoles.Employee)]
        public async Task<ReadOnlyCollection<LabelItemStatistic>> CalcStat(IEndpointContext context, int labelType)
        {
            return (await context.ExecuteAsync<CalcLabelStatisticsResponse>(new CalcLabelStatServiceRequest((LabelLayoutType)labelType)).ConfigureAwait(false)).Entries;
        }

        //[HttpPost]
        //[Authorization(CommerceRoles.Anonymous, CommerceRoles.Customer, CommerceRoles.Employee)]
        //public async Task<PrinterJob> Format(IEndpointContext context, LabelItemStatistic stat, IEnumerable<long> recordIds)
        //{
        //    ThrowIf.Null(stat, "stat");
        //    LabelItemFormatRequest request = new LabelItemFormatRequest(recordIds, stat, false);
        //    return (await context.ExecuteAsync<LabelItemFormatResponse>(request).ConfigureAwait(false)).Job;
        //}

        [HttpPost]
        [Authorization(CommerceRoles.Anonymous, CommerceRoles.Customer, CommerceRoles.Employee)]
        public async Task<PrinterJob> Format(IEndpointContext context, LabelItemStatistic stat)
        {
            ThrowIf.Null(stat, "stat");
            LabelItemFormatRequest request = new LabelItemFormatRequest(stat, false);
            return (await context.ExecuteAsync<LabelItemFormatResponse>(request).ConfigureAwait(false)).Job;
        }

        [HttpPost]
        [Authorization(CommerceRoles.Anonymous, CommerceRoles.Customer, CommerceRoles.Employee)]
        public async Task<PrinterJob> FormatLabelItem(IEndpointContext context, long recordId, int quantity)
        {
            return (await context.ExecuteAsync<LabelItemFormatResponse>(new FormatLabelItemServiceRequest(recordId, quantity)).ConfigureAwait(false)).Job;
        }

        [HttpPost]
        [Authorization(CommerceRoles.Anonymous, CommerceRoles.Customer, CommerceRoles.Employee)]
        public async Task<LabelPrinterTask> PrintLabelItem(IEndpointContext context, long recordId, int quantity, int templateNo)
        {
            return (await context.ExecuteAsync<LabelItemFormatResponse>(new PrintLabelItemServiceRequest(recordId, quantity, templateNo)).ConfigureAwait(false)).Task;
        }

        [HttpPost]
        [Authorization(CommerceRoles.Anonymous, CommerceRoles.Customer, CommerceRoles.Employee)]
        public async Task<NullResponse> CommitLabelItem(IEndpointContext context, long recordId)
        {
            return await context.ExecuteAsync<NullResponse>(new ConfirmLabelItemServiceRequest(recordId)).ConfigureAwait(false);
        }

        [HttpPost]
        [Authorization(CommerceRoles.Anonymous, CommerceRoles.Customer, CommerceRoles.Employee)]
        public async Task<NullResponse> CommitItems(IEndpointContext context, IEnumerable<long> recordIds)
        {
            return await context.ExecuteAsync<NullResponse>(new ConfirmLabelItemServiceRequest(recordIds)).ConfigureAwait(false);
        }

        [HttpPost]
        [Authorization(CommerceRoles.Anonymous, CommerceRoles.Customer, CommerceRoles.Employee)]
        public async Task<NullResponse> Submit(IEndpointContext context, LabelItemStatistic stat)
        {

            ThrowIf.Null(stat, "stat");
            LabelItemCommitRequest request = new LabelItemCommitRequest(stat);
            return await context.ExecuteAsync<NullResponse>(request).ConfigureAwait(false);
        }

        [HttpPost]
        [Authorization(CommerceRoles.Anonymous, CommerceRoles.Customer, CommerceRoles.Employee)]
        public async Task<NullResponse> Clear(IEndpointContext context, int labelType)
        {
            return await context.ExecuteAsync<NullResponse>(new LabelItemClearDataRequest((LabelLayoutType)labelType)).ConfigureAwait(false);
        }

        [HttpGet]
        [Authorization(CommerceRoles.Anonymous, CommerceRoles.Customer, CommerceRoles.Employee)]
        public async Task<PagedResult<Category>> GetCategoryHierarchy(IEndpointContext context)
        {
            long hierarchyId = (await context.ExecuteAsync<SingleEntityDataServiceResponse<UtilitiesConfiguration>>(new GetUtilitiesConfigurationDataRequest()).ConfigureAwait(false)).Entity.LabelHierarchyRecId;
            return (await context.ExecuteAsync<EntityDataServiceResponse<Category>>(new GetCategoryHierarchyDataRequest(hierarchyId)).ConfigureAwait(false)).PagedEntityCollection;

        }

        [HttpGet]
        [Authorization(CommerceRoles.Anonymous, CommerceRoles.Customer, CommerceRoles.Employee)]
        public async Task<PagedResult<LabelCode>> GetCodes(IEndpointContext context)
        {
            return (await context.ExecuteAsync<EntityDataServiceResponse<LabelCode>>(new GetLabelCodeDataRequest()).ConfigureAwait(false)).PagedEntityCollection;
        }

    }

}
