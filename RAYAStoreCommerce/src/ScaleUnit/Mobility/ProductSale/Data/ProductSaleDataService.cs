using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Dynamics.Commerce.Runtime;
using Microsoft.Dynamics.Commerce.Runtime.Messages;
using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
using Microsoft.Dynamics.Commerce.Runtime.Data;
using Microsoft.Dynamics.Commerce.Runtime.DataModel;

namespace RR.Commerce.Runtime.Mobility
{
    public class ProductSaleDataService : IRequestHandlerAsync
    {
        public IEnumerable<Type> SupportedRequestTypes
        {
            get
            {
                return new[]
                {
                    typeof(ProductSaleReasonGetDataRequest),
                    typeof(ProductSaleReasonSearchDataRequest)
                };
            }
        }

        public async Task<Response> Execute(Request request)
        {
            if (request == null)
                throw new ArgumentNullException("request");

            Type requestType = request.GetType();
            Response response;

            if (requestType == typeof(ProductSaleReasonGetDataRequest))
                response = await this.GetProductSaleReason((ProductSaleReasonGetDataRequest)request).ConfigureAwait(false);
            else if (requestType == typeof(ProductSaleReasonSearchDataRequest))
                response = await this.SearchProductSaleReason((ProductSaleReasonSearchDataRequest)request).ConfigureAwait(false);
            else
                throw new NotSupportedException(String.Format("Request '{0}' is not supported.", request.GetType()));

            return await Task.FromResult(response).ConfigureAwait(false);
        }

        private async Task<SingleEntityDataServiceResponse<ProductSaleReason>> GetProductSaleReason(ProductSaleReasonGetDataRequest request)
        {
            ThrowIf.Null(request, "request");
            MobilityDataManager dataManager = MobilityDataManager.Instantiate(request.RequestContext);
            return new SingleEntityDataServiceResponse<ProductSaleReason>(await dataManager.GetProductSaleReason(request.RecordId, request.Code).ConfigureAwait(false));
        }

        private async Task<EntityDataServiceResponse<ProductSaleReason>> SearchProductSaleReason(ProductSaleReasonSearchDataRequest request)
        {
            ThrowIf.Null(request, "request");
            MobilityDataManager dataManager = MobilityDataManager.Instantiate(request.RequestContext);
            return new EntityDataServiceResponse<ProductSaleReason>(await dataManager.GetProductSaleReasons().ConfigureAwait(false));
        }

    }
}