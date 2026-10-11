using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RR.Commerce.Contracts.Reporting;
using Microsoft.Dynamics.Commerce.Runtime;

namespace RR.Commerce.Runtime.Reporting
{
    public class LabelItemVariable : ILabelVariableResolver
    {
        private IFormItemLabel _form;

        public LabelItemVariable(IFormItemLabel form)
        {
            ThrowIf.Null<IFormItemLabel>(form, "form");
            _form = form;
        }

        public int NoOfLabels { get { return _form.NumberOfLabels; } }

        public object Resolve(string name)
        {
            switch (name)
            {
                case "ItemId": return _form.ItemId;
                case "VariantId": return _form.VariantId;
                case "Barcode": return _form.Barcode;
                case "Description": return _form.Description;
                case "DescriptionAlias": return _form.DescriptionAlias;
                case "UnitId": return _form.UnitId;
                case "UnitName": return _form.UnitName;
                case "UnitPrice": return _form.UnitPrice;
                case "UnitPriceOld": return _form.UnitPriceOld;
                case "UnitPriceCur": return _form.UnitPriceCur;
                case "BaseUnitId": return _form.BaseUnitId;
                case "BaseUnitName": return _form.BaseUnitName;
                case "BaseUnitFactor": return _form.BaseUnitFactor;
                case "EquivUnitId": return _form.EquivUnitId;
                case "EquivUnitName": return _form.EquivUnitName;
                case "EquivUnitFactor": return _form.EquivUnitFactor;
                case "BatchId": return _form.BatchId;
                case "BatchExpDate": return _form.BatchExpDate;
                case "SerialId": return _form.SerialId;
                case "DimensionColorId": return _form.DimensionColorId;
                case "DimensionColorName": return _form.DimensionColorName;
                case "DimensionSizeId": return _form.DimensionSizeId;
                case "DimensionSizeName": return _form.DimensionSizeName;
                case "DimensionStyleId": return _form.DimensionStyleId;
                case "DimensionStyleName": return _form.DimensionStyleName;
                case "DimensionConfigId": return _form.DimensionConfigId;
                case "DimensionConfigName": return _form.DimensionConfigName;
                case "SeasonId": return _form.SeasonId;
                case "SeasonName": return _form.SeasonName;
                case "SectionId": return _form.SectionId;
                case "SectionName": return _form.SectionName;
                case "ShelfId": return _form.ShelfId;
                case "ShelfName": return _form.ShelfName;
                case "HierarchyDepartment": return _form.HierarchyDepartment;
                case "HierarchyCategory": return _form.HierarchyCategory;
                case "Text1": return _form.Text1;
                case "Text2": return _form.Text2;
                case "Text3": return _form.Text3;
                case "Number": return _form.Number;
                case "Period": return _form.Period;
                case "Image": return _form.Image;
                case "QRCode": return _form.QRCode;
                case "SalePrice": return _form.SalePrice;
                case "SaleExpDate": return _form.SaleExpDate;
                case "ReasonId": return _form.ReasonId;
                case "ReasonName": return _form.ReasonName;
            }
            return null;
        }
    }
}
