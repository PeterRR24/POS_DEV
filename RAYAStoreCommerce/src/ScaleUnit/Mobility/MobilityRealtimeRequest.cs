using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Runtime.Serialization;

using Microsoft.Dynamics.Commerce.Runtime;
using Microsoft.Dynamics.Commerce.Runtime.DataModel;
using Microsoft.Dynamics.Commerce.Runtime.RealtimeServices.Messages;

namespace RR.Commerce.Runtime.Mobility
{
    [DataContract]
    public sealed class CommitRangingRealtimeRequest : RealtimeRequest
    {
        public CommitRangingRealtimeRequest(string storeId, string sectionId, string shelfId, IEnumerable<RangingItem> items)
        {
            this.StoreId = storeId;
            this.SectionId = sectionId;
            this.ShelfId = shelfId;
            this.Items = items.AsReadOnly();
        }

        [DataMember]
        public string StoreId { get; }

        [DataMember]
        public string SectionId { get; }

        [DataMember]
        public string ShelfId { get; }

        [DataMember]
        public ReadOnlyCollection<RangingItem> Items { get; }
    }

    [DataContract]
    public sealed class CommitStockCountRealtimeRequest : RealtimeRequest
    {
        public CommitStockCountRealtimeRequest(StockCountJournal journal)
        {
            this.Journal = journal;
        }

        [DataMember]
        public StockCountJournal Journal { get; }
    }
}
