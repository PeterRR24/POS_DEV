using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

using Microsoft.Dynamics.Commerce.Runtime;
using Microsoft.Dynamics.Commerce.Runtime.Messages;
using Microsoft.Dynamics.Commerce.Runtime.DataModel;

namespace RR.Commerce.Runtime.Mobility
{
    [DataContract]
    public sealed class SCJournalGetDataRequest : Request
    {
        public SCJournalGetDataRequest(int recordId)
        {
            this.RecordId = recordId;
        }

        public SCJournalGetDataRequest(string journalId)
        {
            this.JournalId = journalId;
        }

        [DataMember]
        public int RecordId { get; }

        [DataMember]
        public string JournalId { get; }
    }

    [DataContract]
    public sealed class SCJournalCreateDataRequest : Request
    {
        public SCJournalCreateDataRequest(SCJournal journal, ReadOnlyCollection<StockCountJournalTransaction> transactions)
        {
            ThrowIf.Null(journal, "journal");
            this.Journal = journal;
            this.Transactions = transactions;
        }

        [DataMember]
        public SCJournal Journal { get; }

        [DataMember]
        public ReadOnlyCollection<StockCountJournalTransaction> Transactions { get; }
    }


    [DataContract]
    public sealed class SCJournalSaveDataRequest : Request
    {
        public SCJournalSaveDataRequest(SCJournal journal)
        {
            ThrowIf.Null(journal, "journal");
            this.Journal = journal;
        }

        [DataMember]
        public SCJournal Journal { get; }
    }

    [DataContract]
    public sealed class SCJournalDeleteDataRequest : Request
    {
        public SCJournalDeleteDataRequest(int journalId)
        {
            this.JournalId = journalId;
        }

        [DataMember]
        public int JournalId { get; }
    }

    [DataContract]
    public sealed class SCJournalSearchDataRequest : Request
    {
        public SCJournalSearchDataRequest(SCJournalStatus status, string keyword, QueryResultSettings settings)
        {
            this.Status = status;
            this.Keyword = keyword;
            this.QueryResultSettings = settings;
        }

        [DataMember]
        public SCJournalStatus Status { get; }

        [DataMember]
        public string Keyword { get; }
    }

    //

    [DataContract]
    public sealed class SCJournalTransactionGetDataRequest : Request
    {
        public SCJournalTransactionGetDataRequest(int recordId)
        {
            this.RecordId = recordId;
        }

        [DataMember]
        public int RecordId { get; }
    }

    [DataContract]
    public sealed class SCJournalTransactionFindDataRequest : Request
    {
        public SCJournalTransactionFindDataRequest(int journalId, StockCountJournalBuffer buffer)
        {
            ThrowIf.Null(buffer, "buffer");
            this.JournalId = journalId;
            this.Buffer = buffer;
        }

        [DataMember]
        public int JournalId { get; }

        [DataMember]
        public StockCountJournalBuffer Buffer { get; }
    }

    [DataContract]
    public sealed class SCJournalTransactionSaveDataRequest : Request
    {
        public SCJournalTransactionSaveDataRequest(SCJournalTransaction transaction)
        {
            ThrowIf.Null(transaction, "transaction");
            this.Transaction = transaction;
        }

        [DataMember]
        public SCJournalTransaction Transaction { get; }
    }

    [DataContract]
    public sealed class SCJournalTransactionSearchDataRequest : Request
    {
        public SCJournalTransactionSearchDataRequest(int journalId, string keyword, QueryResultSettings settings)
        {
            this.JournalId = journalId;
            this.Keyword = keyword;
            this.QueryResultSettings = settings;
        }

        public SCJournalTransactionSearchDataRequest(int journalId, bool adgustment)
        {
            this.JournalId = journalId;
            this.Adgustment = adgustment;
            this.QueryResultSettings = QueryResultSettings.AllRecords;
        }


        [DataMember]
        public int JournalId { get; }

        [DataMember]
        public string Keyword { get; }

        [DataMember]
        public bool Adgustment { get; }
    }

    [DataContract]
    public sealed class SCJournalTransactionSalesDataRequest : Request
    {
        public SCJournalTransactionSalesDataRequest(string inventLocationId, string itemId, string variantId, string inventBatchId, string inventSerialId, DateTime tranDate, int tranTime)
        {
            this.InventLocationId = inventLocationId;
            this.ItemId = itemId;
            this.VariantId = variantId;
            this.InventBatchId = inventBatchId;
            this.InventSerialId = inventSerialId;
            this.TranDate = tranDate;
            this.TranTime = tranTime;
        }

        [DataMember]
        public string InventLocationId { get; }

        [DataMember]
        public string ItemId { get; }

        [DataMember]
        public string VariantId { get; }

        [DataMember]
        public string InventBatchId { get; }

        [DataMember]
        public string InventSerialId { get; }

        [DataMember]
        public DateTime TranDate { get; }

        [DataMember]
        public int TranTime { get; }
    }

    //

    [DataContract]
    public sealed class SCJournalEntryGetDataRequest : Request
    {
        public SCJournalEntryGetDataRequest(int recordId)
        {
            this.RecordId = recordId;
        }

        [DataMember]
        public int RecordId { get; }
    }

    [DataContract]
    public sealed class SCJournalEntryDeleteDataRequest : Request
    {
        public SCJournalEntryDeleteDataRequest(int recordId)
        {
            this.RecordId = recordId;
        }

        [DataMember]
        public int RecordId { get; }
    }

    [DataContract]
    public sealed class SCJournalEntrySearchDataRequest : Request
    {
        public SCJournalEntrySearchDataRequest(int journalId, string keyword, QueryResultSettings settings)
        {
            this.JournalId = journalId;
            this.Keyword = keyword;
            this.QueryResultSettings = settings;
        }

        public SCJournalEntrySearchDataRequest(int transId, QueryResultSettings settings)
        {
            this.TransactionId = transId;
            this.QueryResultSettings = settings;
        }

        [DataMember]
        public int TransactionId { get; }


        [DataMember]
        public int JournalId { get; }

        [DataMember]
        public string Keyword { get; }
    }

    //

    [DataContract]
    public sealed class SCJournalBufferCalcDataRequest : Request
    {
        public SCJournalBufferCalcDataRequest(int journalId)
        {
            this.JournalId = journalId;
        }

        [DataMember]
        public int JournalId { get; }
    }

    [DataContract]
    public class SCJournalBufferCalcDataResponse : Response
    {
        public SCJournalBufferCalcDataResponse(IEnumerable<StockCountJournalBuffer> entries)
        {
            this.Entries = IEnumerableExtensions.AsReadOnly<StockCountJournalBuffer>(entries);
        }

        [DataMember]
        public ReadOnlyCollection<StockCountJournalBuffer> Entries { get; }
    }

    [DataContract]
    public sealed class SCJournalBufferCommitDataRequest : Request
    {
        public SCJournalBufferCommitDataRequest(int journalId, ReadOnlyCollection<StockCountJournalBuffer> entries)
        {
            this.JournalId = journalId;
            this.Entries = entries;
        }

        [DataMember]
        public int JournalId { get; }
        [DataMember]
        public ReadOnlyCollection<StockCountJournalBuffer> Entries { get; }
    }
}
