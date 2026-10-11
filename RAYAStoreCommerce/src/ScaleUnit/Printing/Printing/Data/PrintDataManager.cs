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
    public sealed class PrintDataManager //: DataManager
    {
        //public PrintDataManager(RequestContext context) : base(context)
        //{
        //    this.DataStoreManagerInstance.RegisterDataStoreAccessor(DataStoreType.Database, new Func<IDataStore, RequestContext, DataStoreAccessor>(PrintDatabaseAccessor.Instantiate), context);
        //    this.DataStoreManagerInstance.RegisterDataStoreAccessor(DataStoreType.L2Cache, new Func<IDataStore, RequestContext, DataStoreAccessor>(PrintL2CacheAccessor.Instantiate), context);
        //}
        readonly RequestContext Context;
        public PrintDataManager(RequestContext context) { Context = context; }
        public static PrintDataManager Instantiate(RequestContext context)
        {
            return new PrintDataManager(context);
        }

        public async Task<PRPrinter> GetPrinter(long printerId)
        {
            if (printerId == 0)
                return null;

            //PrintL2CacheAccessor cacheAccessor = (PrintL2CacheAccessor)this.DataStoreManagerInstance.RegisteredAccessors[DataStoreType.L2Cache];
            //bool mustUpdateOnMiss = DataStoreManager.DataStores[DataStoreType.L2Cache].Policy.MustUpdateOnMiss;

            //ReadOnlyCollection<PRPrinter> list = cacheAccessor.GetPrinters();
            //bool flag = mustUpdateOnMiss & list == null;
            //if (list == null)
            //{
            //    PrintDatabaseAccessor databaseAccessor = (PrintDatabaseAccessor)this.DataStoreManagerInstance.RegisteredAccessors[DataStoreType.Database];
            //    list = databaseAccessor.GetPrinters();
            //    flag &= list != null;
            //}
            //if (flag)
            //    cacheAccessor.PutPrinters(list);

            PrintDatabaseAccessor databaseAccessor = new PrintDatabaseAccessor(Context);
            ReadOnlyCollection<PRPrinter> list = await databaseAccessor.GetPrinters().ConfigureAwait(false);

            if (list != null)
                return list.SingleOrDefault(l => l.Id == printerId);
            else
                return null;
        }

        public async Task<PRTerminal> GetTerminal(string terminalId)
        {
            if (String.IsNullOrEmpty(terminalId))
                return null;

            //PrintL2CacheAccessor cacheAccessor = (PrintL2CacheAccessor)this.DataStoreManagerInstance.RegisteredAccessors[DataStoreType.L2Cache];
            //bool mustUpdateOnMiss = DataStoreManager.DataStores[DataStoreType.L2Cache].Policy.MustUpdateOnMiss;

            //ReadOnlyCollection<PRTerminal> list = cacheAccessor.GetTerminals();
            //bool flag = mustUpdateOnMiss & list == null;
            //if (list == null)
            //{
            //    PrintDatabaseAccessor databaseAccessor = (PrintDatabaseAccessor)this.DataStoreManagerInstance.RegisteredAccessors[DataStoreType.Database];
            //    list = databaseAccessor.GetTerminals();
            //    flag &= list != null;
            //}
            //if (flag)
            //    cacheAccessor.PutTerminals(list);

            PrintDatabaseAccessor databaseAccessor = new PrintDatabaseAccessor(Context);
            ReadOnlyCollection<PRTerminal> list = await databaseAccessor.GetTerminals().ConfigureAwait(false);

            if (list != null)
                return list.SingleOrDefault(l => l.TerminalId == terminalId);
            else
                return null;
        }

        public async Task<StorePrinter> GetStorePrinter(long storeId)
        {
            if (storeId == 0)
                return null;

            //PrintL2CacheAccessor cacheAccessor = (PrintL2CacheAccessor)this.DataStoreManagerInstance.RegisteredAccessors[DataStoreType.L2Cache];
            //bool mustUpdateOnMiss = DataStoreManager.DataStores[DataStoreType.L2Cache].Policy.MustUpdateOnMiss;

            //ReadOnlyCollection<StorePrinter> list = cacheAccessor.GetStorePrinters();
            //bool flag = mustUpdateOnMiss & list == null;
            //if (list == null)
            //{
            //    PrintDatabaseAccessor databaseAccessor = (PrintDatabaseAccessor)this.DataStoreManagerInstance.RegisteredAccessors[DataStoreType.Database];
            //    list = databaseAccessor.GetStorePrinters();
            //    flag &= list != null;
            //}
            //if (flag)
            //    cacheAccessor.PutStorePrinters(list);

            PrintDatabaseAccessor databaseAccessor = new PrintDatabaseAccessor(Context);
            ReadOnlyCollection<StorePrinter> list = await databaseAccessor.GetStorePrinters().ConfigureAwait(false);

            if (list != null)
                return list.SingleOrDefault(l => l.StoreRecId == storeId);
            else
                return null;
        }

    }
}
