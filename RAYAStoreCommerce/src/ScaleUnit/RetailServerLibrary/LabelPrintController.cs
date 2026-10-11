
using Microsoft.Dynamics.Commerce.Runtime;
using Microsoft.Dynamics.Commerce.Runtime.DataModel;
using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
using Microsoft.Dynamics.Commerce.Runtime.Hosting.Contracts;
using Microsoft.Dynamics.Commerce.Runtime.Messages;

using RR.Commerce.Runtime.Mobility;
using RR.Commerce.Runtime.Printing;

using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace RR.Retail.RetailServerLibrary
{
    /// <summary>
    /// The controller to retrieve a new entity.
    /// </summary>
    [RoutePrefix("LabelPrint")]
    [BindEntity(typeof(LabelPrintItem))]
    public class LabelPrintController : IController
    {

        public LabelPrintController()
        {
        }

        [HttpPost]
        [Authorization(CommerceRoles.Employee, CommerceRoles.Anonymous)]
        public async Task<PagedResult<LabelPrintItem>> Select(IEndpointContext context, string mToken, int labelType, QueryResultSettings queryResultSettings)
        {
            //return this.ProcessPagedResults<LabelPrintItem>(this.labelPrintManager.Select((LabelLayoutType)labelType, this.GetQueryResultSettings()));
            //QueryResultSettings settings = QueryResultSettings.AllRecords; 
            //settings.Paging = new PagingInfo(50);
            if (queryResultSettings == null)
                throw new ArgumentNullException("settings");
            if (queryResultSettings.Sorting == null || !queryResultSettings.Sorting.IsSpecified)
                queryResultSettings.Sorting = new SortingInfo(new SortColumn("ITEMID", true), new SortColumn("INVENTDIMID", true));

            await MobilityAuthentication(context, mToken).ConfigureAwait(false);
            LabelItemSearchDataRequest request = new LabelItemSearchDataRequest((LabelLayoutType)labelType, null, queryResultSettings);
            request.Personal = true;

            PagedResult<LabelItem> labelItems = (await context.ExecuteAsync<EntityDataServiceResponse<LabelItem>>(request).ConfigureAwait(false)).PagedEntityCollection;

            PagedResult<LabelPrintItem> LabelPrintItems = labelItems.ConvertTo<LabelPrintItem>(ConvertToMobile);

            return LabelPrintItems;
        }

        [HttpPost]
        [Authorization(CommerceRoles.Employee, CommerceRoles.Anonymous)]
        public async Task<LabelPrintStat> Reset(IEndpointContext context, string mToken, int labelType)
        {
            //return this.labelPrintManager.Reset((LabelLayoutType)parameters["labelType"]);
            await MobilityAuthentication(context, mToken).ConfigureAwait(false);
            await context.ExecuteAsync<NullResponse>(new LabelItemClearDataRequest((LabelLayoutType)labelType)).ConfigureAwait(false);
            return await CalculateStat(context, (LabelLayoutType)labelType).ConfigureAwait(false);
        }

        [HttpPost]
        [Authorization(CommerceRoles.Employee, CommerceRoles.Anonymous)]
        public async Task<LabelPrintStat> GetStat(IEndpointContext context, string mToken, int labelType)
        {
            //return this.labelPrintManager.GetStat((LabelLayoutType)parameters["labelType"]);
            await MobilityAuthentication(context, mToken).ConfigureAwait(false);
            return await CalculateStat(context, (LabelLayoutType)labelType).ConfigureAwait(false);
        }

        [HttpPost]
        [Authorization(CommerceRoles.Employee, CommerceRoles.Anonymous)]
        public async Task<LabelPrintItem> FindItem(IEndpointContext context, string mToken, int labelType, LabelPrintParams param, string barcodeId)
        {
            //ThrowIf.Null<ODataActionParameters>(parameters, "parameters");
            //return this.labelPrintManager.FindItem((LabelLayoutType)parameters["labelType"], (LabelPrintParams)parameters["param"], (string)parameters["barcodeId"]);
            ThrowIf.NullOrWhiteSpace(barcodeId, "barcodeId");
            await MobilityAuthentication(context, mToken).ConfigureAwait(false);
            return (await context.ExecuteAsync<SingleEntityDataServiceResponse<LabelPrintItem>>(new FindLabelPrintItemServiceRequest((LabelLayoutType)labelType, await CreateParams(context, param).ConfigureAwait(false), barcodeId)).ConfigureAwait(false)).Entity;
        }

        [HttpPost]
        [Authorization(CommerceRoles.Employee, CommerceRoles.Anonymous)]
        public async Task<LabelPrintStat> AddItem(IEndpointContext context, string mToken, int labelType, LabelPrintParams param, LabelPrintItem item)
        {
            //ThrowIf.Null<ODataActionParameters>(parameters, "parameters");
            //return this.labelPrintManager.AddItem((LabelLayoutType)parameters["labelType"], (LabelPrintParams)parameters["param"], (LabelPrintItem)parameters["item"]);
            ThrowIf.Null<LabelPrintItem>(item, "item");
            await MobilityAuthentication(context, mToken).ConfigureAwait(false);
            await context.ExecuteAsync<SingleEntityDataServiceResponse<LabelItem>>(new AddLabelPrintItemServiceRequest((LabelLayoutType)labelType, await CreateParams(context, param).ConfigureAwait(false), item)).ConfigureAwait(false);
            return await CalculateStat(context, (LabelLayoutType)labelType).ConfigureAwait(false);
        }

        [HttpPost]
        [Authorization(CommerceRoles.Employee, CommerceRoles.Anonymous)]
        public async Task<LabelPrintStat> UpdateItem(IEndpointContext context, string mToken, int labelType, LabelPrintItem item)
        {
            //ThrowIf.Null<ODataActionParameters>(parameters, "parameters");
            //return this.labelPrintManager.UpdateItem((LabelLayoutType)parameters["labelType"], (LabelPrintItem)parameters["item"]);
            ThrowIf.Null<LabelPrintItem>(item, "item");
            await MobilityAuthentication(context, mToken).ConfigureAwait(false);
            if (item.Quantity > 0)
                await context.ExecuteAsync<SingleEntityDataServiceResponse<LabelItem>>(new LabelItemUpdateDataRequest(ConvertToLabel(item))).ConfigureAwait(false);
            else
                await context.ExecuteAsync<NullResponse>(new LabelItemDeleteDataRequest(item.Id)).ConfigureAwait(false);
            return await CalculateStat(context, (LabelLayoutType)labelType).ConfigureAwait(false);
        }

        [HttpPost]
        [Authorization(CommerceRoles.Employee, CommerceRoles.Anonymous)]
        public async Task<LabelPrintStat> DeleteItem(IEndpointContext context, string mToken, int labelType, long key)
        {
            //ThrowIf.Null<ODataActionParameters>(parameters, "parameters");
            //return this.labelPrintManager.DeleteItem((LabelLayoutType)parameters["labelType"], (long)parameters["recordId"]);
            await MobilityAuthentication(context, mToken).ConfigureAwait(false);
            await context.ExecuteAsync<NullResponse>(new LabelItemDeleteDataRequest(key)).ConfigureAwait(false);
            return await CalculateStat(context, (LabelLayoutType)labelType).ConfigureAwait(false);
        }

        [HttpPost]
        [Authorization(CommerceRoles.Employee, CommerceRoles.Anonymous)]
        public async Task<ReadOnlyCollection<LabelItemStatistic>> CalcBatch(IEndpointContext context, string mToken, int labelType)
        {
            //ThrowIf.Null<ODataActionParameters>(parameters, "parameters");
            //return this.labelPrintManager.CalcBatch((LabelLayoutType)parameters["labelType"]);
            await MobilityAuthentication(context, mToken).ConfigureAwait(false);
            CalcLabelStatServiceRequest request = new CalcLabelStatServiceRequest((LabelLayoutType)labelType, true);
            return (await context.ExecuteAsync<CalcLabelStatisticsResponse>(request).ConfigureAwait(false)).Entries;
        }

        [HttpPost]
        [Authorization(CommerceRoles.Employee, CommerceRoles.Anonymous)]
        public async Task<PrinterJob> FormatBatch(IEndpointContext context, string mToken, LabelItemStatistic batch)
        {
            //ThrowIf.Null<ODataActionParameters>(parameters, "parameters");
            //return this.labelPrintManager.FormatBatch((LabelItemStatistic)parameters["batch"]);
            ThrowIf.Null(batch, "batch");
            await MobilityAuthentication(context, mToken).ConfigureAwait(false);
            LabelItemFormatRequest request = new LabelItemFormatRequest(batch, true);
            return (await context.ExecuteAsync<LabelItemFormatResponse>(request).ConfigureAwait(false)).Job;
        }

        [HttpPost]
        [Authorization(CommerceRoles.Employee, CommerceRoles.Anonymous)]
        public async Task<NullResponse> SubmitBatch(IEndpointContext context, string mToken, LabelItemStatistic batch)
        {
            //ThrowIf.Null<ODataActionParameters>(parameters, "parameters");
            //this.labelPrintManager.CommitBatch((LabelItemStatistic)parameters["batch"]);
            ThrowIf.Null(batch, "batch");
            await MobilityAuthentication(context, mToken).ConfigureAwait(false);
            LabelItemCommitRequest request = new LabelItemCommitRequest(batch);
            return await context.ExecuteAsync<NullResponse>(request).ConfigureAwait(false);
        }

        //

        [HttpPost]
        [Authorization(CommerceRoles.Employee, CommerceRoles.Anonymous)]
        public async Task<LabelPrintItem> GetItem(IEndpointContext context, string mToken, int labelType, string barcodeId)
        {
            //ThrowIf.Null<ODataActionParameters>(parameters, "parameters");
            //return this.labelPrintManager.GetItem((LabelLayoutType)parameters["labelType"], (string)parameters["barcodeId"]);
            ThrowIf.NullOrWhiteSpace(barcodeId, "barcodeId");
            await MobilityAuthentication(context, mToken).ConfigureAwait(false);
            return (await context.ExecuteAsync<SingleEntityDataServiceResponse<LabelPrintItem>>(new GetLabelPrintItemServiceRequest((LabelLayoutType)labelType, barcodeId)).ConfigureAwait(false)).Entity;
        }

        [HttpPost]
        [Authorization(CommerceRoles.Employee, CommerceRoles.Anonymous)]
        public async Task<PrinterJob> FormatItem(IEndpointContext context, string mToken, int labelType, LabelPrintParams param, LabelPrintRequest item)
        {
            //ThrowIf.Null<ODataActionParameters>(parameters, "parameters");
            //return this.labelPrintManager.FormatItem((LabelLayoutType)parameters["labelType"], (LabelPrintParams)parameters["param"], (LabelPrintRequest)parameters["item"]);
            await MobilityAuthentication(context, mToken).ConfigureAwait(false);
            return (await context.ExecuteAsync<LabelItemFormatResponse>(new FormatLabelPrintItemServiceRequest((LabelLayoutType)labelType, await CreateParams(context, param).ConfigureAwait(false), item)).ConfigureAwait(false)).Job;
        }

        [HttpPost]
        [Authorization(CommerceRoles.Employee, CommerceRoles.Anonymous)]
        public async Task<LabelPrinterTask> PrintItem(IEndpointContext context, string mToken, int labelType, LabelPrintParams param, LabelPrintRequest item, int templateNo)
        {
            //ThrowIf.Null<ODataActionParameters>(parameters, "parameters");
            //return this.labelPrintManager.PrintItem((LabelLayoutType)parameters["labelType"], (LabelPrintParams)parameters["param"], (LabelPrintRequest)parameters["item"], (int)parameters["templateNo"]);
            await MobilityAuthentication(context, mToken).ConfigureAwait(false);
            return (await context.ExecuteAsync<LabelItemFormatResponse>(new PrintLabelPrintItemServiceRequest((LabelLayoutType)labelType, await CreateParams(context, param).ConfigureAwait(false), item, templateNo)).ConfigureAwait(false)).Task;
        }

        //

        [HttpPost]
        [Authorization(CommerceRoles.Employee, CommerceRoles.Anonymous)]
        public async Task<PagedResult<LabelCode>> GetCodes(IEndpointContext context, string mToken)
        {
            //return this.ProcessPagedResults<LabelCode>(this.labelPrintManager.GetLabelCodes(), 0);
            await MobilityAuthentication(context, mToken).ConfigureAwait(false);
            return (await context.ExecuteAsync<EntityDataServiceResponse<LabelCode>>(new GetLabelCodeDataRequest()).ConfigureAwait(false)).PagedEntityCollection;
        }

        private async Task MobilityAuthentication(IEndpointContext context, string mToken)
        {
            var mobilityAuthRequest = new MobilityAuthenticationRequest(mToken, null);
            await context.ExecuteAsync<SingleEntityDataServiceResponse<MobilityToken>>(mobilityAuthRequest).ConfigureAwait(false);
        }

        private async Task<LabelCalculateParams> CreateParams(IEndpointContext context, LabelPrintParams parameters)
        {
            if (parameters == null)
                return LabelCalculateParams.Default();

            GetLabelParamsServiceRequest request = new GetLabelParamsServiceRequest();
            request.CodeRecId = parameters.CodeRecId;
            request.LabelCode = parameters.LabelCode;
            request.PriceDate = parameters.PriceDate;

            return (await context.ExecuteAsync<SingleEntityDataServiceResponse<LabelCalculateParams>>(request).ConfigureAwait(false)).Entity;
        }

        private async Task<LabelPrintStat> CalculateStat(IEndpointContext context, LabelLayoutType labelType)
        {
            int[] result = (await context.ExecuteAsync<SingleEntityDataServiceResponse<int[]>>(new LabelItemCountDataRequest(labelType, true)).ConfigureAwait(false)).Entity;

            LabelPrintStat stat = new LabelPrintStat();
            stat.LabelType = labelType;
            stat.Count = result[0];
            stat.Quantity = result[1];
            return stat;
        }

        private static LabelPrintItem ConvertToMobile(LabelItem li)
        {
            LabelPrintItem lpi = new LabelPrintItem();

            lpi.BarcodeId = li.Barcode;
            lpi.ItemId = li.ItemId;
            lpi.InventDimId = li.InventDimId;
            lpi.VariantId = li.VariantId;
            lpi.UnitId = li.UnitId;
            lpi.Quantity = li.NoOfLabels;

            lpi.Id = li.Id;
            lpi.ItemName = li.Name;
            lpi.InventSizeId = li.DimSizeId;
            lpi.InventColorId = li.DimColorId;
            lpi.InventStyleId = li.DimStyleId;
            lpi.ConfigId = li.DimConfigId;

            lpi.CreateInventDim();

            return lpi;
        }

        private static LabelItem ConvertToLabel(LabelPrintItem lpi)
        {
            LabelItem li = new LabelItem();

            li.Barcode = lpi.BarcodeId;
            li.ItemId = lpi.ItemId;
            li.InventDimId = lpi.InventDimId;
            li.VariantId = lpi.VariantId;
            li.UnitId = lpi.UnitId;
            li.NoOfLabels = lpi.Quantity;

            li.Id = lpi.Id;
            li.Name = lpi.ItemName;
            li.DimSizeId = lpi.InventSizeId;
            li.DimColorId = lpi.InventColorId;
            li.DimStyleId = lpi.InventStyleId;
            li.DimConfigId = lpi.ConfigId;

            return li;
        }
    }
}
