using Contoso.Commerce.Runtime;
using Microsoft.Dynamics.Commerce.Runtime.Messages;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;


namespace Microsoft.Dynamics.Commerce.Runtime.Services.Messages
{
    using System.Collections.ObjectModel;
    using System.Runtime.Serialization;
    using Commerce.Runtime.DataModel;
    using Contoso.Commerce.Runtime.DataModel;
    using Microsoft.Dynamics.Commerce.Runtime;
    using Microsoft.Dynamics.Commerce.Runtime.Messages;
    [DataContract]
    public sealed class AddressResponse : Response
    {

        public AddressResponse(PagedResult<Cities> city)
        {
            this.City = city;
        }

        /// <summary>
        /// Gets the found store hours as a paged result set.
        /// </summary>
        [DataMember]
        // public PagedResult<StoreDayHours> DayHours { get; private set; }
        public PagedResult<Cities>  City { get; private set; }

    }

}
