using System;
using System.Runtime.Serialization;
using SystemDataAnnotations = System.ComponentModel.DataAnnotations;

using Microsoft.Dynamics.Commerce.Runtime.DataModel;
using Microsoft.Dynamics.Commerce.Runtime.ComponentModel.DataAnnotations;

namespace RR.Commerce.Runtime.Printing
{
    public class LabelFormat : CommerceEntity
    {
        private const string IdColumn = "RECID";

        public LabelFormat() : base("LabelFormat")
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
        [Column("NAME")]
        public string Name
        {
            get { return (string)this["NAME"]; }
            set { this["NAME"] = value; }
        }

        [DataMember]
        [Column("PRICE")]
        public bool Price
        {
            get { return Convert.ToBoolean(this["PRICE"]); }
            set { this["PRICE"] = (value ? 1 : 0); }
        }

        [DataMember]
        [Column("PRICEOLD")]
        public bool OldPrice
        {
            get { return Convert.ToBoolean(this["PRICEOLD"]); }
            set { this["PRICEOLD"] = (value ? 1 : 0); }
        }

        [DataMember]
        [Column("PRICECUR")]
        public bool PriceCurrency
        {
            get { return Convert.ToBoolean(this["PRICECUR"]); }
            set { this["PRICECUR"] = (value ? 1 : 0); }
        }

        [DataMember]
        [Column("SALEPRICE")]
        public bool SalePrice
        {
            get { return Convert.ToBoolean(this["SALEPRICE"]); }
            set { this["SALEPRICE"] = (value ? 1 : 0); }
        }

        [DataMember]
        [Column("TRACKING")]
        public bool Tracking
        {
            get { return Convert.ToBoolean(this["TRACKING"]); }
            set { this["TRACKING"] = (value ? 1 : 0); }
        }

        public static LabelFormat Default()
        {
            LabelFormat lf = new LabelFormat();
            lf.Id = 0;
            lf.Price = true;
            lf.PriceCurrency = false;
            lf.Tracking = false;
            return lf;
        }
    }
}
