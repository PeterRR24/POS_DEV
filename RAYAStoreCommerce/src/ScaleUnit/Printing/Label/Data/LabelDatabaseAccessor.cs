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
    public sealed class LabelDatabaseAccessor //: DatabaseAccessor
    {
        //private LabelDatabaseAccessor(IDataStore dataStore, RequestContext context) : base(dataStore, context)
        //{
        //}

        //internal static LabelDatabaseAccessor Instantiate(IDataStore dataStore, RequestContext context)
        //{
        //    return new LabelDatabaseAccessor(dataStore, context);
        //}

        //internal static LabelDatabaseAccessor Instantiate(RequestContext context)
        //{
        //    DataStoreManager.InstantiateDataStoreManager(context);
        //    return Instantiate(DataStoreManager.DataStores[DataStoreType.Database], context);
        //}
        readonly RequestContext Context;
        public LabelDatabaseAccessor(RequestContext context) { Context = context; }

        public async Task<long> GetLabelProfileForItem(long productRecId, long hierarchyRecId)
        {
            SqlQuery query = new SqlQuery();
            query.QueryString = "SELECT ext.GETPRODUCTLABELPROFILEID(@ProductId, @HierarchyId)";
            query.Parameters["@ProductId"] = productRecId;
            query.Parameters["@HierarchyId"] = hierarchyRecId;
            using (DatabaseContext databaseContext = new DatabaseContext(this.Context))
            {
                return await databaseContext.ExecuteScalarAsync<long>(query).ConfigureAwait(false);
            }
        }

        public async Task<PagedResult<LabelCode>> GetLabelCodes()
        {
            SqlPagedQuery query = new SqlPagedQuery(QueryResultSettings.AllRecords)
            {
                DatabaseSchema = "ext",
                From = "PRLABELCODE"
            };
            //return this.ExecuteSelect<LabelCode>(query);
            using (DatabaseContext databaseContext = new DatabaseContext(Context))
            {
                return await databaseContext.ReadEntityAsync<LabelCode>(query).ConfigureAwait(false);
            }
        }

        public async Task<ReadOnlyCollection<LabelProfile>> GetLabelProfiles()
        {
            SqlPagedQuery query = new SqlPagedQuery(QueryResultSettings.AllRecords)
            {
                DatabaseSchema = "ext",
                From = "PRLABELPROFILE"
            };
            //return this.ExecuteSelect<LabelProfile>(query).Results;
            using (DatabaseContext databaseContext = new DatabaseContext(Context))
            {
                return (await databaseContext.ReadEntityAsync<LabelProfile>(query).ConfigureAwait(false)).Results;
            }
        }

        public async Task<ReadOnlyCollection<LabelProfileLine>> GetLabelProfileLines()
        {
            SqlPagedQuery query = new SqlPagedQuery(QueryResultSettings.AllRecords)
            {
                DatabaseSchema = "ext",
                From = "PRLABELPROFILELINE"
            };
            //return this.ExecuteSelect<LabelProfileLine>(query).Results;
            using (DatabaseContext databaseContext = new DatabaseContext(Context))
            {
                return (await databaseContext.ReadEntityAsync<LabelProfileLine>(query).ConfigureAwait(false)).Results;
            }
        }

        public async Task<ReadOnlyCollection<LabelFormat>> GetLabelFormats()
        {
            SqlPagedQuery query = new SqlPagedQuery(QueryResultSettings.AllRecords)
            {
                DatabaseSchema = "ext",
                From = "PRLABELFORMAT"
            };
            //return this.ExecuteSelect<LabelFormat>(query).Results;
            using (DatabaseContext databaseContext = new DatabaseContext(Context))
            {
                return (await databaseContext.ReadEntityAsync<LabelFormat>(query).ConfigureAwait(false)).Results;
            }
        }

        public async Task<ReadOnlyCollection<LabelLayout>> GetLabelLayouts()
        {
            SqlPagedQuery query = new SqlPagedQuery(QueryResultSettings.AllRecords)
            {
                DatabaseSchema = "ext",
                From = "PRLABELLAYOUT"
            };
            //return this.ExecuteSelect<LabelLayout>(query).Results;
            using (DatabaseContext databaseContext = new DatabaseContext(Context))
            {
                return (await databaseContext.ReadEntityAsync<LabelLayout>(query).ConfigureAwait(false)).Results;
            }
        }

        public async Task<LabelData> GetLabelData(string itemId, string variantId)
        {
            ThrowIf.NullOrWhiteSpace(itemId, "itemId");

            // ChannelConfiguration c1 = this.Context.GetChannelConfiguration();
            // DeviceConfiguration c2 = this.Context.GetDeviceConfiguration();
            // OrgUnit c3 = this.Context.GetOrgUnit();
            // ICommercePrincipal c4 = this.Context.GetPrincipal();
            // Terminal c5 = this.Context.GetTerminal();

            SqlPagedQuery query = new SqlPagedQuery(QueryResultSettings.SingleRecord)
            {
                DatabaseSchema = "ext",
                From = "PRLABELDATA",
                Where = "ITEMID = @ITEMID AND (VARIANTID = @VARIANTID OR VARIANTID = '') AND DATAAREAID = @DATAAREAID",
                OrderBy = "ITEMID, VARIANTID DESC"
            };

            query.Parameters["@DATAAREAID"] = this.Context.GetChannelConfiguration().InventLocationDataAreaId;
            query.Parameters["@ITEMID"] = itemId;
            query.Parameters["@VARIANTID"] = variantId ?? String.Empty;

            //return this.ExecuteSelect<LabelData>(query).FirstOrDefault();
            using (DatabaseContext databaseContext = new DatabaseContext(Context))
            {
                return (await databaseContext.ReadEntityAsync<LabelData>(query).ConfigureAwait(false)).FirstOrDefault();
            }
        }

        public async Task<LabelData> GetLabelData(long recId)
        {
            SqlPagedQuery query = new SqlPagedQuery(QueryResultSettings.SingleRecord)
            {
                DatabaseSchema = "ext",
                From = "PRLABELDATA",
                Where = "RECID = @RECID"
            };

            query.Parameters["@RECID"] = recId;

            //return this.ExecuteSelect<LabelData>(query).FirstOrDefault();
            using (DatabaseContext databaseContext = new DatabaseContext(Context))
            {
                return (await databaseContext.ReadEntityAsync<LabelData>(query).ConfigureAwait(false)).FirstOrDefault();
            }
        }
    }
}
