using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace RR.Commerce.Runtime.Mobility
{
    [DataContract]
    public sealed class StockTakeJournal
    {
        [DataMember]
        public int Id { get; set; }
        [DataMember]
        public string JournalId { get; set; }
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public SCJournalCalculation CalcMode { get; set; }
        [DataMember]
        public SCJournalLocation LocationMode { get; set; }
        [DataMember]
        public DateTime CreatedDate { get; set; }
    }
}
