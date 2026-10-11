using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

using RR.Commerce.Runtime.Printing;

namespace RR.Commerce.Runtime.Mobility
{
    [DataContract]
    public class LabelPrintStat
    {
        public LabelLayoutType LabelType { get; set; }

        [DataMember]
        public int LabelTypeValue
        {
            get { return (int)this.LabelType; }
            set { this.LabelType = (LabelLayoutType)value; }
        }

        [DataMember]
        public int Count { get; set; }
        [DataMember]
        public int Quantity { get; set; }
    }
}
