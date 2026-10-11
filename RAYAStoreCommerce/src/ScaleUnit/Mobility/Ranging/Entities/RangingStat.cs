using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace RR.Commerce.Runtime.Mobility
{
    [DataContract]
    public class RangingStat
    {
        [DataMember]
        public string SectionId { get; set; }
        [DataMember]
        public string ShelfId { get; set; }
        [DataMember]
        public int Count { get; set; }
        [DataMember]
        public int ItemKey { get; set; }
        [DataMember]
        public string ItemId { get; set; }
        [DataMember]
        public string ItemName { get; set; }
    }
}
