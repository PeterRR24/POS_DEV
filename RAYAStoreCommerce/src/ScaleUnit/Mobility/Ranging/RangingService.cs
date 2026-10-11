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
    public class RangingService : IRequestHandlerAsync
    {
        public IEnumerable<Type> SupportedRequestTypes
        {
            get
            {
                return new[]
                {
                    typeof(GetRangingItemServiceRequest),
                    typeof(CommitRangingServiceRequest),
                };
            }
        }

        public async Task<Response> Execute(Request request)
        {
            ThrowIf.Null(request, "request");

            Type requestType = request.GetType();
            Response response;

            if (requestType == typeof(GetRangingItemServiceRequest))
                response = await this.GetRangingItem((GetRangingItemServiceRequest)request).ConfigureAwait(false);
            else if (requestType == typeof(CommitRangingServiceRequest))
                response = await this.CommitRanging((CommitRangingServiceRequest)request).ConfigureAwait(false);
            else
                throw new NotSupportedException(String.Format("Request '{0}' is not supported.", request.GetType()));

            return response;
        }

        private async Task<SingleEntityDataServiceResponse<RangingItem>> GetRangingItem(GetRangingItemServiceRequest request)
        {
            ThrowIf.Null(request, "request");
            ThrowIf.NullOrWhiteSpace(request.BarcodeId, "request.BarcodeId");

            ItemBarcode barcode = null;

            GetProductBarcodeDataResponse pbr = await request.RequestContext.ExecuteAsync<GetProductBarcodeDataResponse>(new GetProductBarcodeDataRequest(request.BarcodeId)).ConfigureAwait(false);
            if (pbr != null && pbr.Barcode != null && !string.IsNullOrWhiteSpace(pbr.Barcode.ItemId))
                barcode = pbr.Barcode;

            Item item = (await request.RequestContext.ExecuteAsync<GetItemsDataResponse>(new GetItemsDataRequest(new string[] { barcode == null ? request.BarcodeId : barcode.ItemId })).ConfigureAwait(false)).Items.FirstOrDefault();
            if (item == null)
                throw new NotFoundException(String.Format("Item ‘{0}’ not found", request.BarcodeId));

            if (barcode == null && item != null)
            {
                PagedResult<ProductVariant> productVariants = (await request.RequestContext.ExecuteAsync<EntityDataServiceResponse<ProductVariant>>(new GetProductVariantsByItemDataRequest(item.ItemId)).ConfigureAwait(false)).PagedEntityCollection;
                if (productVariants != null && productVariants.Results.Any())
                    throw new NotFoundException(String.Format("Item ‘{0}’ dimensions are not valid.", request.BarcodeId));
            }

            RangingItem ritem = (await request.RequestContext.ExecuteAsync<SingleEntityDataServiceResponse<RangingItem>>(new RangingItemGetDataRequest(item.ItemId, request.SectionId, request.ShelfId)).ConfigureAwait(false)).Entity;
            if (ritem == null)
            {
                ritem = new RangingItem();
                ritem.ItemId = item.ItemId;
                ritem.ItemName = item.Name;
                ritem.SectionId = request.SectionId;
                ritem.ShelfId = request.ShelfId;
                ritem.SerialNumber = request.RequestContext.GetPrincipal().DeviceNumber;
            }

            return new SingleEntityDataServiceResponse<RangingItem>(ritem);
        }

        private async Task<SingleEntityDataServiceResponse<RangingStat>> CommitRanging(CommitRangingServiceRequest request)
        {
            ThrowIf.Null(request, "request");

            PagedResult<RangingItem> RangingList = (await request.RequestContext.ExecuteAsync<SingleEntityDataServiceResponse<PagedResult<RangingItem>>>(new RangingSelectDataRequest(request.SectionId, request.ShelfId)).ConfigureAwait(false)).Entity;
            ReadOnlyCollection<RangingItem> list = RangingList.Results;
            if (list.Count <= 0)
                throw new ValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidOperation, "Ranging list is empty.");

#if !DEBUG
            await Mobilityicense.Check(request.RequestContext).ConfigureAwait(false);
#endif

            // Local Ranging >>
            IEnumerable<StoreLocationItem> items = list.Select((item, idx) => new StoreLocationItem() { Location = idx + 1, ItemId = item.ItemId, FacingCount = item.Faces });
            await request.RequestContext.ExecuteAsync<NullResponse>(new PlaceStoreLocationItemsDataRequest(request.SectionId, request.ShelfId, items)).ConfigureAwait(false);
            // string storeNumber = request.RequestContext.GetOrgUnit().OrgUnitNumber;
            // request.RequestContext.Execute<NullResponse>(new CommitRangingRealtimeRequest(storeNumber, list.SectionId, list.ShelfId, list.List));
            // Local Ranging <<

            return await request.RequestContext.ExecuteAsync<SingleEntityDataServiceResponse<RangingStat>>(new RangingResetDataRequest(request.SectionId, request.ShelfId)).ConfigureAwait(false);
        }
    }
}
