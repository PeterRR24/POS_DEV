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
    public sealed class GetRangingItemServiceRequest : BaseRangingRequest
    {
        public GetRangingItemServiceRequest(string barcodeId)
        {
            this.BarcodeId = barcodeId;
        }
        public GetRangingItemServiceRequest(string barcodeId, string sectionId, string shelfId)
        {
            this.BarcodeId = barcodeId;
            this.SectionId = sectionId;
            this.ShelfId = shelfId;
        }
        [DataMember]
        public string BarcodeId { get; }
    }

    [DataContract]
    public sealed class CommitRangingServiceRequest : BaseRangingRequest
    {
        public CommitRangingServiceRequest()
        {
        }
        public CommitRangingServiceRequest(string sectionId, string shelfId)
        {
            this.SectionId = sectionId;
            this.ShelfId = shelfId;
        }
    }
    
}
