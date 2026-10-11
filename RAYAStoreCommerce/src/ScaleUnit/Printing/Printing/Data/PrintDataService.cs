using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Dynamics.Commerce.Runtime;
using Microsoft.Dynamics.Commerce.Runtime.Messages;
using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
using Microsoft.Dynamics.Commerce.Runtime.Data;
using Microsoft.Dynamics.Commerce.Runtime.DataModel;

using RR.Commerce.Runtime.Utilities;

namespace RR.Commerce.Runtime.Printing
{
    public class PrintItemDataService : IRequestHandlerAsync
    {
        public IEnumerable<Type> SupportedRequestTypes
        {
            get
            {
                return new[]
                {
                    typeof(GetPRPrinterDataRequest),
                    typeof(GetPRTerminalDataRequest),
                    typeof(GetStorePrinterDataRequest)
                };
            }
        }

        public async Task<Response> Execute(Request request)
        {
            if (request == null)
                throw new ArgumentNullException("request");

            Type requestType = request.GetType();
            Response response;

            if (requestType == typeof(GetPRPrinterDataRequest))
                response = await this.GetPrinter((GetPRPrinterDataRequest)request).ConfigureAwait(false);
            else if (requestType == typeof(GetPRTerminalDataRequest))
                response = await this.GetTerminal((GetPRTerminalDataRequest)request).ConfigureAwait(false);
            else if (requestType == typeof(GetStorePrinterDataRequest))
                response = await this.GetStorePrinter((GetStorePrinterDataRequest)request).ConfigureAwait(false);
            else
                throw new NotSupportedException(String.Format("Request '{0}' is not supported.", request.GetType()));

            return await Task.FromResult(response).ConfigureAwait(false);
        }

        private async Task<SingleEntityDataServiceResponse<PRPrinter>> GetPrinter(GetPRPrinterDataRequest request)
        {
            ThrowIf.Null(request, "request");

            PrintDataManager dataManager = PrintDataManager.Instantiate(request.RequestContext);
            PRPrinter result = await dataManager.GetPrinter(request.PrinterId).ConfigureAwait(false);

            return new SingleEntityDataServiceResponse<PRPrinter>(result);
        }

        private async Task<SingleEntityDataServiceResponse<PRTerminal>> GetTerminal(GetPRTerminalDataRequest request)
        {
            ThrowIf.Null(request, "request");

            PrintDataManager dataManager = PrintDataManager.Instantiate(request.RequestContext);
            PRTerminal result = await dataManager.GetTerminal(request.TeminalId).ConfigureAwait(false);

            return new SingleEntityDataServiceResponse<PRTerminal>(result);
        }

        private async Task<SingleEntityDataServiceResponse<StorePrinter>> GetStorePrinter(GetStorePrinterDataRequest request)
        {
            ThrowIf.Null(request, "request");

            PrintDataManager dataManager = PrintDataManager.Instantiate(request.RequestContext);
            StorePrinter result = await dataManager.GetStorePrinter(request.StoreRecId).ConfigureAwait(false);

            return new SingleEntityDataServiceResponse<StorePrinter>(result);
        }

    }
}
