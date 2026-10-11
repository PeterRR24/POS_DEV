using System;
using System.Runtime.Serialization;
using System.Text;

namespace RR.Commerce.Runtime.Mobility
{
    [DataContract]
    public sealed class LabelPrintParams
    {
        [DataMember]
        public long CodeRecId { get; set; }
        [DataMember]
        public string LabelCode { get; set; }
        [DataMember]
        public DateTime? PriceDate { get; set; }
    }

}
