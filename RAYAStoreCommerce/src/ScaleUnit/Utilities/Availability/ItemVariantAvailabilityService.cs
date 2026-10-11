using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Dynamics.Commerce.Runtime;
using Microsoft.Dynamics.Commerce.Runtime.DataModel;
using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
using Microsoft.Dynamics.Commerce.Runtime.Messages;

namespace RR.Commerce.Runtime.Utilities
{
    //TODO - Remove
    public class ItemVariantAvailabilityService : IRequestHandlerAsync
    {
        public IEnumerable<Type> SupportedRequestTypes
        {
            get
            {
                return new[]
                {
                    typeof(GetItemVariantAvailabilityRequest)
                };
            }
        }

        public async Task<Response> Execute(Request request)
        {
            if (request == null)
                throw new ArgumentNullException("request");

            Type requestType = request.GetType();
            Response response;

            if (requestType == typeof(GetItemVariantAvailabilityRequest))
                response = await GetItemAvailabilities((GetItemVariantAvailabilityRequest)request).ConfigureAwait(false);
            else
                throw new NotSupportedException(String.Format(CultureInfo.InvariantCulture, "Request type '{0}' is not supported.", request.GetType()));

            return response;
        }

        private static async Task<GetItemVariantAvailabilityResponse> GetItemAvailabilities(GetItemVariantAvailabilityRequest request)
        {
            QueryResultSettings settings = request.QueryResultSettings ?? QueryResultSettings.AllRecords;
            GetItemVariantAvailabilityDataRequest dataRequest = new GetItemVariantAvailabilityDataRequest(request.ItemVariants, settings);
            PagedResult<ItemAvailableQuantity> itemAvailabilities = (await request.RequestContext.ExecuteAsync<EntityDataServiceResponse<ItemAvailableQuantity>>(dataRequest).ConfigureAwait(false)).PagedEntityCollection;
            return new GetItemVariantAvailabilityResponse(itemAvailabilities);
        }
    }
}
