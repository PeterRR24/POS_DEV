using System;
using System.Runtime.Serialization;
using SystemDataAnnotations = System.ComponentModel.DataAnnotations;

using Microsoft.Dynamics.Commerce.Runtime.DataModel;
using Microsoft.Dynamics.Commerce.Runtime.ComponentModel.DataAnnotations;

namespace RR.Commerce.Runtime.Printing
{
    public class LabelProfileLine : CommerceEntity
    {
        private const string IdColumn = "RECID";

        public LabelProfileLine() : base("LabelProfileLine")
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
        [Column("PROFILERECID")]
        public long ProfileId
        {
            get { return (long)this["PROFILERECID"]; }
            set { this["PROFILERECID"] = value; }
        }

        [DataMember]
        [Column("LABELTYPE")]
        public LabelLayoutType LabelType
        {
            get { return (LabelLayoutType)(this["LABELTYPE"] ?? LabelLayoutType.Shelf); }
            set { this["LABELTYPE"] = value; }
        }

        [DataMember]
        [Column("CODERECID")]
        public long CodeRecId
        {
            get { return (long)(this["CODERECID"] ); }
            set { this["CODERECID"] = value; }
        }

        [DataMember]
        [Column("LAYOUTRECID")]
        public long LayoutId
        {
            get { return (long)this["LAYOUTRECID"]; }
            set { this["LAYOUTRECID"] = value; }
        }

        [DataMember]
        [Column("PRINTERRECID")]
        public long PrinterId
        {
            get { return (long)this["PRINTERRECID"]; }
            set { this["PRINTERRECID"] = value; }
        }

        [DataMember]
        [Column("FORMATRECID")]
        public long FormatId
        {
            get { return (long)this["FORMATRECID"]; }
            set { this["FORMATRECID"] = value; }
        }

        internal bool Active { get { return this.LayoutId != 0 && this.PrinterId != 0; } }
    }
}
