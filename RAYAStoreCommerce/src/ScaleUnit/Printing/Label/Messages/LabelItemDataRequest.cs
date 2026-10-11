using System.Runtime.Serialization;

using Microsoft.Dynamics.Commerce.Runtime;
using Microsoft.Dynamics.Commerce.Runtime.Messages;
using Microsoft.Dynamics.Commerce.Runtime.DataModel;
using System.Collections.Generic;

namespace RR.Commerce.Runtime.Printing
{
    [DataContract]
    public sealed class LabelItemSearchDataRequest : Request
    {
        public LabelItemSearchDataRequest(LabelLayoutType labelType, string keyword, QueryResultSettings settings)
        {
            this.LabelType = labelType;
            this.Keyword = keyword;
            this.QueryResultSettings = settings;
        }

        [DataMember]
        public LabelLayoutType LabelType { get; }
        [DataMember]
        public string Keyword { get; }
        [DataMember]
        public bool Personal { get; set; }
    }

    [DataContract]
    public sealed class LabelItemSaveDataRequest : Request
    {
        public LabelItemSaveDataRequest(LabelItem item)
        {
            this.Item = item;
        }

        [DataMember]
        public LabelItem Item { get; }
    }

    [DataContract]
    public sealed class LabelItemGetDataRequest : Request
    {
        public LabelItemGetDataRequest(long recordId)
        {
            this.RecordId = recordId;
        }

        [DataMember]
        public long RecordId { get; }
    }

    [DataContract]
    public sealed class LabelItemFindDataRequest : Request
    {
        public LabelItemFindDataRequest(LabelLayoutType labelType, long codeRecId, string itemId, string inventDimId, bool personal) : this(labelType, codeRecId, itemId, inventDimId)
        {
            this.Personal = personal;
        }

        public LabelItemFindDataRequest(LabelLayoutType labelType, long codeRecId, string itemId, string inventDimId)
        {
            this.LabelType = labelType;
            this.CodeRecId = codeRecId;
            this.ItemId = itemId;
            this.InventDimId = inventDimId;
        }

        [DataMember]
        public LabelLayoutType LabelType { get; }
        [DataMember]
        public long CodeRecId { get; set; }
        [DataMember]
        public string ItemId { get; }
        [DataMember]
        public string InventDimId { get; }
        [DataMember]
        public bool Personal { get; set; }
    }

    [DataContract]
    public sealed class LabelItemUpdateDataRequest : Request
    {
        public LabelItemUpdateDataRequest(LabelItem item)
        {
            this.Item = item;
        }

        [DataMember]
        public LabelItem Item { get; }
    }

    [DataContract]
    public sealed class LabelItemDeleteDataRequest : Request
    {
        public LabelItemDeleteDataRequest(long recordId)
        {
            this.RecordId = recordId;
        }

        [DataMember]
        public long RecordId { get; }
    }

    [DataContract]
    public sealed class LabelItemClearDataRequest : Request
    {
        public LabelItemClearDataRequest(long recordId)
        {
            this.RecordId = recordId;
        }

        public LabelItemClearDataRequest(LabelLayoutType labelType)
        {
            this.LabelType = labelType;
        }

        public LabelItemClearDataRequest(LabelLayoutType labelType, bool personal) : this(labelType)
        {
            this.Personal = personal;
        }

        [DataMember]
        public LabelLayoutType LabelType { get; }
        [DataMember]
        public bool Personal { get; set; }
        [DataMember]
        public long LayoutRecId { get; set; }
        [DataMember]
        public long PrinterRecId { get; set; }
        [DataMember]
        public long RecordId { get; set; }
    }

    [DataContract]
    public sealed class LabelItemStatisticsDataRequest : Request
    {
        public LabelItemStatisticsDataRequest(LabelLayoutType labelType, bool personal)
        {
            this.LabelType = labelType;
            this.Personal = personal;
        }

        [DataMember]
        public LabelLayoutType LabelType { get; set; }

        [DataMember]
        public bool Personal { get; set; }
    }

    [DataContract]
    public sealed class LabelItemFormatDataRequest : Request
    {
        public LabelItemFormatDataRequest(LabelItemStatistic item)
        {
            this.Item = item;
        }
        //public LabelItemFormatDataRequest(LabelItemStatistic item, IEnumerable<long> labelIds)
        //{
        //    this.Item = item;
        //    this.LabelIds = labelIds;
        //}
        [DataMember]
        public LabelItemStatistic Item { get; set; }
        //[DataMember]
        //public IEnumerable<long> LabelIds { get; set; }
    }

    [DataContract]
    public sealed class LabelItemCountDataRequest : Request
    {
        public LabelItemCountDataRequest(LabelLayoutType labelType)
        {
            this.LabelType = labelType;
        }

        public LabelItemCountDataRequest(LabelLayoutType labelType, bool personal) : this(labelType)
        {
            this.Personal = personal;
        }

        [DataMember]
        public LabelLayoutType LabelType { get; }

        [DataMember]
        public bool Personal { get; set; }
    }

    [DataContract]
    public sealed class LabelItemLoadDataRequest : Request
    {
        public LabelItemLoadDataRequest(LabelItem item)
        {
            this.Item = item;
        }

        [DataMember]
        public LabelItem Item { get; }
    }

    [DataContract]
    public sealed class LabelHistoryGetDataRequest : Request
    {
        public LabelHistoryGetDataRequest(LabelItem item)
        {
            this.Item = item;
        }

        [DataMember]
        public LabelItem Item { get; }
    }

    [DataContract]
    public sealed class LabelHistorySaveDataRequest : Request
    {
        public LabelHistorySaveDataRequest(long recordId)
        {
            this.RecordId = recordId;
        }

        public LabelHistorySaveDataRequest(LabelLayoutType labelType)
        {
            this.LabelType = labelType;
        }

        public LabelHistorySaveDataRequest(LabelLayoutType labelType, bool personal) : this(labelType)
        {
            this.Personal = personal;
        }

        [DataMember]
        public LabelLayoutType LabelType { get; }
        [DataMember]
        public bool Personal { get; set; }
        [DataMember]
        public long LayoutRecId { get; set; }
        [DataMember]
        public long PrinterRecId { get; set; }
        [DataMember]
        public long RecordId { get; set; }
    }
}
