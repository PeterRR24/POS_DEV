using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

using RR.Commerce.Runtime.Printing;
using RR.Commerce.Contracts.Reporting;
using System.Xml.Serialization;
using System.Runtime.Serialization;

namespace RR.Commerce.Runtime.Reporting
{
    public class FormItemLabel : IFormItemLabel
    {
        private LabelItem ITEM;
        private LabelItemSale IFS;
        private int _numberOfLabels;

        private Image _image = null;
        private Image _qrCode = null;

        public FormItemLabel()
        {
        }

        public FormItemLabel(LabelItem item, LabelItemSale ifs, int numberOfLabels)
        {
            if (item == null)
                throw new ArgumentNullException("item");
            ITEM = item;
            IFS = ifs;
            _numberOfLabels = numberOfLabels;
        }
        public string ItemId { get { return ITEM.ItemId; } set { return; } }
        public string VariantId { get { return ITEM.VariantId; } set { return; } }
        public string Barcode { get { return ITEM.Barcode; } set { return; } }
        public string Description { get { return ITEM.Name; } set { return; } }
        public string DescriptionAlias { get { return ITEM.Description; } set { return; } }
        public string UnitId { get { return ITEM.UnitId; } set { return; } }
        public string UnitName { get { return ITEM.UnitName; } set { return; } }
        public decimal UnitPrice { get { return ITEM.UnitPrice; } set { return; } }
        public decimal? UnitPriceOld { get { return ITEM.UnitPriceOld; } set { return; } }
        public decimal? UnitPriceCur { get { return ITEM.UnitPriceCurrency; } set { return; } }
        public string BaseUnitId { get { return ITEM.BaseUnitId; } set { return; } }
        public string BaseUnitName { get { return ITEM.BaseUnitName; } set { return; } }
        public decimal BaseUnitFactor { get { return ITEM.BaseUnitQty; } set { return; } }
        public string EquivUnitId { get { return ITEM.EquivUnitId; } set { return; } }
        
        public string EquivUnitName { get { return ITEM.EquivUnitName; } set { return; } }
        
        public decimal EquivUnitFactor { get { return ITEM.EquivUnitQty; } set { return; } }
        
        public string BatchId { get { return String.Empty; } set { return; } }
        public DateTime? BatchExpDate { get { return null; } set { return; } }
        
        public string SerialId { get { return String.Empty; } set { return; } }
        
        public string DimensionColorId { get { return ITEM.DimColorId; } set { return; } }
        
        public string DimensionColorName { get { return ITEM.DimColorName; } set { return; } }
        
        public string DimensionSizeId { get { return ITEM.DimSizeId; } set { return; } }
        
        public string DimensionSizeName { get { return ITEM.DimSizeName; } set { return; } }
        
        public string DimensionStyleId { get { return ITEM.DimStyleId; } set { return; } }
        
        public string DimensionStyleName { get { return ITEM.DimStyleName; } set { return; } }
        
        public string DimensionConfigId { get { return ITEM.DimConfigId; } set { return; } }
        
        public string DimensionConfigName { get { return ITEM.DimConfigName; } set { return; } }
        
        public string SeasonId { get { return ITEM.SeasonId; } set { return; } }
        
        public string SeasonName { get { return ITEM.SeasonName; } set { return; } }
        
        public string SectionId { get { return ITEM.SectionId; } set { return; } }
        
        public string SectionName { get { return ITEM.SectionName; } set { return; } }
        
        public string ShelfId { get { return ITEM.ShelfId; } set { return; } }
        
        public string ShelfName { get { return ITEM.ShelfName; } set { return; } }
        
        public string HierarchyDepartment { get { return ITEM.Department; } set { return; } }
        
        public string HierarchyCategory { get { return ITEM.Category; } set { return; } }
        
        public string Text1 { get { return ITEM.Text1; } set { return; } }
        
        public string Text2 { get { return ITEM.Text2; } set { return; } }
        
        public string Text3 { get { return ITEM.Text3; } set { return; } }
        
        public decimal Number { get { return ITEM.Number; } set { return; } }
        public DateTime? Period { get { return ITEM.Period; } set { return; } }
        public Image Image { get { return _image; } set { return; } }
        
        public Image QRCode { get { return _qrCode; } set { return; } }
        
        public decimal? SalePrice { get { if (IFS == null) return null; else return IFS.Price; } set { return; } }
        
        public string ReasonId { get { return IFS == null ? String.Empty : IFS.ReasonId; } set { return; } }
        
        public string ReasonName { get { return IFS == null ? String.Empty : IFS.ReasonName; } set { return; } }
        
        public DateTime? SaleExpDate { get { return IFS == null ? null : IFS.ExpiryDate; } set { return; } }
        
        public int NumberOfLabels { get { return _numberOfLabels; } set { return; } }

        internal void SetImage(byte[] value)
        {
            _image = ObjectEncoder.TryDecodeImage(value);
        }

        internal void SetQRCode(byte[] value)
        {
            _qrCode = ObjectEncoder.TryDecodeImage(value);
        }
    }
}
