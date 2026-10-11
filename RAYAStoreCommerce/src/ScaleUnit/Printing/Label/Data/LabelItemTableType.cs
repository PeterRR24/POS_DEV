using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Dynamics.Commerce.Runtime;
using Microsoft.Dynamics.Commerce.Runtime.DataModel;
using Microsoft.Dynamics.Commerce.Runtime.Data.Types;

namespace RR.Commerce.Runtime.Printing
{
    public sealed class LabelItemTableType : TableType
    {
        public LabelItemTableType(IEnumerable<LabelItem> items) : base("ext.RRPRLABELITEMTABLETYPE")
        {
            ThrowIf.Null<IEnumerable<LabelItem>>(items, "items");
            this.CreateTableSchema();
            foreach (LabelItem item in items.Distinct<LabelItem>())
                this.DataTable.Rows.Add(this.GetDataRow(item));
        }

        public LabelItemTableType(LabelItem item) : base("ext.RRPRLABELITEMTABLETYPE")
        {
            ThrowIf.Null<LabelItem>(item, "item");
            this.CreateTableSchema();
            this.DataTable.Rows.Add(this.GetDataRow(item));
        }

        protected override void CreateTableSchema()
        {
            this.DataTable.Columns.Add("RECID", typeof(long));
            this.DataTable.Columns.Add("DATAAREAID", typeof(string));
            this.DataTable.Columns.Add("CHANNELID", typeof(long));
            this.DataTable.Columns.Add("DEVICEID", typeof(string));
            this.DataTable.Columns.Add("ITEMID", typeof(string));
            this.DataTable.Columns.Add("INVENTDIMID", typeof(string));
            this.DataTable.Columns.Add("LABELTYPE", typeof(int));
            this.DataTable.Columns.Add("DATECREATED", typeof(DateTimeOffset));
            this.DataTable.Columns.Add("LAYOUTRECID", typeof(long));
            this.DataTable.Columns.Add("FORMATRECID", typeof(long));
            this.DataTable.Columns.Add("PRINTERRECID", typeof(long));
            this.DataTable.Columns.Add("NOOFLABELS", typeof(int));
            this.DataTable.Columns.Add("BARCODE", typeof(string));
            this.DataTable.Columns.Add("NAME", typeof(string));
            this.DataTable.Columns.Add("DESCRIPTION", typeof(string));
            this.DataTable.Columns.Add("UNITID", typeof(string));
            this.DataTable.Columns.Add("BASEUNITID", typeof(string));
            this.DataTable.Columns.Add("BASEUNITQTY", typeof(decimal));
            this.DataTable.Columns.Add("EQUIVUNITID", typeof(string));
            this.DataTable.Columns.Add("EQUIVUNITQTY", typeof(decimal));
            this.DataTable.Columns.Add("UNITPRICE", typeof(decimal));
            this.DataTable.Columns.Add("UNITPRICEOLD", typeof(decimal));
            this.DataTable.Columns.Add("UNITPRICECUR", typeof(decimal));
            this.DataTable.Columns.Add("SALEPRICE", typeof(decimal));
            this.DataTable.Columns.Add("SECTIONID", typeof(string));
            this.DataTable.Columns.Add("SHELFID", typeof(string));
            this.DataTable.Columns.Add("SEASONID", typeof(string));
            this.DataTable.Columns.Add("DEPARTMENT", typeof(string));
            this.DataTable.Columns.Add("CATEGORY", typeof(string));
            this.DataTable.Columns.Add("DATARECID", typeof(long));
            this.DataTable.Columns.Add("CODERECID", typeof(long));
            this.DataTable.Columns.Add("PRICEDATE", typeof(DateTime));
            this.DataTable.Columns.Add("LOCATION", typeof(int));
        }

        private DataRow GetDataRow(LabelItem item)
        {
            DataRow dataRow = this.DataTable.NewRow();
            dataRow["RECID"] = item.Id;
            dataRow["DATAAREAID"] = item.DataAreaId ?? String.Empty;
            dataRow["CHANNELID"] = item.ChannelId;
            dataRow["DEVICEID"] = item.DeviceId ?? String.Empty;
            dataRow["ITEMID"] = item.ItemId ?? String.Empty;
            dataRow["INVENTDIMID"] = item.InventDimId ?? String.Empty;
            dataRow["LABELTYPE"] = (int)item.LabelType;
            dataRow["DATECREATED"] = item.DateCreated == DateTimeOffset.MinValue ? DateTimeOffset.Now : item.DateCreated;
            dataRow["DATECREATED"] = DateTime.Now;
            dataRow["LAYOUTRECID"] = item.LayoutlId;
            dataRow["FORMATRECID"] = item.FormatId;
            dataRow["PRINTERRECID"] = item.PrinterId;
            dataRow["NOOFLABELS"] = item.NoOfLabels;
            dataRow["BARCODE"] = item.Barcode ?? String.Empty;
            dataRow["NAME"] = item.Name ?? String.Empty;
            dataRow["DESCRIPTION"] = item.Description ?? String.Empty;
            dataRow["UNITID"] = item.UnitId ?? String.Empty;
            dataRow["BASEUNITID"] = item.BaseUnitId ?? String.Empty;
            dataRow["BASEUNITQTY"] = item.BaseUnitQty;
            dataRow["EQUIVUNITID"] = item.EquivUnitId ?? String.Empty;
            dataRow["EQUIVUNITQTY"] = item.EquivUnitQty;
            dataRow["UNITPRICE"] = item.UnitPrice;
            if (item.UnitPriceOld.HasValue)
                dataRow["UNITPRICEOLD"] = item.UnitPriceOld.Value;
            else
                dataRow["UNITPRICEOLD"] = null;
            if (item.UnitPriceCurrency.HasValue)
                dataRow["UNITPRICECUR"] = item.UnitPriceCurrency.Value;
            else
                dataRow["UNITPRICECUR"] = null;
            if (item.SalePrice.HasValue)
                dataRow["SALEPRICE"] = item.SalePrice.Value;
            else
                dataRow["SALEPRICE"] = null;
            dataRow["SECTIONID"] = item.SectionId ?? String.Empty;
            dataRow["SHELFID"] = item.ShelfId ?? String.Empty;
            dataRow["SEASONID"] = item.SeasonId ?? String.Empty;
            dataRow["DEPARTMENT"] = item.Department ?? String.Empty;
            dataRow["CATEGORY"] = item.Category ?? String.Empty;
            dataRow["DATARECID"] = item.DataRecId;
            dataRow["CODERECID"] = item.CodeRecId;
            dataRow["PRICEDATE"] = item.PriceDate == DateTime.MinValue ? DateTime.Today : item.PriceDate;
            dataRow["LOCATION"] = item.Location;
            return dataRow;
        }
    }
}
