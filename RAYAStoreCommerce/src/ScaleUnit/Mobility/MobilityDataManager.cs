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
{
    public sealed class MobilityDataManager //: DataManager
    {
        readonly RequestContext Context;
        public MobilityDataManager(RequestContext context) //: base(context)
        {
            //this.DataStoreManagerInstance.RegisterDataStoreAccessor(DataStoreType.Database, new Func<IDataStore, RequestContext, DataStoreAccessor>(MobilityDatabaseAccessor.Instantiate), context);
            //this.DataStoreManagerInstance.RegisterDataStoreAccessor(DataStoreType.L2Cache, new Func<IDataStore, RequestContext, DataStoreAccessor>(MobilityL2CacheAccessor.Instantiate), context);
            Context = context;
        }

        public static MobilityDataManager Instantiate(RequestContext context)
        {
            return new MobilityDataManager(context);
        }

        public async Task<ProductSaleReason> GetProductSaleReason(long recordId, string reasonCode)
        {
            if (recordId == 0 && String.IsNullOrEmpty(reasonCode))
                return null;

            PagedResult<ProductSaleReason> productSaleReasons = await GetProductSaleReasons().ConfigureAwait(false);
            if (productSaleReasons != null)
            {
                if (recordId == 0)
                    return productSaleReasons.SingleOrDefault(l => l.Code == reasonCode);
                else
                    return productSaleReasons.SingleOrDefault(l => l.Id == recordId);
            }
            else
                return null;
        }

        public async Task<PagedResult<ProductSaleReason>> GetProductSaleReasons()
        {
            //MobilityL2CacheAccessor cacheAccessor = (MobilityL2CacheAccessor)this.DataStoreManagerInstance.RegisteredAccessors[DataStoreType.L2Cache];
            //bool mustUpdateOnMiss = DataStoreManager.DataStores[DataStoreType.L2Cache].Policy.MustUpdateOnMiss;

            //PagedResult<ProductSaleReason> productSaleReasons = cacheAccessor.GetProductSaleReasons();
            //bool flag = mustUpdateOnMiss & productSaleReasons == null;
            //if (productSaleReasons == null)
            //{
            //    MobilityDatabaseAccessor databaseAccessor = (MobilityDatabaseAccessor)this.DataStoreManagerInstance.RegisteredAccessors[DataStoreType.Database];
            //    productSaleReasons = databaseAccessor.GetProductSaleReasons();
            //    flag &= productSaleReasons != null;
            //}
            //if (flag)
            //    cacheAccessor.PutProductSaleReasons(productSaleReasons);

            MobilityDatabaseAccessor databaseAccessor = new MobilityDatabaseAccessor(Context);
            PagedResult<ProductSaleReason> productSaleReasons = await databaseAccessor.GetProductSaleReasons().ConfigureAwait(false);

            return productSaleReasons;
        }
    }
}
