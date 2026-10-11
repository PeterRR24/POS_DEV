using System;
using System.Threading.Tasks;
using Microsoft.Dynamics.Commerce.Runtime;
using Microsoft.Dynamics.Commerce.Runtime.Data;

namespace RR.Commerce.Runtime.Utilities
{
    internal sealed class LicenseDataManager //: DataManager
    {
        //LicenseL2CacheAccessor cacheAccessor;
        LicenseDatabaseAccessor databaseAccessor;

        //private LicenseDataManager(RequestContext context) //: base(context)
        //{
        //    this.DataStoreManagerInstance.RegisterDataStoreAccessor(DataStoreType.Database, new Func<IDataStore, RequestContext, DataStoreAccessor>(LicenseDatabaseAccessor.Instantiate), context);
        //    this.DataStoreManagerInstance.RegisterDataStoreAccessor(DataStoreType.L2Cache, new Func<IDataStore, RequestContext, DataStoreAccessor>(LicenseL2CacheAccessor.Instantiate), context);

        //    cacheAccessor = (LicenseL2CacheAccessor)this.DataStoreManagerInstance.RegisteredAccessors[DataStoreType.L2Cache];
        //    databaseAccessor = (LicenseDatabaseAccessor)this.DataStoreManagerInstance.RegisteredAccessors[DataStoreType.Database];
        //}
        public LicenseDataManager(RequestContext context)
        {
            databaseAccessor = new LicenseDatabaseAccessor(context);
        }
        //public static LicenseDataManager Instantiate(RequestContext context)
        //{
        //    return new LicenseDataManager(context);
        //}

        //public StoreProductLicense GetCacheLicense(int productType)
        //{
        //    return cacheAccessor.GetProductLicense(productType);
        //}

        //public void PutCacheLicense(int productType, StoreProductLicense license)
        //{
        //    cacheAccessor.PutProductLicense(productType, license);
        //}

        public async Task<string> GetProductSerialNumber(int productType)
        {
            return await databaseAccessor.GetProductSerialNumber(productType).ConfigureAwait(false);
        }

        public async Task<int> GetMobilityLicensedDeviceCount()
        {
            return await databaseAccessor.GetMobilityLicensedDeviceCount().ConfigureAwait(false);
        }

        public async Task<string> GetSystemTenantId()
        {
            return await databaseAccessor.GetSystemTenantId().ConfigureAwait(false);
        }
    }
}
