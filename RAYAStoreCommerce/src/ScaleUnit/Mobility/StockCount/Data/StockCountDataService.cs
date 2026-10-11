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

namespace RR.Commerce.Runtime.Mobility
{
    public class StockCountDataService : IRequestHandlerAsync
    {
        public IEnumerable<Type> SupportedRequestTypes
        {
            get
            {
                return new[]
                {
                    typeof(SCJournalGetDataRequest),
                    typeof(SCJournalSearchDataRequest),
                    typeof(SCJournalCreateDataRequest),
                    typeof(SCJournalDeleteDataRequest),
                    typeof(SCJournalSaveDataRequest),
                    typeof(SCJournalTransactionGetDataRequest),
                    typeof(SCJournalTransactionFindDataRequest),
                    typeof(SCJournalTransactionSaveDataRequest),
                    typeof(SCJournalTransactionSearchDataRequest),
                    typeof(SCJournalTransactionSalesDataRequest),
                    typeof(SCJournalEntryGetDataRequest),
                    typeof(SCJournalEntryDeleteDataRequest),
                    typeof(SCJournalEntrySearchDataRequest),
                    typeof(SCJournalBufferCalcDataRequest),
                    typeof(SCJournalBufferCommitDataRequest)
                };
            }
        }

        public async Task<Response> Execute(Request request)
        {
            if (request == null)
                throw new ArgumentNullException("request");

            Type requestType = request.GetType();
            Response response;

            if (requestType == typeof(SCJournalGetDataRequest))
                response = await this.GetSCJournal((SCJournalGetDataRequest)request).ConfigureAwait(false);
            else if (requestType == typeof(SCJournalSearchDataRequest))
                response = await this.SearchSCJournals((SCJournalSearchDataRequest)request).ConfigureAwait(false);
            else if (requestType == typeof(SCJournalCreateDataRequest))
                response = await this.CreateSCJournal((SCJournalCreateDataRequest)request).ConfigureAwait(false);
            else if (requestType == typeof(SCJournalDeleteDataRequest))
                response = await this.DeleteSCJournal((SCJournalDeleteDataRequest)request).ConfigureAwait(false);
            else if (requestType == typeof(SCJournalSaveDataRequest))
                response = await this.UpdateSCJournal((SCJournalSaveDataRequest)request).ConfigureAwait(false);
            else if (requestType == typeof(SCJournalTransactionGetDataRequest))
                response = await this.GetSCJournalTransaction((SCJournalTransactionGetDataRequest)request).ConfigureAwait(false);
            else if (requestType == typeof(SCJournalTransactionFindDataRequest))
                response = await this.FindSCJournalTransaction((SCJournalTransactionFindDataRequest)request).ConfigureAwait(false);
            else if (requestType == typeof(SCJournalTransactionSaveDataRequest))
                response = await this.UpdateSCJournalTransaction((SCJournalTransactionSaveDataRequest)request).ConfigureAwait(false);
            else if (requestType == typeof(SCJournalTransactionSearchDataRequest))
                response = await this.SearchSCJournalTransactions((SCJournalTransactionSearchDataRequest)request).ConfigureAwait(false);
            else if (requestType == typeof(SCJournalTransactionSalesDataRequest))
                response = await this.SalesSCJournalTransaction((SCJournalTransactionSalesDataRequest)request).ConfigureAwait(false);
            else if (requestType == typeof(SCJournalEntryGetDataRequest))
                response = await this.GetSCJournalEntry((SCJournalEntryGetDataRequest)request).ConfigureAwait(false);
            else if (requestType == typeof(SCJournalEntryDeleteDataRequest))
                response = await this.DeleteSCJournalEntry((SCJournalEntryDeleteDataRequest)request).ConfigureAwait(false);
            else if (requestType == typeof(SCJournalEntrySearchDataRequest))
                response = await this.SearchSCJournalEntries((SCJournalEntrySearchDataRequest)request).ConfigureAwait(false);
            else if (requestType == typeof(SCJournalBufferCalcDataRequest))
                response = await this.CalculateSCJournalBuffer((SCJournalBufferCalcDataRequest)request).ConfigureAwait(false);
            else if (requestType == typeof(SCJournalBufferCommitDataRequest))
                response = await this.CommitSCJournalBuffer((SCJournalBufferCommitDataRequest)request).ConfigureAwait(false);
            else
                throw new NotSupportedException(String.Format("Request '{0}' is not supported.", request.GetType()));

            return response;
        }

        private async Task<SingleEntityDataServiceResponse<SCJournal>> GetSCJournal(SCJournalGetDataRequest request)
        {
            ThrowIf.Null(request, "request");
            if (request.RecordId != 0)
                return await GetSCJournalById(request.RequestContext, request.RecordId).ConfigureAwait(false);
            else
                return await GetSCJournalByJournal(request.RequestContext, request.JournalId).ConfigureAwait(false);
        }

        private async Task<SingleEntityDataServiceResponse<SCJournal>> GetSCJournalById(RequestContext context, int id)
        {
            SqlPagedQuery query = new SqlPagedQuery(QueryResultSettings.SingleRecord)
            {
                DatabaseSchema = "ext",
                From = "SCJOURNAL",
                Where = "(ID = @Id)"
            };

            query.Parameters["@Id"] = id;

            using (DatabaseContext databaseContext = new DatabaseContext(context))
            {
                return new SingleEntityDataServiceResponse<SCJournal>((await databaseContext.ReadEntityAsync<SCJournal>(query).ConfigureAwait(false)).FirstOrDefault());
            }
        }

        private async Task<SingleEntityDataServiceResponse<SCJournal>> GetSCJournalByJournal(RequestContext context, string id)
        {
            SqlPagedQuery query = new SqlPagedQuery(QueryResultSettings.SingleRecord)
            {
                DatabaseSchema = "ext",
                From = "SCJOURNAL",
                Where = "(JOURNALID = @Id)"
            };

            query.Parameters["@Id"] = id;

            using (DatabaseContext databaseContext = new DatabaseContext(context))
            {
                return new SingleEntityDataServiceResponse<SCJournal>((await databaseContext.ReadEntityAsync<SCJournal>(query).ConfigureAwait(false)).FirstOrDefault());
            }
        }

        private async Task<EntityDataServiceResponse<SCJournal>> SearchSCJournals(SCJournalSearchDataRequest request)
        {
            ThrowIf.Null(request, "request");
            ThrowIf.Null(request.QueryResultSettings, "request.QueryResultSettings");

            SqlPagedQuery query = new SqlPagedQuery(request.QueryResultSettings)
            {
                DatabaseSchema = "ext",
                From = "SCJOURNAL",
                Where = "(STOREID = @STOREID) AND (STATUS = @STATUS)"
            };

            query.Parameters["@STOREID"] = request.RequestContext.GetOrgUnit().OrgUnitNumber;
            query.Parameters["@STATUS"] = (int)request.Status;

            if (!String.IsNullOrEmpty(request.Keyword))
            {
                query.Where += " AND ((JOURNALID LIKE @SearchText) OR (DESCRIPTION LIKE @SearchText))";
                if (request.Keyword.Contains("%"))
                    query.Parameters["@SearchText"] = request.Keyword;
                else
                    query.Parameters["@SearchText"] = '%' + request.Keyword + '%';
            }

            using (DatabaseContext databaseContext = new DatabaseContext(request.RequestContext))
            {
                return new EntityDataServiceResponse<SCJournal>(await databaseContext.ReadEntityAsync<SCJournal>(query).ConfigureAwait(false));
            }
        }

        private async Task<SingleEntityDataServiceResponse<SCJournal>> CreateSCJournal(SCJournalCreateDataRequest request)
        {
            ThrowIf.Null(request, "request");

            int journalId = 0;

            /*
            using (DatabaseContext databaseContext = new DatabaseContext(request.RequestContext, true))
            {
                using (IDatabaseTransaction transaction = databaseContext.BeginTransaction())
                {
                    journalId = CreateSCJournal(databaseContext, request.Journal);
                    CreateSCJournalTransactions(databaseContext, request.Transactions, journalId);
                    transaction.Commit();
                }
            }
            */

            using (DatabaseContext databaseContext = new DatabaseContext(request.RequestContext, DatabaseConnectionMode.IsPersisted))
            {
                journalId = await CreateSCJournal(databaseContext, request.Journal).ConfigureAwait(false);
                await CreateSCJournalTransactions(databaseContext, request.Transactions, journalId).ConfigureAwait(false);
            }

            return await GetSCJournalById(request.RequestContext, journalId).ConfigureAwait(false);
        }

        private async Task<int> CreateSCJournal(DatabaseContext databaseContext, SCJournal journal)
        {
            ThrowIf.Null(journal, "journal");

            SqlQuery query = new SqlQuery();

            query.QueryString =
            "INSERT INTO ext.SCJOURNAL ( " +
                "RECID, JOURNALID, DESCRIPTION, STOREID, INVENTLOCID, DIMSETUP, CALCMODE, LOCMODE, STATUS, CREATEDDATE " +
            ") VALUES ( " +
                "@RECID, @JOURNALID, @DESCRIPTION, @STOREID, @INVENTLOCID, @DIMSETUP, @CALCMODE, @LOCMODE, @STATUS, @CREATEDDATE " +
            "); " +
            "SELECT ID FROM ext.SCJOURNAL WHERE (ID = SCOPE_IDENTITY())";

            query.Parameters["@RECID"] = journal.RecordId;
            query.Parameters["@JOURNALID"] = journal.JournalId;
            query.Parameters["@DESCRIPTION"] = journal.Description ?? String.Empty;
            query.Parameters["@STOREID"] = journal.StoreId ?? String.Empty;
            query.Parameters["@INVENTLOCID"] = journal.InventLocationId ?? String.Empty;
            query.Parameters["@DIMSETUP"] = journal.DimSetup;
            query.Parameters["@CALCMODE"] = journal.CalcModeValue;
            query.Parameters["@LOCMODE"] = journal.LocationModeValue;
            query.Parameters["@STATUS"] = journal.StatusValue;
            query.Parameters["@CREATEDDATE"] = journal.CreatedDate == DateTimeOffset.MinValue ? DateTimeOffset.Now : journal.CreatedDate;

            return await databaseContext.ExecuteScalarAsync<int>(query).ConfigureAwait(false);
        }

        private async Task CreateSCJournalTransactions(DatabaseContext databaseContext, IEnumerable<StockCountJournalTransaction> journalTrans, int journalId)
        {
            ThrowIf.Null(journalTrans, "journalTrans");

            SqlQuery query = new SqlQuery();

            query.QueryString =
            "INSERT INTO ext.SCJOURNALTRANS ( " +
               "JOURNALID, RECID, ITEMID, ITEMNAME, UNIT, INVENTDIMID, INVENTBATCHID, INVENTSERIALID, INVENTSIZEID, INVENTCOLORID, INVENTSTYLEID, CONFIGID, QUANTITY, STATUS " +
            ") VALUES (" +
               "@JOURNALID, @RECID, @ITEMID, @ITEMNAME, @UNIT, @INVENTDIMID, @INVENTBATCHID, @INVENTSERIALID, @INVENTSIZEID, @INVENTCOLORID, @INVENTSTYLEID, @CONFIGID, @QUANTITY, @STATUS " +
            ")";

            foreach (StockCountJournalTransaction journalTran in journalTrans)
            {
                query.Parameters["@JOURNALID"] = journalId;
                query.Parameters["@RECID"] = journalTran.RecordId;
                query.Parameters["@ITEMID"] = journalTran.ItemId ?? String.Empty;
                query.Parameters["@ITEMNAME"] = journalTran.ItemName ?? String.Empty;
                query.Parameters["@UNIT"] = journalTran.Unit ?? String.Empty;
                query.Parameters["@INVENTDIMID"] = journalTran.InventDimId ?? String.Empty;
                query.Parameters["@INVENTBATCHID"] = journalTran.InventBatchId ?? String.Empty;
                query.Parameters["@INVENTSERIALID"] = journalTran.InventSerialId ?? String.Empty;
                query.Parameters["@INVENTSIZEID"] = journalTran.InventSizeId ?? String.Empty;
                query.Parameters["@INVENTCOLORID"] = journalTran.InventColorId ?? String.Empty;
                query.Parameters["@INVENTSTYLEID"] = journalTran.InventStyleId ?? String.Empty;
                query.Parameters["@CONFIGID"] = journalTran.ConfigId ?? String.Empty;
                query.Parameters["@QUANTITY"] = journalTran.Quantity;
                query.Parameters["@STATUS"] = (int)SCJournalTransStatus.Default;

                await databaseContext.ExecuteNonQueryAsync(query).ConfigureAwait(false);
            }
        }

        private async Task<SingleEntityDataServiceResponse<SCJournal>> UpdateSCJournal(SCJournalSaveDataRequest request)
        {
            ThrowIf.Null(request, "request");

            SCJournal journal = request.Journal;

            SqlQuery query = new SqlQuery();

            query.QueryString = "UPDATE ext.SCJOURNAL SET STATUS = @STATUS, COMMITDATE = @COMMITDATE, STAFFID = @STAFFID WHERE (ID = @ID) ";

            query.Parameters["@ID"] = journal.Id;
            query.Parameters["@STATUS"] = journal.StatusValue;
            query.Parameters["@COMMITDATE"] = journal.CommitDate;
            query.Parameters["@STAFFID"] = journal.StaffId;

            using (DatabaseContext databaseContext = new DatabaseContext(request.RequestContext))
            {
                await databaseContext.ExecuteNonQueryAsync(query).ConfigureAwait(false);
            }

            return new SingleEntityDataServiceResponse<SCJournal>(journal);
        }

        private async Task<NullResponse> DeleteSCJournal(SCJournalDeleteDataRequest request)
        {
            ThrowIf.Null(request, "request");

            SqlQuery query = new SqlQuery();

            query.QueryString = "DELETE FROM ext.SCJOURNAL WHERE (ID = @ID); " +
                "DELETE FROM ext.SCJOURNALTRANS WHERE (JOURNALID = @ID); " +
                "DELETE FROM ext.SCJOURNALENTRY WHERE (JOURNALID = @ID)";

            query.Parameters["@ID"] = request.JournalId;

            using (DatabaseContext databaseContext = new DatabaseContext(request.RequestContext))
            {
                await databaseContext.ExecuteNonQueryAsync(query).ConfigureAwait(false);
            }

            return NullResponse.Instance; //   return new NullResponse();
        }

        // Transaction

        private async Task<SingleEntityDataServiceResponse<SCJournalTransaction>> GetSCJournalTransaction(SCJournalTransactionGetDataRequest request)
        {
            ThrowIf.Null(request, "request");

            SqlPagedQuery query = new SqlPagedQuery(QueryResultSettings.SingleRecord)
            {
                DatabaseSchema = "ext",
                From = "SCJOURNALTRANS",
                Where = "(ID = @ID)"
            };

            query.Parameters["@ID"] = request.RecordId;

            using (DatabaseContext databaseContext = new DatabaseContext(request.RequestContext))
            {
                return new SingleEntityDataServiceResponse<SCJournalTransaction>((await databaseContext.ReadEntityAsync<SCJournalTransaction>(query).ConfigureAwait(false)).FirstOrDefault());
            }
        }

        private async Task<SingleEntityDataServiceResponse<SCJournalTransaction>> FindSCJournalTransaction(SCJournalTransactionFindDataRequest request)
        {
            ThrowIf.Null(request, "request");

            SqlPagedQuery query = new SqlPagedQuery(QueryResultSettings.SingleRecord)
            {
                DatabaseSchema = "ext",
                From = "SCJOURNALTRANS",
                Where = "(JOURNALID = @JOURNALID) AND " +
                    "(ITEMID = @ITEMID) AND (INVENTSIZEID = @INVENTSIZEID) AND (INVENTCOLORID = @INVENTCOLORID) AND (INVENTSTYLEID = @INVENTSTYLEID) AND (CONFIGID = @CONFIGID) AND " +
                    "(INVENTBATCHID = @INVENTBATCHID) AND (INVENTSERIALID = @INVENTSERIALID)"
            };

            query.Parameters["@JOURNALID"] = request.JournalId;
            query.Parameters["@ITEMID"] = request.Buffer.ItemId ?? String.Empty;
            query.Parameters["@INVENTSIZEID"] = request.Buffer.InventSizeId ?? String.Empty;
            query.Parameters["@INVENTCOLORID"] = request.Buffer.InventColorId ?? String.Empty;
            query.Parameters["@INVENTSTYLEID"] = request.Buffer.InventStyleId ?? String.Empty;
            query.Parameters["@CONFIGID"] = request.Buffer.ConfigId ?? String.Empty;
            query.Parameters["@INVENTBATCHID"] = request.Buffer.InventBatchId ?? String.Empty;
            query.Parameters["@INVENTSERIALID"] = request.Buffer.InventSerialId ?? String.Empty;

            using (DatabaseContext databaseContext = new DatabaseContext(request.RequestContext))
            {
                return new SingleEntityDataServiceResponse<SCJournalTransaction>((await databaseContext.ReadEntityAsync<SCJournalTransaction>(query).ConfigureAwait(false)).FirstOrDefault());
            }
        }

        private async Task<SingleEntityDataServiceResponse<SCJournalTransaction>> UpdateSCJournalTransaction(SCJournalTransactionSaveDataRequest request)
        {
            ThrowIf.Null(request, "request");

            SCJournalTransaction tran = request.Transaction;

            SqlQuery query = new SqlQuery();

            query.QueryString = "UPDATE ext.SCJOURNALTRANS SET STATUS = @STATUS, QTYADJUST = @QTYADJUST, QTYENTRY = @QTYENTRY, QTYCOUNT = @QTYCOUNT, COUNTDATE = @COUNTDATE, COMMENT = @COMMENT WHERE (ID = @ID) ";

            query.Parameters["@ID"] = tran.Id;
            query.Parameters["@STATUS"] = tran.StatusValue;
            query.Parameters["@QTYADJUST"] = tran.QtyAdjust;
            query.Parameters["@QTYENTRY"] = tran.QtyEntry;
            query.Parameters["@QTYCOUNT"] = tran.QtyCount;
            query.Parameters["@COUNTDATE"] = tran.CountedDate;
            query.Parameters["@COMMENT"] = tran.Comment;

            using (DatabaseContext databaseContext = new DatabaseContext(request.RequestContext))
            {
                await databaseContext.ExecuteNonQueryAsync(query).ConfigureAwait(false);
            }

            return new SingleEntityDataServiceResponse<SCJournalTransaction>(tran);
        }

        private async Task<EntityDataServiceResponse<SCJournalTransaction>> SearchSCJournalTransactions(SCJournalTransactionSearchDataRequest request)
        {
            ThrowIf.Null(request, "request");
            ThrowIf.Null(request.QueryResultSettings, "request.QueryResultSettings");

            SqlPagedQuery query = new SqlPagedQuery(request.QueryResultSettings)
            {
                DatabaseSchema = "ext",
                From = "SCJOURNALTRANS",
                Where = "(JOURNALID = @JOURNALID)"
            };

            query.Parameters["@JOURNALID"] = request.JournalId;

            if (request.Adgustment)
            {
                query.Where += " AND (COUNTDATE IS NOT NULL)";
            }

            if (!String.IsNullOrEmpty(request.Keyword))
            {
                query.Where += " AND ((ITEMID LIKE @SearchText) OR (ITEMNAME LIKE @SearchText))";
                if (request.Keyword.Contains("%"))
                    query.Parameters["@SearchText"] = request.Keyword;
                else
                    query.Parameters["@SearchText"] = '%' + request.Keyword + '%';
            }

            using (DatabaseContext databaseContext = new DatabaseContext(request.RequestContext))
            {
                return new EntityDataServiceResponse<SCJournalTransaction>(await databaseContext.ReadEntityAsync<SCJournalTransaction>(query).ConfigureAwait(false));
            }
        }

        private async Task<SingleEntityDataServiceResponse<decimal>> SalesSCJournalTransaction(SCJournalTransactionSalesDataRequest request)
        {
            ThrowIf.Null(request, "request");

            SqlQuery query = new SqlQuery();

            query.QueryString = "SELECT SUM(QTY) FROM ax.RETAILTRANSACTIONSALESTRANS " +
                    "WHERE INVENTLOCATIONID = @INVENTLOCATIONID AND ITEMID = @ITEMID AND VARIANTID = @VARIANTID AND INVENTBATCHID = @INVENTBATCHID AND INVENTSERIALID = @INVENTSERIALID " +
                        "AND ((TRANSDATE = @TRANSDATE AND TRANSTIME > @TRANSTIME) OR (TRANSDATE > @TRANSDATE))";

            query.Parameters["@INVENTLOCATIONID"] = request.InventLocationId;
            query.Parameters["@ITEMID"] = request.ItemId;
            query.Parameters["@VARIANTID"] = request.VariantId;
            query.Parameters["@INVENTBATCHID"] = request.InventBatchId;
            query.Parameters["@INVENTSERIALID"] = request.InventSerialId;
            query.Parameters["@TRANSDATE"] = request.TranDate;
            query.Parameters["@TRANSTIME"] = request.TranTime;

            decimal qty = Decimal.Zero;

            using (DatabaseContext databaseContext = new DatabaseContext(request.RequestContext))
            {
                object value = await databaseContext.ExecuteScalarAsync<object>(query).ConfigureAwait(false);
                if (value != null)
                    qty = -Convert.ToDecimal(value);
            }

            return new SingleEntityDataServiceResponse<decimal>(qty);
        }
      

        // Entry
        private async Task<SingleEntityDataServiceResponse<SCJournalEntry>> GetSCJournalEntry(SCJournalEntryGetDataRequest request)
        {
            ThrowIf.Null(request, "request");

            SqlPagedQuery query = new SqlPagedQuery(QueryResultSettings.SingleRecord)
            {
                DatabaseSchema = "ext",
                From = "SCJOURNALENTRY",
                Where = "(ID = @ID)"
            };

            query.Parameters["@ID"] = request.RecordId;

            using (DatabaseContext databaseContext = new DatabaseContext(request.RequestContext))
            {
                return new SingleEntityDataServiceResponse<SCJournalEntry>((await databaseContext.ReadEntityAsync<SCJournalEntry>(query).ConfigureAwait(false)).FirstOrDefault());
            }
        }

        private async Task<NullResponse> DeleteSCJournalEntry(SCJournalEntryDeleteDataRequest request)
        {
            ThrowIf.Null(request, "request");

            SqlQuery query = new SqlQuery();

            query.QueryString = "DELETE FROM ext.SCJOURNALENTRY WHERE (ID = @ID)";

            query.Parameters["@ID"] = request.RecordId;

            using (DatabaseContext databaseContext = new DatabaseContext(request.RequestContext))
            {
                await databaseContext.ExecuteNonQueryAsync(query).ConfigureAwait(false);
            }

            return NullResponse.Instance; //   return new NullResponse();
        }

        private async Task<EntityDataServiceResponse<SCJournalEntry>> SearchSCJournalEntries(SCJournalEntrySearchDataRequest request)
        {
            ThrowIf.Null(request, "request");
            ThrowIf.Null(request.QueryResultSettings, "request.QueryResultSettings");

            SqlPagedQuery query = new SqlPagedQuery(request.QueryResultSettings)
            {
                DatabaseSchema = "ext",
                From = "SCJOURNALENTRY"
            };

            if (request.TransactionId != 0)
            {
                query.Where = "(TRANSID = @ID)";
                query.Parameters["@ID"] = request.TransactionId;
            }
            else
            {
                query.Where = "(JOURNALID = @ID)";
                query.Parameters["@ID"] = request.JournalId;
            }


            if (!String.IsNullOrEmpty(request.Keyword))
            {
                query.Where += " AND (ITEMID LIKE @SearchText)";
                if (request.Keyword.Contains("%"))
                    query.Parameters["@SearchText"] = request.Keyword;
                else
                    query.Parameters["@SearchText"] = '%' + request.Keyword + '%';
            }

            using (DatabaseContext databaseContext = new DatabaseContext(request.RequestContext))
            {
                return new EntityDataServiceResponse<SCJournalEntry>(await databaseContext.ReadEntityAsync<SCJournalEntry>(query).ConfigureAwait(false));
            }
        }

        // Buffer

        private async Task<SCJournalBufferCalcDataResponse> CalculateSCJournalBuffer(SCJournalBufferCalcDataRequest request)
        {
            ThrowIf.Null(request, "request");

            List<StockCountJournalBuffer> list = new List<StockCountJournalBuffer>();

            SqlQuery query = new SqlQuery();

            query.QueryString =
                "SELECT ITEMID, INVENTSIZEID, INVENTCOLORID, INVENTSTYLEID, CONFIGID, INVENTBATCHID, INVENTSERIALID, SUM(QTYCOUNT) AS QTYCOUNT, MIN(COUNTDATE) AS COUNTDATE " +
                "FROM[ext].[SCJOURNALENTRY] WHERE JOURNALID = @ID " +
                "GROUP BY ITEMID, INVENTSIZEID, INVENTCOLORID, INVENTSTYLEID, CONFIGID, INVENTBATCHID, INVENTSERIALID";

            query.Parameters["@ID"] = request.JournalId;

            using (DatabaseQuery databaseContext = new DatabaseQuery(request.RequestContext))
            {
                using (IDatabaseResult reader = await databaseContext.Execute(query).ConfigureAwait(false))
                {
                    while (await reader.ReadAsync().ConfigureAwait(false))
                    {
                        StockCountJournalBuffer entry = new StockCountJournalBuffer();
                        entry.ItemId = reader.GetValue<string>(0);
                        entry.InventSizeId = reader.GetValue<string>(1);
                        entry.InventColorId = reader.GetValue<string>(2);
                        entry.InventStyleId = reader.GetValue<string>(3);
                        entry.ConfigId = reader.GetValue<string>(4);
                        entry.InventBatchId = reader.GetValue<string>(5);
                        entry.InventSerialId = reader.GetValue<string>(6);

                        object value = reader.GetValue(7, typeof(decimal));
                        entry.QtyCount = value == null ? 0 : (decimal)value;

                        value = reader.GetValue(8, typeof(DateTime));
                        // entry.CountedDate = value == null ? DateTime.Now : (DateTime)value;
                        entry.CountedDate = value == null ? request.RequestContext.GetNowInChannelTimeZone().DateTime : (DateTime)value;

                        list.Add(entry);
                    }
                }
            }

            return new SCJournalBufferCalcDataResponse(list);
        }

        #region CommitSCJournalBuffer
        private async Task<NullResponse> CommitSCJournalBuffer(SCJournalBufferCommitDataRequest request)
        {
            ThrowIf.Null(request, "request");

            // MS
            // using (var databaseContext = new SqlServerDatabaseContext(request))
            // using (var transactionScope = new TransactionScope())

            using (DatabaseContext databaseContext = new DatabaseContext(request.RequestContext, DatabaseConnectionMode.IsPersisted))
            {
                // using (IDatabaseTransaction transaction = databaseContext.BeginTransaction())
                // {

                await CommitSCJournalReset(databaseContext, request.JournalId).ConfigureAwait(false);
                foreach (StockCountJournalBuffer entry in request.Entries)
                {
                    if (entry.Status == SCJournalEntryStatus.Counted)
                    {
                        if (entry.TransactionId == 0)
                            entry.TransactionId = await CommitSCJournalTransCreate(databaseContext, request.JournalId, entry).ConfigureAwait(false);
                        else
                            await CommitSCJournalTransUpdate(databaseContext, entry).ConfigureAwait(false);
                    }
                    await CommitSCJournalEntry(databaseContext, request.JournalId, entry).ConfigureAwait(false);
                }
                await CommitSCJournalCommit(databaseContext, request.JournalId).ConfigureAwait(false);

                //     transaction.Commit();
                // }
            }

            return NullResponse.Instance; //   return new NullResponse();
        }

        private async Task CommitSCJournalReset(DatabaseContext databaseContext, int journalId)
        {
            SqlQuery query = new SqlQuery();

            query.QueryString = "UPDATE ext.SCJOURNALENTRY SET STATUS = 0, TRANSID = 0 WHERE JOURNALID = @JOURNALID; " +
                "UPDATE ext.SCJOURNALTRANS SET QTYENTRY = NULL, QTYCOUNT = NULL, COUNTDATE = NULL WHERE JOURNALID = @JOURNALID";

            query.Parameters["@JOURNALID"] = journalId;

            await databaseContext.ExecuteNonQueryAsync(query).ConfigureAwait(false);
        }

        private async Task<int> CommitSCJournalTransCreate(DatabaseContext databaseContext, int journalId, StockCountJournalBuffer entry)
        {
            ThrowIf.Null(entry, "entry");

            SqlQuery query = new SqlQuery();

            query.QueryString =
            "INSERT INTO ext.SCJOURNALTRANS ( " +
               "JOURNALID, RECID, ITEMID, ITEMNAME, UNIT, INVENTDIMID, INVENTBATCHID, INVENTSERIALID, INVENTSIZEID, INVENTCOLORID, INVENTSTYLEID, CONFIGID, STATUS, QUANTITY, QTYENTRY, QTYCOUNT, COUNTDATE " +
            ") VALUES (" +
               "@JOURNALID, 0, @ITEMID, @ITEMNAME, @UNIT, '', @INVENTBATCHID, @INVENTSERIALID, @INVENTSIZEID, @INVENTCOLORID, @INVENTSTYLEID, @CONFIGID, @STATUS, 0, @QTYENTRY, @QTYCOUNT, @COUNTDATE " +
            "); " +
            "SELECT ID FROM ext.SCJOURNALTRANS WHERE (ID = SCOPE_IDENTITY())";

            query.Parameters["@JOURNALID"] = journalId;
            query.Parameters["@ITEMID"] = entry.ItemId ?? String.Empty;
            query.Parameters["@ITEMNAME"] = entry.ItemName ?? String.Empty;
            query.Parameters["@UNIT"] = entry.Unit ?? String.Empty;
            query.Parameters["@INVENTBATCHID"] = entry.InventBatchId ?? String.Empty;
            query.Parameters["@INVENTSERIALID"] = entry.InventSerialId ?? String.Empty;
            query.Parameters["@INVENTSIZEID"] = entry.InventSizeId ?? String.Empty;
            query.Parameters["@INVENTCOLORID"] = entry.InventColorId ?? String.Empty;
            query.Parameters["@INVENTSTYLEID"] = entry.InventStyleId ?? String.Empty;
            query.Parameters["@CONFIGID"] = entry.ConfigId ?? String.Empty;
            query.Parameters["@STATUS"] = (int)SCJournalTransStatus.Default;
            query.Parameters["@QTYENTRY"] = entry.QtyCount;
            query.Parameters["@QTYCOUNT"] = entry.QtyCount;
            query.Parameters["@COUNTDATE"] = entry.CountedDate;

            return await databaseContext.ExecuteScalarAsync<int>(query).ConfigureAwait(false);
        }

        private async Task CommitSCJournalTransUpdate(DatabaseContext databaseContext, StockCountJournalBuffer entry)
        {
            ThrowIf.Null(entry, "entry");

            SqlQuery query = new SqlQuery();

            query.QueryString = "UPDATE ext.SCJOURNALTRANS SET QTYENTRY = @QTYENTRY, QTYCOUNT = @QTYCOUNT, COUNTDATE = @COUNTDATE WHERE (ID = @ID) ";

            query.Parameters["@ID"] = entry.TransactionId;
            query.Parameters["@QTYENTRY"] = entry.QtyCount;
            query.Parameters["@QTYCOUNT"] = entry.QtyCount;
            query.Parameters["@COUNTDATE"] = entry.CountedDate;

            await databaseContext.ExecuteNonQueryAsync(query).ConfigureAwait(false);
        }

        private async Task CommitSCJournalEntry(DatabaseContext databaseContext, int journalId, StockCountJournalBuffer entry)
        {
            ThrowIf.Null(entry, "entry");

            SqlQuery query = new SqlQuery();

            query.QueryString = "UPDATE ext.SCJOURNALENTRY SET STATUS = @STATUS, TRANSID = @TRANSID " +
                "WHERE (JOURNALID = @JOURNALID) AND (ITEMID = @ITEMID) AND (INVENTSIZEID = @INVENTSIZEID) AND (INVENTCOLORID = @INVENTCOLORID) AND (INVENTSTYLEID = @INVENTSTYLEID) AND (CONFIGID = @CONFIGID) AND (INVENTBATCHID = @INVENTBATCHID) AND (INVENTSERIALID = @INVENTSERIALID) ";

            query.Parameters["@JOURNALID"] = journalId;
            query.Parameters["@ITEMID"] = entry.ItemId ?? String.Empty;
            query.Parameters["@INVENTSIZEID"] = entry.InventSizeId ?? String.Empty;
            query.Parameters["@INVENTCOLORID"] = entry.InventColorId ?? String.Empty;
            query.Parameters["@INVENTSTYLEID"] = entry.InventStyleId ?? String.Empty;
            query.Parameters["@CONFIGID"] = entry.ConfigId ?? String.Empty;
            query.Parameters["@INVENTBATCHID"] = entry.InventBatchId ?? String.Empty;
            query.Parameters["@INVENTSERIALID"] = entry.InventSerialId ?? String.Empty;
            query.Parameters["@STATUS"] = (int)entry.Status;
            query.Parameters["@TRANSID"] = entry.TransactionId;

            await databaseContext.ExecuteNonQueryAsync(query).ConfigureAwait(false);
        }

        private async Task CommitSCJournalCommit(DatabaseContext databaseContext, int journalId)
        {
            SqlQuery query = new SqlQuery();

            query.QueryString = "DELETE FROM ext.SCJOURNALTRANS WHERE JOURNALID = @JOURNALID AND RECID = 0 AND QTYENTRY IS NULL";

            query.Parameters["@JOURNALID"] = journalId;

            await databaseContext.ExecuteNonQueryAsync(query).ConfigureAwait(false);
        }
        #endregion
    }
}