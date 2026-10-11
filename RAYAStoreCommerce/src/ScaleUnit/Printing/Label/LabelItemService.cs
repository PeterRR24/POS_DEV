using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Dynamics.Commerce.Runtime;
using Microsoft.Dynamics.Commerce.Runtime.Data;
using Microsoft.Dynamics.Commerce.Runtime.DataModel;
using Microsoft.Dynamics.Commerce.Runtime.Messages;
using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;

using RR.Commerce.Runtime.Utilities;

namespace RR.Commerce.Runtime.Printing
{
    public class LabelItemService : IRequestHandlerAsync
    {
        private static LabelFormat defaultLabelFormat = LabelFormat.Default();

        public IEnumerable<Type> SupportedRequestTypes
        {
            get
            {
                return new[]
                {
                    typeof(GetLabelProfileServiceRequest),
                    typeof(GetLabelItemServiceRequest),
                    typeof(CalcLabelItemServiceRequest),
                    typeof(FormatLabelItemServiceRequest),
                    typeof(ConfirmLabelItemServiceRequest),
                    typeof(PrintLabelItemServiceRequest),
                    //typeof(ConfirmLabelItemServiceRequest),
                    typeof(GetLabelStatServiceRequest),
                    typeof(CalcLabelStatServiceRequest),
                    typeof(TrackLabelItemServiceRequest),
                    typeof(GetLabelParamsServiceRequest)
                };
            }
        }

        public async Task<Response> Execute(Request request)
        {
            if (request == null)
                throw new ArgumentNullException("request");

            Type requestType = request.GetType();
            Response response;

            if (requestType == typeof(GetLabelProfileServiceRequest))
                response = await this.GetLabelProfile((GetLabelProfileServiceRequest)request).ConfigureAwait(false);
            else if (requestType == typeof(GetLabelItemServiceRequest))
                response = await this.GetLabelItem((GetLabelItemServiceRequest)request).ConfigureAwait(false);
            else if (requestType == typeof(CalcLabelItemServiceRequest))
                response = await this.CalcLabelItem((CalcLabelItemServiceRequest)request).ConfigureAwait(false);
            else if (requestType == typeof(FormatLabelItemServiceRequest))
                response = await this.FormatLabelItem((FormatLabelItemServiceRequest)request).ConfigureAwait(false);
            else if (requestType == typeof(PrintLabelItemServiceRequest))
                response = await this.PrintLabelItem((PrintLabelItemServiceRequest)request).ConfigureAwait(false);
            else if (requestType == typeof(ConfirmLabelItemServiceRequest))
                response = await this.ConfirmLabelItem((ConfirmLabelItemServiceRequest)request).ConfigureAwait(false);
            else if (requestType == typeof(GetLabelStatServiceRequest))
                response = await this.GetLabelStat((GetLabelStatServiceRequest)request).ConfigureAwait(false);
            else if (requestType == typeof(CalcLabelStatServiceRequest))
                response = await this.CalcLabelStat((CalcLabelStatServiceRequest)request).ConfigureAwait(false);
            else if (requestType == typeof(TrackLabelItemServiceRequest))
                response = await this.TrackLabelItem((TrackLabelItemServiceRequest)request).ConfigureAwait(false);
            else if (requestType == typeof(GetLabelParamsServiceRequest))
                response = await this.GetLabelParams((GetLabelParamsServiceRequest)request).ConfigureAwait(false);
            else
                throw new NotSupportedException(String.Format("Request '{0}' is not supported.", request.GetType()));

            return response;
        }

        private async Task<GetLabelProfileServiceResponse> GetLabelProfile(GetLabelProfileServiceRequest request)
        {
            ThrowIf.Null(request, "request");

            GetLabelProfileServiceResponse response = new GetLabelProfileServiceResponse();

            LabelProfile profile = (await request.RequestContext.ExecuteAsync<SingleEntityDataServiceResponse<LabelProfile>>(new GetLabelProfileDataRequest(request.ProductId)).ConfigureAwait(false)).Entity;
            if (profile != null)
            {
                response.Profile = (await request.RequestContext.ExecuteAsync<SingleEntityDataServiceResponse<LabelProfileLine>>(new GetLabelProfileLineDataRequest(profile.Id, request.LayoutType, request.CodeRecId)).ConfigureAwait(false)).Entity;
                if (response.Profile != null)
                {
                    if (response.Profile.PrinterId == 0)
                        response.Profile.PrinterId = profile.PrinterId;

                    if (response.Profile.FormatId != 0)
                        response.Format = (await request.RequestContext.ExecuteAsync<SingleEntityDataServiceResponse<LabelFormat>>(new GetLabelFormatDataRequest(response.Profile.FormatId)).ConfigureAwait(false)).Entity;
                    if (response.Format == null)
                        response.Format = defaultLabelFormat;
                }
            }

            return response;
        }

        //

        private async Task<SingleEntityDataServiceResponse<LabelItem>> GetLabelItem(GetLabelItemServiceRequest request)
        {
            ThrowIf.Null(request, "request");
            ThrowIf.NullOrWhiteSpace(request.ItemId, "request.ItemId");

            LabelItem litem = null;

            Item item = (await request.RequestContext.ExecuteAsync<GetItemsDataResponse>(new GetItemsDataRequest(new string[] { request.ItemId })).ConfigureAwait(false)).Items.FirstOrDefault();
            if (item != null)
            {
                LabelItemCalculator calculator = new LabelItemCalculator(request.RequestContext);

                GetLabelProfileServiceResponse lbsr = (await request.RequestContext.ExecuteAsync<GetLabelProfileServiceResponse>(new GetLabelProfileServiceRequest(request.LabelType, request.Parameters.CodeRecId, item.Product)).ConfigureAwait(false));

                //TEST
                if (request.Parameters.CodeRecId < 0)
                {
                    lbsr = new GetLabelProfileServiceResponse();
                    lbsr.Profile = new LabelProfileLine();
                    lbsr.Profile.LayoutId = -1;
                    lbsr.Profile.PrinterId = -1;
                    lbsr.Profile.FormatId = -1;
                    lbsr.Format = LabelFormat.Default();
                }
                //

                if (lbsr.Profile != null && lbsr.Format != null && lbsr.Profile.Active)
                {
                    ProductVariant variant = null;
                    if (request.LabelType == LabelLayoutType.Product && !String.IsNullOrEmpty(request.InventDimId))
                        variant = (await request.RequestContext.ExecuteAsync<EntityDataServiceResponse<ProductVariant>>(new GetProductVariantsDataRequest(new ItemVariantInventoryDimension[] { new ItemVariantInventoryDimension() { ItemId = request.ItemId, VariantInventoryDimensionId = request.InventDimId } })).ConfigureAwait(false)).FirstOrDefault();

                    litem = await calculator.CreateLabelItem(request.LabelType, request.Parameters, true, item, variant, lbsr.Profile).ConfigureAwait(false);

                    litem.UnitId = request.UnitId;
                    litem.Barcode = request.BarcodeId;

                    await calculator.CalculateLabelItemPrice(litem, lbsr.Format).ConfigureAwait(false);

                    await CalcLabelItem(request.RequestContext, litem).ConfigureAwait(false);
                }
            }

            return new SingleEntityDataServiceResponse<LabelItem>(litem);
        }

        private async Task<NullResponse> CalcLabelItem(CalcLabelItemServiceRequest request)
        {
            ThrowIf.Null(request, "request");
            ThrowIf.Null(request.Item, "request.Item");
            await CalcLabelItem(request.RequestContext, request.Item).ConfigureAwait(false);
            //return new NullResponse();
            return NullResponse.Instance;
        }

        private async Task CalcLabelItem(RequestContext context, LabelItem item)
        {
            await GetLabelData(context, item).ConfigureAwait(false);
            await CalcLabelItemUoms(context, item).ConfigureAwait(false);
            GetLabelItemLocation(context, item);
            await GetLabelItemCategories(context, item).ConfigureAwait(false);
        }

        private async Task GetLabelData(RequestContext context, LabelItem item)
        {
            GetLabelDataDataRequest request = new GetLabelDataDataRequest(item.ItemId, item.VariantId);
            LabelData labelData = (await context.ExecuteAsync<SingleEntityDataServiceResponse<LabelData>>(request).ConfigureAwait(false)).Entity;
            if (labelData != null)
            {
                item.DataRecId = labelData.Id;
                item.EquivUnitId = labelData.EquivUnitId;
            }
        }

        private async Task CalcLabelItemUoms(RequestContext context, LabelItem item)
        {
            if (String.IsNullOrEmpty(item.UnitId))
                return;

            List<ItemUnitConversion> uomConversions = new List<ItemUnitConversion>();

            if (!String.IsNullOrEmpty(item.BaseUnitId) && !String.Equals(item.UnitId, item.BaseUnitId, StringComparison.OrdinalIgnoreCase))
            {
                ItemUnitConversion uomConversion = new ItemUnitConversion
                {
                    FromUnitOfMeasure = item.UnitId,
                    ToUnitOfMeasure = item.BaseUnitId,
                    ItemId = item.ItemId
                };
                uomConversions.Add(uomConversion);
            }

            if (!String.IsNullOrEmpty(item.EquivUnitId) && !String.Equals(item.UnitId, item.EquivUnitId, StringComparison.OrdinalIgnoreCase) && !String.Equals(item.BaseUnitId, item.EquivUnitId, StringComparison.OrdinalIgnoreCase))
            {
                ItemUnitConversion uomConversion = new ItemUnitConversion
                {
                    FromUnitOfMeasure = item.UnitId,
                    ToUnitOfMeasure = item.EquivUnitId,
                    ItemId = item.ItemId
                };
                uomConversions.Add(uomConversion);
            }

            ReadOnlyCollection<UnitOfMeasureConversion> uomConverters;
            UnitOfMeasureConversion uomConverter;

            if (uomConversions.Any())
            {
                GetUnitOfMeasureConversionDataRequest request = new GetUnitOfMeasureConversionDataRequest(uomConversions, QueryResultSettings.AllRecords);
                uomConverters = (await context.ExecuteAsync<GetUnitOfMeasureConversionDataResponse>(request).ConfigureAwait(false)).UnitConversions.Results;
            }
            else
                uomConverters = new List<UnitOfMeasureConversion>().AsReadOnly();

            if (!String.IsNullOrEmpty(item.BaseUnitId))
            {
                uomConverter = uomConverters.SingleOrDefault(u => String.Equals(u.ToUnitOfMeasureSymbol, item.BaseUnitId, StringComparison.OrdinalIgnoreCase));
                if (uomConverter == null)
                    item.BaseUnitQty = Decimal.One;
                else
                    item.BaseUnitQty = uomConverter.GetFactorForQuantity(Decimal.One);
            }

            if (!String.IsNullOrEmpty(item.EquivUnitId))
            {
                uomConverter = uomConverters.SingleOrDefault(u => String.Equals(u.ToUnitOfMeasureSymbol, item.EquivUnitId, StringComparison.OrdinalIgnoreCase));
                if (uomConverter == null)
                    item.EquivUnitQty = Decimal.One;
                else
                    item.EquivUnitQty = uomConverter.GetFactorForQuantity(Decimal.One);
            }
        }

        private void GetLabelItemLocation(RequestContext context, LabelItem item)
        {
            //24.09.18 - Disabled
            /*
            GetItemStoreLocationDataRequest request = new GetItemStoreLocationDataRequest(item.ItemId);
            ItemStoreLocation itemLocation = context.Execute<SingleEntityDataServiceResponse<ItemStoreLocation>>(request).Entity;
            if (itemLocation != null)
            {
                item.SectionId = itemLocation.SectionId;
                item.ShelfId = itemLocation.ShelfId;
            }
            */
        }

        private async Task GetLabelItemCategories(RequestContext context, LabelItem item)
        {
            if (item.ProductId == 0)
                return;

            UtilitiesConfiguration utilitiesConfiguration = (UtilitiesConfiguration)context.GetProperty("UtilitiesConfiguration");
            if (utilitiesConfiguration == null || utilitiesConfiguration.LabelHierarchyRecId == 0)
                return;

            GetProductCategoryHierarchyDataRequest request = new GetProductCategoryHierarchyDataRequest(item.ProductId, utilitiesConfiguration.LabelHierarchyRecId);
            ReadOnlyCollection<ProductCategoryHierarchy> categories = (await context.ExecuteAsync<EntityDataServiceResponse<ProductCategoryHierarchy>>(request).ConfigureAwait(false)).PagedEntityCollection.Results;
            foreach (ProductCategoryHierarchy category in categories)
            {
                item.Category = item.Department;
                item.Department = category.Name;
            }
        }

        // 

        private async Task<LabelItemFormatResponse> FormatLabelItem(FormatLabelItemServiceRequest request)
        {
            ThrowIf.Null(request, "request");
            return await LabelItemFormat(request.RequestContext, request.RecordId, request.Quantity, LabelPrinterTemplate.None).ConfigureAwait(false);
        }

        private async Task<LabelItemFormatResponse> PrintLabelItem(PrintLabelItemServiceRequest request)
        {
            ThrowIf.Null(request, "request");
            return await LabelItemFormat(request.RequestContext, request.RecordId, request.Quantity, GetLabelPrinterTemplate(request.TemplateNo)).ConfigureAwait(false);
        }

        private async Task<LabelItemFormatResponse> LabelItemFormat(RequestContext requestContext, long recordId, int quantity, LabelPrinterTemplate printerTemplate)
        {
            if (quantity <= 0)
                throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidQuantity, "Number of Labels must be positive.");

            LabelItem litem = (await requestContext.ExecuteAsync<SingleEntityDataServiceResponse<LabelItem>>(new LabelItemGetDataRequest(recordId)).ConfigureAwait(false)).Entity;
            if (litem == null)
                throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidOperation, "Label is not defined for item.");

            litem.NoOfLabels = quantity;
            
            LabelItemStatistic lstat = (await requestContext.ExecuteAsync<SingleEntityDataServiceResponse<LabelItemStatistic>>(new GetLabelStatServiceRequest(litem.LabelType, litem, printerTemplate)).ConfigureAwait(false)).Entity;
            if (lstat == null)
                throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidOperation, "Label is not defined for item.");
            if (!String.IsNullOrEmpty(lstat.ErrorText))
                throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidOperation, lstat.ErrorText);

            return await requestContext.ExecuteAsync<LabelItemFormatResponse>(new LabelItemFormatRequest(lstat, litem, null, printerTemplate != LabelPrinterTemplate.None, false)).ConfigureAwait(false);
        }

        private LabelPrinterTemplate GetLabelPrinterTemplate(int TemplateNo)
        {
            switch (TemplateNo)
            {
                case 1: return LabelPrinterTemplate.Template1;
                case 2: return LabelPrinterTemplate.Template2;
                case 3: return LabelPrinterTemplate.Template3;
            }
            throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidOperation, "Invalid Printer Type.");
        }

        private async Task<NullResponse> ConfirmLabelItem(ConfirmLabelItemServiceRequest request)
        {
            ThrowIf.Null(request, "request");
            long[] recIds = request.RecordIds == null ? new long[] { } : request.RecordIds.ToArray();
            if (recIds.Any())
            {
                for (int i = 0; i < recIds.Length; i++)
                {
                    await request.RequestContext.ExecuteAsync<NullResponse>(new LabelHistorySaveDataRequest(recIds[i])).ConfigureAwait(false);
                    await request.RequestContext.ExecuteAsync<NullResponse>(new LabelItemClearDataRequest(recIds[i])).ConfigureAwait(false);
                }
            }
            else
            {
                await request.RequestContext.ExecuteAsync<NullResponse>(new LabelHistorySaveDataRequest(request.RecordId)).ConfigureAwait(false);
                await request.RequestContext.ExecuteAsync<NullResponse>(new LabelItemClearDataRequest(request.RecordId)).ConfigureAwait(false);
            }
            //return new NullResponse();
            return NullResponse.Instance;
        }

        //

        private async Task<SingleEntityDataServiceResponse<LabelItemStatistic>> GetLabelStat(GetLabelStatServiceRequest request)
        {
            ThrowIf.Null(request, "request");
            ThrowIf.Null(request.Item, "request.Item");

            LabelItemStatistic lstat = new LabelItemStatistic();
            lstat.LabelType = request.LabelType;
            lstat.LayoutRecId = request.Item.LayoutlId;
            lstat.PrinterRecId = request.Item.PrinterId;
            lstat.NoOfLabels = request.Item.NoOfLabels;

            LabelStatCalculator calculator = new LabelStatCalculator(request.RequestContext);

            await calculator.Validate(lstat, request.Template).ConfigureAwait(false);

            return new SingleEntityDataServiceResponse<LabelItemStatistic>(lstat);
        }

        private async Task<CalcLabelStatisticsResponse> CalcLabelStat(CalcLabelStatServiceRequest request)
        {
            ThrowIf.Null(request, "request");

            LabelStatCalculator calculator = new LabelStatCalculator(request.RequestContext);

            ReadOnlyCollection<LabelItemStatistic> list = (await request.RequestContext.ExecuteAsync<CalcLabelStatisticsResponse>(new LabelItemStatisticsDataRequest(request.LabelType, request.Personal)).ConfigureAwait(false)).Entries;
            foreach (LabelItemStatistic item in list)
            {
                await calculator.Validate(item).ConfigureAwait(false);
            }

            return new CalcLabelStatisticsResponse(list);
        }

        //

        private async Task<SingleEntityDataServiceResponse<bool>> TrackLabelItem(TrackLabelItemServiceRequest request)
        {
            ThrowIf.Null(request, "request");
            ThrowIf.Null(request.Item, "request.Item");
            ThrowIf.Null(request.Format, "request.Format");
            return new SingleEntityDataServiceResponse<bool>(await TrackLabelItem(request.RequestContext, request.Item, request.Format, request.ChangeTracking).ConfigureAwait(false));
        }

        private async Task<bool> TrackLabelItem(RequestContext context, LabelItem litem, LabelFormat format, LabelItemChangeTracking changeTracking)
        {
            if (changeTracking == LabelItemChangeTracking.None)
                return true;

            if (changeTracking == LabelItemChangeTracking.OldPrice)
            {
                if (litem.UnitPriceOld.HasValue)
                    return litem.UnitPrice != litem.UnitPriceOld;
                else
                    return false;
            }

            LabelHistory lhistory = (await context.ExecuteAsync<SingleEntityDataServiceResponse<LabelHistory>>(new LabelHistoryGetDataRequest(litem)).ConfigureAwait(false)).Entity;
            if (lhistory == null)
                return true;

            if (lhistory.LayoutlId != litem.LayoutlId || lhistory.FormatId != litem.FormatId)
                return true;

            if (format.Price && lhistory.UnitPrice != litem.UnitPrice)
                return true;

            if (format.PriceCurrency && lhistory.UnitPriceCurrency != (litem.UnitPriceCurrency.HasValue ? litem.UnitPriceCurrency.Value : Decimal.Zero))
                return true;

            if (changeTracking == LabelItemChangeTracking.PriceAndName)
                return String.Compare(lhistory.Barcode, litem.Barcode, StringComparison.OrdinalIgnoreCase) != 0 ||
                    String.Compare(lhistory.Name, litem.Name, StringComparison.OrdinalIgnoreCase) != 0 ||
                    String.Compare(lhistory.Description, litem.Description, StringComparison.OrdinalIgnoreCase) != 0;
            else
                return false;
        }

        //

        private async Task<SingleEntityDataServiceResponse<LabelCalculateParams>> GetLabelParams(GetLabelParamsServiceRequest request)
        {
            LabelCalculateParams parameters = new LabelCalculateParams();

            if (request.PriceDate.HasValue)
                parameters.PriceDate = request.RequestContext.ConvertDateTimeToChannelDate(request.PriceDate.Value).DateTime;
            else
                parameters.PriceDate = request.RequestContext.GetNowInChannelTimeZone().DateTime;
            //parameters.PriceDate = request.PriceDate.HasValue ? request.PriceDate.Value : DateTime.Today;

            if (request.CodeRecId == 0)
            {
                if (!String.IsNullOrEmpty(request.LabelCode))
                {
                    PagedResult<LabelCode> labelCodes = (await request.RequestContext.ExecuteAsync<EntityDataServiceResponse<LabelCode>>(new GetLabelCodeDataRequest()).ConfigureAwait(false)).PagedEntityCollection;
                    if (labelCodes != null)
                    {
                        LabelCode labelCode = labelCodes.SingleOrDefault(c => String.Compare(c.Code, request.LabelCode, StringComparison.OrdinalIgnoreCase) == 0);
                        if (labelCode != null)
                            parameters.CodeRecId = labelCode.Id;
                    }
                }
            }
            else
                parameters.CodeRecId = request.CodeRecId;

            return new SingleEntityDataServiceResponse<LabelCalculateParams>(parameters);
        }
    }
}
