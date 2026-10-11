using System;
using System.Runtime.Serialization;

using Microsoft.Dynamics.Commerce.Runtime.DataModel;
using Microsoft.Dynamics.Commerce.Runtime.ComponentModel.DataAnnotations;

namespace RR.Commerce.Runtime.Utilities
{
    [DataContract]
    public class ItemStoreLocation
    {
        [DataMember]
        public string ItemId { get; set; }
        [DataMember]
        public string SectionId { get; set; }
        [DataMember]
        public string ShelfId { get; set; }
        [DataMember]
        public int Location { get; set; }
    }
}
