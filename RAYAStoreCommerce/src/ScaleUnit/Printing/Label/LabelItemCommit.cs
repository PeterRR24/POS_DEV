using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Dynamics.Commerce.Runtime;
using Microsoft.Dynamics.Commerce.Runtime.DataModel;
using Microsoft.Dynamics.Commerce.Runtime.Messages;
using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
using Microsoft.Dynamics.Commerce.Runtime.Services.Messages;
using Microsoft.Dynamics.Commerce.Runtime.Data;

using RR.Commerce.Runtime.Utilities;

namespace RR.Commerce.Runtime.Printing
{
    public sealed class LabelItemCommitRequestHandler : SingleAsyncRequestHandler<LabelItemCommitRequest>
    {
        protected override async Task<Response> Process(LabelItemCommitRequest request)
        {
            ThrowIf.Null(request, "request");

            await SaveLabelHistory(request).ConfigureAwait(false);
            await DeleteLabelItems(request).ConfigureAwait(false);

            //return new NullResponse();
            return NullResponse.Instance;
        }

        private async Task SaveLabelHistory(LabelItemCommitRequest request1)
        {
            LabelItemStatistic item = request1.Item;
            LabelHistorySaveDataRequest request = new LabelHistorySaveDataRequest(item.LabelType, item.Personal);
            request.LayoutRecId = item.LayoutRecId;
            request.PrinterRecId = item.PrinterRecId;
            await request1.RequestContext.ExecuteAsync<NullResponse>(request).ConfigureAwait(false);
        }

        private async Task DeleteLabelItems(LabelItemCommitRequest request1)
        {
            LabelItemStatistic item = request1.Item;
            LabelItemClearDataRequest request = new LabelItemClearDataRequest(item.LabelType, item.Personal);
            request.LayoutRecId = item.LayoutRecId;
            request.PrinterRecId = item.PrinterRecId;
            await request1.RequestContext.ExecuteAsync<NullResponse>(request).ConfigureAwait(false);
        }
    }
}
