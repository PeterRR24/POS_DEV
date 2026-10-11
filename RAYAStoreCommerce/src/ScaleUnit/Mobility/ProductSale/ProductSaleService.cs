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
using RR.Commerce.Runtime.Utilities;

namespace RR.Commerce.Runtime.Mobility
{
    public class ProductSaleService: IRequestHandlerAsync
    {
        public IEnumerable<Type> SupportedRequestTypes
        {
            get
            {
                return new[]
                {
                    typeof(GetProductSaleServiceRequest),
                    typeof(CalcProductSaleServiceRequest),
                    typeof(FormatProductSaleServiceRequest),
                    typeof(LabelProductSaleServiceRequest)
                };
            }
        }

        public async Task<Response> Execute(Request request)
        {
            ThrowIf.Null(request, "request");

            Type requestType = request.GetType();
            Response response;

            if (requestType == typeof(GetProductSaleServiceRequest))
                response = await this.GetProductSale((GetProductSaleServiceRequest)request).ConfigureAwait(false);
            else if (requestType == typeof(CalcProductSaleServiceRequest))
                response = await this.CalcProductSale((CalcProductSaleServiceRequest)request).ConfigureAwait(false);
            else if (requestType == typeof(FormatProductSaleServiceRequest))
                response = await this.FormatProductSale((FormatProductSaleServiceRequest)request).ConfigureAwait(false);
            else if (requestType == typeof(LabelProductSaleServiceRequest))
                response = await this.LabelProductSale((LabelProductSaleServiceRequest)request).ConfigureAwait(false);
            else
                throw new NotSupportedException(String.Format("Request '{0}' is not supported.", request.GetType()));

            return response;
        }

        private async Task<SingleEntityDataServiceResponse<ProductSale>> GetProductSale(GetProductSaleServiceRequest request)
        {
            ThrowIf.Null(request, "request");
            ThrowIf.NullOrWhiteSpace(request.BarcodeId, "request.BarcodeId");

            UtilitiesConfiguration utilitiesConfiguration = await GetConfiguration(request.RequestContext).ConfigureAwait(false);

            string barcodeId = request.BarcodeId;

            ProductSaleBarcode psBarcode = FireSaleBarcode.Evaluate(utilitiesConfiguration.FireSaleBarcodePrefix, barcodeId);
            if (psBarcode != null)
                barcodeId = psBarcode.BarcodeId;

            ProductSale product =await GetProductSale(request.RequestContext, barcodeId).ConfigureAwait(false);

            if (psBarcode != null)
                product.DiscPrice = psBarcode.Price;

            return new SingleEntityDataServiceResponse<ProductSale>(product);
        }

        private async Task<SingleEntityDataServiceResponse<ProductSale>> CalcProductSale(CalcProductSaleServiceRequest request)
        {
            ThrowIf.Null(request, "request");
            ThrowIf.NullOrWhiteSpace(request.BarcodeId, "request.BarcodeId");

            UtilitiesConfiguration utilitiesConfiguration = await GetConfiguration(request.RequestContext).ConfigureAwait(false);

            ProductSaleReason reason = null;
            string barcodeId = request.BarcodeId;
            string reasonCode = request.ReasonCode;
            decimal discValue = Decimal.Zero;

            if (!String.IsNullOrEmpty(reasonCode))
            {
                reason = (await request.RequestContext.ExecuteAsync<SingleEntityDataServiceResponse<ProductSaleReason>>(new ProductSaleReasonGetDataRequest(reasonCode)).ConfigureAwait(false)).Entity;
                if (reason == null)
                    throw new ValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_ObjectNotFound, String.Format("Invalid reason '{0}'", reasonCode));
            }

            ProductSaleBarcode psBarcode = FireSaleBarcode.Evaluate(utilitiesConfiguration.FireSaleBarcodePrefix, barcodeId);
            if (psBarcode != null)
                barcodeId = psBarcode.BarcodeId;

            ProductSale product = await GetProductSale(request.RequestContext, barcodeId).ConfigureAwait(false);

            if (reason != null)
            {
                discValue = reason.Discount;
                if (discValue != Decimal.Zero)
                {
                    decimal basePrice = product.Price;
                    decimal discPrice = basePrice * (1 - (discValue / 100));
                    product.DiscPrice = await RoundProductPrice(request.RequestContext, discPrice).ConfigureAwait(false);
                }
            }

            if (discValue == Decimal.Zero)
            {
                if (psBarcode == null)
                    product.DiscPrice = Decimal.Zero;
                else
                    product.DiscPrice = psBarcode.Price;
            }

            return new SingleEntityDataServiceResponse<ProductSale>(product);
        }

        private async Task<SingleEntityDataServiceResponse<string>> FormatProductSale(FormatProductSaleServiceRequest request)
        {
            ThrowIf.Null(request, "request");
            ThrowIf.Null(request.SaleRequest, "request.SaleRequest");

            UtilitiesConfiguration utilitiesConfiguration = await GetConfiguration(request.RequestContext).ConfigureAwait(false);

            ProductSaleBarcode barcode = ParseBarcode(request.SaleRequest);

            string barcodeId = FireSaleBarcode.Format(utilitiesConfiguration.FireSaleBarcodePrefix, barcode);

            return new SingleEntityDataServiceResponse<string>(barcodeId);
        }

        private async Task<ProductSale> GetProductSale(RequestContext requestContext, string barcodeId)
        {
            ItemBarcode barcode = null;
            long ProductID = 0;
            // To get Price Embedded iin Barcode
            // GetBarcodeRequest br = new GetBarcodeRequest(new ScanInfo { ScannedText = barcodeId });

            GetProductBarcodeDataResponse pbr = await requestContext.ExecuteAsync<GetProductBarcodeDataResponse>(new GetProductBarcodeDataRequest(barcodeId)).ConfigureAwait(false);
            if (pbr != null && pbr.Barcode != null && !string.IsNullOrWhiteSpace(pbr.Barcode.ItemId))
                barcode = pbr.Barcode;

            Item item = (await requestContext.ExecuteAsync<GetItemsDataResponse>(new GetItemsDataRequest(new string[] { barcode == null ? barcodeId : barcode.ItemId })).ConfigureAwait(false)).Items.FirstOrDefault();
            if (item == null)
                throw new NotFoundException(String.Format("Item ‘{0}’ not found", barcodeId));

            ProductSale product = new ProductSale();
            product.ItemId = item.ItemId;
            product.ItemType = ItemType.Item;
            product.Description = item.Name;
            ProductID = item.Product;

            if (barcode != null)
            {
                product.BarcodeId = barcode.ItemBarcodeValue;
                product.UnitId = barcode.UnitId;

                if (!String.IsNullOrEmpty(barcode.InventoryDimensionId))
                {
                    GetProductVariantsDataRequest variantRequest = new GetProductVariantsDataRequest(new ItemVariantInventoryDimension[] { new ItemVariantInventoryDimension() { ItemId = barcode.ItemId, VariantInventoryDimensionId = barcode.InventoryDimensionId } });
                    ProductVariant productVariant = (await requestContext.ExecuteAsync<EntityDataServiceResponse<ProductVariant>>(variantRequest).ConfigureAwait(false)).PagedEntityCollection.Results.FirstOrDefault();
                    if (productVariant != null)
                    {
                        product.VariantId = productVariant.VariantId;
                        product.InventDimId = productVariant.InventoryDimensionId;
                        product.SizeId = productVariant.SizeId;
                        product.ColorId = productVariant.ColorId;
                        product.StyleId = productVariant.StyleId;
                        product.ConfigId = productVariant.ConfigId;
                        product.ColorName = productVariant.Color;
                        product.SizeName = productVariant.Size;
                        product.StyleName = productVariant.Style;
                        product.ConfigName = productVariant.Configuration;
                        ProductID = productVariant.DistinctProductVariantId;
                    }
                }
            }
            else
            {
                product.BarcodeId = item.ItemId;
                product.UnitId = item.InventoryUnitOfMeasure;
            }

            await CalcProductPrice(requestContext, product).ConfigureAwait(false);
            await CalcProductAvailability(requestContext, product, ProductID).ConfigureAwait(false);
            await CalcProductFaces(requestContext, product).ConfigureAwait(false);
            return product;
        }

        private async Task CalcProductPrice(RequestContext requestContext, ProductSale product)
        {
            /*
            if (barcode.BarcodePrice <= Decimal.Zero)
            {
                ItemPriceResult price = SM.Pricing.Calculate(item, variant, barcode.UnitId);
                product.Price = price.Price;
                product.DiscPrice = price.AdjustedPrice;
            }
            else
                product.Price = barcode.BarcodePrice;
            */

            SalesLine salesLine = new SalesLine
            {
                LineId = System.Guid.NewGuid().ToString("N"),
                ItemId = product.ItemId,
                InventoryDimensionId = product.InventDimId,
                SalesOrderUnitOfMeasure = product.UnitId,
                Quantity = 1m
            };

            if (product.BarcodeId != product.ItemId)
                salesLine.Barcode = product.BarcodeId;

            SalesTransaction salesTransaction = new SalesTransaction();
            salesTransaction.SalesLines.Add(salesLine);

            // GetPriceServiceRequest request = new GetPriceServiceRequest(salesTransaction);
            GetIndependentPriceDiscountServiceRequest request = new GetIndependentPriceDiscountServiceRequest(salesTransaction);
            GetPriceServiceResponse response = await requestContext.ExecuteAsync<GetPriceServiceResponse>(request).ConfigureAwait(false);
            SalesLine resultLine = response.Transaction.SalesLines[0];

            product.Price = resultLine.Price;
            product.DiscPrice = resultLine.AdjustedPrice;
        }

        private async Task CalcProductAvailability(RequestContext requestContext, ProductSale product, long productID)
        {
            product.AvailableQty = Decimal.ToInt32((await requestContext.ExecuteAsync<SingleEntityDataServiceResponse<decimal>>(new GetItemVariantAvailableQtyDataRequest(product.ItemId, product.InventDimId, productID)).ConfigureAwait(false)).Entity);
        }

        private async Task CalcProductFaces(RequestContext requestContext, ProductSale product)
        {
            product.Faces = (await requestContext.ExecuteAsync<GetItemStoreLocationFacesResponse>(new GetItemStoreLocationFacesDataRequest(product.ItemId, product.InventDimId)).ConfigureAwait(false)).Faces;
        }

        private async Task<decimal> RoundProductPrice(RequestContext requestContext, decimal price)
        {
            string currencyCode = requestContext.GetChannelConfiguration().Currency;
            return (await requestContext.ExecuteAsync<GetRoundedValueServiceResponse>(new GetRoundedValueServiceRequest(price, currencyCode)).ConfigureAwait(false)).RoundedValue;
        }

        private ProductSaleBarcode ParseBarcode(ProductSaleRequest request)
        {
            ProductSaleBarcode barcode = new ProductSaleBarcode();
            barcode.BarcodeId = request.BarcodeId;
            barcode.ReasonCode = request.ReasonCode;
            barcode.Price = Convert.ToDecimal(request.Price);
            if (request.ExpDate > 0)
            {
                int year = request.ExpDate / 1000;
                int day = request.ExpDate % 1000;
                barcode.ExpDate = new DateTime(year, 1, 1).AddDays(day - 1);
            }
            return barcode;
        }

        //

        private async Task<SingleEntityDataServiceResponse<LabelPrinterTask>> LabelProductSale(LabelProductSaleServiceRequest request)
        {
            ThrowIf.Null(request, "request");
            ThrowIf.Null(request.SaleRequest, "request.SaleRequest");

            UtilitiesConfiguration utilitiesConfiguration = await GetConfiguration(request.RequestContext).ConfigureAwait(false);

            ProductSaleRequest saleRequest = request.SaleRequest;

            ProductSaleReason reason = null;
            if (!String.IsNullOrEmpty(saleRequest.ReasonCode))
            {
                reason = (await request.RequestContext.ExecuteAsync<SingleEntityDataServiceResponse<ProductSaleReason>>(new ProductSaleReasonGetDataRequest(saleRequest.ReasonCode)).ConfigureAwait(false)).Entity;
                if (reason == null)
                    throw new ValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_ObjectNotFound, String.Format("Invalid reason '{0}'", saleRequest.ReasonCode));
            }

            ProductSaleBarcode barcode = ParseBarcode(saleRequest);

            ItemLabelRequest itemLabel = new ItemLabelRequest();
            itemLabel.BarcodeId = FireSaleBarcode.Format(utilitiesConfiguration.FireSaleBarcodePrefix, barcode);
            itemLabel.ItemId = saleRequest.ItemId;
            itemLabel.InventDimId = saleRequest.InventDimId;
            itemLabel.UnitId = saleRequest.UnitId;
            itemLabel.Quantity = request.NoOfLabels;

            LabelItemSale labelItemSale = new LabelItemSale();
            labelItemSale.Price = barcode.Price;
            labelItemSale.ReasonId = barcode.ReasonCode;
            labelItemSale.ExpiryDate = barcode.ExpDate;
            labelItemSale.ReasonName = reason == null ? null : reason.Name;

            LabelCalculateParams parameters = new LabelCalculateParams(reason == null ? 0 : reason.LabelCodeId, DateTime.Today);

            PrintLabelPrintItemServiceRequest plsRequest = new PrintLabelPrintItemServiceRequest(LabelLayoutType.FireSale, parameters, itemLabel, request.TemplateNo);
            plsRequest.ItemSale = labelItemSale;
            LabelPrinterTask labelPrinterTask = (await request.RequestContext.ExecuteAsync<LabelItemFormatResponse>(plsRequest).ConfigureAwait(false)).Task;

            return new SingleEntityDataServiceResponse<LabelPrinterTask>(labelPrinterTask);
        }

        private async Task<UtilitiesConfiguration> GetConfiguration(RequestContext requestContext)
        {
            return (await requestContext.ExecuteAsync<SingleEntityDataServiceResponse<UtilitiesConfiguration>>(new GetUtilitiesConfigurationDataRequest()).ConfigureAwait(false)).Entity;
        }
    }
}
