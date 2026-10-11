using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Dynamics.Commerce.Runtime;
using Microsoft.Dynamics.Commerce.Runtime.Data;
using Microsoft.Dynamics.Commerce.Runtime.DataModel;
using Microsoft.Dynamics.Commerce.Runtime.Messages;
using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
using Microsoft.Dynamics.Commerce.Runtime.Services.Messages;

using RR.Commerce.Runtime.Printing;

namespace RR.Commerce.Runtime.Mobility
{
    public class LabelPrintService : IRequestHandlerAsync
    {
        public IEnumerable<Type> SupportedRequestTypes
        {
            get
            {
                return new[]
                {
                    typeof(FindLabelPrintItemServiceRequest),
                    typeof(GetLabelPrintItemServiceRequest),
                    typeof(AddLabelPrintItemServiceRequest),
                    typeof(FormatLabelPrintItemServiceRequest),
                    typeof(PrintLabelPrintItemServiceRequest)
                };
            }
        }

        public async Task<Response> Execute(Request request)
        {
            ThrowIf.Null(request, "request");

            Type requestType = request.GetType();
            Response response;

            if (requestType == typeof(FindLabelPrintItemServiceRequest))
                response = await this.FindLabelPrintItem((FindLabelPrintItemServiceRequest)request).ConfigureAwait(false);
            else if (requestType == typeof(GetLabelPrintItemServiceRequest))
                response = await this.GetLabelPrintItem((GetLabelPrintItemServiceRequest)request).ConfigureAwait(false);
            else if (requestType == typeof(AddLabelPrintItemServiceRequest))
                response = await this.AddLabelPrintItem((AddLabelPrintItemServiceRequest)request).ConfigureAwait(false);
            else if (requestType == typeof(FormatLabelPrintItemServiceRequest))
                response = await this.FormatLabelPrintItem((FormatLabelPrintItemServiceRequest)request).ConfigureAwait(false);
            else if (requestType == typeof(PrintLabelPrintItemServiceRequest))
                response = await this.PrintLabelPrintItem((PrintLabelPrintItemServiceRequest)request).ConfigureAwait(false);
            else
                throw new NotSupportedException(String.Format("Request '{0}' is not supported.", request.GetType()));

            return response;
        }

        private async Task<SingleEntityDataServiceResponse<LabelPrintItem>> FindLabelPrintItem(FindLabelPrintItemServiceRequest request)
        {
            ThrowIf.Null(request, "request");
            ThrowIf.NullOrWhiteSpace(request.BarcodeId, "request.BarcodeId");

            ItemBarcode barcode = null;

            GetProductBarcodeDataResponse pbr = await request.RequestContext.ExecuteAsync<GetProductBarcodeDataResponse>(new GetProductBarcodeDataRequest(request.BarcodeId)).ConfigureAwait(false);
            if (pbr != null && pbr.Barcode != null && !string.IsNullOrWhiteSpace(pbr.Barcode.ItemId))
                barcode = pbr.Barcode;
            if (barcode == null)
                throw new NotFoundException(String.Format("Barcode not found for an item ‘{0}’", request.BarcodeId));

            LabelPrintItem item = await GetLabelPrintItem(request.RequestContext, request.LabelType, request.BarcodeId).ConfigureAwait(false);

            LabelItem litem = (await request.RequestContext.ExecuteAsync<SingleEntityDataServiceResponse<LabelItem>>(new LabelItemFindDataRequest(request.LabelType, request.Parameters.CodeRecId, item.ItemId, item.InventDimId, true)).ConfigureAwait(false)).Entity;
            if (litem != null)
            {
                item.Id = litem.Id;
                item.Quantity = litem.NoOfLabels;
            }
            else
                item.Quantity = 1;

            return new SingleEntityDataServiceResponse<LabelPrintItem>(item);
        }

        private async Task<SingleEntityDataServiceResponse<LabelPrintItem>> GetLabelPrintItem(GetLabelPrintItemServiceRequest request)
        {
            ThrowIf.Null(request, "request");
            ThrowIf.NullOrWhiteSpace(request.BarcodeId, "request.BarcodeId");

            LabelPrintItem item = await GetLabelPrintItem(request.RequestContext, request.LabelType, request.BarcodeId).ConfigureAwait(false);

            item.Quantity = 1;

            return new SingleEntityDataServiceResponse<LabelPrintItem>(item);
        }

        private async Task<LabelPrintItem> GetLabelPrintItem(RequestContext requestContext, LabelLayoutType labelType, string barcodeId)
        {
            ItemBarcode barcode = null;

            GetProductBarcodeDataResponse pbr = await requestContext.ExecuteAsync<GetProductBarcodeDataResponse>(new GetProductBarcodeDataRequest(barcodeId)).ConfigureAwait(false);
            if (pbr != null && pbr.Barcode != null && !string.IsNullOrWhiteSpace(pbr.Barcode.ItemId))
                barcode = pbr.Barcode;

            Item item = (await requestContext.ExecuteAsync<GetItemsDataResponse>(new GetItemsDataRequest(new string[] { barcode == null ? barcodeId : barcode.ItemId })).ConfigureAwait(false)).Items.FirstOrDefault();
            if (item == null)
                throw new NotFoundException(String.Format("Item ‘{0}’ not found", barcodeId));


            LabelPrintItem litem = new LabelPrintItem();
            litem.ItemId = item.ItemId;
            litem.ItemName = item.Name;

            if (barcode != null)
            {
                litem.BarcodeId = barcode.ItemBarcodeValue;
                litem.UnitId = barcode.UnitId;

                if (labelType == LabelLayoutType.Product && !String.IsNullOrEmpty(barcode.InventoryDimensionId))
                {
                    GetProductVariantsDataRequest variantRequest = new GetProductVariantsDataRequest(new ItemVariantInventoryDimension[] { new ItemVariantInventoryDimension() { ItemId = barcode.ItemId, VariantInventoryDimensionId = barcode.InventoryDimensionId } });
                    ProductVariant productVariant = (await requestContext.ExecuteAsync<EntityDataServiceResponse<ProductVariant>>(variantRequest).ConfigureAwait(false)).PagedEntityCollection.Results.FirstOrDefault();
                    if (productVariant != null)
                    {
                        litem.VariantId = productVariant.VariantId;
                        litem.InventDimId = productVariant.InventoryDimensionId;
                        litem.InventSizeId = productVariant.SizeId;
                        litem.InventColorId = productVariant.ColorId;
                        litem.InventStyleId = productVariant.StyleId;
                        litem.ConfigId = productVariant.ConfigId;
                    }
                }
            }
            else
            {
                litem.BarcodeId = item.ItemId;
                litem.UnitId = item.SalesUnitOfMeasure;
            }

            return litem;
        }

        //

        private async Task<SingleEntityDataServiceResponse<LabelItem>> AddLabelPrintItem(AddLabelPrintItemServiceRequest request)
        {
            ThrowIf.Null(request, "request");
            ThrowIf.Null(request.Item, "request.Item");

            if (request.Item.Quantity <= 0)
                throw new ValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidQuantity, "Number of Labels must be positive.");

            LabelItem litem = (await request.RequestContext.ExecuteAsync<SingleEntityDataServiceResponse<LabelItem>>(new GetLabelItemServiceRequest(request.LabelType, request.Parameters, request.Item.BarcodeId, request.Item.ItemId, request.Item.InventDimId, request.Item.UnitId)).ConfigureAwait(false)).Entity;
            if (litem == null)
                throw new ValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidOperation, "Label is not defined for item.");

            litem.NoOfLabels = request.Item.Quantity;

            await request.RequestContext.ExecuteAsync<SingleEntityDataServiceResponse<long>>(new LabelItemSaveDataRequest(litem)).ConfigureAwait(false);

            return new SingleEntityDataServiceResponse<LabelItem>(litem);
        }

        private async Task<LabelItemFormatResponse> FormatLabelPrintItem(FormatLabelPrintItemServiceRequest request)
        {
            ThrowIf.Null(request, "request");
            ThrowIf.Null(request.Item, "request.Item");
            return await LabelItemFormat(request.RequestContext, request.LabelType, request.Parameters, request.Item, request.ItemSale, LabelPrinterTemplate.None).ConfigureAwait(false);
        }

        private async Task<LabelItemFormatResponse> PrintLabelPrintItem(PrintLabelPrintItemServiceRequest request)
        {
            ThrowIf.Null(request, "request");
            ThrowIf.Null(request.Item, "request.Item");
            return await LabelItemFormat(request.RequestContext, request.LabelType, request.Parameters, request.Item, request.ItemSale, GetLabelPrinterTemplate(request.TemplateNo)).ConfigureAwait(false);
        }

        private async Task<LabelItemFormatResponse> LabelItemFormat(RequestContext requestContext, LabelLayoutType labelType, LabelCalculateParams parameters, ItemLabelRequest item, LabelItemSale itemSale, LabelPrinterTemplate printerTemplate)
        {
            if (item.Quantity <= 0)
                throw new ValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidQuantity, "Number of Labels must be positive.");

            LabelItem litem = (await requestContext.ExecuteAsync<SingleEntityDataServiceResponse<LabelItem>>(new GetLabelItemServiceRequest(labelType, parameters, item.BarcodeId, item.ItemId, item.InventDimId, item.UnitId)).ConfigureAwait(false)).Entity;
            if (litem == null)
                throw new ValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidOperation, "Label is not defined for item.");

            litem.NoOfLabels = item.Quantity;

            litem = (await requestContext.ExecuteAsync<SingleEntityDataServiceResponse<LabelItem>>(new LabelItemLoadDataRequest(litem)).ConfigureAwait(false)).Entity;
            if (litem == null)
                throw new ValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidOperation, "Label is not defined for item.");

            LabelItemStatistic lstat = (await requestContext.ExecuteAsync<SingleEntityDataServiceResponse<LabelItemStatistic>>(new GetLabelStatServiceRequest(labelType, litem, printerTemplate)).ConfigureAwait(false)).Entity;
            if (lstat == null)
                throw new ValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidOperation, "Label is not defined for item.");
            if (!String.IsNullOrEmpty(lstat.ErrorText))
                throw new ValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidOperation, lstat.ErrorText);
            if (labelType == LabelLayoutType.FireSale && !string.IsNullOrEmpty(litem.Name))
            {
                litem.Name = litem.Name.Length > 40 ? litem.Name.Substring(0, 39) : litem.Name;
            }
            return await requestContext.ExecuteAsync<LabelItemFormatResponse>(new LabelItemFormatRequest(lstat, litem, itemSale, printerTemplate != LabelPrinterTemplate.None, true)).ConfigureAwait(false);
        }

        private LabelPrinterTemplate GetLabelPrinterTemplate(int TemplateNo)
        {
            switch (TemplateNo)
            {
                case 1: return LabelPrinterTemplate.Template1;
                case 2: return LabelPrinterTemplate.Template2;
                case 3: return LabelPrinterTemplate.Template3;
            }
            throw new ValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidOperation, "Invalid Printer Type.");
        }
    }
}
