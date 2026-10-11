using System;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Commerce.Runtime.DataModel;
using Microsoft.Dynamics.Commerce.Runtime.Messages;

namespace Microsoft.Dynamics.Commerce.Runtime.Services.Messages
{
    [DataContract]
    public class RequestValue : Request
    {
        public RequestValue(string xValue)
        {
            this.XVALUE = xValue;
        }

        /// <summary>
        /// Gets the loyalty card number.
        /// </summary>
        [DataMember]
        public string XVALUE { get;  set; }
    }
}
