using System;
using System.Threading.Tasks;
using Microsoft.Dynamics.Commerce.Runtime;
using Microsoft.Dynamics.Commerce.Runtime.Data;
using Microsoft.Dynamics.Commerce.Runtime.Services.Messages;

namespace RR.Commerce.Runtime.Utilities
{
    internal sealed class LicenseDatabaseAccessor //: DatabaseAccessor
    {
        //private LicenseDatabaseAccessor(IDataStore dataStore, RequestContext context) : base(dataStore, context)
        //{
        //}

        //internal static LicenseDatabaseAccessor Instantiate(IDataStore dataStore, RequestContext context)
        //{
        //    return new LicenseDatabaseAccessor(dataStore, context);
        //}
        readonly RequestContext Context;
        public LicenseDatabaseAccessor(RequestContext context) { Context = context; }

        public async Task<int> GetMobilityLicensedDeviceCount()
        {
            SqlQuery query = new SqlQuery();
            query.QueryString = "SELECT COUNT(RECID) AS ACTIVESEATS FROM ext.RUMOBILEDEVICE WHERE ACTIVE = 1";            
            using (DatabaseQuery databaseContext = new DatabaseQuery(Context))
            {
                return await databaseContext.ExecuteScalarAsync<int>(query).ConfigureAwait(false);
            }
        }

        public async Task<string> GetProductSerialNumber(int productType)
        {
            SqlQuery query = new SqlQuery();
            query.QueryString = "SELECT SERIALNUMBER FROM ext.RUPRODUCTLICENSE WHERE LICENSETYPE = @LICENSETYPE";

            query.Parameters["@LICENSETYPE"] = productType;

            string serialNumber = null;

            using (DatabaseQuery databaseContext = new DatabaseQuery(Context))
            {
                using (IDatabaseResult reader = await databaseContext.Execute(query).ConfigureAwait(false))
                {
                    if (await reader.ReadAsync().ConfigureAwait(false))
                    {
                        serialNumber = reader.GetValue<string>(0);
                    }
                }
            }

            return serialNumber;
        }

        public async Task<string> GetSystemTenantId()
        {
            /*
            SqlQuery query = new SqlQuery();
            query.QueryString = "SELECT [VALUE] FROM ax.SYSSERVICECONFIGURATIONSETTING WHERE [Name] = @Name";

            query.Parameters["@Name"] = "TENANTID";

            string tenantId = null;

            using (DatabaseQuery databaseContext = new DatabaseQuery(Context))
            {
                using (IDatabaseResult reader = databaseContext.Execute(query))
                {
                    if (reader.Read())
                    {
                        tenantId = reader.GetValue<string>(0);
                    }
                }
            }
            */
            GetEnvironmentConfigurationServiceRequest request = new GetEnvironmentConfigurationServiceRequest();
            GetEnvironmentConfigurationServiceResponse envConfig = await Context.ExecuteAsync<GetEnvironmentConfigurationServiceResponse>(request).ConfigureAwait(false);
            string tenantId = Convert.ToString(envConfig.EnvironmentConfiguration.TenantId);
            return tenantId;
        }
    }
}
