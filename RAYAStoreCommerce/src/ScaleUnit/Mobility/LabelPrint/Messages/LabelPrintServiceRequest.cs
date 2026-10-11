using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Runtime.Serialization;

using Microsoft.Dynamics.Commerce.Runtime;
using Microsoft.Dynamics.Commerce.Runtime.Messages;

using RR.Commerce.Runtime.Printing;

namespace RR.Commerce.Runtime.Mobility
{
    [DataContract]
    public sealed class FindLabelPrintItemServiceRequest : Request
    {
        public FindLabelPrintItemServiceRequest(LabelLayoutType labelType, LabelCalculateParams parameters, string barcodeId)
        {
            ThrowIf.Null(parameters, "parameters");

            this.LabelType = labelType;
            this.Parameters = parameters;
            this.BarcodeId = barcodeId;
        }

        [DataMember]
        public LabelLayoutType LabelType { get; }
        [DataMember]
        public LabelCalculateParams Parameters { get; set; }
        [DataMember]
        public string BarcodeId { get; }
    }

    [DataContract]
    public sealed class GetLabelPrintItemServiceRequest : Request
    {
        public GetLabelPrintItemServiceRequest(LabelLayoutType labelType, string barcodeId)
        {
            this.LabelType = labelType;
            this.BarcodeId = barcodeId;
        }

        [DataMember]
        public LabelLayoutType LabelType { get; }
        [DataMember]
        public string BarcodeId { get; }
    }

    [DataContract]
    public sealed class AddLabelPrintItemServiceRequest : Request
    {
        public AddLabelPrintItemServiceRequest(LabelLayoutType labelType, LabelCalculateParams parameters, LabelPrintItem item)
        {
            ThrowIf.Null(parameters, "parameters");

            this.LabelType = labelType;
            this.Parameters = parameters;
            this.Item = item;
        }

        [DataMember]
        public LabelLayoutType LabelType { get; }
        [DataMember]
        public LabelCalculateParams Parameters { get; set; }
        [DataMember]
        public LabelPrintItem Item { get; }
    }

    [DataContract]
    public sealed class FormatLabelPrintItemServiceRequest : Request
    {
        public FormatLabelPrintItemServiceRequest(LabelLayoutType labelType, LabelCalculateParams parameters, ItemLabelRequest item)
        {
            ThrowIf.Null(parameters, "parameters");

            this.LabelType = labelType;
            this.Parameters = parameters;
            this.Item = item;
        }

        [DataMember]
        public LabelLayoutType LabelType { get; }
        [DataMember]
        public LabelCalculateParams Parameters { get; }
        [DataMember]
        public ItemLabelRequest Item { get; }
        [DataMember]
        public LabelItemSale ItemSale { get; set; }
    }

    [DataContract]
    public sealed class PrintLabelPrintItemServiceRequest : Request
    {
        public PrintLabelPrintItemServiceRequest(LabelLayoutType labelType, LabelCalculateParams parameters, ItemLabelRequest item, int templateNo)
        {
            ThrowIf.Null(parameters, "parameters");

            this.LabelType = labelType;
            this.Parameters = parameters;
            this.Item = item;
            this.TemplateNo = templateNo;
        }

        [DataMember]
        public LabelLayoutType LabelType { get; }
        [DataMember]
        public LabelCalculateParams Parameters { get; }
        [DataMember]
        public ItemLabelRequest Item { get; }
        [DataMember]
        public int TemplateNo { get; }
        [DataMember]
        public LabelItemSale ItemSale { get; set; }
    }

}
