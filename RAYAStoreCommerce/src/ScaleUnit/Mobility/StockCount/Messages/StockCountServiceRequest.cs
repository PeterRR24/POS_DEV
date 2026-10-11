using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Runtime.Serialization;

using Microsoft.Dynamics.Commerce.Runtime;
using Microsoft.Dynamics.Commerce.Runtime.Messages;
using Microsoft.Dynamics.Commerce.Runtime.DataModel;

namespace RR.Commerce.Runtime.Mobility
{
    [DataContract]
    public sealed class GetNewStockCountJournalsServiceRequest : Request
    {
        public GetNewStockCountJournalsServiceRequest()
        {
        }
    }

    [DataContract]
    public class GetNewStockCountJournalsServiceResponse : Response
    {
        public GetNewStockCountJournalsServiceResponse(IEnumerable<StockCountJournal> journals)
        {
            this.Journals = IEnumerableExtensions.AsReadOnly<StockCountJournal>(journals);
        }

        [DataMember]
        public ReadOnlyCollection<StockCountJournal> Journals { get; }
    }

    [DataContract]
    public sealed class CreateStockCountJournalServiceRequest : Request
    {
        public CreateStockCountJournalServiceRequest(SCJournalRequest journal)
        {
            ThrowIf.Null(journal, "journal");
            this.Journal = journal;
        }

        [DataMember]
        public SCJournalRequest Journal { get; }
    }

    [DataContract]
    public sealed class CreateStockCountJournalServiceResponse : Response
    {
        public CreateStockCountJournalServiceResponse(SCJournal journal)
        {
            this.Journal = journal;
        }

        [DataMember]
        public SCJournal Journal { get; }
    }

    [DataContract]
    public sealed class CommitStockCountJournalServiceRequest : Request
    {
        public CommitStockCountJournalServiceRequest(int journalId)
        {
            this.JournalId = journalId;
        }

        [DataMember]
        public int JournalId { get; }
    }

    [DataContract]
    public sealed class CalculateStockCountJournalServiceRequest : Request
    {
        public CalculateStockCountJournalServiceRequest(int journalId, bool addLines)
        {
            this.JournalId = journalId;
            this.AddLines = addLines;
        }

        [DataMember]
        public int JournalId { get; }

        [DataMember]
        public bool AddLines { get; }

    }

    [DataContract]
    public sealed class AdjustStockCountJournalServiceRequest : Request
    {
        public AdjustStockCountJournalServiceRequest(int journalId)
        {
            this.JournalId = journalId;
        }

        [DataMember]
        public int JournalId { get; }
    }

}
