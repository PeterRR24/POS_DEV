using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Dynamics.Commerce.Runtime;
using Microsoft.Dynamics.Commerce.Runtime.Data;
using Microsoft.Dynamics.Commerce.Runtime.DataModel;

namespace RR.Commerce.Runtime.Utilities
{/*
    public sealed class UtilitiesL2CacheAccessor : DataCacheAccessor
    {
        private UtilitiesL2CacheAccessor(IDataStore backingStore, RequestContext context) : base(backingStore, context)
        {
        }

        internal static UtilitiesL2CacheAccessor Instantiate(IDataStore dataStore, RequestContext context)
        {
            return new UtilitiesL2CacheAccessor(dataStore, context);
        }

        protected override string GetKeyPattern()
        {
            return String.Format("{0}\\{1}\\", this.ChannelId, "Utilities");
        }

        public UtilitiesConfiguration GetUtilitiesConfiguration()
        {
            string key = this.GenerateKey("Configuration");
            UtilitiesConfiguration utilitiesConfiguration = null;
            this.TryGetItem<UtilitiesConfiguration>(key, out utilitiesConfiguration);
            return utilitiesConfiguration;
        }

        public void PutUtilitiesConfiguration(UtilitiesConfiguration result)
        {
            ThrowIf.Null<UtilitiesConfiguration>(result, "result");
            string key = this.GenerateKey("Configuration");
            DateTimeOffset absoluteExpiration = DateTimeOffset.Now + TimeSpan.FromMinutes(30.0);
            this.PutItem<UtilitiesConfiguration>(key, result, absoluteExpiration);
        }

        public PagedResult<StoreSection> GetStoreSections()
        {
            string key = this.GenerateKey("StoreSections");
            PagedResult<StoreSection> result = null;
            this.TryGetItem<PagedResult<StoreSection>>(key, out result);
            return result;
        }

        public void PutStoreSections(PagedResult<StoreSection> result)
        {
            ThrowIf.Null<PagedResult<StoreSection>>(result, "result");
            string key = this.GenerateKey("StoreSections");
            DateTimeOffset absoluteExpiration = DateTimeOffset.Now + TimeSpan.FromMinutes(5.0);
            this.PutItem<PagedResult<StoreSection>>(key, result, absoluteExpiration);
        }

        public PagedResult<StoreShelf> GetStoreShelves()
        {
            string key = this.GenerateKey("StoreShelves");
            PagedResult<StoreShelf> result = null;
            this.TryGetItem<PagedResult<StoreShelf>>(key, out result);
            return result;
        }

        public void PutStoreShelves(PagedResult<StoreShelf> result)
        {
            ThrowIf.Null<PagedResult<StoreShelf>>(result, "result");
            string key = this.GenerateKey("StoreShelves");
            DateTimeOffset absoluteExpiration = DateTimeOffset.Now + TimeSpan.FromMinutes(5.0);
            this.PutItem<PagedResult<StoreShelf>>(key, result, absoluteExpiration);
        }

        public PagedResult<Category> GetCategoryHierarchy(long hierarchyId)
        {
            string key = this.GenerateKey<long>("CategoryHierarchy", hierarchyId);
            PagedResult<Category> pagedResult = (PagedResult<Category>)null;
            this.TryGetItem<PagedResult<Category>>(key, out pagedResult);
            return pagedResult;
        }

        public void PutCategoryHierarchy(long hierarchyId, PagedResult<Category> result)
        {
            ThrowIf.Null<PagedResult<Category>>(result, "result");
            string key = this.GenerateKey<long>("CategoryHierarchy", hierarchyId);
            DateTimeOffset absoluteExpiration = DateTimeOffset.Now + TimeSpan.FromMinutes(5.0);
            this.PutItem<PagedResult<Category>>(key, result, absoluteExpiration);
        }

        public int GetStoreLocationType(string sectionId, string shelfId)
        {
            string key = this.GenerateKey("StoreLocationType");
            int result = 0;
            this.TryGetItem<int>(key, out result);
            return result;
        }

        public void PutStoreLocationType(string sectionId, string shelfId, bool result)
        {
            string key = this.GenerateKey("StoreLocationType");
            DateTimeOffset absoluteExpiration = DateTimeOffset.Now + TimeSpan.FromMinutes(30.0);
            this.PutItem<int>(key, result ? 2 : 1, absoluteExpiration);
        }
    }*/
}
