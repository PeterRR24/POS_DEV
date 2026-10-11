using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Dynamics.Commerce.Runtime;
using Microsoft.Dynamics.Commerce.Runtime.Messages;
using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
using Microsoft.Dynamics.Commerce.Runtime.DataModel;

namespace RR.Commerce.Runtime.Utilities
{/*
    public sealed class UtilitiesManager
    {
        private readonly ICommerceRuntime runtime;

        private UtilitiesManager(ICommerceRuntime runtime)
        {
            this.runtime = runtime;
        }

        public static UtilitiesManager Create(ICommerceRuntime runtime)
        {
            if (runtime == null)
                throw new ArgumentNullException("runtime");
            return new UtilitiesManager(runtime);
        }

        internal TResponse Execute<TResponse>(Request request) where TResponse : Response
        {
            return this.runtime.Execute<TResponse>(request, null);
        }

        public UtilitiesConfiguration GetUtilitiesConfiguration()
        {
            return Execute<SingleEntityDataServiceResponse<UtilitiesConfiguration>>(new GetUtilitiesConfigurationDataRequest()).Entity;
        }

        public PagedResult<StoreSection> GetStoreSections()
        {
            return Execute<EntityDataServiceResponse<StoreSection>>(new GetStoreSectionDataRequest()).PagedEntityCollection;
        }

        public PagedResult<StoreShelf> GetStoreShelves(string sectionId)
        {
            return Execute<EntityDataServiceResponse<StoreShelf>>(new GetStoreShelfDataRequest(sectionId)).PagedEntityCollection;
        }
    } */
}