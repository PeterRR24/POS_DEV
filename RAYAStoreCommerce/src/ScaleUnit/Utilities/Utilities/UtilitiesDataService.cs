using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Threading.Tasks;

using Microsoft.Dynamics.Commerce.Runtime;
using Microsoft.Dynamics.Commerce.Runtime.Data;
using Microsoft.Dynamics.Commerce.Runtime.Messages;
using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
using Microsoft.Dynamics.Commerce.Runtime.DataModel;

namespace RR.Commerce.Runtime.Utilities
{
    public class UtilitiesDataService : IRequestHandlerAsync
    {
        public IEnumerable<Type> SupportedRequestTypes
        {
            get
            {
                return new[]
                {
                    typeof(GetUtilitiesConfigurationDataRequest),
                    typeof(GetCategoryHierarchyDataRequest),
                    typeof(GetStoreSectionDataRequest),
                    typeof(GetStoreShelfDataRequest)
                };
            }
        }

        public async Task<Response> Execute(Request request)
        {
            if (request == null)
                throw new ArgumentNullException("request");

            Type requestType = request.GetType();
            Response response;

            if (requestType == typeof(GetUtilitiesConfigurationDataRequest))
                response = await this.GetUtilitiesConfiguration((GetUtilitiesConfigurationDataRequest)request).ConfigureAwait(false);
            else if (requestType == typeof(GetCategoryHierarchyDataRequest))
                response = await this.GetCategoryHierarchy((GetCategoryHierarchyDataRequest)request).ConfigureAwait(false);
            else if (requestType == typeof(GetStoreSectionDataRequest))
                response = await this.GetStoreSections((GetStoreSectionDataRequest)request).ConfigureAwait(false);
            else if (requestType == typeof(GetStoreShelfDataRequest))
                response = await this.GetStoreShelves((GetStoreShelfDataRequest)request).ConfigureAwait(false);
            else
                throw new NotSupportedException(String.Format("Request '{0}' is not supported.", request.GetType()));

            return await Task.FromResult(response).ConfigureAwait(false);
        }

        private async Task<SingleEntityDataServiceResponse<UtilitiesConfiguration>> GetUtilitiesConfiguration(GetUtilitiesConfigurationDataRequest request)
        {
            //UtilitiesDataManager manager = UtilitiesDataManager.Instantiate(request.RequestContext);
            //UtilitiesConfiguration result = manager.GetUtilitiesConfiguration();
            UtilitiesDatabaseAccessor databaseAccessor = new UtilitiesDatabaseAccessor(request.RequestContext);
            UtilitiesConfiguration result = await databaseAccessor.GetUtilitiesConfiguration().ConfigureAwait(false);
            return new SingleEntityDataServiceResponse<UtilitiesConfiguration>(result);
        }

        private async Task<EntityDataServiceResponse<StoreSection>> GetStoreSections(GetStoreSectionDataRequest request)
        {
            ThrowIf.Null(request, "request");

            //UtilitiesDataManager manager = UtilitiesDataManager.Instantiate(request.RequestContext);
            //PagedResult<StoreSection> result = manager.GetStoreSections();
            UtilitiesDatabaseAccessor databaseAccessor = new UtilitiesDatabaseAccessor(request.RequestContext);
            PagedResult<StoreSection> result = await databaseAccessor.GetStoreSections().ConfigureAwait(false);
            return new EntityDataServiceResponse<StoreSection>(result);
        }

        private async Task<EntityDataServiceResponse<StoreShelf>> GetStoreShelves(GetStoreShelfDataRequest request)
        {
            ThrowIf.Null(request, "request");

            //UtilitiesDataManager manager = UtilitiesDataManager.Instantiate(request.RequestContext);
            //PagedResult<StoreShelf> result = manager.GetStoreShelves(request.SectionId);
            UtilitiesDatabaseAccessor databaseAccessor = new UtilitiesDatabaseAccessor(request.RequestContext);
            PagedResult<StoreShelf> storeShelfs = await databaseAccessor.GetStoreShelves().ConfigureAwait(false);
            PagedResult<StoreShelf> result;
            if (String.IsNullOrEmpty(request.SectionId))
                result = storeShelfs;
            else
                result = new PagedResult<StoreShelf>(storeShelfs.Results.Where(s => String.Compare(s.SectionId, request.SectionId, StringComparison.OrdinalIgnoreCase) == 0).AsReadOnly());
            return new EntityDataServiceResponse<StoreShelf>(result);
        }

        private async Task<EntityDataServiceResponse<Category>> GetCategoryHierarchy(GetCategoryHierarchyDataRequest request)
        {
            ThrowIf.Null(request, "request");

            //UtilitiesDataManager manager = UtilitiesDataManager.Instantiate(request.RequestContext);
            //PagedResult<Category> result = manager.GetCategoryHierarchy(request.HierarchyId);
            ChannelConfiguration channelConfig = request.RequestContext.GetChannelConfiguration();
            UtilitiesDatabaseAccessor databaseAccessor = new UtilitiesDatabaseAccessor(request.RequestContext);
            string language = String.IsNullOrEmpty(channelConfig.DefaultLanguageId) ? channelConfig.CompanyLanguageId : channelConfig.DefaultLanguageId;
            PagedResult<Category> result = await databaseAccessor.GetGetCategoryHierarchy(request.HierarchyId, language).ConfigureAwait(false);

            return new EntityDataServiceResponse<Category>(result);
        }
    }
}
