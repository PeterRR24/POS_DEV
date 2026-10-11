using System;

using Microsoft.Dynamics.Commerce.Runtime;
using Microsoft.Dynamics.Commerce.Runtime.Data;

namespace RR.Commerce.Runtime.Utilities
{/*
    internal sealed class LicenseL2CacheAccessor : DataCacheAccessor
    {
        private static readonly TimeSpan ValidityInterval = TimeSpan.FromMinutes(60.0);

        private LicenseL2CacheAccessor(IDataStore backingStore, RequestContext context) : base(backingStore, context)
        {
        }

        internal static LicenseL2CacheAccessor Instantiate(IDataStore dataStore, RequestContext context)
        {
            return new LicenseL2CacheAccessor(dataStore, context);
        }

        protected override string GetKeyPattern()
        {
            return String.Format("{0}\\", "License");
        }

        public StoreProductLicense GetProductLicense(int productType)
        {
            string key = this.GenerateKey<int>("ProductLicense", productType);
            StoreProductLicense result = null;
            this.TryGetItem<StoreProductLicense>(key, out result);
            return result;
        }

        public void PutProductLicense(int productType, StoreProductLicense result)
        {
            ThrowIf.Null<StoreProductLicense>(result, "result");
            string key = this.GenerateKey<int>("ProductLicense", productType);
            this.PutItem<StoreProductLicense>(key, result, DateTimeOffset.Now.Add(ValidityInterval));
        }
    }*/
}
