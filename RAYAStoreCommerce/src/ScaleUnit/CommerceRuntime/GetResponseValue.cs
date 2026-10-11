using Microsoft.Dynamics.Commerce.Runtime.Messages;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Microsoft.Dynamics.Commerce.Runtime.Services.Messages
{
    [DataContract]
    public class GetResponseValue : Response
    {
        public GetResponseValue(string value)
        { Value = value; }

        [DataMember]
        public string Value { get; set; }

    }
}
