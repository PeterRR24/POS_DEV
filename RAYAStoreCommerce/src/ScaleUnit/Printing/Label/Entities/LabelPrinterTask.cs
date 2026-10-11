using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

using Microsoft.Dynamics.Commerce.Runtime.DataModel;

namespace RR.Commerce.Runtime.Printing
{
    [DataContract]
    public class LabelPrinterTask
    {
        [DataMember]
        public int Quantity { get; set; }
        [DataMember]
        public string Text { get; set; }
    }
}
