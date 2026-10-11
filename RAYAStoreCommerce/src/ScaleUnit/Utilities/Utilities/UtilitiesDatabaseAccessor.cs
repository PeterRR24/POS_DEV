using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Dynamics.Commerce.Runtime;
using Microsoft.Dynamics.Commerce.Runtime.Data;
using Microsoft.Dynamics.Commerce.Runtime.DataModel;

namespace RR.Commerce.Runtime.Utilities
{
    public sealed class UtilitiesDatabaseAccessor //: DatabaseAccessor
    {
        //private UtilitiesDatabaseAccessor(IDataStore dataStore, RequestContext context) : base(dataStore, context)
        //{
        //}

        //internal static UtilitiesDatabaseAccessor Instantiate(IDataStore dataStore, RequestContext context)
        //{
        //    return new UtilitiesDatabaseAccessor(dataStore, context);
        //}
        readonly RequestContext Context;
        public UtilitiesDatabaseAccessor(RequestContext context) { Context = context; }

        public async Task<UtilitiesConfiguration> GetUtilitiesConfiguration()
        {
            SqlPagedQuery query = new SqlPagedQuery(QueryResultSettings.SingleRecord)
            {
                DatabaseSchema = "ext",
                From = "RUPARAMETERS",
                Where = "KEY_ = 0 AND DATAAREAID = @AREAID",
            };
            query.Parameters["@AREAID"] = this.Context.GetChannelConfiguration().InventLocationDataAreaId;

            //return IEnumerableExtensions.SingleOrDefault<UtilitiesConfiguration>(this.ExecuteSelect<UtilitiesConfiguration>(query));
            using (DatabaseContext databaseContext = new DatabaseContext(Context))
            {
                return (await databaseContext.ReadEntityAsync<UtilitiesConfiguration>(query).ConfigureAwait(false)).FirstOrDefault();
            }

            /*
            SqlPagedQuery query = new SqlPagedQuery(QueryResultSettings.SingleRecord);
            query.QueryString = "SELECT * FROM ext.RUPARAMETERS WHERE KEY_ = 0 AND DATAAREAID = @AREAID";
            query.Parameters["@AREAID"] = this.Context.GetChannelConfiguration().InventLocationDataAreaId;

            using (DatabaseContext databaseContext = new DatabaseContext(this.Context))
            {
                using (IDatabaseResult databaseResult = databaseContext.ExecuteQuery(query))
                {
                    if (databaseResult.Read())
                    {
                        result = new UtilitiesConfiguration();
                        result.LabelHierarchyRecId = databaseResult.GetValue<long>("LABELHIERARCHYRECID");
                    }
                }
                //result = databaseContext.ReadEntity<ChannelConfiguration>(query).Results.SingleOrDefault();
            }
            */
        }

        public async Task<PagedResult<Category>> GetGetCategoryHierarchy(long hierarchyId, QueryResultSettings settings)
        {
            SqlPagedQuery query = new SqlPagedQuery(settings)
            {
                DatabaseSchema = "ax",
                Select = new ColumnSet("RECID", "PARENTCATEGORY", "NAME"),
                From = "ECORESCATEGORY",
                Where = "CATEGORYHIERARCHY = @CategoryHierarchy AND ISACTIVE = 1",
                OrderBy = "PARENTCATEGORY",
                IsQueryByPrimaryKey = false
            };

            query.Parameters["@CategoryHierarchy"] = hierarchyId;

            //return this.ExecuteSelect<Category>(query);
            using (DatabaseContext databaseContext = new DatabaseContext(Context))
            {
                return await databaseContext.ReadEntityAsync<Category>(query).ConfigureAwait(false);
            }
        }

        public async Task<PagedResult<Category>> GetGetCategoryHierarchy(long hierarchyId, string languageId)
        {
            SqlQuery query = new SqlQuery();
            query.QueryString = "SELECT C.RECID, C.PARENTCATEGORY, ISNULL(NULLIF(CT.FRIENDLYNAME,''), C.NAME) AS NAME " +
                "FROM ax.ECORESCATEGORY C " +
                  "LEFT OUTER JOIN ax.ECORESCATEGORYTRANSLATION CT ON C.RECID = CT.CATEGORY AND CT.LANGUAGEID = @LanguageId " +
                "WHERE C.CATEGORYHIERARCHY = @CategoryHierarchy AND C.ISACTIVE = 1 " +
                "ORDER BY C.PARENTCATEGORY";

            query.Parameters["@CategoryHierarchy"] = hierarchyId;
            query.Parameters["@LanguageId"] = languageId;

            using (DatabaseContext databaseContext = new DatabaseContext(this.Context))
            {
                return await databaseContext.ReadEntityAsync<Category>(query).ConfigureAwait(false);
            }
        }

        private async Task<PagedResult<Category>> GetGetCategoryHierarchyNU(long hierarchyId, string languageId)
        {
            // SQLQuery is not correct. Second table don't get prefix.
            // FROM ax.ECORESCATEGORY C LEFT OUTER JOIN ECORESCATEGORYTRANSLATION
            SqlPagedQuery query = new SqlPagedQuery(QueryResultSettings.AllRecords)
            {
                DatabaseSchema = "ax",
                Select = new ColumnSet("C.RECID", "C.PARENTCATEGORY", "ISNULL(NULLIF(CT.FRIENDLYNAME,''), C.NAME) AS NAME"),
                From = "ECORESCATEGORY C LEFT OUTER JOIN ECORESCATEGORYTRANSLATION CT ON C.RECID = CT.CATEGORY AND CT.LANGUAGEID = @LanguageId",
                Where = "C.CATEGORYHIERARCHY = @CategoryHierarchy AND C.ISACTIVE = 1",
                OrderBy = "C.PARENTCATEGORY",
                IsQueryByPrimaryKey = false
            };

            query.Parameters["@CategoryHierarchy"] = hierarchyId;
            query.Parameters["@LanguageId"] = languageId;

            //return this.ExecuteSelect<Category>(query);
            using (DatabaseContext databaseContext = new DatabaseContext(this.Context))
            {
                return await databaseContext.ReadEntityAsync<Category>(query).ConfigureAwait(false);
            }
        }

        public async Task<PagedResult<StoreSection>> GetStoreSections()
        {
            string dataAreaId = this.Context.GetChannelConfiguration().InventLocationDataAreaId;
            string storeNumber = this.Context.GetOrgUnit().OrgUnitNumber;

            SqlPagedQuery query = new SqlPagedQuery(QueryResultSettings.AllRecords)
            {
                DatabaseSchema = "ext",
                From = "RRRETAILSTORESECTION",
                Where = "(DATAAREAID = @DataAreaId) AND (STOREID = @StoreId)"
            };

            query.Parameters["@DataAreaId"] = dataAreaId;
            query.Parameters["@StoreId"] = storeNumber;

            //return this.ExecuteSelect<StoreSection>(query);
            using (DatabaseContext databaseContext = new DatabaseContext(this.Context))
            {
                return await databaseContext.ReadEntityAsync<StoreSection>(query).ConfigureAwait(false);
            }
        }

        public async Task<PagedResult<StoreShelf>> GetStoreShelves()
        {
            string dataAreaId = this.Context.GetChannelConfiguration().InventLocationDataAreaId;
            string storeNumber = this.Context.GetOrgUnit().OrgUnitNumber;

            SqlPagedQuery query = new SqlPagedQuery(QueryResultSettings.AllRecords)
            {
                DatabaseSchema = "ext",
                From = "RRRETAILSTORESHELF",
                Where = "(DATAAREAID = @DataAreaId) AND (STOREID = @StoreId)"
            };

            query.Parameters["@DataAreaId"] = dataAreaId;
            query.Parameters["@StoreId"] = storeNumber;

            //return this.ExecuteSelect<StoreShelf>(query);
            using (DatabaseContext databaseContext = new DatabaseContext(this.Context))
            {
                return await databaseContext.ReadEntityAsync<StoreShelf>(query).ConfigureAwait(false);
            }
        }

        public async Task<bool> GetStoreLocationType(string sectionId, string shelfId)
        {
            SqlQuery query = new SqlQuery();

            query.QueryString = "SELECT TOP 1 * FROM ext.RUITEMSTORELOCATION";

            using (DatabaseQuery databaseContext = new DatabaseQuery(this.Context))
            {
                using (IDatabaseResult reader = await databaseContext.Execute(query).ConfigureAwait(false))
                {
                    return await reader.ReadAsync().ConfigureAwait(false);
                }
            }
        }
    }
}
