using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

using Microsoft.Dynamics.Commerce.Runtime;

using RR.Commerce.Contracts.Reporting;
using System.Xml.Serialization;

namespace RR.Commerce.Runtime.Reporting
{
    public abstract class PrrLabelBase : PrrBase
    {
        protected abstract IFormItemLabel ItemLabel { get; }

        // properties common for Product and Shelf labels
        public string ItemId { get { return NotNull(ItemLabel.ItemId); } }
        public string VariantId { get { return NotNull(ItemLabel.VariantId); } }
        public string Barcode { get { return NotNull(ItemLabel.Barcode); } }
        public string Description { get { return NotNull(ItemLabel.Description); } }
        public string DescriptionAlias { get { return NotNull(ItemLabel.DescriptionAlias); } }
        public string UnitId { get { return NotNull(ItemLabel.UnitId); } }
        public string UnitName { get { return String.IsNullOrEmpty(ItemLabel.UnitName) ? ItemLabel.UnitId : ItemLabel.UnitName; } }
        public decimal UnitPrice { get { return ItemLabel.UnitPrice; } }
        public decimal? UnitPriceOld { get { return ItemLabel.UnitPriceOld; } }
        public decimal? UnitPriceCur { get { return ItemLabel.UnitPriceCur; } }

        public string BaseUnitId { get { return NotNull(ItemLabel.BaseUnitId); } }
        public string BaseUnitName { get { return String.IsNullOrEmpty(ItemLabel.BaseUnitName) ? BaseUnitId : ItemLabel.BaseUnitName; } }
        public decimal BaseUnitFactor { get { return ItemLabel.BaseUnitFactor; } } // ItemLabel.UnitFactor or 1 / ItemLabel.UnitFactor
        public string BaseUnitQty { get { return ItemLabel.BaseUnitFactor == 0 ? String.Empty : ItemLabel.BaseUnitFactor.ToString() + ItemLabel.BaseUnitId; } } // should return 500G
        public decimal UnitPriceBase { get { return ItemLabel.BaseUnitFactor == 0 ? 0 : ItemLabel.UnitPrice / ItemLabel.BaseUnitFactor; } } // ItemLabel.UnitPrice * ItemLabel.UnitFactor 

        public string EquivUnitId { get { return NotNull(ItemLabel.EquivUnitId); } }
        public string EquivUnitName { get { return String.IsNullOrEmpty(ItemLabel.EquivUnitName) ? EquivUnitId : ItemLabel.EquivUnitName; } }
        public decimal EquivUnitFactor { get { return ItemLabel.EquivUnitFactor; } }

        public string EquivUnitQty { get { return ItemLabel.EquivUnitFactor == 0 ? String.Empty : ItemLabel.EquivUnitFactor.ToString() + ItemLabel.EquivUnitId; } }
        public decimal? UnitPriceEquiv { get { return GetUnitOfMeasurePrice(ItemLabel.UnitPrice, ItemLabel.EquivUnitFactor); } }

        public string DimensionColorId { get { return NotNull(ItemLabel.DimensionColorId); } }
        public string DimensionColorName { get { return NotNull(ItemLabel.DimensionColorName); } }
        public string DimensionSizeId { get { return NotNull(ItemLabel.DimensionSizeId); } }
        public string DimensionSizeName { get { return NotNull(ItemLabel.DimensionSizeName); } }
        public string DimensionStyleId { get { return NotNull(ItemLabel.DimensionStyleId); } }
        public string DimensionStyleName { get { return NotNull(ItemLabel.DimensionStyleName); } }
        public string DimensionConfigId { get { return NotNull(ItemLabel.DimensionConfigId); } }
        public string DimensionConfigName { get { return NotNull(ItemLabel.DimensionConfigName); } }

        public string SeasonId { get { return NotNull(ItemLabel.SeasonId); } }
        public string SeasonName { get { return NotNull(ItemLabel.SeasonName); } }

        // Item Extension
        public string Text1 { get { return NotNull(ItemLabel.Text1); } }
        public string Text2 { get { return NotNull(ItemLabel.Text2); } }
        public string Text3 { get { return NotNull(ItemLabel.Text3); } }
        public decimal Number { get { return ItemLabel.Number; } }
        public DateTime? Period { get { return ItemLabel.Period; } }
        public Image Image { get { return ItemLabel.Image; } }
        public Image QRCode { get { return ItemLabel.QRCode; } }

        private decimal? GetUnitOfMeasurePrice(decimal price, decimal factor)
        {
            if (factor == 0)
                return null;
            else
                return price / factor;
        }
    }

    public class PrhFormLabel : RohCore
    {
        private readonly PrcBaseForm RO;
        private ReportTranslation _translation = null;

        internal PrhFormLabel(PrcBaseForm ro)
        {
            RO = ro;
        }

        internal void Build(long layoutRecId, string cultureName)
        {
            // Implement translation in future
            // ReportTranslation translation = new ReportTranslation();
            // IEnumerable<RecReceiptTranslation> list = DocReceiptTranslation.Select(ctx.DefaultScope, layoutId, cultureName);
            // foreach (RecReceiptTranslation rec in list)
            //    translation.Add(rec.Name, rec.Text);
            // return translation.IsEmpty ? null : translation;
        }

        public override RequestContext Context { get { return RO.Context; } }

        public override string TranslateText(string textId, string defaultText)
        {
            if (_translation == null) return defaultText;
            string text = _translation.Get(textId);
            return String.IsNullOrEmpty(text) ? defaultText : text;
        }
    }
}
