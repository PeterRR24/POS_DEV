
using Contoso.Commerce.Runtime.DataModel;
using Microsoft.Dynamics.Commerce.Runtime;
using Microsoft.Dynamics.Commerce.Runtime.DataModel;
using Microsoft.Dynamics.Commerce.Runtime.Hosting.Contracts;
using Microsoft.Dynamics.Commerce.Runtime.Services.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
    

//using SampleDataModel = Contoso.Commerce.Runtime.DataModel;

namespace Contoso.RetailServer.StoreHoursSample
{
    [RoutePrefix("KKAS")]
    //[BindEntity(typeof(StoreDayHours))]
    [BindEntity(typeof(Cities))]

    public class KKASDEVController : IController
    {

        [HttpPost]
        [Authorization(CommerceRoles.Anonymous, CommerceRoles.Customer, CommerceRoles.Device, CommerceRoles.Employee)]
        public async Task<PagedResult<Cities>> GetAllCity(IEndpointContext context, string State)
        {
            QueryResultSettings queryResultSettings = QueryResultSettings.AllRecords;
           // queryResultSettings.Paging = new PagingInfo(50);
            var request = new AddressRequest(State) { QueryResultSettings = queryResultSettings };

            var AddressResponse = await context.ExecuteAsync<AddressResponse>(request).ConfigureAwait(false);
            return AddressResponse.City;
        }
        [HttpPost]
        [Authorization(CommerceRoles.Anonymous, CommerceRoles.Customer, CommerceRoles.Device, CommerceRoles.Employee)]
        public async Task<PagedResult<States>> GetAllState(IEndpointContext context)
        {
            QueryResultSettings queryResultSettings = QueryResultSettings.AllRecords;
            //queryResultSettings.Paging = new PagingInfo(50);
            var request = new StateRequest() { QueryResultSettings = queryResultSettings };

            var AddressResponse = await context.ExecuteAsync<StateResponse>(request).ConfigureAwait(false);
            return AddressResponse.State;
        }



        [HttpPost]
        [Authorization(CommerceRoles.Anonymous, CommerceRoles.Customer, CommerceRoles.Device, CommerceRoles.Employee)]
        public async Task<string> GetCustomRequest(IEndpointContext context, string StoreNumber)
        {
            //QueryResultSettings queryResultSettings = QueryResultSettings.SingleRecord;
            //queryResultSettings.Paging = new PagingInfo(10);

            //var request = new RequestValue(StoreNumber) { QueryResultSettings = queryResultSettings };
            //var hoursResponse = await context.ExecuteAsync<GetResponseValue>(request).ConfigureAwait(false);

            GetResponseValue resp = await context.ExecuteAsync<GetResponseValue>(new RequestValue(StoreNumber)).ConfigureAwait(false);

            //InvokeExtensionMethodRealtimeRequest extensionRequest = new InvokeExtensionMethodRealtimeRequest("Get_Custom", StoreNumber);
            //InvokeExtensionMethodRealtimeResponse response = await context.ExecuteAsync<InvokeExtensionMethodRealtimeResponse>(extensionRequest).ConfigureAwait(false);
            //ReadOnlyCollection<object> results = response.Result;
            //string resValue = (string)results[0];

           // return hoursResponse.Value;
            return resp.Value;
        }


        [HttpPost]
        [Authorization(CommerceRoles.Anonymous, CommerceRoles.Customer, CommerceRoles.Device, CommerceRoles.Employee)]
        public async Task<string> UpdateCustomRequest(IEndpointContext context, string itemid, string value)
        {
            //QueryResultSettings queryResultSettings = QueryResultSettings.SingleRecord;
            //queryResultSettings.Paging = new PagingInfo(10);

            //var request = new RequestValue(StoreNumber) { QueryResultSettings = queryResultSettings };
            //var hoursResponse = await context.ExecuteAsync<GetResponseValue>(request).ConfigureAwait(false);

            GetResponseValue resp = await context.ExecuteAsync<GetResponseValue>(new UpdateValue(itemid,value)).ConfigureAwait(false);

            //InvokeExtensionMethodRealtimeRequest extensionRequest = new InvokeExtensionMethodRealtimeRequest("Get_Custom", StoreNumber);
            //InvokeExtensionMethodRealtimeResponse response = await context.ExecuteAsync<InvokeExtensionMethodRealtimeResponse>(extensionRequest).ConfigureAwait(false);
            //ReadOnlyCollection<object> results = response.Result;
            //string resValue = (string)results[0];

            // return hoursResponse.Value;
            return resp.Value;
        }



    }
}
