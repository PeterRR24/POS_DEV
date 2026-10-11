using System.Collections.Generic;
using System.Runtime.Serialization;

using Microsoft.Dynamics.Commerce.Runtime.Messages;
using Microsoft.Dynamics.Commerce.Runtime.DataModel;

namespace RR.Commerce.Runtime.Utilities
{
    [DataContract]
    public sealed class GetItemVariantAvailabilityDataRequest : Request
    {
        public GetItemVariantAvailabilityDataRequest(IEnumerable<ItemVariantInventoryDimension> itemVariants, QueryResultSettings queryResultSettings)
        {
            this.ItemVariants = itemVariants;
            this.QueryResultSettings = queryResultSettings;
        }

        [DataMember]
        public IEnumerable<ItemVariantInventoryDimension> ItemVariants { get; }
    }

    [DataContract]
    public sealed class GetItemVariantAvailableQtyDataRequest : Request
    {
        public GetItemVariantAvailableQtyDataRequest(string itemId, string inventDimId, long productId)
        {
            this.ItemId = itemId;
            this.InventDimId = inventDimId;
            this.ProductId = productId;
        }

        [DataMember]
        public string ItemId { get; }
        [DataMember]
        public string InventDimId { get; }
        [DataMember]
        public long ProductId {get; }
    }
}
