using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

using Microsoft.Dynamics.Commerce.Runtime;
using Microsoft.Dynamics.Commerce.Runtime.Messages;
using Microsoft.Dynamics.Commerce.Runtime.DataModel;

namespace RR.Commerce.Runtime.Printing
{
    #region LabelItemCalculate
    [DataContract]
    public sealed class LabelItemCalculateRequest : Request
    {
        public LabelItemCalculateRequest(LabelLayoutType labelType)
        {
            this.LabelType = labelType;
        }

        [DataMember]
        public LabelLayoutType LabelType { get; set; }
        [DataMember]
        public LabelCalculateParams Parameters { get; set; }
        [DataMember]
        public LabelItemCalculateCriteria Criteria { get; set; }
        [DataMember]
        public IList<LabelItemCalculateItem> Items { get; set; }

        [DataMember]
        public bool Personal { get; set; }

        [DataMember]
        public int PageNumber { get; set; }
    }

    [DataContract]
    public class LabelItemCalculateResponse : Response
    {
        public LabelItemCalculateResponse(int count, bool hasNext)
        {
            this.Count = count;
            this.HasNext = hasNext;
        }

        [DataMember]
        public int Count { get; }

        [DataMember]
        public bool HasNext { get; }
    }
    #endregion

    #region LabelItemFormat
    [DataContract]
    public sealed class LabelItemFormatRequest : Request
    {
        public LabelItemFormatRequest(LabelItemStatistic stat, bool mobility)
        {
            this.Stat = stat;
            this.Mobility = mobility;
        }

        //public LabelItemFormatRequest(IEnumerable<long> labelItemIds,  LabelItemStatistic stat, bool mobility)
        //{
        //    this.LabelItemIds = labelItemIds;
        //    this.Stat = stat;
        //    this.Mobility = mobility;
        //}

        //public LabelItemFormatRequest(LabelItemStatistic stat, LabelItem item, LabelItemSale itemSale, bool direct) : this(stat, true)
        //{
        //    this.Item = item;
        //    this.ItemSale = itemSale;
        //    this.Direct = direct;
        //}

        public LabelItemFormatRequest(LabelItemStatistic stat, LabelItem item, LabelItemSale itemSale, bool direct, bool isMobility) : this(stat, isMobility)
        {
            this.Item = item;
            this.ItemSale = itemSale;
            this.Direct = direct;
        }

        [DataMember]
        public LabelItemStatistic Stat { get; set; }
        [DataMember]
        public LabelItem Item { get; set; }
        //[DataMember]
        //public IEnumerable<long> LabelItemIds { get; set; }
        [DataMember]
        public LabelItemSale ItemSale { get; set; }
        [DataMember]
        public bool Direct { get; set; }
        [DataMember]
        public bool Mobility { get; set; }
    }

    [DataContract]
    public class LabelItemFormatResponse : Response
    {
        public LabelItemFormatResponse()
        {
        }

        public LabelItemFormatResponse(PrinterJob job)
        {
            this.Job = job;
        }

        public LabelItemFormatResponse(LabelPrinterTask task)
        {
            this.Task = task;
        }

        [DataMember]
        public PrinterJob Job { get; }
        [DataMember]
        public LabelPrinterTask Task { get; }
    }
    #endregion

    #region LabelItemCommit
    [DataContract]
    public sealed class LabelItemCommitRequest : Request
    {
        public LabelItemCommitRequest(LabelItemStatistic item)
        {
            this.Item = item;
        }

        [DataMember]
        public LabelItemStatistic Item { get; set; }
    }
    #endregion
}
