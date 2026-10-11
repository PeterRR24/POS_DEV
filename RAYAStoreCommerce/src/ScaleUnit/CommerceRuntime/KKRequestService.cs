using System;
using System.Collections.Generic;
using System.Text;

namespace Contoso
{

    namespace Commerce.Runtime.KKASDEV1
    {

        using System;
        using System.Collections.Generic;
        using System.Collections.ObjectModel;
        using System.Threading.Tasks;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.RealtimeServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Services.Messages;

        public class ProviderRequestService : IRequestHandlerAsync
        {
            public IEnumerable<Type> SupportedRequestTypes
            {
                get
                {
                    return new[]
                    {
                        //typeof(saveca),
                        typeof(UpdateValue),
                        typeof(AddressRequest),
                        typeof(StateRequest),
                    };
                }
            }

            public async Task<Response> Execute(Request request)
            {
                if (request == null)
                {
                    throw new ArgumentNullException("request");
                }

                Type requestedType = request.GetType();
                if (requestedType == typeof(AddressRequest))
                {
                    return await this.GetCityAsync((AddressRequest)request).ConfigureAwait(false);
                }
                if (requestedType == typeof(StateRequest))
                {
                    return await this.GetStateAsync((StateRequest)request).ConfigureAwait(false);
                }
                if (requestedType == typeof(RequestValue))
                {
                       return await this.KKCustomRequestAsyc((RequestValue)request).ConfigureAwait(false);
                 
                }
                if (requestedType == typeof(UpdateValue))
                {
                    return await this.KKUpdateCustomRequestAsyc((UpdateValue)request).ConfigureAwait(false);

                }

                throw new NotSupportedException(string.Format("Request '{0}' is not supported.", request.GetType()));
            }
            private async Task<Response> GetCityAsync(AddressRequest request)
            {
                //ThrowIf.Null(request, "request");

                using (DatabaseContext databaseContext = new DatabaseContext(request.RequestContext))
                {
                    var query = new SqlPagedQuery(request.QueryResultSettings)
                    {
                        DatabaseSchema = "ax",
                        Select = new ColumnSet("DESCRIPTION", "RECID"),
                        From = "LOGISTICSADDRESSCITY",
                        Where = "STATEID = @state and OLD = 0",
                    };

                    query.Parameters["@state"] = request.State;

                    return new AddressResponse(await databaseContext.ReadEntityAsync<DataModel.Cities>(query).ConfigureAwait(false));
                }
            }
            private async Task<Response> GetStateAsync(StateRequest request)
            {
                //ThrowIf.Null(request, "request");

                using (DatabaseContext databaseContext = new DatabaseContext(request.RequestContext))
                {
                    var query = new SqlPagedQuery(request.QueryResultSettings)
                    {
                        DatabaseSchema = "ax",
                        Select = new ColumnSet("NAME", "RECID"),
                        From = "LOGISTICSADDRESSSTATE",
                        Where = "OLD = 0",
                    };


                    return new StateResponse(await databaseContext.ReadEntityAsync<DataModel.States>(query).ConfigureAwait(false));
                }
            }





            private async Task<Response> KKCustomRequestAsyc(RequestValue request)
            {
                //string receiptFieldName = "test";


                //InvokeExtensionMethodRealtimeRequest extensionRequest = new InvokeExtensionMethodRealtimeRequest("Get_Custom", request.XVALUE);
                //InvokeExtensionMethodRealtimeResponse response = await request.RequestContext.ExecuteAsync<InvokeExtensionMethodRealtimeResponse>(extensionRequest).ConfigureAwait(false);
                //ReadOnlyCollection<object> results = response.Result;
                //string resValue = (string)results[0];

                //Test_POS.Class1 class1 = new Test_POS.Class1();
                


                return new GetResponseValue("");
             //   return new GetResponseValue(resValue);
            }


            private async Task<Response> KKUpdateCustomRequestAsyc(UpdateValue request)
            {
                //string receiptFieldName = "test";


                InvokeExtensionMethodRealtimeRequest extensionRequest = new InvokeExtensionMethodRealtimeRequest(
                    "Update_Custom",
                    request.ItemID,
                    request.UpValue);
                InvokeExtensionMethodRealtimeResponse response = 
                    await request.RequestContext.ExecuteAsync<InvokeExtensionMethodRealtimeResponse>(extensionRequest).ConfigureAwait(false);
                ReadOnlyCollection<object> results = response.Result;


                string resValue = (string)results[0];


                return new GetResponseValue(resValue);
            }


        }
    }
}
