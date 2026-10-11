using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RR.Dynamics.Retail.Payment.NBE.Contract
{
    public enum EftTransactionStatus
    {
        UnProcessed = 0,
        Success = 1,
        Failure = 2,
        QueuedForBatch = 3,
        InProgress = 4,
        None = 5,
        Referral = 6,
        PartialAuthorization = 7,
        ImmediateCaptureRequired = 8,
        ImmediateCaptureFailed = 9,
        MACReturned = 10,
        Declined = 11,
    }

    public enum EftCvvResultCode
    {
        Success = 0,
        Failure = 1,
        IssuerNotRegistered = 2,
        NotProcessed = 3,
        Unknown = 4,
        None = 5,
    }

    public enum EftAvsResultCode
    {
        None = 0,
        Returned = 1,
        NotReturned = 2,
        VerificationNotSupported = 3,
        SystemUnavailable = 4,
        Match = 5,
        NotMatch = 6,
        IncompleteFormat = 7,
        InformationUnavailable = 8,
        NoDataFromIssuer = 9,
        Unknown = 10,
    }

    public enum EftAvsDetailCode
    {
        None = 0,
        AccountHolderName = 1,
        BillingAddress = 2,
        BillingPostalCode = 3,
        BillingAndPostalCode = 4
    }

    public class EftResponse
    {
        private EftErrorList _errors = new EftErrorList();

        public EftOperationStatus Status { get; set; }
        public EftTransactionStatus TransStatus { get; set; }
        public EftErrorList Errors { get { return _errors; } }

        public string TransactionId { get; set; }
        public string ResponseCode { get; set; }
        public string ProviderTransactionId { get; set; }     
        public decimal ApprovedAmount { get; set; }
        public decimal CardBalance { get; set; }
        public string CardExpiry { get; set; }

        public string RequestData { get; set; }
        public string ResponseData { get; set; }
        public string CardReference { get; set; }
        public string CardType { get; set; }
        public string AuthTransId { get; set; }
        public string TerminalId { get; set; }
        public string ReceiptText { get; set; }
        public string ReceiptTextXml { get; set; }
        public string MerchantId { get; set; }
        public string AuthCode { get; set; }
        
    }
}
