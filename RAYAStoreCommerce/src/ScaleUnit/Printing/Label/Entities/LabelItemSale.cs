using System;
using System.Runtime.Serialization;

using Microsoft.Dynamics.Commerce.Runtime.DataModel;
using Microsoft.Dynamics.Commerce.Runtime.ComponentModel.DataAnnotations;

namespace RR.Commerce.Runtime.Printing
{
    [DataContract]
    public class LabelItemSale
    {
        [DataMember]
        public decimal Price { get; set; }
        [DataMember]
        public string ReasonId { get; set; }
        [DataMember]
        public string ReasonName { get; set; }
        [DataMember]
        public DateTime? ExpiryDate { get; set; }
    }
}
