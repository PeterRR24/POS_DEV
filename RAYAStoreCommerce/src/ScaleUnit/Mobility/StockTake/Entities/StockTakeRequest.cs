using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace RR.Commerce.Runtime.Mobility
{
    [DataContract]
    public sealed class StockTakeRequest
    {
        [DataMember]
        public int JournalId { get; set; }
        [DataMember]
        public string SectionId { get; set; }
        [DataMember]
        public string ShelfId { get; set; }
        [DataMember]
        public string BarcodeId { get; set; }
    }
}
