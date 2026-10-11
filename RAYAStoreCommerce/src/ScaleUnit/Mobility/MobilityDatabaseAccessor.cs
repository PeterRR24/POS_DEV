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
    public sealed class MobilityDatabaseAccessor //: DatabaseAccessor
    {
        //private MobilityDatabaseAccessor(IDataStore dataStore, RequestContext context) : base(dataStore, context)
        //{
        //}

        //internal static MobilityDatabaseAccessor Instantiate(IDataStore dataStore, RequestContext context)
        //{
        //    return new MobilityDatabaseAccessor(dataStore, context);
        //}

        //internal static MobilityDatabaseAccessor Instantiate(RequestContext context)
        //{
        //    DataStoreManager.InstantiateDataStoreManager(context);
        //    return Instantiate(DataStoreManager.DataStores[DataStoreType.Database], context);
        //}

        readonly RequestContext Context;
        public MobilityDatabaseAccessor(RequestContext context) { Context = context; }

        public async Task<PagedResult<ProductSaleReason>> GetProductSaleReasons()
        {
            SqlQuery query = new SqlQuery();
            query.QueryString = "SELECT F.RECID, F.CODE, F.DESCRIPTION, F.DISCOUNTPERCENT, F.DISCOUNTMETHOD, F.DISCOUNTMODE, F.EXPIRYDATE, F.LABELCODERECID, F.INFOSUBCODERECID, ISC.REASONCODEID, ISC.SUBCODEID " +
                "FROM ext.RUFIRESALEREASON AS F LEFT OUTER JOIN crt.INFOSUBCODEVIEW AS ISC ON ISC.RECID = F.INFOSUBCODERECID AND ISC.DATAAREAID = F.DATAAREAID ORDER BY F.CODE";

            query.Parameters["@AREAID"] = this.Context.GetChannelConfiguration().InventLocationDataAreaId;

            using (DatabaseContext databaseContext = new DatabaseContext(this.Context))
            {
                return await databaseContext.ReadEntityAsync<ProductSaleReason>(query).ConfigureAwait(false);
            }
        }
    }
}
