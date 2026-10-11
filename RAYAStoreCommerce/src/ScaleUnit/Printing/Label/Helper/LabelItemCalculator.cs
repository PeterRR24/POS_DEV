using System;
using System.Threading.Tasks;

using Microsoft.Dynamics.Commerce.Runtime;
using Microsoft.Dynamics.Commerce.Runtime.DataModel;
using Microsoft.Dynamics.Commerce.Runtime.Messages;
using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
using Microsoft.Dynamics.Commerce.Runtime.Services.Messages;

using RR.Commerce.Runtime.Utilities;

namespace RR.Commerce.Runtime.Printing
{
    internal class LabelItemCalculator
    {
        private RequestContext Context;
        private UtilitiesConfiguration Config;

        public LabelItemCalculator(RequestContext context)
        {
            ThrowIf.Null(context, "context");

            Context = context;

#pragma warning disable AvoidBlockingCallsAnalyzer // Avoid blocking asynchronous execution.
            Config = context.ExecuteAsync<SingleEntityDataServiceResponse<UtilitiesConfiguration>>(new GetUtilitiesConfigurationDataRequest()).ConfigureAwait(false).GetAwaiter().GetResult().Entity;
#pragma warning restore AvoidBlockingCallsAnalyzer // Avoid blocking asynchronous execution.

            context.SetProperty("UtilitiesConfiguration", Config);
        }

        public UtilitiesConfiguration Configuration { get { return Config; }  }

        public async Task<LabelItem> CreateLabelItem(LabelLayoutType labelType, LabelCalculateParams parameters, bool personal, Item item, ProductVariant variant, LabelProfileLine profile)
        {
            LabelItem litem = new LabelItem();

            litem.Init(this.Context.GetPrincipal().ChannelId, this.Context.GetChannelConfiguration().InventLocationDataAreaId, personal ? this.Context.GetPrincipal().DeviceNumber : String.Empty);
            litem.LabelType = labelType;

            litem.ItemId = item.ItemId;
            litem.ProductId = item.Product;
            litem.Name = item.Name;
            litem.Description = item.Description;
            litem.UnitId = item.SalesUnitOfMeasure;
            litem.BaseUnitId = item.InventoryUnitOfMeasure;

            if (variant != null)
            {
                litem.CopyVariant(variant);
                //---//---GetProductsServiceRequest getProductsServiceRequest = new GetProductsServiceRequest(channelId, productIds, inventoryLocationId, settings);
                GetProductsDataRequest getProductsDataRequest = new GetProductsDataRequest(new long[] { variant.DistinctProductVariantId }, QueryResultSettings.AllRecords);
                PagedResult<SimpleProduct> productCollection = (await this.Context.ExecuteAsync<EntityDataServiceResponse<SimpleProduct>>(getProductsDataRequest).ConfigureAwait(false)).PagedEntityCollection;
                if (productCollection.Results.Count > 0)
                {
                    var productResult = productCollection.Results[0];
                    litem.Name = productResult.Name;
                    litem.Description = productResult.Description;
                }
            }

            litem.LayoutlId = profile.LayoutId;
            litem.PrinterId = profile.PrinterId;
            litem.FormatId = profile.FormatId;

            litem.CodeRecId = parameters.CodeRecId;
            litem.PriceDate = parameters.PriceDate;

            return litem;
        }

        public async Task<bool> CalculateLabelItem(LabelItem litem, LabelFormat format)
        {
            ItemBarcode ib = (await this.Context.ExecuteAsync<SingleEntityDataServiceResponse<ItemBarcode>>(new GetBarcodeForProductDataRequest(litem.ItemId, litem.InventDimId, litem.UnitId)).ConfigureAwait(false)).Entity;
            if (ib == null)
            {
                if (litem.LabelType == LabelLayoutType.Product)
                    return false;
            }
            else
                litem.Barcode = ib.ItemBarcodeValue;
            /*
            if (litem.LabelType == LabelLayoutType.Product)
            {
                ItemBarcode ib = this.Context.Execute<SingleEntityDataServiceResponse<ItemBarcode>>(new GetBarcodeForProductDataRequest(litem.ItemId, litem.InventDimId, litem.UnitId)).Entity;
                if (ib == null)
                    return false;
                litem.Barcode = ib.ItemBarcodeValue;
            }
            */

            await CalculateLabelItemPrice(litem, format).ConfigureAwait(false);

            await this.Context.ExecuteAsync<NullResponse>(new CalcLabelItemServiceRequest(litem)).ConfigureAwait(false);

            return true;
        }

        public async Task CalculateLabelItemPrice(LabelItem litem, LabelFormat format)
        {
            if (format.Price)
            {
                SalesLine salesLine = await CalculateLabelItemPrice(litem, litem.PriceDate).ConfigureAwait(false);
                if (format.SalePrice)
                {
                    litem.UnitPrice = salesLine.AgreementPrice - salesLine.DiscountAmount;
                    if (litem.UnitPrice != salesLine.Price)
                        litem.SalePrice = salesLine.Price;
                }
                else
                    litem.UnitPrice = salesLine.Price;

                if (format.OldPrice)
                {
                    //salesLine = await CalculateLabelItemPrice(litem, litem.PriceDate.AddDays(-1)).ConfigureAwait(false);
                    //if (format.SalePrice)
                    //    litem.UnitPriceOld = salesLine.AgreementPrice - salesLine.DiscountAmount;
                    //else
                    //    litem.UnitPriceOld = salesLine.Price;
                    litem.UnitPriceOld = salesLine.Price;
                }

                if (format.PriceCurrency && !String.IsNullOrEmpty(Config.LabelCurrencyCode))
                {
                    litem.UnitPriceCurrency = await CalculateLabelItemPriceInCurrency(litem, Config.LabelCurrencyCode).ConfigureAwait(false);
                }
            }
        }

        private async Task<SalesLine> CalculateLabelItemPrice(LabelItem litem, DateTimeOffset? dateWhenActive)
        {
            SalesTransaction salesTransaction = new SalesTransaction();
            SalesLine salesLine = new SalesLine
            {
                LineId = Guid.NewGuid().ToString("N"),
                ItemId = litem.ItemId,
                InventoryDimensionId = litem.InventDimId,
                SalesOrderUnitOfMeasure = litem.UnitId,
                Quantity = 1m
            };
            salesTransaction.SalesLines.Add(salesLine);

            // 10.07.18
            // GetIndependentPriceDiscountServiceRequest request = new GetIndependentPriceDiscountServiceRequest(salesTransaction);
            // GetPriceServiceResponse response = this.Context.Execute<GetPriceServiceResponse>(request);
            // if (dateWhenActive.HasValue)
            // {
            //     dateWhenActive = dateWhenActive.Value.AddDays(1);
            // }
            // Price
            CalculatePricesServiceRequest pricesServiceRequest = new CalculatePricesServiceRequest(salesTransaction, PricingCalculationMode.Independent, dateWhenActive);
            salesTransaction = (await this.Context.ExecuteAsync<GetPriceServiceResponse>((Request)pricesServiceRequest).ConfigureAwait(false)).Transaction;

            // Discount
            CalculateDiscountsServiceRequest discountServiceRequest = new CalculateDiscountsServiceRequest(salesTransaction)
            {
                RequestContext = pricesServiceRequest.RequestContext
            };
            discountServiceRequest.RequestContext.SetProperty("rr-SalesDate", dateWhenActive);
            salesTransaction = (await this.Context.ExecuteAsync<GetPriceServiceResponse>(discountServiceRequest).ConfigureAwait(false)).Transaction;

            return salesTransaction.ActiveSalesLines[0];
        }

        private async Task<decimal> CalculateLabelItemPriceInCurrency(LabelItem litem, string labelCurrency)
        {
            string storeCurrency = this.Context.GetChannelConfiguration().Currency;

            GetCurrencyValueServiceRequest request = new GetCurrencyValueServiceRequest(storeCurrency, labelCurrency, litem.UnitPrice);
            GetCurrencyValueServiceResponse response = await this.Context.ExecuteAsync<GetCurrencyValueServiceResponse>(request).ConfigureAwait(false);

            return response.RoundedConvertedAmount;
        }
    }

}
