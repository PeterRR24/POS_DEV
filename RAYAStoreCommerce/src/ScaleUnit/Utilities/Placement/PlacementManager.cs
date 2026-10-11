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
    public sealed class PlacementManager
    {
        private readonly ICommerceRuntime runtime;

        private PlacementManager(ICommerceRuntime runtime)
        {
            this.runtime = runtime;
        }

        public static PlacementManager Create(ICommerceRuntime runtime)
        {
            if (runtime == null)
                throw new ArgumentNullException("runtime");
            return new PlacementManager(runtime);
        }

        internal TResponse Execute<TResponse>(Request request) where TResponse : Response
        {
            return this.runtime.Execute<TResponse>(request, null);
        }

        //

        public PagedResult<StoreLocationItem> Select(string sectionId, string shelfId, QueryResultSettings settings)
        {
            ThrowIf.NullOrWhiteSpace(sectionId, "sectionId");
            ThrowIf.NullOrWhiteSpace(shelfId, "shelfId");
            return Execute<EntityDataServiceResponse<StoreLocationItem>>(new GetStoreLocationItemsServiceRequest(sectionId, shelfId, settings)).PagedEntityCollection;
        }

        public void Clear(string sectionId, string shelfId)
        {
            Execute<NullResponse>(new ClearStoreLocationItemsDataRequest(sectionId, shelfId));
        }

        public int Commit(string sectionId, string shelfId)
        {
            return Execute<SingleEntityDataServiceResponse<int>>(new CommitStoreLocationItemsServiceRequest(sectionId, shelfId)).Entity;
        }

        //

        public void Place(string sectionId, string shelfId, int location, string itemId, int facingCount)
        {
            ThrowIf.NullOrWhiteSpace(sectionId, "sectionId");
            ThrowIf.NullOrWhiteSpace(shelfId, "shelfId");
            ThrowIf.NullOrWhiteSpace(itemId, "itemId");
            Execute<NullResponse>(new PlaceStoreLocationItemDataRequest(sectionId, shelfId, location, itemId, facingCount));
        }

        public void Remove(string sectionId, string shelfId, int location)
        {
            ThrowIf.NullOrWhiteSpace(sectionId, "sectionId");
            ThrowIf.NullOrWhiteSpace(shelfId, "shelfId");
            Execute<NullResponse>(new RemoveStoreLocationItemDataRequest(sectionId, shelfId, location));
        }

        public bool Move(string sectionId, string shelfId, int oldLocation, int newLocation)
        {
            ThrowIf.NullOrWhiteSpace(sectionId, "sectionId");
            ThrowIf.NullOrWhiteSpace(shelfId, "shelfId");
            return Execute<SingleEntityDataServiceResponse<bool>>(new MoveStoreLocationItemServiceRequest(sectionId, shelfId, oldLocation, newLocation)).Entity;
        }
    }*/
}