using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;

using Microsoft.Dynamics.Commerce.Runtime;
//using Microsoft.Dynamics.Commerce.Runtime.Client;
using Microsoft.Dynamics.Commerce.Runtime.DataModel;
using Microsoft.Dynamics.Commerce.Runtime.Messages;
using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;

using RR.Commerce.Runtime.Printing;

namespace RR.Commerce.Runtime.Mobility
{
    public sealed class LabelPrintManager
    {
        private readonly ICommerceRuntime runtime;

        private LabelPrintManager(ICommerceRuntime runtime)
        {
            this.runtime = runtime;
        }

        public static LabelPrintManager Create(ICommerceRuntime runtime)
        {
            if (runtime == null)
                throw new ArgumentNullException("runtime");
            return new LabelPrintManager(runtime);
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

        public PagedResult<LabelPrintItem> Select(LabelLayoutType labelType, QueryResultSettings settings)
        {
            if (settings == null)
                throw new ArgumentNullException("settings");
            if (settings.Sorting == null || !settings.Sorting.IsSpecified)
                settings.Sorting = new SortingInfo(new SortColumn("ITEMID", true), new SortColumn("INVENTDIMID", true));

            LabelItemSearchDataRequest request = new LabelItemSearchDataRequest(labelType, null, settings);
            request.Personal = true;

            PagedResult<LabelItem> labelItems = Execute<EntityDataServiceResponse<LabelItem>>(request).PagedEntityCollection;

            PagedResult<LabelPrintItem> LabelPrintItems = labelItems.ConvertTo<LabelPrintItem>(ConvertToMobile);

            return LabelPrintItems;
        }

        public LabelPrintStat Reset(LabelLayoutType labelType)
        {
            Execute<NullResponse>(new LabelItemClearDataRequest(labelType, true));
            return CalculateStat(labelType);
        }

        public LabelPrintStat GetStat(LabelLayoutType labelType)
        {
            return CalculateStat(labelType);
        }

        public LabelPrintItem FindItem(LabelLayoutType labelType, LabelPrintParams parameters, string barcodeId)
        {
            ThrowIf.NullOrWhiteSpace(barcodeId, "barcodeId");
            return Execute<SingleEntityDataServiceResponse<LabelPrintItem>>(new FindLabelPrintItemServiceRequest(labelType, CreateParams(parameters), barcodeId)).Entity;
        }

        public LabelPrintStat AddItem(LabelLayoutType labelType, LabelPrintParams parameters, LabelPrintItem item)
        {
            ThrowIf.Null<LabelPrintItem>(item, "item");
            Execute<SingleEntityDataServiceResponse<LabelItem>>(new AddLabelPrintItemServiceRequest(labelType, CreateParams(parameters), item));
            return CalculateStat(labelType);
        }

        internal LabelCalculateParams CreateParams(LabelPrintParams parameters)
        {
            if (parameters == null)
                return LabelCalculateParams.Default();

            GetLabelParamsServiceRequest request = new GetLabelParamsServiceRequest();
            request.CodeRecId = parameters.CodeRecId;
            request.LabelCode = parameters.LabelCode;
            request.PriceDate = parameters.PriceDate;

            return Execute<SingleEntityDataServiceResponse<LabelCalculateParams>>(request).Entity;
        }

        public LabelPrintStat UpdateItem(LabelLayoutType labelType, LabelPrintItem item)
        {
            ThrowIf.Null<LabelPrintItem>(item, "item");
            if (item.Quantity > 0)
                Execute<SingleEntityDataServiceResponse<LabelItem>>(new LabelItemUpdateDataRequest(ConvertToLabel(item)));
            else
                Execute<NullResponse>(new LabelItemDeleteDataRequest(item.Id));
            return CalculateStat(labelType);
        }

        public LabelPrintStat DeleteItem(LabelLayoutType labelType, long key)
        {
            Execute<NullResponse>(new LabelItemDeleteDataRequest(key));
            return CalculateStat(labelType);
        }

        public ReadOnlyCollection<LabelItemStatistic> CalcBatch(LabelLayoutType labelType)
        {
            CalcLabelStatServiceRequest request = new CalcLabelStatServiceRequest(labelType, true);
            return Execute<CalcLabelStatisticsResponse>(request).Entries;
        }

        public PrinterJob FormatBatch(LabelItemStatistic item)
        {
            ThrowIf.Null(item, "item");
            LabelItemFormatRequest request = new LabelItemFormatRequest(item, true);
            return Execute<LabelItemFormatResponse>(request).Job;
        }

        public void CommitBatch(LabelItemStatistic item)
        {
            ThrowIf.Null(item, "item");
            LabelItemCommitRequest request = new LabelItemCommitRequest(item);
            Execute<NullResponse>(request);
        }

        //

        public LabelPrintItem GetItem(LabelLayoutType labelType, string barcodeId)
        {
            ThrowIf.NullOrWhiteSpace(barcodeId, "barcodeId");
            return Execute<SingleEntityDataServiceResponse<LabelPrintItem>>(new GetLabelPrintItemServiceRequest(labelType, barcodeId)).Entity;
        }

        public PrinterJob FormatItem(LabelLayoutType labelType, LabelPrintParams parameters, LabelPrintRequest request)
        {
            ThrowIf.Null(request, "request");
            return Execute<LabelItemFormatResponse>(new FormatLabelPrintItemServiceRequest(labelType, CreateParams(parameters), request)).Job;
        }

        public LabelPrinterTask PrintItem(LabelLayoutType labelType, LabelPrintParams parameters, LabelPrintRequest request, int templateNo)
        {
            ThrowIf.Null(request, "request");
            return Execute<LabelItemFormatResponse>(new PrintLabelPrintItemServiceRequest(labelType, CreateParams(parameters), request, templateNo)).Task;
        }

        //

        public PagedResult<LabelCode> GetLabelCodes()
        {
            return Execute<EntityDataServiceResponse<LabelCode>>(new GetLabelCodeDataRequest()).PagedEntityCollection;
        }

        //

        private LabelPrintStat CalculateStat(LabelLayoutType labelType)
        {
            int[] result = Execute<SingleEntityDataServiceResponse<int[]>>(new LabelItemCountDataRequest(labelType, true)).Entity;

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
