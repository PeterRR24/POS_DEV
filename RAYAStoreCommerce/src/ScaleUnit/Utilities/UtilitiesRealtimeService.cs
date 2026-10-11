using System;
using System.Xml;
using System.Text;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml.Linq;
using System.Threading.Tasks;

using Microsoft.Dynamics.Commerce.Runtime;
using Microsoft.Dynamics.Commerce.Runtime.Data;
using Microsoft.Dynamics.Commerce.Runtime.DataModel;
using Microsoft.Dynamics.Commerce.Runtime.Messages;
using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
using Microsoft.Dynamics.Commerce.Runtime.RealtimeServices.Messages;
//using Microsoft.Dynamics.Commerce.Runtime.TransactionService;

using RR.Commerce.Runtime.Utilities;

namespace RR.Commerce.Runtime.Mobility
{
    public class UtilitiesRealtimeService : IRequestHandlerAsync
    {
        public IEnumerable<Type> SupportedRequestTypes
        {
            get
            {
                return new[]
                {
                    typeof(CommitStoreLocationItemsRealtimeRequest)
                };
            }
        }

        public async Task<Response> Execute(Request request)
        {
            ThrowIf.Null(request, "request");

            Type requestType = request.GetType();
            Response response;

            if (requestType == typeof(CommitStoreLocationItemsRealtimeRequest))
                response = await this.CommitStoreLocationItems((CommitStoreLocationItemsRealtimeRequest)request).ConfigureAwait(false);
            else
                throw new NotSupportedException(String.Format("Request '{0}' is not supported.", request.GetType()));

            return response;
        }

        private async Task<NullResponse> CommitStoreLocationItems(CommitStoreLocationItemsRealtimeRequest request)
        {
            ThrowIf.Null(request, "request");
            ThrowIf.NullOrWhiteSpace(request.SectionId, "request.SectionId");
            ThrowIf.NullOrWhiteSpace(request.ShelfId, "request.ShelfId");

            HashSet<string> itemIds = new HashSet<string>();

            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("<Ranging");
            stringBuilder.Append(" StoreId=\"" + request.StoreId + "\"");
            stringBuilder.Append(" SectionId=\"" + request.SectionId + "\"");
            stringBuilder.Append(" ShelfId=\"" + request.ShelfId + "\"");
            stringBuilder.Append(">");

            foreach (StoreLocationItem item in request.Items)
            {
                if (!itemIds.Contains(item.ItemId))
                {
                    itemIds.Add(item.ItemId);

                    stringBuilder.Append("<Item");
                    stringBuilder.Append(" Id=\"" + EncodeString(item.ItemId) + "\"");
                    stringBuilder.Append(" Order=\"" + item.Location + "\"");
                    stringBuilder.Append(" Faces=\"" + item.FacingCount + "\"");
                    stringBuilder.Append(">");
                    stringBuilder.Append("</Item>");
                }
            }

            stringBuilder.Append("</Ranging>");

            //TransactionServiceClient transactionClient = new TransactionServiceClient(request.RequestContext);
            //transactionClient.InvokeExtensionMethod("createItemSectionLocation", stringBuilder.ToString());

            InvokeExtensionMethodRealtimeRequest extensionRequest = new InvokeExtensionMethodRealtimeRequest("createItemSectionLocation", stringBuilder.ToString());
            await request.RequestContext.ExecuteAsync<InvokeExtensionMethodRealtimeResponse>(extensionRequest).ConfigureAwait(false); 

            //return new NullResponse();
            return NullResponse.Instance;
        }

        private string EncodeString(string str)
        {
            string encoded = str.Replace("\"", "&quot;");
            return encoded;
        }
    }
}
