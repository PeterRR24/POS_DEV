using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace RR.Commerce.Runtime.Mobility
{
    [DataContract]
    public class ProductSaleRequest
    {
        [DataMember]
        public string BarcodeId { get; set; }

        [DataMember]
        public string ItemId { get; set; }

        [DataMember]
        public string InventDimId { get; set; }

        [DataMember]
        public string UnitId { get; set; }

        [DataMember]
        public string ReasonCode { get; set; }

        [DataMember]
        public double Price { get; set; }

        [DataMember]
        public int ExpDate { get; set; }
    }
}
