using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Runtime.Serialization;

using Microsoft.Dynamics.Commerce.Runtime;
using Microsoft.Dynamics.Commerce.Runtime.Messages;
using Microsoft.Dynamics.Commerce.Runtime.DataModel;

namespace RR.Commerce.Runtime.Mobility
{
    [DataContract]
    public sealed class GetStockTakeJournalsServiceRequest : Request
    {
        public GetStockTakeJournalsServiceRequest(QueryResultSettings settings)
        {
            this.QueryResultSettings = settings;
        }
    }

    [DataContract]
    public sealed class GetStockTakeItemServiceRequest : Request
    {
        public GetStockTakeItemServiceRequest(StockTakeRequest request)
        {
            ThrowIf.Null(request, "request");

            this.BarcodeId = request.BarcodeId;
            this.JournalId = request.JournalId;
            this.SectionId = request.SectionId;
            this.ShelfId = request.ShelfId;
        }

        [DataMember]
        public string BarcodeId { get; }
        [DataMember]
        public int JournalId { get; }
        [DataMember]
        public string SectionId { get; }
        [DataMember]
        public string ShelfId { get; }
    }

    [DataContract]
    public sealed class CountStockTakeItemServiceRequest : Request
    {
        public CountStockTakeItemServiceRequest(StockTakeItem item, bool addMode)
        {
            ThrowIf.Null(item, "item");

            this.Item = item;
            this.AddMode = addMode;
        }

        [DataMember]
        public StockTakeItem Item { get; }
        [DataMember]
        public bool AddMode { get; }
    }

    // Offline StockTake >>
    [DataContract]
    public sealed class CountOfflineStockTakeItemsRequest : Request
    {
        public CountOfflineStockTakeItemsRequest(IEnumerable<StockTakeItem> items)
        {
            this.Items = items;
        }

        [DataMember]
        public IEnumerable<StockTakeItem> Items { get; }
    }
    [DataContract]
    public sealed class CountOfflineStockTakeItemsResponse : Response
    {
        public CountOfflineStockTakeItemsResponse(decimal quantity)
        {
            this.Quantity = quantity;
        }

        [DataMember]
        public decimal Quantity { get; }
    }
    // Offline StockTake <<
}
