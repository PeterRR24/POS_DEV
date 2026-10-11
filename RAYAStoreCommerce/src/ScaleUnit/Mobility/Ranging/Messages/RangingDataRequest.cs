using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

using Microsoft.Dynamics.Commerce.Runtime;
using Microsoft.Dynamics.Commerce.Runtime.Messages;
using Microsoft.Dynamics.Commerce.Runtime.DataModel;

namespace RR.Commerce.Runtime.Mobility
{
    [DataContract]
    public class BaseRangingRequest : Request
    {
        public BaseRangingRequest()
        {
        }
        public BaseRangingRequest(string sectionId, string shelfId)
        {
            this.SectionId = sectionId;
            this.ShelfId = shelfId;
        }
        [DataMember]
        public string SectionId { get; set; }
        [DataMember]
        public string ShelfId { get; set; }
    }

    [DataContract]
    public sealed class RangingCreateDataRequest : BaseRangingRequest
    {
        public RangingCreateDataRequest()
        {
        }
        public RangingCreateDataRequest(string sectionId, string shelfId)
        {
            this.SectionId = sectionId;
            this.ShelfId = shelfId;
        }
    }

    [DataContract]
    public sealed class RangingSelectDataRequest : BaseRangingRequest
    {
        public RangingSelectDataRequest()
        {
        }
        public RangingSelectDataRequest(string sectionId, string shelfId)
        {
            this.SectionId = sectionId;
            this.ShelfId = shelfId;
        }
    }

    [DataContract]
    public sealed class RangingItemGetDataRequest : BaseRangingRequest
    {
        public RangingItemGetDataRequest(string itemId)
        {
            this.ItemId = itemId;
        }
        public RangingItemGetDataRequest(string itemId, string sectionId, string shelfId)
        {
            this.ItemId = itemId;
            this.SectionId = sectionId;
            this.ShelfId = shelfId;
        }
        [DataMember]
        public string ItemId { get; }
    }

    [DataContract]
    public sealed class RangingItemAddDataRequest : Request
    {
        public RangingItemAddDataRequest(RangingItem item)
        {
            this.Item = item;
        }

        [DataMember]
        public RangingItem Item { get; }
    }

    [DataContract]
    public sealed class RangingItemUndoDataRequest : BaseRangingRequest
    {
        public RangingItemUndoDataRequest()
        {            
        }

        public RangingItemUndoDataRequest(string sectionId, string shelfId)
        {
            this.SectionId = sectionId;
            this.ShelfId = shelfId;
        }        
    }

    [DataContract]
    public sealed class RangingItemDeleteDataRequest : Request
    {
        public RangingItemDeleteDataRequest(RangingItem item)
        {
            this.Item = item;
        }        
        [DataMember]
        public RangingItem Item { get; }        
    }

    [DataContract]
    public sealed class RangingStatDataRequest : BaseRangingRequest
    {
        public RangingStatDataRequest()
        {
        }
        public RangingStatDataRequest(string sectionId, string shelfId)
        {
            this.SectionId = sectionId;
            this.ShelfId = shelfId;
        }
    }

    [DataContract]
    public sealed class RangingResetDataRequest : BaseRangingRequest
    {
        public RangingResetDataRequest()
        {
        }
        public RangingResetDataRequest(string sectionId, string shelfId)
        {
            this.SectionId = sectionId;
            this.ShelfId = shelfId;
        }
    }

}

