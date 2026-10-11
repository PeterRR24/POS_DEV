using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

using Microsoft.Dynamics.Commerce.Runtime.DataModel;

namespace RR.Commerce.Runtime.Printing
{
    [DataContract]
    public sealed class LabelItemCalculateParams
    {
        [DataMember]
        public long CodeRecId { get; set; }
        [DataMember]
        public string LabelCode { get; set; }
        [DataMember]
        public DateTime? PriceDate { get; set; }
    }

    [DataContract]
    public sealed class LabelCalculateParams
    {
        public LabelCalculateParams()
        {
        }

        public LabelCalculateParams(long codeRecId, DateTime priceDate)
        {
            this.CodeRecId = codeRecId;
            this.PriceDate = priceDate;
        }

        [DataMember]
        public long CodeRecId { get; set; }
        [DataMember]
        public DateTime PriceDate { get; set; }

        public static LabelCalculateParams Default()
        {
            return new LabelCalculateParams(0, DateTime.Today);
        }
    }
}
