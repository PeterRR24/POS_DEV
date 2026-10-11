using System;
using System.Runtime.Serialization;

using Microsoft.Dynamics.Commerce.Runtime.DataModel;
using Microsoft.Dynamics.Commerce.Runtime.ComponentModel.DataAnnotations;

using RR.Commerce.Runtime.Utilities;

namespace RR.Commerce.Runtime.Printing
{
    [DataContract]
    public class LabelItemStatistic
    {
        public LabelLayoutType LabelType { get; set; }

        [DataMember]
        public int LabelTypeValue
        {
            get { return (int)this.LabelType; }
            set { this.LabelType = (LabelLayoutType)value; }
        }

        [DataMember]
        public bool Personal { get; set; }
        [DataMember]
        public long LayoutRecId { get; set; }
        [DataMember]
        public string LayoutId { get; set; }
        [DataMember]
        public string LayoutName { get; set; }
        [DataMember]
        public string LayoutText { get; set; }

        [DataMember]
        public long PrinterRecId { get; set; }
        [DataMember]
        public string PrinterName { get; set; }
        [DataMember]
        public bool LabelPrinter { get; set; }

        [DataMember]
        public int NoOfLabels { get; set; }

        [DataMember]
        public string LabelRecIds { get; set; }

        [DataMember]
        public string ErrorText { get; set; }

        internal bool HasError { get { return !String.IsNullOrEmpty(ErrorText); } }

        internal void AddError(string errorText)
        {
            if (!HasError)
                this.ErrorText = errorText;
        }
    }
}
