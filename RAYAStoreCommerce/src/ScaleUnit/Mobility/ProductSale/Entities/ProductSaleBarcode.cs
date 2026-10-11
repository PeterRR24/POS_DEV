using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace RR.Commerce.Runtime.Mobility
{
    public class ProductSaleBarcode
    {
        public string BarcodeId { get; set; }
        public string ReasonCode { get; set; }
        public decimal Price { get; set; }
        public DateTime? ExpDate { get; set; }

    }
}
