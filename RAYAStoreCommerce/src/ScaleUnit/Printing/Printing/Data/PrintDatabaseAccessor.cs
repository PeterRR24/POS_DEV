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
    public sealed class PrintDatabaseAccessor //: DatabaseAccessor
    {
        //private PrintDatabaseAccessor(IDataStore dataStore, RequestContext context) : base(dataStore, context)
        //{
        //}

        //internal static PrintDatabaseAccessor Instantiate(IDataStore dataStore, RequestContext context)
        //{
        //    return new PrintDatabaseAccessor(dataStore, context);
        //}

        //internal static PrintDatabaseAccessor Instantiate(RequestContext context)
        //{
        //    DataStoreManager.InstantiateDataStoreManager(context);
        //    return Instantiate(DataStoreManager.DataStores[DataStoreType.Database], context);
        //}
        readonly RequestContext Context;
        public PrintDatabaseAccessor(RequestContext context) { Context = context; }

        public async Task<ReadOnlyCollection<PRPrinter>> GetPrinters()
        {
            SqlPagedQuery query = new SqlPagedQuery(QueryResultSettings.AllRecords)
            {
                DatabaseSchema = "ext",
                From = "PRPRINTERTABLE"
            };
            //return this.ExecuteSelect<PRPrinter>(query).Results;
            using (DatabaseContext databaseContext = new DatabaseContext(Context))
            {
                return (await databaseContext.ReadEntityAsync<PRPrinter>(query).ConfigureAwait(false)).Results;
            }
        }

        public async Task<ReadOnlyCollection<PRTerminal>> GetTerminals()
        {
            SqlPagedQuery query = new SqlPagedQuery(QueryResultSettings.AllRecords)
            {
                DatabaseSchema = "ext",
                From = "PRTERMINALTABLE"
            };
            //return this.ExecuteSelect<PRTerminal>(query).Results;
            using (DatabaseContext databaseContext = new DatabaseContext(Context))
            {
                return (await databaseContext.ReadEntityAsync<PRTerminal>(query).ConfigureAwait(false)).Results;
            }
        }

        public async Task<ReadOnlyCollection<StorePrinter>> GetStorePrinters()
        {
            SqlPagedQuery query = new SqlPagedQuery(QueryResultSettings.AllRecords)
            {
                DatabaseSchema = "ext",
                From = "PRSTOREPRINTER"
            };
            //return this.ExecuteSelect<StorePrinter>(query).Results;
            using (DatabaseContext databaseContext = new DatabaseContext(Context))
            {
                return (await databaseContext.ReadEntityAsync<StorePrinter>(query).ConfigureAwait(false)).Results;
            }
        }

    }
}
