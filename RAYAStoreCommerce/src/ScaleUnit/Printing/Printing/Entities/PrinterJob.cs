using System;
using System.Runtime.Serialization;

using Microsoft.Dynamics.Commerce.Runtime.DataModel;

namespace RR.Commerce.Runtime.Printing
{
    public class PrinterJob : CommerceEntity
    {
        public PrinterJob() : base("PrinterJob")
        {
        }

        [DataMember]
        public string PrinterName { get; set; }
        [DataMember]
        public string PrinterType { get; set; }
        [DataMember]
        public string Document { get; set; }
        [DataMember]
        public string Template { get; set; }
        [DataMember]
        public string LabelData { get; set; }
    }
}
