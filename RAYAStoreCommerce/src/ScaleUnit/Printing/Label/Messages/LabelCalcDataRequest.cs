using System.Runtime.Serialization;

using Microsoft.Dynamics.Commerce.Runtime;
using Microsoft.Dynamics.Commerce.Runtime.Messages;
using Microsoft.Dynamics.Commerce.Runtime.DataModel;

namespace RR.Commerce.Runtime.Printing
{
    [DataContract]
    public sealed class GetLabelProfileDataRequest : Request
    {
        public GetLabelProfileDataRequest(long productId)
        {
            this.ProductId = productId;
        }

        [DataMember]
        public long ProductId { get; }
    }

    [DataContract]
    public sealed class GetLabelProfileLineDataRequest : Request
    {
        public GetLabelProfileLineDataRequest(long profileId, LabelLayoutType layoutType, long codeRecId)
        {
            this.ProfileId = profileId;
            this.LayoutType = layoutType;
            this.CodeRecId = codeRecId;
        }

        [DataMember]
        public LabelLayoutType LayoutType { get; }
        [DataMember]
        public long CodeRecId { get; set; }
        [DataMember]
        public long ProfileId { get; }
    }

    [DataContract]
    public sealed class GetLabelFormatDataRequest : Request
    {
        public GetLabelFormatDataRequest(long formatId)
        {
            this.FormatId = formatId;
        }

        [DataMember]
        public long FormatId { get; }
    }

    [DataContract]
    public sealed class GetLabelLayoutDataRequest : Request
    {
        public GetLabelLayoutDataRequest(long layoutId)
        {
            this.LayoutId = layoutId;
        }

        [DataMember]
        public long LayoutId { get; }
    }

    [DataContract]
    public sealed class GetLabelDataDataRequest : Request
    {
        public GetLabelDataDataRequest(long recId)
        {
            this.RecId = recId;
        }

        public GetLabelDataDataRequest(string itemId, string variantId)
        {
            this.ItemId = itemId;
            this.VariantId = variantId;
        }

        [DataMember]
        public long RecId { get; }
        [DataMember]
        public string ItemId { get; }
        [DataMember]
        public string VariantId { get; }
    }

    [DataContract]
    public sealed class GetLabelCodeDataRequest : Request
    {
        public GetLabelCodeDataRequest()
        {
        }
    }
}
