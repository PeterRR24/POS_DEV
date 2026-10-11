using System.Collections.Generic;
using System.Runtime.Serialization;

using Microsoft.Dynamics.Commerce.Runtime.Messages;
using Microsoft.Dynamics.Commerce.Runtime.DataModel;

namespace RR.Commerce.Runtime.Utilities
{
    [DataContract]
    public sealed class GetItemVariantAvailabilityRequest : Request
    {
        public GetItemVariantAvailabilityRequest(IEnumerable<ItemVariantInventoryDimension> itemVariants) : this(itemVariants, QueryResultSettings.AllRecords)
        {
        }

        public GetItemVariantAvailabilityRequest(IEnumerable<ItemVariantInventoryDimension> itemVariants, QueryResultSettings queryResultSettings)
        {
            this.ItemVariants = itemVariants;
            this.QueryResultSettings = queryResultSettings;
        }

        [DataMember]
        public IEnumerable<ItemVariantInventoryDimension> ItemVariants { get; }
    }
}
