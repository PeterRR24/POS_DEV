using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Dynamics.Commerce.Runtime.Services.Messages
{
    using System.Runtime.Serialization;
    using Microsoft.Dynamics.Commerce.Runtime.Messages;

    /// <summary>
    [DataContract]
    public sealed class StateRequest : Request
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GetStoreHoursDataRequest"/> class.
        /// </summary>
        /// <param name="state">The state.</param>
        public StateRequest()
        {
           
        }
        
    }
}
