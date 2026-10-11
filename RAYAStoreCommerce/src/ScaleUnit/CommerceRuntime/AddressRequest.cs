using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Dynamics.Commerce.Runtime.Services.Messages
{
    using System.Runtime.Serialization;
    using Microsoft.Dynamics.Commerce.Runtime.Messages;

    /// <summary>
    [DataContract]
    public sealed class AddressRequest : Request
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GetStoreHoursDataRequest"/> class.
        /// </summary>
        /// <param name="state">The state.</param>
        public AddressRequest(string state)
        {
            this.State = state;
        }

        /// <summary>
        /// Gets the store number related to the request.
        /// </summary>
        [DataMember]
        public string State { get; private set; }
    }
}
