using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Runtime.Serialization;

namespace RR.Commerce.Contracts.Reporting
{
    public interface IFormItemLabel
    {
        string ItemId { get; }
        string VariantId { get; }       // Retail Variant - unique Id for matrix dimensions combination
        string Barcode { get; }
        string Description { get; }
        string DescriptionAlias { get; }
        string UnitId { get; }          // Sales UOM
        string UnitName { get; }
        decimal UnitPrice { get; }      // Standard Price
        decimal? UnitPriceOld { get; }  // Old Price
        decimal? UnitPriceCur { get; }  // Price in currency

        string BaseUnitId { get; }      // Base UOM
        string BaseUnitName { get; }
        decimal BaseUnitFactor { get; } // Unit Conversion form Sale to Base
        string EquivUnitId { get; }
        string EquivUnitName { get; }
        decimal EquivUnitFactor { get; }

        string BatchId { get; }
        DateTime? BatchExpDate { get; }
        string SerialId { get; }

        // Matrix dimensions
        string DimensionColorId { get; }
        string DimensionColorName { get; }
        string DimensionSizeId { get; }
        string DimensionSizeName { get; }
        string DimensionStyleId { get; }
        string DimensionStyleName { get; }
        string DimensionConfigId { get; }
        string DimensionConfigName { get; }

        string SeasonId { get; }
        string SeasonName { get; }

        // Item Store Location - tables must be added for synch.
        string SectionId { get; }
        string SectionName { get; }
        string ShelfId { get; }
        string ShelfName { get; }

        // From assigned product hierarchy
        string HierarchyDepartment { get; }
        string HierarchyCategory { get; }

        // Item Extension
        string Text1 { get; }
        string Text2 { get; }
        string Text3 { get; }
        decimal Number { get; }
        DateTime? Period { get; }
        Image Image { get; }
        Image QRCode { get; }   // QR Code

        // FireSale
        decimal? SalePrice { get; }
        string ReasonId { get; }
        string ReasonName { get; }
        DateTime? SaleExpDate { get; }

        // Number Of Labels
        int NumberOfLabels { get; }
    }
}
