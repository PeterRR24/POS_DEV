using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RR.Commerce.Runtime.Utilities
{
    // Offline StockTake
    public class BarcodeItem
    {
        public long RecordId { get; set; }
        public string ItemId { get; set; }
        public string InventDimId { get; set; }
        public string UnitId { get; set; }
        public string Barcode { get; set; }

    }
}
