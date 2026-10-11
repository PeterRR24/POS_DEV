using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

using Microsoft.Dynamics.Commerce.Runtime.ComponentModel.DataAnnotations;

namespace RR.Commerce.Runtime.Mobility
{
    public class StockCountJournalBuffer
    {
        public string ItemId { get; set; }
        public string InventSizeId { get; set; }
        public string InventColorId { get; set; }
        public string InventStyleId { get; set; }
        public string ConfigId { get; set; }
        public string InventBatchId { get; set; }
        public string InventSerialId { get; set; }

        //

        public decimal QtyCount { get; set; }
        public DateTime CountedDate { get; set; }

        //

        public SCJournalEntryStatus Status { get; set; }
        public int TransactionId { get; set; }

        //

        public string ItemName { get; set; }
        public string Unit { get; set; }
    }
}
