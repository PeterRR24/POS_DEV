using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

using Microsoft.Dynamics.Commerce.Runtime.DataModel;

namespace RR.Commerce.Runtime.Printing
{
    public enum LabelItemSearchBy
    {
        CategoryHierarchy,
        LocationHierarchy,
        ItemFilter
    }

    public enum LabelItemQtySource
    {
        Available,
        OrderReceived,
        Fixed
    }

    public enum LabelItemChangeTracking
    {
        None,
        Price,
        PriceAndName,
        OldPrice
    }

    [DataContract]
    public sealed class LabelItemCalculateItem
    {
        public LabelItemCalculateItem()
        {
        }

        public LabelItemCalculateItem(string itemId)
        {
            this.ItemId = itemId;
        }
        public LabelItemCalculateItem(string itemId, string inventDimId)
        {
            this.ItemId = itemId;
            this.InventDimensionId = inventDimId;
        }
        public LabelItemCalculateItem(string itemId, long productId)
        {
            this.ItemId = itemId;
            this.ProductId = productId;
        }
        public LabelItemCalculateItem(string itemId, string inventDimId, int quantity)
        {
            this.ItemId = itemId;
            this.InventDimensionId = inventDimId;
            this.Quantity = quantity;
        }

        [DataMember]
        public string BarCode { get; set; }
        [DataMember]
        public string ItemId { get; set; }
        [DataMember]
        public string InventDimensionId { get; set; }
        [DataMember]
        public long ProductId { get; set; }
        [DataMember]
        public string UnitOfMeasure { get; set; }
        [DataMember]
        public int Quantity { get; set; }
    }

    [DataContract]
    public sealed class LabelItemCalculateCriteria
    {
        public LabelItemCalculateCriteria()
        {
        }

        public LabelItemCalculateCriteria(LabelItemSearchBy searchOption)
        {
            this.SearchOption = searchOption;
        }

        [DataMember]
        public LabelItemSearchBy SearchOption { get; set; }
        [DataMember]
        public long CategoryId { get; set; }
        [DataMember]
        public long HierarchyId { get; set; }
        [DataMember]
        public string SectionId { get; set; }
        [DataMember]
        public string ShelfId { get; set; }
        [DataMember]
        public string ItemFilter { get; set; }

        [DataMember]
        public LabelItemChangeTracking ChangeTracking { get; set; }

        [DataMember]
        public LabelItemQtySource QtySource { get; set; }
        [DataMember]
        public DateTime? FromDate { get; set; }
        [DataMember]
        public DateTime? ToDate { get; set; }
        [DataMember]
        public int Quantity { get; set; }
    }
}
