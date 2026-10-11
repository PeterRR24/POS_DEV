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

using RR.Commerce.Runtime.Utilities;

namespace RR.Commerce.Runtime.Mobility
{
    public class ProductSaleTrigger : IRequestTriggerAsync
    {
        private const string FSBarcode = "RR.FSBarcode";

        public IEnumerable<Type> SupportedRequestTypes
        {
            get
            {
                return new[]
                {
                    typeof(GetBarcodeRequest),
                    typeof(CalculateSalesTransactionServiceRequest)
                };
            }
        }

        public async Task OnExecuting(Request request)
        {
            ThrowIf.Null(request, "request");

            Type requestType = request.GetType();

            if (requestType == typeof(GetBarcodeRequest))
                await GetBarcodeExecuting((GetBarcodeRequest)request).ConfigureAwait(false);
            else if (requestType == typeof(CalculateSalesTransactionServiceRequest))
                await CalculateSalesTransactionExecuting((CalculateSalesTransactionServiceRequest)request).ConfigureAwait(false);

        }

        public Task OnExecuted(Request request, Response response)
        {
            ThrowIf.Null(request, "request");
            ThrowIf.Null(response, "response");

            Type requestType = request.GetType();

            if (requestType == typeof(GetBarcodeRequest))
                GetBarcodeExecuted((GetBarcodeRequest)request, (GetBarcodeResponse)response);

            return Task.CompletedTask;
        }

        private async Task GetBarcodeExecuting(GetBarcodeRequest request)
        {
            ThrowIf.Null(request, "request");

            if (request.ScanInfo != null)
            {
                UtilitiesConfiguration config = await GetConfiguration(request.RequestContext).ConfigureAwait(false);
                if (config == null)
                    return;

                string fsBarcodePrefix = config.FireSaleBarcodePrefix;
                ProductSaleBarcode psBarcode = FireSaleBarcode.Evaluate(fsBarcodePrefix, request.ScanInfo.ScannedText);
                if (psBarcode != null)
                {
                    int licenseOk = 0;

#if !DEBUG
                    licenseOk = await Mobilityicense.Validate(request.RequestContext).ConfigureAwait(false);
#endif

                    if (licenseOk == 0)
                    {
                        if (psBarcode.ExpDate.HasValue && psBarcode.ExpDate.Value <= DateTime.Today)
                            throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_BarcodeNotFound, "Barcode Expired.");
                        request.SetProperty(FSBarcode, request.ScanInfo.ScannedText);
                        request.ScanInfo.ScannedText = psBarcode.BarcodeId;
                    }
                }
            }
        }

        private void GetBarcodeExecuted(GetBarcodeRequest request, GetBarcodeResponse response)
        {
            ThrowIf.Null(request, "request");
            ThrowIf.Null(response, "response");

            object fsBarcodeId = request.GetProperty(FSBarcode);
            if (fsBarcodeId != null && response.Barcode != null)
            {
                response.Barcode.BarcodeId = Convert.ToString(fsBarcodeId);
            }
        }

        private async Task CalculateSalesTransactionExecuting(CalculateSalesTransactionServiceRequest request)
        {
            ThrowIf.Null(request, "request");

            if (request.CalculatePricesForNewSalesLinesOnly)
            {
                Collection<SalesLine> salesLines = new Collection<SalesLine>();
                Dictionary<string, ProductSaleBarcode> psBarcodes = new Dictionary<string, ProductSaleBarcode>();
                ProductSaleBarcode psBarcode;
                bool priceInBarcode;

                Lazy<string> fsBarcodePrefix = new Lazy<string>(() =>
                {
#pragma warning disable AvoidBlockingCallsAnalyzer // Avoid blocking asynchronous execution.
                    UtilitiesConfiguration config = GetConfiguration(request.RequestContext).ConfigureAwait(false).GetAwaiter().GetResult();
#pragma warning restore AvoidBlockingCallsAnalyzer // Avoid blocking asynchronous execution.
                    if (config == null)
                        return string.Empty;
                    else
                        return config.FireSaleBarcodePrefix;
                });

                foreach (string lineId in request.NewSalesLineIdSet)
                {
                    SalesLine salesLine = request.Transaction.SalesLines.FirstOrDefault(l => String.Compare(l.LineId, lineId, StringComparison.OrdinalIgnoreCase) == 0);

                    //TEST
                    /*
                    if (salesLine != null)
                    {
                        psBarcode = new ProductSaleBarcode();
                        psBarcode.BarcodeId = "2246587365223";
                        psBarcode.ReasonCode = "01";
                        psBarcode.Price = 2.3m;
                        salesLine.Barcode = FireSaleBarcode.Format(fsBarcodePrefix.Value, psBarcode);
                    }
                    */

                    if (salesLine != null && !String.IsNullOrEmpty(salesLine.Barcode))
                    {
                        psBarcode = FireSaleBarcode.Evaluate(fsBarcodePrefix.Value, salesLine.Barcode);
                        if (psBarcode != null)
                        {
                            salesLines.Add(salesLine);
                            psBarcodes.Add(salesLine.LineId, psBarcode);
                        }
                    }
                }

                if (salesLines.Any())
                {
                    SalesTransaction salesTransaction = new SalesTransaction();
                    salesTransaction.CustomerId = request.Transaction.CustomerId;
                    salesTransaction.CurrencyCode = request.Transaction.CurrencyCode;
                    salesTransaction.IsTaxIncludedInPrice = request.Transaction.IsTaxIncludedInPrice;
                    salesTransaction.AffiliationLoyaltyTierLines.AddRange(request.Transaction.AffiliationLoyaltyTierLines);

                    foreach (SalesLine salesLine in salesLines)
                    {
                        priceInBarcode = salesLine.PriceInBarcode && salesLine.Quantity != 0;
                        if (!priceInBarcode)
                        {
                            SalesLine saleLineCopy = new SalesLine()
                            {
                                LineId = salesLine.LineId,
                                ProductId = salesLine.ProductId,
                                MasterProductId = salesLine.MasterProductId,
                                ItemId = salesLine.ItemId,
                                InventoryDimensionId = salesLine.InventoryDimensionId,
                                Variant = salesLine.Variant,
                                UnitOfMeasureSymbol = salesLine.UnitOfMeasureSymbol,
                                SalesOrderUnitOfMeasure = salesLine.SalesOrderUnitOfMeasure,
                                OriginalSalesOrderUnitOfMeasure = salesLine.OriginalSalesOrderUnitOfMeasure,
                                UnitOfMeasureConversion = salesLine.UnitOfMeasureConversion,
                                Quantity = salesLine.Quantity,
                                SalesDate = salesLine.SalesDate,
                                CatalogId = salesLine.CatalogId
                            };
                            saleLineCopy.CatalogIds.AddRange(salesLine.CatalogIds);

                            salesTransaction.SalesLines.Add(saleLineCopy);
                        }
                    }

                    if (salesTransaction.SalesLines.Count > 0)
                    {
                        CalculatePricesServiceRequest pricesServiceRequest = new CalculatePricesServiceRequest(salesTransaction);
                        salesTransaction = (await request.RequestContext.ExecuteAsync<GetPriceServiceResponse>((Request)pricesServiceRequest).ConfigureAwait(false)).Transaction;
                    }

                    decimal salePrice, defaultPrice, discAmount;

                    foreach (SalesLine salesLine in salesLines)
                    {
                        if (psBarcodes.TryGetValue(salesLine.LineId, out psBarcode))
                        {
                            if (psBarcode != null && !String.IsNullOrEmpty(salesLine.Barcode))
                            {
                                priceInBarcode = salesLine.PriceInBarcode && salesLine.Quantity != 0;

                                if (priceInBarcode)
                                {
                                    salePrice = (await request.RequestContext.ExecuteAsync<GetRoundedValueServiceResponse>(new GetRoundedValueServiceRequest(psBarcode.Price / salesLine.Quantity, request.Transaction.CurrencyCode)).ConfigureAwait(false)).RoundedValue;
                                    defaultPrice = salesLine.Price;
                                }
                                else
                                {
                                    salePrice = psBarcode.Price;

                                    SalesLine salesLineCalculated = salesTransaction.SalesLines.FirstOrDefault(s => String.Compare(s.LineId, salesLine.LineId, StringComparison.OrdinalIgnoreCase) == 0);
                                    defaultPrice = salesLineCalculated == null ? 0 : salesLineCalculated.Price;
                                }
                                discAmount = defaultPrice - salePrice;

                                ProductSaleReason psr = (await request.RequestContext.ExecuteAsync<SingleEntityDataServiceResponse<ProductSaleReason>>(new ProductSaleReasonGetDataRequest(psBarcode.ReasonCode)).ConfigureAwait(false)).Entity;

                                if (psr != null && psr.DiscountMode == FireSaleDiscMode.Discount)
                                {
                                    salesLine.Price = defaultPrice;
                                    salesLine.LineManualDiscountAmount = discAmount;

                                    DiscountLine discountLine = new DiscountLine();
                                    discountLine.ConcurrencyMode = ConcurrencyMode.Exclusive;
                                    discountLine.DiscountLineType = DiscountLineType.ManualDiscount;
                                    discountLine.ManualDiscountType = ManualDiscountType.LineDiscountAmount;
                                    discountLine.Amount = discAmount;
                                    discountLine.EffectiveAmount = discAmount;
                                    discountLine.DiscountCost = discAmount;

                                    salesLine.DiscountLines.Add(discountLine);
                                }
                                else
                                    salesLine.Price = salePrice;

                                salesLine.IsPriceLocked = true;

                                if (psr != null)
                                {
                                    ReasonCodeLine reasonCodeLine = new ReasonCodeLine();
                                    reasonCodeLine.LineId = Guid.NewGuid().ToString("N");
                                    reasonCodeLine.LineType = ReasonCodeLineType.Sales;
                                    reasonCodeLine.ParentLineId = salesLine.LineId;
                                    reasonCodeLine.TransactionId = request.Transaction.Id;
                                    reasonCodeLine.ReasonCodeId = psr.ReasonCodeId;
                                    reasonCodeLine.SubReasonCodeId = psr.SubReasonCodeId;
                                    reasonCodeLine.Information = salesLine.Barcode.Substring(0, salesLine.Barcode.Length - psBarcode.BarcodeId.Length);
                                    reasonCodeLine.InformationAmount = discAmount;

                                    salesLine.ReasonCodeLines.Add(reasonCodeLine);
                                }
                            }
                        }
                    }
                }
            }
        }

        private async Task<UtilitiesConfiguration> GetConfiguration(RequestContext requestContext)
        {
            return (await requestContext.ExecuteAsync<SingleEntityDataServiceResponse<UtilitiesConfiguration>>(new GetUtilitiesConfigurationDataRequest()).ConfigureAwait(false)).Entity;
        }
    }
}
