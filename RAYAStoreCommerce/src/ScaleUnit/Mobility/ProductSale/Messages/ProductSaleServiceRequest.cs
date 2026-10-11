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
    public sealed class GetProductSaleServiceRequest : Request
    {
        public GetProductSaleServiceRequest(string barcodeId)
        {
            this.BarcodeId = barcodeId;
        }

        [DataMember]
        public string BarcodeId { get; }
    }

    [DataContract]
    public sealed class CalcProductSaleServiceRequest : Request
    {
        public CalcProductSaleServiceRequest(string barcodeId, string reasonCode)
        {
            this.BarcodeId = barcodeId;
            this.ReasonCode = reasonCode;
        }

        [DataMember]
        public string BarcodeId { get; }
        [DataMember]
        public string ReasonCode { get; }
    }

    [DataContract]
    public sealed class FormatProductSaleServiceRequest : Request
    {
        public FormatProductSaleServiceRequest(ProductSaleRequest saleRequest)
        {
            this.SaleRequest = saleRequest;
        }

        [DataMember]
        public ProductSaleRequest SaleRequest { get; }
    }

    [DataContract]
    public sealed class LabelProductSaleServiceRequest : Request
    {
        public LabelProductSaleServiceRequest(ProductSaleRequest saleRequest, int templateNo, int noOfLabels)
        {
            this.SaleRequest = saleRequest;
            this.TemplateNo = templateNo;
            this.NoOfLabels = noOfLabels;
        }

        [DataMember]
        public ProductSaleRequest SaleRequest { get; }
        [DataMember]
        public int TemplateNo { get; }
        [DataMember]
        public int NoOfLabels { get; }
    }

}
