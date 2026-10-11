using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

using Microsoft.Dynamics.Commerce.Runtime;
using Microsoft.Dynamics.Commerce.Runtime.Messages;
using Microsoft.Dynamics.Commerce.Runtime.DataModel;

namespace RR.Commerce.Runtime.Mobility
{
    [DataContract]
    public sealed class ProductSaleReasonGetDataRequest : Request
    {
        public ProductSaleReasonGetDataRequest(int recordId)
        {
            this.RecordId = recordId;
        }

        public ProductSaleReasonGetDataRequest(string code)
        {
            this.Code = code;
        }

        [DataMember]
        public int RecordId { get; }

        [DataMember]
        public string Code { get; }
    }

    [DataContract]
    public sealed class ProductSaleReasonSearchDataRequest : Request
    {
        public ProductSaleReasonSearchDataRequest()
        {
        }
    }
}
