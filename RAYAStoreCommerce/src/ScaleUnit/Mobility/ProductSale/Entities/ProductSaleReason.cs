using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using SystemDataAnnotations = System.ComponentModel.DataAnnotations;

using Microsoft.Dynamics.Commerce.Runtime.DataModel;
using Microsoft.Dynamics.Commerce.Runtime.ComponentModel.DataAnnotations;

namespace RR.Commerce.Runtime.Mobility
{
    public enum FireSaleDiscMethod
    {
        None,
        Discount,
    }

    public enum FireSaleDiscMode
    {
        Price,
        Discount,
    }

    public sealed class ProductSaleReason : CommerceEntity
    {
        private const string IdColumn = "RECID";

        public ProductSaleReason() : base("ProductSaleReason")
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
        [Column("CODE")]
        public string Code
        {
            get { return (string)this["CODE"]; }
            set { this["CODE"] = value; }
        }

        [DataMember]
        [Column("DESCRIPTION")]
        public string Name
        {
            get { return (string)this["DESCRIPTION"]; }
            set { this["DESCRIPTION"] = value; }
        }

        [DataMember]
        [Column("EXPIRYDATE")]
        public bool ExpDate
        {
            get { return Convert.ToBoolean(this["EXPIRYDATE"]); }
            set { this["EXPIRYDATE"] = (value ? 1 : 0); }
        }

        [DataMember]
        [Column("DISCOUNTPERCENT")]
        public decimal Discount
        {
            get { return Convert.ToDecimal(this["DISCOUNTPERCENT"] ?? Decimal.Zero); }
            set { this["DISCOUNTPERCENT"] = value; }
        }

        //

        [DataMember]
        [Column("LABELCODERECID")]
        internal long LabelCodeId
        {
            get { return (long)(this["LABELCODERECID"] ?? 0); }
        }

        //

        [DataMember]
        [Column("INFOSUBCODERECID")]
        internal long SubReasonRecId
        {
            get { return (long)this["INFOSUBCODERECID"]; }
        }

        [DataMember]
        [Column("REASONCODEID")]
        internal string ReasonCodeId
        {
            get { return (string)(this["REASONCODEID"] ?? String.Empty); }
        }

        [DataMember]
        [Column("SUBCODEID")]
        internal string SubReasonCodeId
        {
            get { return (string)(this["SUBCODEID"] ?? String.Empty); }
        }

        //

        [DataMember]
        [Column("DISCOUNTMETHOD")]
        internal FireSaleDiscMethod DiscountMethod
        {
            get { return (FireSaleDiscMethod)this["DISCOUNTMETHOD"]; }
        }

        [DataMember]
        [Column("DISCOUNTMODE")]
        internal FireSaleDiscMode DiscountMode
        {
            get { return (FireSaleDiscMode)this["DISCOUNTMODE"]; }
        }
    }
}
