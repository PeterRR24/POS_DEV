using System;
using System.Collections.Generic;

using Microsoft.Dynamics.Commerce.Runtime;
using Microsoft.Dynamics.Commerce.Runtime.Messages;
using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
using Microsoft.Dynamics.Commerce.Runtime.Data;
using Microsoft.Dynamics.Commerce.Runtime.DataModel;
using Microsoft.Dynamics.Commerce.Runtime.Services.Messages;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace RR.Commerce.Runtime.Utilities
{
    public class ItemVariantAvailabilityDataService : IRequestHandlerAsync
    {
        public IEnumerable<Type> SupportedRequestTypes
        {
            get
            {
                return new[]
                {
                    typeof(GetItemVariantAvailabilityDataRequest),
                    typeof(GetItemVariantAvailableQtyDataRequest)
                };
            }
        }

        public async Task<Response> Execute(Request request)
        {
            if (request == null)
                throw new ArgumentNullException("request");

            Type requestType = request.GetType();
            Response response;

            if (requestType == typeof(GetItemVariantAvailabilityDataRequest))
                response = await this.GetItemAvailabilities((GetItemVariantAvailabilityDataRequest)request).ConfigureAwait(false);
            else if (requestType == typeof(GetItemVariantAvailableQtyDataRequest))
                response = await this.GetAvailableQty((GetItemVariantAvailableQtyDataRequest)request).ConfigureAwait(false);
            else
                throw new NotSupportedException(String.Format("Request '{0}' is not supported.", request.GetType()));

            return response;
        }

        private async Task<EntityDataServiceResponse<ItemAvailableQuantity>> GetItemAvailabilities(GetItemVariantAvailabilityDataRequest request)
        {
            QueryResultSettings settings = request.QueryResultSettings ?? QueryResultSettings.AllRecords;
            ItemVariantAvailabilityDataManager dataManager = new ItemVariantAvailabilityDataManager(request.RequestContext);
            PagedResult<ItemAvailableQuantity> itemAvailabilities = await dataManager.GetItemAvailableQuantities(request.ItemVariants, settings).ConfigureAwait(false);
            return new EntityDataServiceResponse<ItemAvailableQuantity>(itemAvailabilities);
        }

        private async Task<SingleEntityDataServiceResponse<decimal>> GetAvailableQty(GetItemVariantAvailableQtyDataRequest request)
        {
            if (request.RequestContext.GetChannelConfiguration().InventAvailabilityCalculationMode == InventAvailabilityCalculationMode.Channel)
            {
                return await SearchOrgUnitAvailabilitiesInChannel(request).ConfigureAwait(false);
            }
            return await GetItemAvailableQty(request).ConfigureAwait(false);
        }

        private async Task<SingleEntityDataServiceResponse<decimal>> SearchOrgUnitAvailabilitiesInChannel(GetItemVariantAvailableQtyDataRequest request)
        {
            decimal itemAvailableQty = 0;
            GetInventoryAvailabilityForFulfillmentStoresServiceRequest getInventoryAvailabilityForFulfillmentStoresServiceRequest = new GetInventoryAvailabilityForFulfillmentStoresServiceRequest(request.ProductId)
            {
                QueryResultSettings = new QueryResultSettings(new PagingInfo(5))
            };
            ReadOnlyCollection<ItemAvailabilityStore> readOnlyCollection = (await request.RequestContext.Runtime.ExecuteAsync<EntityDataServiceResponse<ItemAvailabilityStore>>(getInventoryAvailabilityForFulfillmentStoresServiceRequest, request.RequestContext).ConfigureAwait(false)).PagedEntityCollection.Results;
            string inventoryLocationId = request.RequestContext.GetOrgUnit().InventoryLocationId;
            for (int i = 0; i < readOnlyCollection.Count; i++)
            {
                if (readOnlyCollection[i].InventoryLocationId == inventoryLocationId)
                {
                    itemAvailableQty = readOnlyCollection[i].AvailableQuantity;
                    break;
                }
            }
            return new SingleEntityDataServiceResponse<decimal>(itemAvailableQty);
        }

        private async Task<SingleEntityDataServiceResponse<decimal>> GetItemAvailableQty(GetItemVariantAvailableQtyDataRequest request)
        {
            ThrowIf.Null(request, "request");

            SqlQuery query = new SqlQuery();
            query.QueryString = "SELECT ext.GETITEMVARIANTAVAILABLEQTY(@bi_ChannelId, @nvc_ItemId, @nvc_InventDimId)";
            query.Parameters["@bi_ChannelId"] = request.RequestContext.GetPrincipal().ChannelId;
            query.Parameters["@nvc_ItemId"] = request.ItemId ?? String.Empty;
            query.Parameters["@nvc_InventDimId"] = request.InventDimId ?? String.Empty;

            decimal itemAvailableQty = 0;

            using (DatabaseContext databaseContext = new DatabaseContext(request.RequestContext))
            {
                itemAvailableQty = await databaseContext.ExecuteScalarAsync<decimal>(query).ConfigureAwait(false);
            }

            return new SingleEntityDataServiceResponse<decimal>(itemAvailableQty);
        }
    }
}
