using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using SystemDataAnnotations = System.ComponentModel.DataAnnotations;

using Microsoft.Dynamics.Commerce.Runtime.DataModel;
using Microsoft.Dynamics.Commerce.Runtime.ComponentModel.DataAnnotations;

namespace RR.Commerce.Runtime.Mobility
{
    public enum SCJournalEntryStatus
    {
        Open,
        Counted,
        Missing,
        Skipped
    }

    public class SCJournalEntry : CommerceEntity
    {
        private const string IdColumn = "ID";

        public SCJournalEntry() : base("SCJournalEntry")
        {
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
        [Column("UNIT")]
        public string Unit
        {
            get { return (string)this["UNIT"]; }
            set { this["UNIT"] = value; }
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

        [DataMember]
        [Column("DEVICEID")]
        public string DeviceId
        {
            get { return (string)this["DEVICEID"]; }
            set { this["DEVICEID"] = value; }
        }

        [DataMember]
        [Column("SECTIONID")]
        public string SectionId
        {
            get { return (string)this["SECTIONID"]; }
            set { this["SECTIONID"] = value; }
        }

        [DataMember]
        [Column("SHELFID")]
        public string ShelfId
        {
            get { return (string)this["SHELFID"]; }
            set { this["SHELFID"] = value; }
        }

        [DataMember]
        [Column("QTYCOUNT")]
        public decimal QtyCount
        {
            get { return Convert.ToDecimal(this["QTYCOUNT"] ?? Decimal.Zero); }
            set { this["QTYCOUNT"] = value; }
        }

        [DataMember]
        [Column("COUNTDATE")]
        public DateTime CountedDate
        {
            get { return (DateTime)(this["COUNTDATE"] ?? DateTime.MinValue); }
            set { this["COUNTDATE"] = (object)value; }
        }

        [IgnoreDataMember]
        [Column("STATUS")]
        public SCJournalEntryStatus Status
        {
            get { return (SCJournalEntryStatus)(this["STATUS"] ?? SCJournalEntryStatus.Open); }
            set { this["STATUS"] = value; }
        }

        [DataMember]
        public int StatusValue
        {
            get { return (int)this.Status; }
            set { this.Status = (SCJournalEntryStatus)value; }
        }

        [DataMember]
        [Column("TRANSID")]
        public int TransactionId
        {
            get { return (int)this["TRANSID"]; }
            set { this["TRANSID"] = value; }
        }
    }
}
