using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Threading.Tasks;

using Microsoft.Dynamics.Commerce.Runtime;
using Microsoft.Dynamics.Commerce.Runtime.Messages;
using Microsoft.Dynamics.Commerce.Runtime.DataModel;
using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;

namespace RR.Commerce.Runtime.Utilities
{
    public class PlacementService : IRequestHandlerAsync
    {
        public IEnumerable<Type> SupportedRequestTypes
        {
            get
            {
                return new[]
                {
                    typeof(GetStoreLocationItemsServiceRequest),
                    typeof(GetStoreLocationItemIdsServiceRequest),
                    typeof(GetItemStoreLocationsServiceRequest),
                    typeof(CommitStoreLocationItemsServiceRequest),
                    typeof(MoveStoreLocationItemServiceRequest)
                };
            }
        }

        public async Task<Response> Execute(Request request)
        {
            if (request == null)
                throw new ArgumentNullException("request");

            Type requestType = request.GetType();
            Response response;

            if (requestType == typeof(GetStoreLocationItemsServiceRequest))
                response = await this.GetStoreLocationItems((GetStoreLocationItemsServiceRequest)request).ConfigureAwait(false);
            else if (requestType == typeof(GetStoreLocationItemIdsServiceRequest))
                response = await this.GetStoreLocationItemIds((GetStoreLocationItemIdsServiceRequest)request).ConfigureAwait(false);
            else if (requestType == typeof(GetItemStoreLocationsServiceRequest))
                response = await this.GetItemStoreLocations((GetItemStoreLocationsServiceRequest)request).ConfigureAwait(false);
            else if (requestType == typeof(CommitStoreLocationItemsServiceRequest))
                response = await this.CommitStoreLocationItems((CommitStoreLocationItemsServiceRequest)request).ConfigureAwait(false);
            else if (requestType == typeof(MoveStoreLocationItemServiceRequest))
                response = await this.MoveStoreLocationItem((MoveStoreLocationItemServiceRequest)request).ConfigureAwait(false);
            else
                throw new NotSupportedException(String.Format("Request '{0}' is not supported.", request.GetType()));

            return response;
        }

        private async Task<EntityDataServiceResponse<StoreLocationItem>> GetStoreLocationItems(GetStoreLocationItemsServiceRequest request)
        {
            ThrowIf.Null(request, "request");

            PagedResult<StoreLocationItem> storeLocationItems;

            // if (String.IsNullOrEmpty(request.SectionId) || String.IsNullOrEmpty(request.ShelfId))
            //     storeLocationItems = new PagedResult<StoreLocationItem>(new Collection<StoreLocationItem>().AsReadOnly());
            // else
            storeLocationItems = (await request.RequestContext.ExecuteAsync<EntityDataServiceResponse<StoreLocationItem>>(new GetStoreLocationItemsDataRequest(request)).ConfigureAwait(false)).PagedEntityCollection;

            // Item 

            IEnumerable<string> itemIds = storeLocationItems.Select(i => i.ItemId).Distinct();
            if (itemIds.Any())
            {
                ReadOnlyCollection<Item> items = (await request.RequestContext.ExecuteAsync<GetItemsDataResponse>(new GetItemsDataRequest(itemIds)).ConfigureAwait(false)).Items;
                foreach (StoreLocationItem slItem in storeLocationItems)
                {
                    Item item = items.SingleOrDefault(i => String.Compare(i.ItemId, slItem.ItemId, StringComparison.OrdinalIgnoreCase) == 0);
                    if (item != null)
                    {
                        slItem.Description = item.Name;
                        slItem.UnitId = item.SalesUnitOfMeasure;
                    }
                }
            }

            // Variant

            IEnumerable<ItemVariantInventoryDimension> inventDimensionIds =
                storeLocationItems.Where(i => !String.IsNullOrEmpty(i.InventDimId)).Select(i => new ItemVariantInventoryDimension() { ItemId = i.ItemId, VariantInventoryDimensionId = i.InventDimId }).Distinct();
            if (inventDimensionIds.Any())
            {
                ReadOnlyCollection<ProductVariant> variants = (await request.RequestContext.ExecuteAsync<EntityDataServiceResponse<ProductVariant>>(new GetProductVariantsDataRequest(inventDimensionIds)).ConfigureAwait(false)).PagedEntityCollection.Results;
                foreach (StoreLocationItem slItem in storeLocationItems.Where(i => !String.IsNullOrEmpty(i.InventDimId)))
                {
                    ProductVariant variant = variants.SingleOrDefault(i => String.Compare(i.ItemId, slItem.ItemId, StringComparison.OrdinalIgnoreCase) == 0 &&
                                                                           String.Compare(i.InventoryDimensionId, slItem.InventDimId, StringComparison.OrdinalIgnoreCase) == 0);
                    if (variant != null)
                    {
                        slItem.VariantId = variant.VariantId;
                        slItem.DimColorId = variant.ColorId;
                        slItem.DimConfigId = variant.ColorId;
                        slItem.DimSizeId = variant.SizeId;
                        slItem.DimStyleId = variant.StyleId;
                    }
                }
            }

            return new EntityDataServiceResponse<StoreLocationItem>(storeLocationItems);
        }

        private async Task<GetItemIdsDataResponse> GetStoreLocationItemIds(GetStoreLocationItemIdsServiceRequest request)
        {
            ThrowIf.Null(request, "request");
            bool local = (await request.RequestContext.ExecuteAsync<SingleEntityDataServiceResponse<bool>>(new GetStoreLocationTypeDataRequest(request.SectionId, request.ShelfId)).ConfigureAwait(false)).Entity;
            return await request.RequestContext.ExecuteAsync<GetItemIdsDataResponse>(new GetStoreLocationItemIdsDataRequest(request, local)).ConfigureAwait(false);
        }
        private async Task<GetItemStoreLocationsResponse> GetItemStoreLocations(GetItemStoreLocationsServiceRequest request)
        {
            ThrowIf.Null(request, "request");

            bool local = (await request.RequestContext.ExecuteAsync<SingleEntityDataServiceResponse<bool>>(new GetStoreLocationTypeDataRequest(request.SectionId, request.ShelfId)).ConfigureAwait(false)).Entity;

            if (String.IsNullOrEmpty(request.SectionId) || String.IsNullOrEmpty(request.ShelfId) || local)
            {
                GetItemStoreLocationsResponse response = await request.RequestContext.ExecuteAsync<GetItemStoreLocationsResponse>(new GetItemStoreLocationsDataRequest(request, local)).ConfigureAwait(false);
                if (response.Locations.Count > 0)
                    return response;
            }

            IList<ItemStoreLocation> locations = new List<ItemStoreLocation>
            {
                new ItemStoreLocation { ItemId = request.ItemId, SectionId = request.SectionId, ShelfId = request.ShelfId, Location = 0 },
            };
            return new GetItemStoreLocationsResponse(locations);
        }

        private async Task<SingleEntityDataServiceResponse<bool>> MoveStoreLocationItem(MoveStoreLocationItemServiceRequest request)
        {
            ThrowIf.Null(request, "request");

            bool result = false;

            StoreLocationItem oldStoreLocationItem = (await request.RequestContext.ExecuteAsync<SingleEntityDataServiceResponse<StoreLocationItem>>(new GetStoreLocationItemDataRequest(request.SectionId, request.ShelfId, request.OldLocation)).ConfigureAwait(false)).Entity;
            if (oldStoreLocationItem != null)
            {
                StoreLocationItem newStoreLocationItem = (await request.RequestContext.ExecuteAsync<SingleEntityDataServiceResponse<StoreLocationItem>>(new GetStoreLocationItemDataRequest(request.SectionId, request.ShelfId, request.NewLocation)).ConfigureAwait(false)).Entity;
                if (newStoreLocationItem != null)
                {
                    await request.RequestContext.ExecuteAsync<NullResponse>(new PlaceStoreLocationItemDataRequest(request.SectionId, request.ShelfId, request.OldLocation, newStoreLocationItem)).ConfigureAwait(false);
                    await request.RequestContext.ExecuteAsync<NullResponse>(new PlaceStoreLocationItemDataRequest(request.SectionId, request.ShelfId, request.NewLocation, oldStoreLocationItem)).ConfigureAwait(false);
                    result = true;
                }
            }

            return new SingleEntityDataServiceResponse<bool>(result);
        }

        private async Task<SingleEntityDataServiceResponse<int>> CommitStoreLocationItems(CommitStoreLocationItemsServiceRequest request)
        {
            ThrowIf.Null(request, "request");

            string storeNumber = request.RequestContext.GetOrgUnit().OrgUnitNumber;
            int shelfCount = 0;

            if (String.IsNullOrWhiteSpace(request.SectionId) || String.IsNullOrWhiteSpace(request.ShelfId))
            {
                PagedResult<StoreShelf> shelves = (await request.RequestContext.ExecuteAsync<EntityDataServiceResponse<StoreShelf>>(new GetStoreShelfDataRequest(request.SectionId)).ConfigureAwait(false)).PagedEntityCollection;
                foreach (StoreShelf shelf in shelves)
                {
                    await CommitStoreLocationItems(request.RequestContext, storeNumber, shelf.SectionId, shelf.ShelfId).ConfigureAwait(false);
                    shelfCount++;
                }
            }
            else
            {
                await CommitStoreLocationItems(request.RequestContext, storeNumber, request.SectionId, request.ShelfId).ConfigureAwait(false);
                shelfCount++;
            }

            return new SingleEntityDataServiceResponse<int>(shelfCount);
        }

        private async Task CommitStoreLocationItems(RequestContext requestContext, string storeNumber, string sectionId, string shelfId)
        {
            GetStoreLocationItemsDataRequest getRequest = new GetStoreLocationItemsDataRequest(sectionId, shelfId, QueryResultSettings.AllRecords);

            PagedResult<StoreLocationItem> storeLocationItems = (await requestContext.ExecuteAsync<EntityDataServiceResponse<StoreLocationItem>>(getRequest).ConfigureAwait(false)).PagedEntityCollection;

            CommitStoreLocationItemsRealtimeRequest commitRequest = new CommitStoreLocationItemsRealtimeRequest(storeNumber, sectionId, shelfId, storeLocationItems.Results);

            await requestContext.ExecuteAsync<NullResponse>(commitRequest).ConfigureAwait(false);
        }

    }
}