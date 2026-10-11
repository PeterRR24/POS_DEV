using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace RR.Commerce.Runtime.Mobility
{
    [DataContract]
    public sealed class SCJournalRequest
    {
        [DataMember]
        public string RecordId { get; set; }
        [DataMember]
        public string JournalId { get; set; }
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public int CalcMode { get; set; }
        [DataMember]
        public int LocationMode { get; set; }
    }
}
