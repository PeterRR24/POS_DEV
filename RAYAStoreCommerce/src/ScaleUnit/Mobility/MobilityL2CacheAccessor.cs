using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Dynamics.Commerce.Runtime;
using Microsoft.Dynamics.Commerce.Runtime.Data;
using Microsoft.Dynamics.Commerce.Runtime.DataModel;

namespace RR.Commerce.Runtime.Mobility
{/*
    public sealed class MobilityL2CacheAccessor : DataCacheAccessor
    {
        private MobilityL2CacheAccessor(IDataStore backingStore, RequestContext context) : base(backingStore, context)
        {
        }

        internal static MobilityL2CacheAccessor Instantiate(IDataStore dataStore, RequestContext context)
        {
            return new MobilityL2CacheAccessor(dataStore, context);
        }

        protected override string GetKeyPattern()
        {
            return String.Format("{0}\\{1}\\", this.ChannelId, "Mobility");
        }

        public PagedResult<ProductSaleReason> GetProductSaleReasons()
        {
            string key = this.GenerateKey("ProductSaleReasons");
            PagedResult<ProductSaleReason> result = null;
            this.TryGetItem<PagedResult<ProductSaleReason>>(key, out result);
            return result;
        }

        public void PutProductSaleReasons(PagedResult<ProductSaleReason> result)
        {
            ThrowIf.Null<PagedResult<ProductSaleReason>>(result, "result");
            string key = this.GenerateKey("ProductSaleReasons");
            DateTimeOffset absoluteExpiration = DateTimeOffset.Now + TimeSpan.FromMinutes(5.0);
            this.PutItem<PagedResult<ProductSaleReason>>(key, result, absoluteExpiration);
        }

        // Ranging

        public RangingList GetRangingList(string deviceId)
        {
            string key = this.GenerateKey<string>("RangingList", deviceId);
            RangingList result = null;
            this.TryGetItem<RangingList>(key, out result);
            return result;
        }

        public void PutRangingList(string deviceId, RangingList result)
        {
            ThrowIf.Null<RangingList>(result, "result");
            string key = this.GenerateKey<string>("RangingList", deviceId);
            this.EvictItem(key);
            DateTimeOffset absoluteExpiration = DateTimeOffset.Now + TimeSpan.FromMinutes(30);
            this.PutItem<RangingList>(key, result, absoluteExpiration);
        }

        public void ClearRangingList(string deviceId)
        {
            string key = this.GenerateKey<string>("RangingList", deviceId);
            this.EvictItem(key);
        }

    }*/
}
