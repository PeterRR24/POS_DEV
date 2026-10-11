using System;
using System.Runtime.Serialization;

using Microsoft.Dynamics.Commerce.Runtime;
using Microsoft.Dynamics.Commerce.Runtime.DataModel;
using Microsoft.Dynamics.Commerce.Runtime.ComponentModel.DataAnnotations;

using RR.Commerce.Runtime.Utilities;

namespace RR.Commerce.Runtime.Printing
{
    public class LabelHistory : CommerceEntity
    {
        public LabelHistory() : base("LabelHistory")
        {
        }

        [DataMember]
        [Column("CHANNELID")]
        public long ChannelId
        {
            get { return (long)this["CHANNELID"]; }
            set { this["CHANNELID"] = value; }
        }

        [DataMember]
        [Column("ITEMID")]
        public string ItemId
        {
            get { return (string)this["ITEMID"]; }
            set { this["ITEMID"] = value; }
        }

        [DataMember]
        [Column("INVENTDIMID")]
        public string InventDimId
        {
            get { return (string)this["INVENTDIMID"]; }
            set { this["INVENTDIMID"] = value; }
        }

        [DataMember]
        [Column("UNITID")]
        public string UnitId
        {
            get { return (string)this["UNITID"]; }
            set { this["UNITID"] = value; }
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
            get { return (long)(this["CODERECID"] ?? 0); }
            set { this["CODERECID"] = value; }
        }

        [DataMember]
        [Column("DATAAREAID")]
        public string DataAreaId
        {
            get { return (string)this["DATAAREAID"]; }
            set { this["DATAAREAID"] = value; }
        }

        [DataMember]
        [Column("LASTUPDATED")]
        public DateTimeOffset LastUpdated
        {
            get { return (DateTimeOffset)(this["LASTUPDATED"] ?? DateTimeOffsetExtensions.AxMinDateValue); }
            set { this["LASTUPDATED"] = value; }
        }

        [DataMember]
        [Column("LAYOUTRECID")]
        public long LayoutlId
        {
            get { return (long)(this["LAYOUTRECID"] ?? 0); }
            set { this["LAYOUTRECID"] = value; }
        }

        [DataMember]
        [Column("FORMATRECID")]
        public long FormatId
        {
            get { return (long)(this["FORMATRECID"] ?? 0); }
            set { this["FORMATRECID"] = value; }
        }

        [DataMember]
        [Column("BARCODE")]
        public string Barcode
        {
            get { return (string)this["BARCODE"]; }
            set { this["BARCODE"] = value; }
        }

        [DataMember]
        [Column("NAME")]
        public string Name
        {
            get { return (string)this["NAME"]; }
            set { this["NAME"] = value; }
        }

        [DataMember]
        [Column("DESCRIPTION")]
        public string Description
        {
            get { return (string)this["DESCRIPTION"]; }
            set { this["DESCRIPTION"] = value; }
        }

        [DataMember]
        [Column("UNITPRICE")]
        public decimal UnitPrice
        {
            get { return Convert.ToDecimal(this["UNITPRICE"] ?? Decimal.Zero); }
            set { this["UNITPRICE"] = value; }
        }

        [DataMember]
        [Column("UNITPRICEOLD")]
        public decimal UnitPriceOld
        {
            get { return Convert.ToDecimal(this["UNITPRICEOLD"] ?? Decimal.Zero); }
            set { this["UNITPRICEOLD"] = value; }
        }

        [DataMember]
        [Column("UNITPRICECUR")]
        public decimal UnitPriceCurrency
        {
            get { return Convert.ToDecimal(this["UNITPRICECUR"] ?? Decimal.Zero); }
            set { this["UNITPRICECUR"] = value; }
        }
    }
}
