using System.Collections.Generic;
using System.Runtime.Serialization;

using Microsoft.Dynamics.Commerce.Runtime;
using Microsoft.Dynamics.Commerce.Runtime.Messages;
using Microsoft.Dynamics.Commerce.Runtime.DataModel;

namespace RR.Commerce.Runtime.Utilities
{
    [DataContract]
    public sealed class GetItemVariantAvailabilityResponse : Response
    {
        public GetItemVariantAvailabilityResponse(PagedResult<ItemAvailableQuantity> quantities)
        {
            this.ItemAvailableQuantities = quantities;
        }

        [DataMember]
        public PagedResult<ItemAvailableQuantity> ItemAvailableQuantities { get; }
    }
}
