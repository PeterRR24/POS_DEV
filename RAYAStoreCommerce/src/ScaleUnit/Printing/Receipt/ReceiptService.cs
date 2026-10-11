using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Dynamics.Commerce.Runtime;
using Microsoft.Dynamics.Commerce.Runtime.DataModel;
using Microsoft.Dynamics.Commerce.Runtime.Messages;
using Microsoft.Dynamics.Commerce.Runtime.Services;
using Microsoft.Dynamics.Commerce.Runtime.Services.Messages;

namespace RR.Commerce.Runtime.Printing
{
    /*
    public class ReceiptService : IRequestHandler
    {
        public IEnumerable<Type> SupportedRequestTypes
        {
            get
            {
                return new[]
                {
                    typeof(GetReceiptServiceRequest),
                };
            }
        }

        public Response Execute(Request request)
        {
            if (request == null)
            {
                return null;
            }

            // 1. First we need to get the original receipts.
            ReceiptService receiptService = new ReceiptService();
            GetReceiptServiceResponse originalReceiptsResponse = request.RequestContext.Runtime.Execute<GetReceiptServiceResponse>(request, request.RequestContext, receiptService, skipRequestTriggers: false);

            // 2. Find the X/Z report from the original receipts.
            foreach (Receipt receipt in originalReceiptsResponse.Receipts)
            {
                if (receipt.ReceiptType == ReceiptType.XReport)
                {
                    this.CustomizeXReport(receipt);
                }
                else if (receipt.ReceiptType == ReceiptType.ZReport)
                {
                    this.CustomizeZReport(receipt);
                }
            }

            return originalReceiptsResponse;
        }
    }
    */
}
