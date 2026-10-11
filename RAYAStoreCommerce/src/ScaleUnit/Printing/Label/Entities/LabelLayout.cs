using System;
using System.Runtime.Serialization;
using SystemDataAnnotations = System.ComponentModel.DataAnnotations;

using Microsoft.Dynamics.Commerce.Runtime.DataModel;
using Microsoft.Dynamics.Commerce.Runtime.ComponentModel.DataAnnotations;

namespace RR.Commerce.Runtime.Printing
{
    public class LabelLayout : CommerceEntity
    {
        private const string IdColumn = "RECID";

        public LabelLayout() : base("LabelLayout")
        {
        }
        [SystemDataAnnotations.Key]

        [Key]
        [DataMember]
        [Column(IdColumn)]
        public long Id
        {
            get { return (long)this[IdColumn]; }
            set { this[IdColumn] = value; }
        }

        [DataMember]
        [Column("LAYOUTID")]
        public string LayoutId
        {
            get { return (string)this["LAYOUTID"]; }
            set { this["LAYOUTID"] = value; }
        }

        [DataMember]
        [Column("NAME")]
        public string Name
        {
            get { return (string)this["NAME"]; }
            set { this["NAME"] = value; }
        }

        [DataMember]
        [Column("LABELTYPE")]
        public LabelLayoutType LabelType
        {
            get { return (LabelLayoutType)(this["LABELTYPE"] ?? LabelLayoutType.Shelf); }
            set { this["LABELTYPE"] = value; }
        }

        [DataMember]
        [Column("INACTIVE")]
        public bool Inactive
        {
            get { return Convert.ToBoolean(this["INACTIVE"]); }
            set { this["INACTIVE"] = (value ? 1 : 0); }
        }

        [DataMember]
        [Column("TEMPLATE")]
        public string Template
        {
            get { return (string)this["TEMPLATE"]; }
            set { this["TEMPLATE"] = value; }
        }

        [DataMember]
        [Column("TEMPLATE1")]
        public string Template1
        {
            get { return (string)this["TEMPLATE1"]; }
            set { this["TEMPLATE1"] = value; }
        }

        [DataMember]
        [Column("TEMPLATE2")]
        public string Template2
        {
            get { return (string)this["TEMPLATE2"]; }
            set { this["TEMPLATE2"] = value; }
        }

        [DataMember]
        [Column("TEMPLATE3")]
        public string Template3
        {
            get { return (string)this["TEMPLATE3"]; }
            set { this["TEMPLATE3"] = value; }
        }
    }
}
