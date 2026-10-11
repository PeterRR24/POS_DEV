using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

using Microsoft.Dynamics.Commerce.Runtime.DataModel;

namespace RR.Commerce.Runtime.Mobility
{
    [DataContract]
    public class MobilityToken
    {
        [DataMember]
        public string DeviceId { get; set; }
        [DataMember]
        public string UserId { get; set; }
    }
}
