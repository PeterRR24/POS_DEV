using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Runtime.Serialization;

using Microsoft.Dynamics.Commerce.Runtime;
using Microsoft.Dynamics.Commerce.Runtime.Messages;
using Microsoft.Dynamics.Commerce.Runtime.DataModel;

namespace RR.Commerce.Runtime.Printing
{
    [DataContract]
    public sealed class GetLabelProfileServiceRequest : Request
    {
        public GetLabelProfileServiceRequest(LabelLayoutType layoutType, long codeRecId, long productId)
        {
            this.LayoutType = layoutType;
            this.CodeRecId = codeRecId;
            this.ProductId = productId;
        }

        [DataMember]
        public LabelLayoutType LayoutType { get; }
        [DataMember]
        public long CodeRecId { get; set; }
        [DataMember]
        public long ProductId { get; }
    }

    [DataContract]
    public class GetLabelProfileServiceResponse : Response
    {
        public GetLabelProfileServiceResponse()
        {
        }

        [DataMember]
        public LabelProfileLine Profile { get; set; }

        [DataMember]
        public LabelFormat Format { get; set; }
    }

    [DataContract]
    public sealed class GetLabelItemServiceRequest : Request
    {
        public GetLabelItemServiceRequest(LabelLayoutType labelType, LabelCalculateParams parameters, string barcodeId, string itemId, string inventDimId, string unitId)
        {
            ThrowIf.Null(parameters, "parameters");

            this.LabelType = labelType;
            this.Parameters = parameters;
            this.BarcodeId = barcodeId;
            this.ItemId = itemId;
            this.InventDimId = inventDimId;
            this.UnitId = unitId;
        }

        [DataMember]
        public LabelLayoutType LabelType { get; }
        [DataMember]
        public LabelCalculateParams Parameters { get; set; }
        [DataMember]
        public string BarcodeId { get; }
        [DataMember]
        public string ItemId { get; }
        [DataMember]
        public string InventDimId { get; }
        [DataMember]
        public string UnitId { get; }
    }

    [DataContract]
    public sealed class CalcLabelItemServiceRequest : Request
    {
        public CalcLabelItemServiceRequest(LabelItem item)
        {
            this.Item = item;
        }

        [DataMember]
        public LabelItem Item { get; }
    }


    [DataContract]
    public sealed class FormatLabelItemServiceRequest : Request
    {
        public FormatLabelItemServiceRequest(long recordId, int quantity)
        {
            this.RecordId = recordId;
            this.Quantity = quantity;
        }

        [DataMember]
        public long RecordId { get; }
        [DataMember]
        public int Quantity { get; }
    }

    [DataContract]
    public sealed class PrintLabelItemServiceRequest : Request
    {
        public PrintLabelItemServiceRequest(long recordId, int quantity, int templateNo)
        {
            this.RecordId = recordId;
            this.Quantity = quantity;
        }

        [DataMember]
        public long RecordId { get; }
        [DataMember]
        public int Quantity { get; }
        [DataMember]
        public int TemplateNo { get; }
    }

    [DataContract]
    public sealed class ConfirmLabelItemServiceRequest : Request
    {
        public ConfirmLabelItemServiceRequest(long recordId)
        {
            this.RecordId = recordId;
        }

        public ConfirmLabelItemServiceRequest(IEnumerable<long> recordIds)
        {
            this.RecordIds = recordIds;
        }

        [DataMember]
        public long RecordId { get; }

        [DataMember]
        public IEnumerable<long> RecordIds { get; }
    }

    [DataContract]
    public sealed class GetLabelStatServiceRequest : Request
    {
        public GetLabelStatServiceRequest(LabelLayoutType labelType, LabelItem item, LabelPrinterTemplate template)
        {
            this.LabelType = labelType;
            this.Item = item;
            this.Template = template;
        }

        [DataMember]
        public LabelLayoutType LabelType { get; }
        [DataMember]
        public LabelItem Item { get; }
        [DataMember]
        public LabelPrinterTemplate Template { get; }
    }

    [DataContract]
    public sealed class CalcLabelStatServiceRequest : Request
    {
        public CalcLabelStatServiceRequest(LabelLayoutType labelType)
        {
            this.LabelType = labelType;
        }

        public CalcLabelStatServiceRequest(LabelLayoutType labelType, bool personal) : this(labelType)
        {
            this.Personal = personal;
        }

        [DataMember]
        public LabelLayoutType LabelType { get; }
        [DataMember]
        public bool Personal { get; set; }
    }

    [DataContract]
    public class CalcLabelStatisticsResponse : Response
    {
        public CalcLabelStatisticsResponse(IEnumerable<LabelItemStatistic> entries)
        {
            this.Entries = IEnumerableExtensions.AsReadOnly<LabelItemStatistic>(entries);
        }

        [DataMember]
        public ReadOnlyCollection<LabelItemStatistic> Entries { get; }
    }

    [DataContract]
    public sealed class TrackLabelItemServiceRequest : Request
    {
        public TrackLabelItemServiceRequest(LabelItem item, LabelFormat format, LabelItemChangeTracking changeTracking)
        {
            this.Item = item;
            this.Format = format;
            this.ChangeTracking = changeTracking;
        }

        [DataMember]
        public LabelItem Item { get; }
        [DataMember]
        public LabelFormat Format { get; }
        [DataMember]
        public LabelItemChangeTracking ChangeTracking { get; }
    }


    [DataContract]
    public sealed class GetLabelParamsServiceRequest : Request
    {
        [DataMember]
        public long CodeRecId { get; set; }
        [DataMember]
        public string LabelCode { get; set; }
        [DataMember]
        public DateTime? PriceDate { get; set; }
    }
}
