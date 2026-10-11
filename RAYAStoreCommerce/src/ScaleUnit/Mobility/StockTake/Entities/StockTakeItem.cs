using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace RR.Commerce.Runtime.Mobility
{
    [DataContract]
    public sealed class StockTakeItem
    {
        [DataMember]
        public int JournalId { get; set; }
        [DataMember]
        public string ItemId { get; set; }
        [DataMember]
        public string ItemName { get; set; }
        [DataMember]
        public string Unit { get; set; }
        [DataMember]
        public string InventBatchId { get; set; }
        [DataMember]
        public string InventSerialId { get; set; }
        [DataMember]
        public string InventSizeId { get; set; }
        [DataMember]
        public string InventColorId { get; set; }
        [DataMember]
        public string InventStyleId { get; set; }
        [DataMember]
        public string ConfigId { get; set; }
        [DataMember]
        public string SectionId { get; set; }
        [DataMember]
        public string ShelfId { get; set; }
        [DataMember]
        public decimal Counted { get; set; }
        [DataMember]
        public int JournalInfo { get; set; }
        [DataMember]
        public int TrackingInfo { get; set; }
    }
}
