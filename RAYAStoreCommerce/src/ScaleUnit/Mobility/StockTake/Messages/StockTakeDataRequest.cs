using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

using Microsoft.Dynamics.Commerce.Runtime;
using Microsoft.Dynamics.Commerce.Runtime.Messages;
using Microsoft.Dynamics.Commerce.Runtime.DataModel;

namespace RR.Commerce.Runtime.Mobility
{
    [DataContract]
    public sealed class CalculateStockTakeItemDataRequest : Request
    {
        public CalculateStockTakeItemDataRequest(StockTakeItem item)
        {
            ThrowIf.Null(item, "item");
            this.Item = item;
        }

        [DataMember]
        public StockTakeItem Item { get; }
    }

    [DataContract]
    public sealed class CheckStockTakeItemDataRequest : Request
    {
        public CheckStockTakeItemDataRequest(StockTakeItem item)
        {
            ThrowIf.Null(item, "item");
            this.Item = item;
        }

        [DataMember]
        public StockTakeItem Item { get; }
    }

    [DataContract]
    public sealed class CalculateStockTakeTrackingDataRequest : Request
    {
        public CalculateStockTakeTrackingDataRequest(StockTakeItem item)
        {
            ThrowIf.Null(item, "item");
            this.Item = item;
        }

        [DataMember]
        public StockTakeItem Item { get; }
    }

    [DataContract]
    public sealed class CalculateStockTakeDeviceDataRequest : Request
    {
        public CalculateStockTakeDeviceDataRequest(StockTakeItem item)
        {
            ThrowIf.Null(item, "item");
            this.Item = item;
        }

        [DataMember]
        public StockTakeItem Item { get; }
    }

    [DataContract]
    public sealed class UpdateStockTakeItemDataRequest : Request
    {
        public UpdateStockTakeItemDataRequest(int id, decimal quantity)
        {
            this.Id = id;
            this.Quantity = quantity;
        }

        [DataMember]
        public int Id { get; }
        [DataMember]
        public decimal Quantity { get; }
    }

    [DataContract]
    public sealed class CreateStockTakeItemDataRequest : Request
    {
        public CreateStockTakeItemDataRequest(StockTakeItem item, decimal quantity)
        {
            ThrowIf.Null(item, "item");
            this.Item = item;
            this.Quantity = quantity;
        }

        [DataMember]
        public StockTakeItem Item { get; }
        [DataMember]
        public decimal Quantity { get; }
    }

}
