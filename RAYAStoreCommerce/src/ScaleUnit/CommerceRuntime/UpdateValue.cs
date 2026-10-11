using Microsoft.Dynamics.Commerce.Runtime.Messages;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Microsoft.Dynamics.Commerce.Runtime.Services.Messages
{
    [DataContract]
    public class UpdateValue : Request
    {
        public UpdateValue(string itemid,string upValue)
        {
            ItemID = itemid;
            UpValue = upValue;
        }

        /// <summary>
        /// Gets the loyalty card number.
        /// </summary>
        [DataMember]
        public string ItemID { get; set; }

        [DataMember]
        public string UpValue { get; set; }
    }
}
