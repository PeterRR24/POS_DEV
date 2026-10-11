using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;

using Microsoft.Dynamics.Commerce.Runtime;
//using Microsoft.Dynamics.Commerce.Runtime.Client;
using Microsoft.Dynamics.Commerce.Runtime.DataModel;
using Microsoft.Dynamics.Commerce.Runtime.Messages;
using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;

namespace RR.Commerce.Runtime.Mobility
{
    public sealed class RangingManager
    {
        private readonly ICommerceRuntime runtime;

        private RangingManager(ICommerceRuntime runtime)
        {
            this.runtime = runtime;
        }

        public static RangingManager Create(ICommerceRuntime runtime)
        {
            if (runtime == null)
                throw new ArgumentNullException("runtime");
            return new RangingManager(runtime);
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

        public RangingStat Create(string sectionId, string shelfId)
        {
            ThrowIf.NullOrWhiteSpace(sectionId, "sectionId");
            ThrowIf.NullOrWhiteSpace(shelfId, "shelfId");
            return Execute<SingleEntityDataServiceResponse<RangingStat>>(new RangingCreateDataRequest(sectionId, shelfId)).Entity;
        }

        public PagedResult<RangingItem> Select(string sectionId, string shelfId, QueryResultSettings settings)
        {
            return Execute<SingleEntityDataServiceResponse<PagedResult<RangingItem>>>(new RangingSelectDataRequest(sectionId, shelfId)).Entity;
            //return list.List.AsPagedResult(settings);
        }

        public RangingStat Add(RangingItem item)
        {
            ThrowIf.Null<RangingItem>(item, "item");
            return Execute<SingleEntityDataServiceResponse<RangingStat>>(new RangingItemAddDataRequest(item)).Entity;
        }

        public RangingStat Delete(RangingItem item)
        {
            return Execute<SingleEntityDataServiceResponse<RangingStat>>(new RangingItemDeleteDataRequest(item)).Entity;
        }

        public RangingStat UndoItem(string sectionId, string shelfId)
        {
            return Execute<SingleEntityDataServiceResponse<RangingStat>>(new RangingItemUndoDataRequest(sectionId, shelfId)).Entity;
        }

        public RangingStat GetStat(string sectionId, string shelfId)
        {
            return Execute<SingleEntityDataServiceResponse<RangingStat>>(new RangingStatDataRequest(sectionId, shelfId)).Entity;
        }

        public RangingStat Reset(string sectionId, string shelfId)
        {
            return Execute<SingleEntityDataServiceResponse<RangingStat>>(new RangingResetDataRequest(sectionId, shelfId)).Entity;
        }

        public RangingItem GetItem(string barcodeId, string sectionId, string shelfId)
        {
            ThrowIf.NullOrWhiteSpace(barcodeId, barcodeId);
            return Execute<SingleEntityDataServiceResponse<RangingItem>>(new GetRangingItemServiceRequest(barcodeId, sectionId, shelfId)).Entity;
        }

        public RangingStat Commit(string sectionId, string shelfId)
        {
            return Execute<SingleEntityDataServiceResponse<RangingStat>>(new CommitRangingServiceRequest(sectionId, shelfId)).Entity;
        }
    }
}
