using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using SystemDataAnnotations = System.ComponentModel.DataAnnotations;

using Microsoft.Dynamics.Commerce.Runtime;
using Microsoft.Dynamics.Commerce.Runtime.DataModel;
using Microsoft.Dynamics.Commerce.Runtime.ComponentModel.DataAnnotations;

namespace RR.Commerce.Runtime.Mobility
{
    public enum SCJournalStatus
    {
        Open,
        Committed,
    }

    public enum SCJournalCalculation
    {
        Default,
        EmptyAsDiscrepant,
        NegativeAsDiscrepant,
    }

    public enum SCJournalLocation
    {
        Store,
        Section,
        Shelf,
    }

    [DefaultSortOrder(ColumnName = "JOURNALID", IsDescending = true)]
    public sealed class SCJournal : CommerceEntity
    {
        private const string IdColumn = "ID";

        public SCJournal() : base("SCJournal")
        {
        }

        public SCJournal(SCJournalRequest request, string storeId, string inventLocationId) : this()
        {
            RecordId = Convert.ToInt64(request.RecordId);
            JournalId = request.JournalId;
            Description = request.Description;
            CalcMode = (SCJournalCalculation)request.CalcMode;
            LocationMode = (SCJournalLocation)request.LocationMode;
            Status = SCJournalStatus.Open;
            CreatedDate = DateTime.Now;
            StoreId = storeId;
            InventLocationId = inventLocationId;
        }

        public StockCountJournal ToModel(string dataAreaId, string worker)
        {
            StockCountJournal journal = new StockCountJournal();
            journal.RecordId = RecordId.ToString();
            journal.JournalId = JournalId;
            journal.Description = Description;
            journal.InventLocationId = InventLocationId;
            journal.DataAreaId = dataAreaId;
            journal.Worker = worker;
            return journal;
        }

        public StockTakeJournal ToStockTake()
        {
            StockTakeJournal journal = new StockTakeJournal();
            journal.Id = this.Id;
            journal.JournalId = this.JournalId;
            journal.Description = this.Description;
            journal.CalcMode = this.CalcMode;
            journal.LocationMode = this.LocationMode;
            journal.CreatedDate = this.CreatedDate;
            return journal;
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
        public string JournalId
        {
            get { return (string)this["JOURNALID"]; }
            set { this["JOURNALID"] = value; }
        }

        [DataMember]
        [Column("DESCRIPTION")]
        public string Description
        {
            get { return (string)this["DESCRIPTION"]; }
            set { this["DESCRIPTION"] = value; }
        }

        [DataMember]
        [Column("STOREID")]
        public string StoreId
        {
            get { return (string)this["STOREID"]; }
            set { this["STOREID"] = value; }
        }

        [DataMember]
        [Column("INVENTLOCID")]
        public string InventLocationId
        {
            get { return (string)this["INVENTLOCID"]; }
            set { this["INVENTLOCID"] = value; }
        }

        [DataMember]
        [Column("DIMSETUP")]
        public int DimSetup
        {
            get { return (int)(this["DIMSETUP"] ?? 0); }
            set { this["DIMSETUP"] = value; }
        }

        [IgnoreDataMember]
        [Column("CALCMODE")]
        public SCJournalCalculation CalcMode
        {
            get { return (SCJournalCalculation)(this["CALCMODE"] ?? SCJournalCalculation.Default); }
            set { this["CALCMODE"] = value; }
        }

        [DataMember]
        public int CalcModeValue
        {
            get { return (int)this.CalcMode; }
            set { this.CalcMode = (SCJournalCalculation)value; }
        }

        [IgnoreDataMember]
        [Column("LOCMODE")]
        public SCJournalLocation LocationMode
        {
            get { return (SCJournalLocation)(this["LOCMODE"] ?? SCJournalLocation.Store); }
            set { this["LOCMODE"] = value; }
        }

        [DataMember]
        public int LocationModeValue
        {
            get { return (int)this.LocationMode; }
            set { this.LocationMode = (SCJournalLocation)value; }
        }

        [IgnoreDataMember]
        [Column("STATUS")]
        public SCJournalStatus Status
        {
            get { return (SCJournalStatus)(this["STATUS"] ?? SCJournalStatus.Open); }
            set { this["STATUS"] = value; }
        }

        [DataMember]
        public int StatusValue
        {
            get { return (int)this.Status; }
            set { this.Status = (SCJournalStatus)value; }
        }

        [DataMember]
        [Column("CREATEDDATE")]
        public DateTime CreatedDate
        {
            get { return (DateTime)(this["CREATEDDATE"] ?? DateTime.MinValue); }
            set { this["CREATEDDATE"] = (object)value; }
        }

        [DataMember]
        [Column("ADJUSTDATE")]
        public DateTime? AdjustDate
        {
            get { return (DateTime?)this["ADJUSTDATE"]; }
            set { this["ADJUSTDATE"] = (object)value; }
        }

        [DataMember]
        [Column("COMMITDATE")]
        public DateTime? CommitDate
        {
            get { return (DateTime?)this["COMMITDATE"]; }
            set { this["COMMITDATE"] = (object)value; }
        }

        [DataMember]
        [Column("STAFFID")]
        public string StaffId
        {
            get { return (string)(this["STAFFID"] ?? String.Empty); }
            set { this["STAFFID"] = value; }
        }
    }
}
