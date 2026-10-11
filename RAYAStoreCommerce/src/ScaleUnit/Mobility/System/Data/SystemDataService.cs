using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Dynamics.Commerce.Runtime;
using Microsoft.Dynamics.Commerce.Runtime.Messages;
using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
using Microsoft.Dynamics.Commerce.Runtime.Data;

namespace RR.Commerce.Runtime.Mobility
{
    public class SystemDataService : IRequestHandlerAsync
    {
        public IEnumerable<Type> SupportedRequestTypes
        {
            get
            {
                return new[]
                {
                    typeof(ValidateMobilityDeviceDataRequest),
                };
            }
        }

        public async Task<Response> Execute(Request request)
        {
            if (request == null)
                throw new ArgumentNullException("request");

            Type requestType = request.GetType();
            Response response;

            if (requestType == typeof(ValidateMobilityDeviceDataRequest))
                response = await this.ValidateMobilityDevice((ValidateMobilityDeviceDataRequest)request).ConfigureAwait(false);
            else
                throw new NotSupportedException(String.Format("Request '{0}' is not supported.", request.GetType()));

            return response;
        }

        private async Task<SingleEntityDataServiceResponse<bool>> ValidateMobilityDevice(ValidateMobilityDeviceDataRequest request)
        {
            ThrowIf.Null(request, "request");
            ThrowIf.Null(request.DeviceNumber, "request.DeviceNumber");

            SqlQuery query = new SqlQuery();

            query.QueryString = "SELECT RECID FROM ext.RUMOBILEDEVICE " +
                "WHERE SERIALNUMBER = @SERIALNUMBER AND (CHANNEL = @CHANNEL OR CHANNEL = 0) AND ACTIVE = 1";

            query.Parameters["@SERIALNUMBER"] = request.DeviceNumber;
            query.Parameters["@CHANNEL"] = request.RequestContext.GetPrincipal().ChannelId;

            bool isValid = false;

            using (DatabaseQuery databaseContext = new DatabaseQuery(request.RequestContext))
            {
                using (IDatabaseResult reader = await databaseContext.Execute(query).ConfigureAwait(false))
                {
                    if (await reader.ReadAsync().ConfigureAwait(false))
                        isValid = true;
                }
            }

            return new SingleEntityDataServiceResponse<bool>(isValid);
        }
    }
}
