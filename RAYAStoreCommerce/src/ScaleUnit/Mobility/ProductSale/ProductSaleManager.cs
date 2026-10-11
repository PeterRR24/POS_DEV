using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;

using Microsoft.Dynamics.Commerce.Runtime;
//using Microsoft.Dynamics.Commerce.Runtime.Client;
using Microsoft.Dynamics.Commerce.Runtime.Messages;
using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;

using RR.Commerce.Runtime.Printing;

namespace RR.Commerce.Runtime.Mobility
{
    public sealed class ProductSaleManager
    {
        private readonly ICommerceRuntime runtime;

        private ProductSaleManager(ICommerceRuntime runtime)
        {
            this.runtime = runtime;
        }

        public static ProductSaleManager Create(ICommerceRuntime runtime)
        {
            if (runtime == null)
                throw new ArgumentNullException("runtime");
            return new ProductSaleManager(runtime);
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

        public ProductSale Get(string barcodeId)
        {
            ThrowIf.NullOrWhiteSpace(barcodeId, "barcodeId");
            return Execute<SingleEntityDataServiceResponse<ProductSale>>(new GetProductSaleServiceRequest(barcodeId)).Entity;
        }

        public ProductSale Sale(string barcodeId, string reasonCode)
        {
            ThrowIf.NullOrWhiteSpace(barcodeId, "barcodeId");
            return Execute<SingleEntityDataServiceResponse<ProductSale>>(new CalcProductSaleServiceRequest(barcodeId, reasonCode)).Entity;
        }

        public string Format(ProductSaleRequest request)
        {
            ThrowIf.Null<ProductSaleRequest>(request, "request");
            ThrowIf.NullOrWhiteSpace(request.BarcodeId, "request.BarcodeId");
            ThrowIf.NullOrWhiteSpace(request.ReasonCode, "request.ReasonCode");
            return Execute<SingleEntityDataServiceResponse<string>>(new FormatProductSaleServiceRequest(request)).Entity;
        }

        public LabelPrinterTask Label(ProductSaleRequest request, int templateNo, int noOfLabels)
        {
            ThrowIf.Null<ProductSaleRequest>(request, "request");
            ThrowIf.NullOrWhiteSpace(request.BarcodeId, "request.BarcodeId");
            ThrowIf.NullOrWhiteSpace(request.ItemId, "request.ItemId");
            ThrowIf.NullOrWhiteSpace(request.ReasonCode, "request.ReasonCode");
            return Execute<SingleEntityDataServiceResponse<LabelPrinterTask>>(new LabelProductSaleServiceRequest(request, templateNo, noOfLabels)).Entity;
        }

        public PagedResult<ProductSaleReason> GetSaleReasons()
        {
            return Execute<EntityDataServiceResponse<ProductSaleReason>>(new ProductSaleReasonSearchDataRequest()).PagedEntityCollection;
        }
    }
}
