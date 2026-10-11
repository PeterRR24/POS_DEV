using System;
using System.Runtime.Serialization;
using SystemDataAnnotations = System.ComponentModel.DataAnnotations;

using Microsoft.Dynamics.Commerce.Runtime.DataModel;
using Microsoft.Dynamics.Commerce.Runtime.ComponentModel.DataAnnotations;

using RR.Commerce.Runtime.Utilities;

namespace RR.Commerce.Runtime.Printing
{
    public class LabelData : CommerceEntity
    {
        private const string IdColumn = "RECID";

        public LabelData() : base("LabelData")
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
        [Column("ITEMID")]
        public string ItemId
        {
            get { return (string)this["ITEMID"]; }
            set { this["ITEMID"] = value; }
        }

        [DataMember]
        [Column("VARIANTID")]
        public string VariantId
        {
            get { return (string)this["VARIANTID"]; }
            set { this["VARIANTID"] = value; }
        }

        [DataMember]
        [Column("EQUIVUNITID")]
        public string EquivUnitId
        {
            get { return (string)this["EQUIVUNITID"]; }
            set { this["EQUIVUNITID"] = value; }
        }

        [DataMember]
        [Column("TEXT")]
        public string Text1 { get { return (string)(this["TEXT"] ?? String.Empty); } }

        [DataMember]
        [Column("TEXT2")]
        public string Text2 { get { return (string)(this["TEXT2"] ?? String.Empty); } }

        [DataMember]
        [Column("TEXT3")]
        public string Text3 { get { return (string)(this["TEXT3"] ?? String.Empty); } }

        [DataMember]
        [Column("NUMBER")]
        public decimal Number { get { return Convert.ToDecimal(this["NUMBER"] ?? Decimal.Zero); } }

        [DataMember]
        [Column("PERIOD")]
        public DateTime? Period { get { return DataReaderHelper.GetDateTime(this["PERIOD"]); } }

        [DataMember]
        [Column("IMAGE")]
        public byte[] Image{ get { return (byte[])this["IMAGE"]; } }

        [DataMember]
        [Column("QRCODE")]
        public byte[] QRCode { get { return (byte[])this["QRCODE"]; } }
    }
}
