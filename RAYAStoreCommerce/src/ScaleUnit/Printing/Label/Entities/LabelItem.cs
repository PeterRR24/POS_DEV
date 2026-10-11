using System;
using System.Runtime.Serialization;
using SystemDataAnnotations = System.ComponentModel.DataAnnotations;

using Microsoft.Dynamics.Commerce.Runtime;
using Microsoft.Dynamics.Commerce.Runtime.DataModel;
using Microsoft.Dynamics.Commerce.Runtime.ComponentModel.DataAnnotations;

using RR.Commerce.Runtime.Utilities;

namespace RR.Commerce.Runtime.Printing
{
    public enum LabelLayoutType
    {
        Shelf = 0,
        Product = 1,
        FireSale = 5
    }

    public class LabelItem : CommerceEntity
    {
        private const string IdColumn = "RECID";

        public LabelItem() : base("LabelItem")
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

        [IgnoreDataMember]
        [Column("LABELTYPE")]
        public LabelLayoutType LabelType
        {
            get { return (LabelLayoutType)(this["LABELTYPE"] ?? LabelLayoutType.Shelf); }
            set { this["LABELTYPE"] = value; }
        }

        [DataMember]
        public int LabelTypeValue
        {
            get { return (int)this.LabelType; }
            set { this.LabelType = (LabelLayoutType)value; }
        }

        [DataMember]
        [Column("CODERECID")]
        public long CodeRecId
        {
            get { return (long)(this["CODERECID"] ?? 0); }
            set { this["CODERECID"] = value; }
        }

        [DataMember]
        [Column("LABELCODE")]
        public string LabelCode { get { return (string)(this["LABELCODE"] ?? String.Empty); } }

        [DataMember]
        [Column("DEVICEID")]
        public string DeviceId
        {
            get { return (string)this["DEVICEID"]; }
            set { this["DEVICEID"] = value; }
        }

        [DataMember]
        [Column("DATECREATED")]
        public DateTimeOffset DateCreated
        {
            get { return (DateTimeOffset)(this["DATECREATED"] ?? DateTimeOffsetExtensions.AxMinDateValue); }
            set { this["DATECREATED"] = value; }
        }

        [DataMember]
        [Column("DATAAREAID")]
        public string DataAreaId
        {
            get { return (string)this["DATAAREAID"]; }
            set { this["DATAAREAID"] = value; }
        }

        #region Item

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
        [Column("UNITID")]
        public string UnitId
        {
            get { return (string)this["UNITID"]; }
            set { this["UNITID"] = value; }
        }

        [DataMember]
        [Column("UNITNAME")]
        public string UnitName { get { return (string)(this["UNITNAME"] ?? String.Empty); } }

        [DataMember]
        [Column("BASEUNITID")]
        public string BaseUnitId
        {
            get { return (string)this["BASEUNITID"]; }
            set { this["BASEUNITID"] = value; }
        }

        [DataMember]
        [Column("BASEUNITNAME")]
        public string BaseUnitName { get { return (string)(this["BASEUNITNAME"] ?? String.Empty); } }

        [DataMember]
        [Column("BASEUNITQTY")]
        public decimal BaseUnitQty
        {
            get { return Convert.ToDecimal(this["BASEUNITQTY"] ?? Decimal.Zero); }
            set { this["BASEUNITQTY"] = value; }
        }

        [DataMember]
        [Column("EQUIVUNITID")]
        public string EquivUnitId
        {
            get { return (string)this["EQUIVUNITID"]; }
            set { this["EQUIVUNITID"] = value; }
        }

        [DataMember]
        [Column("EQUIVUNITNAME")]
        public string EquivUnitName { get { return (string)(this["EQUIVUNITNAME"] ?? String.Empty); } }

        [DataMember]
        [Column("EQUIVUNITQTY")]
        public decimal EquivUnitQty
        {
            get { return Convert.ToDecimal(this["EQUIVUNITQTY"] ?? Decimal.Zero); }
            set { this["EQUIVUNITQTY"] = value; }
        }

        [DataMember]
        [Column("PRICEDATE")]
        public DateTime PriceDate
        {
            get { return (DateTime)(this["PRICEDATE"] ?? DateTime.Today); }
            set { this["PRICEDATE"] = value; }
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
        public decimal? UnitPriceOld
        {
            get
            {
                object val = this["UNITPRICEOLD"];
                if (val == null)
                    return null;
                else
                    return Convert.ToDecimal(val);
            }
            set { this["UNITPRICEOLD"] = value; }
        }

        [DataMember]
        [Column("UNITPRICECUR")]
        public decimal? UnitPriceCurrency
        {
            get
            {
                object val = this["UNITPRICECUR"];
                if (val == null)
                    return null;
                else
                    return Convert.ToDecimal(val);
            }
            set { this["UNITPRICECUR"] = value; }
        }
        
        [DataMember]
        [Column("SALEPRICE")]
        public decimal? SalePrice
        {
            get
            {
                object val = this["SALEPRICE"];
                if (val == null)
                    return null;
                else
                    return Convert.ToDecimal(val);
            }
            set { this["SALEPRICE"] = value; }
        }
        #endregion

        #region Variant
        [DataMember]
        [Column("VARIANTID")]
        public string VariantId
        {
            get { return (string)this["VARIANTID"]; }
            set { this["VARIANTID"] = value; }
        }

        [DataMember]
        [Column("DIMCOLORID")]
        public string DimColorId
        {
            get { return (string)this["DIMCOLORID"]; }
            set { this["DIMCOLORID"] = value; }
        }

        [DataMember]
        [Column("DIMCOLORNAME")]
        public string DimColorName
        {
            get { return (string)this["DIMCOLORNAME"]; }
            set { this["DIMCOLORNAME"] = value; }
        }

        [DataMember]
        [Column("DIMSIZEID")]
        public string DimSizeId
        {
            get { return (string)this["DIMSIZEID"]; }
            set { this["DIMSIZEID"] = value; }
        }

        [DataMember]
        [Column("DIMSIZENAME")]
        public string DimSizeName
        {
            get { return (string)this["DIMSIZENAME"]; }
            set { this["DIMSIZENAME"] = value; }
        }

        [DataMember]
        [Column("DIMSTYLEID")]
        public string DimStyleId
        {
            get { return (string)this["DIMSTYLEID"]; }
            set { this["DIMSTYLEID"] = value; }
        }

        [DataMember]
        [Column("DIMSTYLENAME")]
        public string DimStyleName
        {
            get { return (string)this["DIMSTYLENAME"]; }
            set { this["DIMSTYLENAME"] = value; }
        }

        [DataMember]
        [Column("DIMCONFIGID")]
        public string DimConfigId
        {
            get { return (string)this["DIMCONFIGID"]; }
            set { this["DIMCONFIGID"] = value; }
        }

        [DataMember]
        [Column("DIMCONFIGNAME")]
        public string DimConfigName
        {
            get { return (string)this["DIMCONFIGNAME"]; }
            set { this["DIMCONFIGNAME"] = value; }
        }
        #endregion

        #region Misc
        [DataMember]
        [Column("SEASONID")]
        public string SeasonId
        {
            get { return (string)this["SEASONID"]; }
            set { this["SEASONID"] = value; }
        }

        [DataMember]
        [Column("SEASONNAME")]
        public string SeasonName { get { return (string)(this["SEASONNAME"] ?? String.Empty); } }

        [DataMember]
        [Column("SECTIONID")]
        public string SectionId
        {
            get { return (string)this["SECTIONID"]; }
            set { this["SECTIONID"] = value; }
        }

        [DataMember]
        [Column("SECTIONNAME")]
        public string SectionName { get { return (string)(this["SECTIONNAME"] ?? String.Empty); } }

        [DataMember]
        [Column("SHELFID")]
        public string ShelfId
        {
            get { return (string)this["SHELFID"]; }
            set { this["SHELFID"] = value; }
        }

        [DataMember]
        [Column("SHELFNAME")]
        public string ShelfName { get { return (string)(this["SHELFNAME"] ?? String.Empty); } }

        [DataMember]
        [Column("LOCATION")]
        public int Location
        {
            get { return (int)this["LOCATION"]; }
            set { this["LOCATION"] = value; }
        }

        [DataMember]
        [Column("DEPARTMENT")]
        public string Department
        {
            get { return (string)this["DEPARTMENT"]; }
            set { this["DEPARTMENT"] = value; }
        }

        [DataMember]
        [Column("CATEGORY")]
        public string Category
        {
            get { return (string)this["CATEGORY"]; }
            set { this["CATEGORY"] = value; }
        }
        #endregion

        #region Extension
        [DataMember]
        [Column("DATARECID")]
        public long DataRecId
        {
            get { return (long)this["DATARECID"]; }
            set { this["DATARECID"] = value; }
        }

        [DataMember]
        [Column("TEXT")]
        public string Text1 //{ get { return (string)(this["TEXT"] ?? String.Empty); }  }
        {
            get { return (string)(this["TEXT"] ?? String.Empty); }
            set { this["TEXT"] = value; }
        }

        [DataMember]
        [Column("TEXT2")]
        public string Text2 //{ get { return (string)(this["TEXT2"] ?? String.Empty); } }
        {
            get { return (string)(this["TEXT2"] ?? String.Empty); }
            set { this["TEXT2"] = value; }
        }

        [DataMember]
        [Column("TEXT3")]
        public string Text3 { get { return (string)(this["TEXT3"] ?? String.Empty); } }

        [DataMember]
        [Column("NUMBER")]
        public decimal Number { get { return Convert.ToDecimal(this["NUMBER"] ?? Decimal.Zero); } }

        [DataMember]
        [Column("PERIOD")]
        public DateTime? Period { get { return DataReaderHelper.GetDateTime(this["PERIOD"]); } }
        #endregion

        #region Printing
        [DataMember]
        [Column("LAYOUTRECID")]
        public long LayoutlId
        {
            get { return (long)(this["LAYOUTRECID"] ?? 0); }
            set { this["LAYOUTRECID"] = value; }
        }

        [DataMember]
        [Column("PRINTERRECID")]
        public long PrinterId
        {
            get { return (long)(this["PRINTERRECID"] ?? 0); }
            set { this["PRINTERRECID"] = value; }
        }

        [DataMember]
        [Column("FORMATRECID")]
        public long FormatId
        {
            get { return (long)(this["FORMATRECID"] ?? 0); }
            set { this["FORMATRECID"] = value; }
        }

        [DataMember]
        [Column("NOOFLABELS")]
        public int NoOfLabels
        {
            get { return Convert.ToInt32(this["NOOFLABELS"] ?? 0); }
            set { this["NOOFLABELS"] = value; }
        }

        [DataMember]
        [Column("PRINTBATCHNO")]
        public int PrintBatchNo
        {
            get { return Convert.ToInt32(this["PRINTBATCHNO"] ?? 0); }
            set { this["PRINTBATCHNO"] = value; }
        }
        #endregion

        [DataMember]
        public long ProductId
        {
            get { return (long)(this["PRODUCTID"] ?? 0L); }
            set { this["PRODUCTID"] = value; }
        }

        public void Init(long channelId, string dataAreaId, string deviceId)
        {
            this.Id = 0;
            this.ChannelId = channelId;
            this.ItemId = String.Empty;
            this.InventDimId = String.Empty;
            this.LabelType = LabelLayoutType.Shelf;
            this.CodeRecId = 0;
            this.DeviceId = deviceId;
            this.DateCreated = DateTimeOffset.Now;
            this.DataAreaId = dataAreaId;
            this.DataRecId = 0;
            this.PriceDate = DateTime.Today;
            this.UnitPrice = 0;
            this.Location = 0;
            this.PrintBatchNo = 0;
        }

        public void CopyVariant(ProductVariant variant)
        {
            ThrowIf.Null(variant, "variant");
            this.InventDimId = variant.InventoryDimensionId;
            this.VariantId = variant.VariantId;
            this.DimColorId = variant.ColorId;
            this.DimColorName = variant.Color;
            this.DimConfigId = variant.ConfigId;
            this.DimConfigName = variant.Configuration;
            this.DimSizeId = variant.SizeId;
            this.DimSizeName = variant.Size;
            this.DimStyleId = variant.StyleId;
            this.DimStyleName = variant.Style;
        }
    }
}
