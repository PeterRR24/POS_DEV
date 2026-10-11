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
    /*  >> Obsolete class <<
    public sealed class RangingDataManager : DataManager
    {
        private string _deviceId;

        public RangingDataManager(RequestContext context) : base(context)
        {
            this.DataStoreManagerInstance.RegisterDataStoreAccessor(DataStoreType.L2Cache, new Func<IDataStore, RequestContext, DataStoreAccessor>(MobilityL2CacheAccessor.Instantiate), context);

            _deviceId = context.GetPrincipal().DeviceNumber;
        }

        public static RangingDataManager Instantiate(RequestContext context)
        {
            return new RangingDataManager(context);
        }

        //

        private void SaveList(RangingList list)
        {
            GetCacheAccessor().PutRangingList(_deviceId, list);
        }

        private RangingList TryGetList()
        {
            return GetCacheAccessor().GetRangingList(_deviceId);
        }

        public RangingList GetList()
        {
            RangingList list = TryGetList();
            if (list == null)
                throw new ValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidRequest, ResText.Text("VAL_RANGING_START"));
            return list;
        }

        public void CreateList(string sectionId, string shelfId)
        {
            GetCacheAccessor().PutRangingList(_deviceId, new RangingList(sectionId, shelfId));
        }

        public void ResetList()
        {
            GetCacheAccessor().ClearRangingList(_deviceId);
        }

        public RangingStat GetStat()
        {
            RangingList list = TryGetList();
            if (list == null)
                return null;
            else
                return list.GetStat();
        }

        public int Count()
        {
            RangingList list = TryGetList();
            if (list == null)
                return 0;
            else
                return list.Count;
        }

        public RangingItem Get(string itemId)
        {
            return GetList().Get(itemId);
        }

        public void Add(RangingItem item)
        {
            RangingList list = GetList();
            list.Add(item);
            SaveList(list);
        }

        public void Delete(int key)
        {
            RangingList list = GetList();
            list.Delete(key);
            SaveList(list);
        }

        public void DeleteLast()
        {
            RangingList list = GetList();
            list.DeleteLast();
            SaveList(list);
        }

        private MobilityL2CacheAccessor GetCacheAccessor()
        {
            return (MobilityL2CacheAccessor)this.DataStoreManagerInstance.RegisteredAccessors[DataStoreType.L2Cache];
        }
    }
    */
}
