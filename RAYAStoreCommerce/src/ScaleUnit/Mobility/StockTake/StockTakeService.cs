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

using RR.Commerce.Runtime.Utilities;

namespace RR.Commerce.Runtime.Mobility
{
    public class StockTakeService : IRequestHandlerAsync
    {
        public IEnumerable<Type> SupportedRequestTypes
        {
            get
            {
                return new[]
                {
                    typeof(GetStockTakeJournalsServiceRequest),
                    typeof(GetStockTakeItemServiceRequest),
                    typeof(CountStockTakeItemServiceRequest),
                    typeof(CountOfflineStockTakeItemsRequest)
                };
            }
        }

        public async Task<Response> Execute(Request request)
        {
            ThrowIf.Null(request, "request");

            Type requestType = request.GetType();
            Response response;

            if (requestType == typeof(GetStockTakeJournalsServiceRequest))
                response = await this.GetStockTakeJournals((GetStockTakeJournalsServiceRequest)request).ConfigureAwait(false);
            else if (requestType == typeof(GetStockTakeItemServiceRequest))
                response = await this.GetStockTakeItemOnDevice((GetStockTakeItemServiceRequest)request).ConfigureAwait(false);
            else if (requestType == typeof(CountStockTakeItemServiceRequest))
                response = await this.CountStockTakeItemOnDevice((CountStockTakeItemServiceRequest)request).ConfigureAwait(false);
            else if (requestType == typeof(CountOfflineStockTakeItemsRequest))
                response = await this.CountOfflineStockTakeItems((CountOfflineStockTakeItemsRequest)request).ConfigureAwait(false);
            else
                throw new NotSupportedException(String.Format("Request '{0}' is not supported.", request.GetType()));

            return response;
        }

        private async Task<EntityDataServiceResponse<StockTakeJournal>> GetStockTakeJournals(GetStockTakeJournalsServiceRequest request)
        {
            PagedResult<SCJournal> scJournals = (await request.RequestContext.ExecuteAsync<EntityDataServiceResponse<SCJournal>>(new SCJournalSearchDataRequest(SCJournalStatus.Open, null, request.QueryResultSettings)).ConfigureAwait(false)).PagedEntityCollection;
            return new EntityDataServiceResponse<StockTakeJournal>(scJournals.ConvertTo<StockTakeJournal>(j => j.ToStockTake()));
        }

        private async Task<SingleEntityDataServiceResponse<StockTakeItem>> GetStockTakeItem(GetStockTakeItemServiceRequest request)
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

            StockTakeItem stitem = new StockTakeItem();
            stitem.JournalId = request.JournalId;
            stitem.SectionId = request.SectionId;
            stitem.ShelfId = request.ShelfId;
            stitem.ItemId = item.ItemId;
            stitem.ItemName = item.Name;
            if (barcode != null)
            {
                stitem.Unit = barcode.UnitId;
                if (!String.IsNullOrEmpty(barcode.InventoryDimensionId))
                {
                    GetProductVariantsDataRequest variantRequest = new GetProductVariantsDataRequest(new ItemVariantInventoryDimension[] { new ItemVariantInventoryDimension() { ItemId = barcode.ItemId, VariantInventoryDimensionId = barcode.InventoryDimensionId } });
                    ProductVariant productVariant = (await request.RequestContext.ExecuteAsync<EntityDataServiceResponse<ProductVariant>>(variantRequest).ConfigureAwait(false)).PagedEntityCollection.Results.FirstOrDefault();
                    if (productVariant != null)
                    {
                        stitem.InventSizeId = productVariant.SizeId;
                        stitem.InventColorId = productVariant.ColorId;
                        stitem.InventStyleId = productVariant.StyleId;
                        stitem.ConfigId = productVariant.ConfigId;
                    }
                }
            }
            else
                stitem.Unit = item.InventoryUnitOfMeasure;

            stitem.TrackingInfo = (await request.RequestContext.ExecuteAsync<SingleEntityDataServiceResponse<int>>(new GetItemTrackingInfoDataRequest(stitem.ItemId)).ConfigureAwait(false)).Entity;
            stitem.Counted = (await request.RequestContext.ExecuteAsync<SingleEntityDataServiceResponse<decimal>>(new CalculateStockTakeItemDataRequest(stitem)).ConfigureAwait(false)).Entity;
            if ((await request.RequestContext.ExecuteAsync<SingleEntityDataServiceResponse<bool>>(new CheckStockTakeItemDataRequest(stitem)).ConfigureAwait(false)).Entity)
                stitem.JournalInfo = BitEncoder.Set(stitem.JournalInfo, 1, true);

            return new SingleEntityDataServiceResponse<StockTakeItem>(stitem);
        }

        private async Task<SingleEntityDataServiceResponse<decimal>> CountStockTakeItem(CountStockTakeItemServiceRequest request)
        {
            ThrowIf.Null(request, "request");
            ThrowIf.Null(request.Item, "request.Item");

            StockTakeItem item = request.Item;

            decimal thisQty = (await request.RequestContext.ExecuteAsync<SingleEntityDataServiceResponse<decimal>>(new CalculateStockTakeTrackingDataRequest(item)).ConfigureAwait(false)).Entity;
            decimal newQty = request.AddMode ? thisQty + item.Counted : item.Counted;
            decimal deltaQty = newQty - thisQty;

            if (deltaQty != 0)
            {
                Tuple<int, decimal> entry = (await request.RequestContext.ExecuteAsync<SingleEntityDataServiceResponse<Tuple<int, decimal>>>(new CalculateStockTakeDeviceDataRequest(item)).ConfigureAwait(false)).Entity;

                int entryId = entry.Item1;
                if (entryId != 0)
                {
                    decimal updateQty = entry.Item2 + deltaQty;
                    if (updateQty == 0)
                        await request.RequestContext.ExecuteAsync<NullResponse>(new SCJournalEntryDeleteDataRequest(entryId)).ConfigureAwait(false);
                    else
                        await request.RequestContext.ExecuteAsync<NullResponse>(new UpdateStockTakeItemDataRequest(entryId, updateQty)).ConfigureAwait(false);
                }
                else
                    await request.RequestContext.ExecuteAsync<NullResponse>(new CreateStockTakeItemDataRequest(item, deltaQty)).ConfigureAwait(false);
            }

            decimal counted = (await request.RequestContext.ExecuteAsync<SingleEntityDataServiceResponse<decimal>>(new CalculateStockTakeItemDataRequest(request.Item)).ConfigureAwait(false)).Entity;

            return new SingleEntityDataServiceResponse<decimal>(counted);
        }

        private async Task<SingleEntityDataServiceResponse<StockTakeItem>> GetStockTakeItemOnDevice(GetStockTakeItemServiceRequest request)
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

            StockTakeItem stitem = new StockTakeItem();
            stitem.JournalId = request.JournalId;
            stitem.SectionId = request.SectionId;
            stitem.ShelfId = request.ShelfId;
            stitem.ItemId = item.ItemId;
            stitem.ItemName = item.Name;
            if (barcode != null)
            {
                stitem.Unit = barcode.UnitId;
                if (!String.IsNullOrEmpty(barcode.InventoryDimensionId))
                {
                    GetProductVariantsDataRequest variantRequest = new GetProductVariantsDataRequest(new ItemVariantInventoryDimension[] { new ItemVariantInventoryDimension() { ItemId = barcode.ItemId, VariantInventoryDimensionId = barcode.InventoryDimensionId } });
                    ProductVariant productVariant = (await request.RequestContext.ExecuteAsync<EntityDataServiceResponse<ProductVariant>>(variantRequest).ConfigureAwait(false)).PagedEntityCollection.Results.FirstOrDefault();
                    if (productVariant != null)
                    {
                        stitem.InventSizeId = productVariant.SizeId;
                        stitem.InventColorId = productVariant.ColorId;
                        stitem.InventStyleId = productVariant.StyleId;
                        stitem.ConfigId = productVariant.ConfigId;
                    }
                }
            }
            else
                stitem.Unit = item.InventoryUnitOfMeasure;

            stitem.TrackingInfo = (await request.RequestContext.ExecuteAsync<SingleEntityDataServiceResponse<int>>(new GetItemTrackingInfoDataRequest(stitem.ItemId)).ConfigureAwait(false)).Entity;
            //stitem.Counted = (await request.RequestContext.ExecuteAsync<SingleEntityDataServiceResponse<decimal>>(new CalculateStockTakeItemDataRequest(stitem)).ConfigureAwait(false)).Entity;
            Tuple<int, decimal> entry = (await request.RequestContext.ExecuteAsync<SingleEntityDataServiceResponse<Tuple<int, decimal>>>(new CalculateStockTakeDeviceDataRequest(stitem)).ConfigureAwait(false)).Entity;
            stitem.Counted = entry.Item2;

            if ((await request.RequestContext.ExecuteAsync<SingleEntityDataServiceResponse<bool>>(new CheckStockTakeItemDataRequest(stitem)).ConfigureAwait(false)).Entity)
                stitem.JournalInfo = BitEncoder.Set(stitem.JournalInfo, 1, true);

            return new SingleEntityDataServiceResponse<StockTakeItem>(stitem);
        }

        private async Task<SingleEntityDataServiceResponse<decimal>> CountStockTakeItemOnDevice(CountStockTakeItemServiceRequest request)
        {
            ThrowIf.Null(request, "request");
            ThrowIf.Null(request.Item, "request.Item");

            StockTakeItem item = request.Item;
            Tuple<int, decimal> entry = (await request.RequestContext.ExecuteAsync<SingleEntityDataServiceResponse<Tuple<int, decimal>>>(new CalculateStockTakeDeviceDataRequest(item)).ConfigureAwait(false)).Entity;
            int entryId = entry.Item1;
            decimal newQty = request.AddMode ? entry.Item2 + item.Counted : item.Counted;
            if (entryId != 0)
            {
                if (newQty == 0)
                    await request.RequestContext.ExecuteAsync<NullResponse>(new SCJournalEntryDeleteDataRequest(entryId)).ConfigureAwait(false);
                else
                    await request.RequestContext.ExecuteAsync<NullResponse>(new UpdateStockTakeItemDataRequest(entryId, newQty)).ConfigureAwait(false);
            }
            else
                await request.RequestContext.ExecuteAsync<NullResponse>(new CreateStockTakeItemDataRequest(item, newQty)).ConfigureAwait(false);

            return new SingleEntityDataServiceResponse<decimal>(newQty);
        }

        // Offline StockTake >>
        private async Task<CountOfflineStockTakeItemsResponse> CountOfflineStockTakeItems(CountOfflineStockTakeItemsRequest request)
        {
            IEnumerable<StockTakeItem> Items = request.Items;
            CountStockTakeItemServiceRequest countRequest = null;
            decimal totalCount = decimal.Zero;
            foreach (StockTakeItem item in Items)
            {
                countRequest = new CountStockTakeItemServiceRequest(item, false);
                countRequest.RequestContext = request.RequestContext;
                totalCount += (await this.CountStockTakeItem(countRequest).ConfigureAwait(false)).Entity;
            }
            return new CountOfflineStockTakeItemsResponse(totalCount);
        }
        // Offline StockTake <<
    }
}
