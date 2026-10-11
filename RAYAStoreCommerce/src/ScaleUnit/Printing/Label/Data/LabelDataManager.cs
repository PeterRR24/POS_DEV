using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Dynamics.Commerce.Runtime;
using Microsoft.Dynamics.Commerce.Runtime.Data;
using Microsoft.Dynamics.Commerce.Runtime.DataModel;

namespace RR.Commerce.Runtime.Printing
{
    public sealed class LabelDataManager //: DataManager
    {
        //public LabelDataManager(RequestContext context) : base(context)
        //{
        //    this.DataStoreManagerInstance.RegisterDataStoreAccessor(DataStoreType.Database, new Func<IDataStore, RequestContext, DataStoreAccessor>(LabelDatabaseAccessor.Instantiate), context);
        //    this.DataStoreManagerInstance.RegisterDataStoreAccessor(DataStoreType.L2Cache, new Func<IDataStore, RequestContext, DataStoreAccessor>(LabelL2CacheAccessor.Instantiate), context);
        //}
        readonly RequestContext Context;
        public LabelDataManager(RequestContext context) { Context = context; }
        public static LabelDataManager Instantiate(RequestContext context)
        {
            return new LabelDataManager(context);
        }        

        public async Task<PagedResult<LabelCode>> GetLabelCodes()
        {
            //LabelL2CacheAccessor cacheAccessor = (LabelL2CacheAccessor)this.DataStoreManagerInstance.RegisteredAccessors[DataStoreType.L2Cache];
            //bool mustUpdateOnMiss = DataStoreManager.DataStores[DataStoreType.L2Cache].Policy.MustUpdateOnMiss;

            //PagedResult<LabelCode> result = cacheAccessor.GetLabelCodes();
            //bool flag = mustUpdateOnMiss & result == null;
            //if (result == null)
            //{
            //    LabelDatabaseAccessor databaseAccessor = (LabelDatabaseAccessor)this.DataStoreManagerInstance.RegisteredAccessors[DataStoreType.Database];
            //    result = databaseAccessor.GetLabelCodes();
            //    flag &= result != null;
            //}
            //if (flag)
            //    cacheAccessor.PutLabelCodes(result);

            //return result;
            LabelDatabaseAccessor databaseAccessor = new LabelDatabaseAccessor(Context);
            return await databaseAccessor.GetLabelCodes().ConfigureAwait(false);
        }


        public async Task<long> GetLabelProfileForItem(long productRecId, long hierarchyRecId)
        {
            //LabelDatabaseAccessor databaseAccessor = (LabelDatabaseAccessor)this.DataStoreManagerInstance.RegisteredAccessors[DataStoreType.Database];
            LabelDatabaseAccessor databaseAccessor = new LabelDatabaseAccessor(Context);
            return await databaseAccessor.GetLabelProfileForItem(productRecId, hierarchyRecId).ConfigureAwait(false);
        }

        public async Task<LabelProfile> GetLabelProfile(long profileId)
        {
            if (profileId == 0)
                return null;

            //LabelL2CacheAccessor cacheAccessor = (LabelL2CacheAccessor)this.DataStoreManagerInstance.RegisteredAccessors[DataStoreType.L2Cache];
            //bool mustUpdateOnMiss = DataStoreManager.DataStores[DataStoreType.L2Cache].Policy.MustUpdateOnMiss;

            //ReadOnlyCollection<LabelProfile> labelProfiles = cacheAccessor.GetLabelProfiles();
            //bool flag = mustUpdateOnMiss & labelProfiles == null;
            //if (labelProfiles == null)
            //{
            //    LabelDatabaseAccessor databaseAccessor = (LabelDatabaseAccessor)this.DataStoreManagerInstance.RegisteredAccessors[DataStoreType.Database];
            //    labelProfiles = databaseAccessor.GetLabelProfiles();
            //    flag &= labelProfiles != null;
            //}
            //if (flag)
            //    cacheAccessor.PutLabelProfiles(labelProfiles);

            LabelDatabaseAccessor databaseAccessor = new LabelDatabaseAccessor(Context);
            ReadOnlyCollection<LabelProfile> labelProfiles = await databaseAccessor.GetLabelProfiles().ConfigureAwait(false);

            if (labelProfiles != null)
                return labelProfiles.SingleOrDefault(l => l.Id == profileId);
            else
                return null;
        }

        public async Task<LabelProfileLine> GetLabelProfileLine(long profileId, LabelLayoutType layoutType, long codeRecId)
        {
            if (profileId == 0)
                return null;

            //LabelL2CacheAccessor cacheAccessor = (LabelL2CacheAccessor)this.DataStoreManagerInstance.RegisteredAccessors[DataStoreType.L2Cache];
            //bool mustUpdateOnMiss = DataStoreManager.DataStores[DataStoreType.L2Cache].Policy.MustUpdateOnMiss;

            //ReadOnlyCollection<LabelProfileLine> labelProfileLines = cacheAccessor.GetLabelProfileLines();
            //bool flag = mustUpdateOnMiss & labelProfileLines == null;
            //if (labelProfileLines == null)
            //{
            //    LabelDatabaseAccessor databaseAccessor = (LabelDatabaseAccessor)this.DataStoreManagerInstance.RegisteredAccessors[DataStoreType.Database];
            //    labelProfileLines = databaseAccessor.GetLabelProfileLines();
            //    flag &= labelProfileLines != null;
            //}
            //if (flag)
            //    cacheAccessor.PutLabelProfileLines(labelProfileLines);

            LabelDatabaseAccessor databaseAccessor = new LabelDatabaseAccessor(Context);
            ReadOnlyCollection<LabelProfileLine> labelProfileLines = await databaseAccessor.GetLabelProfileLines().ConfigureAwait(false);

            if (labelProfileLines != null)
                return labelProfileLines.SingleOrDefault(l => l.ProfileId == profileId && l.LabelType == layoutType && l.CodeRecId == codeRecId);
            else
                return null;
        }

        public async Task<LabelFormat> GetLabelFormat(long formatId)
        {
            if (formatId == 0)
                return null;

            //LabelL2CacheAccessor cacheAccessor = (LabelL2CacheAccessor)this.DataStoreManagerInstance.RegisteredAccessors[DataStoreType.L2Cache];
            //bool mustUpdateOnMiss = DataStoreManager.DataStores[DataStoreType.L2Cache].Policy.MustUpdateOnMiss;

            //ReadOnlyCollection<LabelFormat> labelFormats = cacheAccessor.GetLabelFormats();
            //bool flag = mustUpdateOnMiss & labelFormats == null;
            //if (labelFormats == null)
            //{
            //    LabelDatabaseAccessor databaseAccessor = (LabelDatabaseAccessor)this.DataStoreManagerInstance.RegisteredAccessors[DataStoreType.Database];
            //    labelFormats = databaseAccessor.GetLabelFormats();
            //    flag &= labelFormats != null;
            //}
            //if (flag)
            //    cacheAccessor.PutLabelFormats(labelFormats);

            LabelDatabaseAccessor databaseAccessor = new LabelDatabaseAccessor(Context);
            ReadOnlyCollection<LabelFormat> labelFormats = await databaseAccessor.GetLabelFormats().ConfigureAwait(false);

            if (labelFormats != null)
                return labelFormats.SingleOrDefault(l => l.Id == formatId);
            else
                return null;
        }

        public async Task<LabelLayout> GetLabelLayout(long layoutId)
        {
            if (layoutId == 0)
                return null;

            //LabelL2CacheAccessor cacheAccessor = (LabelL2CacheAccessor)this.DataStoreManagerInstance.RegisteredAccessors[DataStoreType.L2Cache];
            //bool mustUpdateOnMiss = DataStoreManager.DataStores[DataStoreType.L2Cache].Policy.MustUpdateOnMiss;

            //ReadOnlyCollection<LabelLayout> labelLayouts = cacheAccessor.GetLabelLayouts();
            //bool flag = mustUpdateOnMiss & labelLayouts == null;
            //if (labelLayouts == null)
            //{
            //    LabelDatabaseAccessor databaseAccessor = (LabelDatabaseAccessor)this.DataStoreManagerInstance.RegisteredAccessors[DataStoreType.Database];
            //    labelLayouts = databaseAccessor.GetLabelLayouts();
            //    flag &= labelLayouts != null;
            //}
            //if (flag)
            //    cacheAccessor.PutLabelLayouts(labelLayouts);

            LabelDatabaseAccessor databaseAccessor = new LabelDatabaseAccessor(Context);
            ReadOnlyCollection<LabelLayout> labelLayouts = await databaseAccessor.GetLabelLayouts().ConfigureAwait(false);

            if (labelLayouts != null)
                return labelLayouts.SingleOrDefault(l => l.Id == layoutId);
            else
                return null;
        }
    }
}
