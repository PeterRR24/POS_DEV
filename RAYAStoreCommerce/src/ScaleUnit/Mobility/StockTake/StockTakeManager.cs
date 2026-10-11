using System;
using System.Text;
using System.Collections.ObjectModel;

using Microsoft.Dynamics.Commerce.Runtime;
//using Microsoft.Dynamics.Commerce.Runtime.Client;
using Microsoft.Dynamics.Commerce.Runtime.DataModel;
using Microsoft.Dynamics.Commerce.Runtime.Messages;
using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;


namespace RR.Commerce.Runtime.Mobility
{
    public sealed class StockTakeManager
    {
        private readonly ICommerceRuntime runtime;

        private StockTakeManager(ICommerceRuntime runtime)
        {
            this.runtime = runtime;
        }

        public static StockTakeManager Create(ICommerceRuntime runtime)
        {
            if (runtime == null)
                throw new ArgumentNullException("runtime");
            return new StockTakeManager(runtime);
        }
        internal TResponse Execute<TResponse>(Request request) where TResponse : Response
        {
#pragma warning disable CS0618 // Type or member is obsolete
#pragma warning disable AvoidBlockingCallsAnalyzer // Avoid executing requests synchronously.
            return this.runtime.Execute<TResponse>(request, null);
#pragma warning restore AvoidBlockingCallsAnalyzer // Avoid executing requests synchronously.
#pragma warning restore CS0618 // Type or member is obsolete
        }

        //

        public PagedResult<StockTakeJournal> GetJournals(QueryResultSettings settings)
        {
            ThrowIf.Null<QueryResultSettings>(settings, "settings");
            if (settings.Sorting == null || !settings.Sorting.IsSpecified)
                settings.Sorting = new SortingInfo(new SortColumn("JOURNALID", true));
            return Execute<EntityDataServiceResponse<StockTakeJournal>>(new GetStockTakeJournalsServiceRequest(settings)).PagedEntityCollection;
        }

        public StockTakeItem GetItem(StockTakeRequest request)
        {
            ThrowIf.Null<StockTakeRequest>(request, "request");
            ThrowIf.NullOrWhiteSpace(request.BarcodeId, "request.BarcodeId");
            return Execute<SingleEntityDataServiceResponse<StockTakeItem>>(new GetStockTakeItemServiceRequest(request)).Entity;
        }

        public decimal CountItem(StockTakeItem item, bool addMode)
        {
            ThrowIf.Null<StockTakeItem>(item, "item");
            return Execute<SingleEntityDataServiceResponse<decimal>>(new CountStockTakeItemServiceRequest(item, addMode)).Entity;
        }
    }
}
