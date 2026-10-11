using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using SystemDataAnnotations = System.ComponentModel.DataAnnotations;

using Microsoft.Dynamics.Commerce.Runtime.DataModel;
using Microsoft.Dynamics.Commerce.Runtime.ComponentModel.DataAnnotations;

namespace RR.Commerce.Runtime.Mobility
{
    public enum SCJournalTransStatus
    {
        Default,
    }

    public class SCJournalTransaction : CommerceEntity
    {
        private const string IdColumn = "ID";

        public SCJournalTransaction() : base("SCJournalTransaction")
        {
        }

        public StockCountJournalTransaction ToModel()
        {
            StockCountJournalTransaction tran = new StockCountJournalTransaction();
            tran.RecordId = RecordId;
            tran.ItemId = ItemId;
            tran.ItemName = ItemName;
            tran.Unit = Unit;
            tran.InventDimId = InventDimId;
            tran.InventSizeId = InventSizeId;
            tran.InventColorId = InventColorId;
            tran.InventStyleId = InventStyleId;
            tran.InventBatchId = InventBatchId;
            tran.InventSerialId = InventSerialId;
            tran.ConfigId = ConfigId;
            return tran;
        }
        [SystemDataAnnotations.Key]

        [Key]
        [DataMember]
        [Column(IdColumn)]
        public int Id
        {
            get { return (int)this[IdColumn]; }
            set { this[IdColumn] = value; }
        }

        [DataMember]
        [Column("RECID")]
        public long RecordId
        {
            get { return (long)this["RECID"]; }
            set { this["RECID"] = value; }
        }

        [DataMember]
        [Column("JOURNALID")]
        public int JournalId
        {
            get { return (int)this["JOURNALID"]; }
            set { this["JOURNALID"] = value; }
        }

        [DataMember]
        [Column("ITEMID")]
        public string ItemId
        {
            get { return (string)this["ITEMID"]; }
            set { this["ITEMID"] = value; }
        }

        [DataMember]
        [Column("ITEMNAME")]
        public string ItemName
        {
            get { return (string)this["ITEMNAME"]; }
            set { this["ITEMNAME"] = value; }
        }

        [DataMember]
        [Column("UNIT")]
        public string Unit
        {
            get { return (string)this["UNIT"]; }
            set { this["UNIT"] = value; }
        }

        [DataMember]
        [Column("INVENTDIMID")]
        public string InventDimId
        {
            get { return (string)this["INVENTDIMID"]; }
            set { this["INVENTDIMID"] = value; }
        }

        [DataMember]
        [Column("INVENTSIZEID")]
        public string InventSizeId
        {
            get { return (string)this["INVENTSIZEID"]; }
            set { this["INVENTSIZEID"] = value; }
        }

        [DataMember]
        [Column("INVENTCOLORID")]
        public string InventColorId
        {
            get { return (string)this["INVENTCOLORID"]; }
            set { this["INVENTCOLORID"] = value; }
        }

        [DataMember]
        [Column("INVENTSTYLEID")]
        public string InventStyleId
        {
            get { return (string)this["INVENTSTYLEID"]; }
            set { this["INVENTSTYLEID"] = value; }
        }

        [DataMember]
        [Column("CONFIGID")]
        public string ConfigId
        {
            get { return (string)this["CONFIGID"]; }
            set { this["CONFIGID"] = value; }
        }

        [DataMember]
        [Column("INVENTBATCHID")]
        public string InventBatchId
        {
            get { return (string)this["INVENTBATCHID"]; }
            set { this["INVENTBATCHID"] = value; }
        }

        [DataMember]
        [Column("INVENTSERIALID")]
        public string InventSerialId
        {
            get { return (string)this["INVENTSERIALID"]; }
            set { this["INVENTSERIALID"] = value; }
        }

        [IgnoreDataMember]
        [Column("STATUS")]
        public SCJournalTransStatus Status
        {
            get { return (SCJournalTransStatus)(this["STATUS"] ?? SCJournalTransStatus.Default); }
            set { this["STATUS"] = value; }
        }

        [DataMember]
        public int StatusValue
        {
            get { return (int)this.Status; }
            set { this.Status = (SCJournalTransStatus)value; }
        }

        [DataMember]
        [Column("QUANTITY")]
        public decimal Quantity
        {
            get { return Convert.ToDecimal(this["QUANTITY"] ?? Decimal.Zero); }
            set { this["QUANTITY"] = value; }
        }

        [DataMember]
        [Column("QTYADJUST")]
        public decimal? QtyAdjust
        {
            get
            {
                object val = this["QTYADJUST"];
                if (val == null)
                    return null;
                else
                    return Convert.ToDecimal(val);
            }
            set { this["QTYADJUST"] = value; }
        }

        [DataMember]
        [Column("QTYENTRY")]
        public decimal? QtyEntry
        {
            get
            {
                object val = this["QTYENTRY"];
                if (val == null)
                    return null;
                else
                    return Convert.ToDecimal(val);
            }
            set { this["QTYENTRY"] = value; }
        }

        [DataMember]
        [Column("QTYCOUNT")]
        public decimal? QtyCount
        {
            get
            {
                object val = this["QTYCOUNT"];
                if (val == null)
                    return null;
                else
                    return Convert.ToDecimal(val);
            }
            set { this["QTYCOUNT"] = value; }
        }

        [DataMember]
        [Column("COUNTDATE")]
        public DateTime? CountedDate
        {
            get { return (DateTime?)this["COUNTDATE"]; }
            set { this["COUNTDATE"] = (object)value; }
        }

        [DataMember]
        [Column("COMMENT")]
        public string Comment
        {
            get { return (string)this["COMMENT"]; }
            set { this["COMMENT"] = value; }
        }
    }
}
