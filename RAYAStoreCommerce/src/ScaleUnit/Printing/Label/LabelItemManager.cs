using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Dynamics.Commerce.Runtime;
//using Microsoft.Dynamics.Commerce.Runtime.Client;
using Microsoft.Dynamics.Commerce.Runtime.DataModel;
using Microsoft.Dynamics.Commerce.Runtime.Messages;
using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;

using RR.Commerce.Runtime.Utilities;

namespace RR.Commerce.Runtime.Printing
{
    public sealed class LabelItemManager
    {
        private readonly ICommerceRuntime runtime;

        private LabelItemManager(ICommerceRuntime runtime)
        {
            this.runtime = runtime;
        }

        public static LabelItemManager Create(ICommerceRuntime runtime)
        {
            if (runtime == null)
                throw new ArgumentNullException("runtime");
            return new LabelItemManager(runtime);
        }

        internal TResponse Execute<TResponse>(Request request) where TResponse : Response
        {
#pragma warning disable AvoidBlockingCallsAnalyzer // Avoid blocking asynchronous execution.
            return this.runtime.ExecuteAsync<TResponse>(request, null).ConfigureAwait(false).GetAwaiter().GetResult();
#pragma warning restore AvoidBlockingCallsAnalyzer // Avoid blocking asynchronous execution.
        }

        //

        public int CalculateLabels(LabelLayoutType labelType, LabelItemCalculateParams parameters, LabelItemCalculateCriteria criteria, int pageNumber)
        {
            ThrowIf.Null(criteria, "criteria");
            LabelItemCalculateRequest request = new LabelItemCalculateRequest(labelType);
            request.Criteria = criteria;
            request.Parameters = CreateParams(parameters);
            request.PageNumber = pageNumber;
            LabelItemCalculateResponse licr = Execute<LabelItemCalculateResponse>(request);
            if (licr.HasNext)
                return -1;
            else
                return licr.Count;
        }

        public int CalculateLabels(LabelLayoutType labelType, LabelItemCalculateParams parameters, IEnumerable<LabelItemCalculateItem> items)
        {
            ThrowIf.Null(items, "items");
            LabelItemCalculateRequest request = new LabelItemCalculateRequest(labelType);
            request.Items = IEnumerableExtensions.AsReadOnly<LabelItemCalculateItem>(items);
            request.Parameters = CreateParams(parameters);
            return Execute<LabelItemCalculateResponse>(request).Count;
        }

        internal LabelCalculateParams CreateParams(LabelItemCalculateParams parameters)
        {
            if (parameters == null)
                return LabelCalculateParams.Default();

            GetLabelParamsServiceRequest request = new GetLabelParamsServiceRequest();
            request.CodeRecId = parameters.CodeRecId;
            request.LabelCode = parameters.LabelCode;
            request.PriceDate = parameters.PriceDate;

            return Execute<SingleEntityDataServiceResponse<LabelCalculateParams>>(request).Entity;
        }

        public PagedResult<LabelItem> SearchLabelItems(LabelLayoutType labelType, string keyword, QueryResultSettings settings)
        {
            if (settings == null)
                throw new ArgumentNullException("settings");
            if (settings.Sorting == null || !settings.Sorting.IsSpecified)
                settings.Sorting = new SortingInfo(new SortColumn("ITEMID", true), new SortColumn("INVENTDIMID", true));

            LabelItemSearchDataRequest request = new LabelItemSearchDataRequest(labelType, keyword, settings);
            return Execute<EntityDataServiceResponse<LabelItem>>(request).PagedEntityCollection;
        }

        public ReadOnlyCollection<LabelItemStatistic> CalcLabelStats(LabelLayoutType labelType)
        {
            return Execute<CalcLabelStatisticsResponse>(new CalcLabelStatServiceRequest(labelType)).Entries;
        }

        public PrinterJob FormatLabels(LabelItemStatistic item)
        {
            ThrowIf.Null(item, "item");
            LabelItemFormatRequest request = new LabelItemFormatRequest(item, false);
            return Execute<LabelItemFormatResponse>(request).Job;
        }

        public void CommitLabels(LabelItemStatistic item)
        {
            ThrowIf.Null(item, "item");
            LabelItemCommitRequest request = new LabelItemCommitRequest(item);
            Execute<NullResponse>(request);
        }

        public PrinterJob FormatLabelBatch(LabelItemStatistic item)
        {
            //TODO
            return null;
        }

        public void ConfirmLabelBatch(LabelItemStatistic item)
        {
            //TODO
        }

        //

        public LabelItem GetLabelItem(long recordId)
        {
            return Execute<SingleEntityDataServiceResponse<LabelItem>>(new LabelItemGetDataRequest(recordId)).Entity;
        }

        public LabelItem UpdateLabelItem(LabelItem labeItem)
        {
            if (labeItem == null)
                throw new ArgumentNullException("labeItem");
            return Execute<SingleEntityDataServiceResponse<LabelItem>>(new LabelItemUpdateDataRequest(labeItem)).Entity;
        }

        public void DeleteLabelItem(long recordId)
        {
            Execute<NullResponse>(new LabelItemDeleteDataRequest(recordId));
        }

        public PrinterJob FormatLabelItem(long recordId, int quantity)
        {
            return Execute<LabelItemFormatResponse>(new FormatLabelItemServiceRequest(recordId, quantity)).Job;
        }

        public LabelPrinterTask PrintLabelItem(long recordId, int quantity, int templateNo)
        {
            return Execute<LabelItemFormatResponse>(new PrintLabelItemServiceRequest(recordId, quantity, templateNo)).Task;
        }

        public void ConfirmLabelItem(long recordId)
        {
            Execute<NullResponse>(new ConfirmLabelItemServiceRequest(recordId));
        }

        public void ClearLabelItems(LabelLayoutType labelType)
        {
            Execute<NullResponse>(new LabelItemClearDataRequest(labelType));
        }

        //

        public PagedResult<LabelCode> GetLabelCodes()
        {
            return Execute<EntityDataServiceResponse<LabelCode>>(new GetLabelCodeDataRequest()).PagedEntityCollection;
        }

        public PagedResult<StoreSection> GetStoreSections()
        {
            return Execute<EntityDataServiceResponse<StoreSection>>(new GetStoreSectionDataRequest()).PagedEntityCollection;
        }

        public PagedResult<StoreShelf> GetStoreShelves(string sectionId)
        {
            return Execute<EntityDataServiceResponse<StoreShelf>>(new GetStoreShelfDataRequest(sectionId)).PagedEntityCollection;
        }

        public PagedResult<Category> GetCategoryHierarchy()
        {
            long hierarchyId = Execute<SingleEntityDataServiceResponse<UtilitiesConfiguration>>(new GetUtilitiesConfigurationDataRequest()).Entity.LabelHierarchyRecId;
            return Execute<EntityDataServiceResponse<Category>>(new GetCategoryHierarchyDataRequest(hierarchyId)).PagedEntityCollection;
        }
    }
}
