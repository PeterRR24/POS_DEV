
namespace RR.Dynamics.Retail.Payment.NBE.Portable
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Dynamics.Retail.PaymentSDK.Portable;
    using Microsoft.Dynamics.Retail.PaymentSDK.Portable.Constants;
    using Newtonsoft.Json;

    public abstract class RequestBase
    {
        public RequestBase()
            : base()
        {
        }

        internal string Locale { get; set; }

        internal string AssemblyName { get; set; }

        internal string ConnectorName { get; set; }

        internal string ServiceAccountId { get; set; }

        internal string SupportedCurrencies { get; set; }

        internal string SupportedTenderTypes { get; set; }

        internal string IndustryType { get; set; }

        internal bool? IsTestMode { get; set; }

        internal string UniqueKey { get; set; }
        public string TransactionMode { get; set; }

        // License >>
        internal string AccountId { get { return this.UniqueKey; } }

        internal string SerialNumber { get; set; }
        internal string OldSerialNumber { get; set; }
        internal string ChannelId { get; set; }
        // License <<

        public static string PaymentAcceptWebUrl = @"http://localhost:3973";
        public const string GetPaymentAcceptPointRelativeUri = "/Payments/GetPaymentAcceptPoint";
        public List<KeyValuePair<string, string>> PaymentCardTypes { get; set; }

        public string TransactionId { get; set; }
        public string AuthTransId { get; set; }
        public string CardNumber { get; set; }
        internal string LogAppInsightsKey { get; set; }
        internal string LogEnable { get; set; }
        public string StaffId { get; set; }
        internal string CardMap { get; set; }

        public string Comport { get; set; }
        public string Baudrate { get; set; }
        public string Parity { get; set; }
        public string Stopbit { get; set; }
        public string Timeout { get; set; }
        public bool AllowPrintingProcessorReceipt { get; private set; }
        internal string ProcessorReceipt { get; private set; }
        public bool IsGetTransaction { get; set; }
        private string GetTransaction { get; set; }
        internal PaymentTransactionReferenceData TransactionReferenceData { get; set; }
        public string AuthCode { get; set; }
        protected void ReadBaseProperties(Request request, List<PaymentError> errors)
        {
            if (request == null)
            {
                throw new PaymentException(ErrorCode.InvalidRequest, "Request is null.");
            }

            if (string.IsNullOrWhiteSpace(request.Locale))
            {
                throw new PaymentException(ErrorCode.InvalidRequest, "Locale is null or whitespaces.");
            }
            else
            {
                this.Locale = request.Locale;
            }

            if (request.Properties == null || request.Properties.Length == 0)
            {
                throw new PaymentException(ErrorCode.InvalidRequest, "Request properties is null or empty.");
            }

            Hashtable properties = PaymentProperty.ConvertToHashtable(request.Properties);
            this.AssemblyName = PaymentUtilities.GetPropertyStringValue(
                properties,
                GenericNamespace.MerchantAccount,
                MerchantAccountProperties.AssemblyName,
                errors,
                ErrorCode.InvalidMerchantProperty);
            this.ServiceAccountId = PaymentUtilities.GetPropertyStringValue(
                properties,
                GenericNamespace.MerchantAccount,
                MerchantAccountProperties.ServiceAccountId,
                errors,
                ErrorCode.InvalidMerchantProperty);
            this.SupportedCurrencies = PaymentUtilities.GetPropertyStringValue(
                properties,
                GenericNamespace.MerchantAccount,
                MerchantAccountProperties.SupportedCurrencies,
                errors,
                ErrorCode.InvalidMerchantProperty);
            this.SupportedTenderTypes = PaymentUtilities.GetPropertyStringValue(
                properties,
                GenericNamespace.MerchantAccount,
                MerchantAccountProperties.SupportedTenderTypes,
                errors,
                ErrorCode.InvalidMerchantProperty);
            this.IndustryType = PaymentUtilities.GetPropertyStringValue(
                properties,
                GenericNamespace.TransactionData,
                TransactionDataProperties.IndustryType);
            this.IsTestMode = PaymentUtilities.GetPropertyBooleanValue(
                properties,
                GenericNamespace.TransactionData,
                TransactionDataProperties.IsTestMode);
            this.UniqueKey = PaymentUtilities.GetPropertyStringValue(
                properties,
                GenericNamespace.MerchantAccount,
                NBEMerchantAccountProperty.UniqueKey,
                errors,
                ErrorCode.InvalidMerchantProperty);
            this.TransactionMode = PaymentUtilities.GetPropertyStringValue(
               properties,
               GenericNamespace.MerchantAccount,
               NBEMerchantAccountProperty.TransactionMode,
               errors,
               ErrorCode.InvalidMerchantProperty);
            this.ConnectorName = PaymentUtilities.GetPropertyStringValue(
              properties,
              GenericNamespace.MerchantAccount,
              NBEMerchantAccountProperty.ConnectorName,
              errors,
              ErrorCode.InvalidMerchantProperty);
            this.ConnectorName = PaymentUtilities.GetPropertyStringValue(
              properties,
              GenericNamespace.MerchantAccount,
              NBEMerchantAccountProperty.ConnectorName,
              errors,
              ErrorCode.InvalidMerchantProperty);
            this.CardMap = PaymentUtilities.GetPropertyStringValue(
              properties,
              GenericNamespace.MerchantAccount,
              NBEMerchantAccountProperty.CardMap);
             this.LogEnable = PaymentUtilities.GetPropertyStringValue(
             properties,
             GenericNamespace.TransactionData,
             "IsLogEnabled");

            if (!string.IsNullOrEmpty(this.LogEnable) && this.LogEnable != "2" && this.LogEnable != "0")
            {
                this.LogAppInsightsKey = PaymentUtilities.GetPropertyStringValue(
                    properties,
                    GenericNamespace.TransactionData,
                    "LogAppInsightsKey");
            }

            if (string.IsNullOrEmpty(this.LogAppInsightsKey))
                this.LogAppInsightsKey = PaymentUtilities.GetPropertyStringValue(properties, "Extensions", "LogAppInsightsKey");

            this.TransactionId = PaymentUtilities.GetPropertyStringValue(
               properties,
               GenericNamespace.TransactionData, "EFTTransactionId");
            this.AuthTransId = PaymentUtilities.GetPropertyStringValue(
               properties,
               GenericNamespace.TransactionData, "ProviderTransactionId");
            this.AuthCode = PaymentUtilities.GetPropertyStringValue(
              properties,
              GenericNamespace.TransactionData, "AuthCode");
            this.CardNumber = PaymentUtilities.GetPropertyStringValue(
               properties,
               GenericNamespace.TransactionData, "CardToken");
            this.StaffId = PaymentUtilities.GetPropertyStringValue(
              properties,
              GenericNamespace.TransactionData, "RRStaffId");
            this.ProcessorReceipt = PaymentUtilities.GetPropertyStringValue(
             properties,
             GenericNamespace.TransactionData, "AllowPrintingProcessorReceipt");

            this.AllowPrintingProcessorReceipt = false;
            if (!string.IsNullOrEmpty(ProcessorReceipt))
            {
                AllowPrintingProcessorReceipt = Convert.ToBoolean(Convert.ToInt32(ProcessorReceipt));
            }

            this.GetTransaction  = PaymentUtilities.GetPropertyStringValue(
            properties,
            GenericNamespace.TransactionData, "GetTransaction");

            this.IsGetTransaction = this.GetTransaction == "true";

            string s_value = PaymentUtilities.GetPropertyStringValue(
            properties,
            GenericNamespace.TransactionData, "TransactionReferenceData");

            this.Comport = PaymentUtilities.GetPropertyStringValue(
               properties,
               GenericNamespace.MerchantAccount,
               NBEMerchantAccountProperty.Comport,
               errors,
               ErrorCode.InvalidMerchantProperty);
            this.Baudrate = PaymentUtilities.GetPropertyStringValue(
                properties,
                GenericNamespace.MerchantAccount,
                NBEMerchantAccountProperty.Baudrate);
            this.Parity = PaymentUtilities.GetPropertyStringValue(
                properties,
                GenericNamespace.MerchantAccount,
                NBEMerchantAccountProperty.parity);
            this.Stopbit = PaymentUtilities.GetPropertyStringValue(
                properties,
                GenericNamespace.MerchantAccount,
                NBEMerchantAccountProperty.Stopbit);
            this.Timeout = PaymentUtilities.GetPropertyStringValue(
                properties,
                GenericNamespace.MerchantAccount,
                NBEMerchantAccountProperty.Timeout.ToString());

            if (!string.IsNullOrEmpty(s_value))
                TransactionReferenceData = JsonConvert.DeserializeObject<PaymentTransactionReferenceData>(s_value);

            if (this.CardMap != string.Empty)
            {
                this.PaymentCardTypes = new List<KeyValuePair<string, string>>();
                string[] ctypes = this.CardMap.Split(';');
                for (int i = 0; i < ctypes.Length; i++)
                {
                    if (!string.IsNullOrWhiteSpace(ctypes[i]))
                    {
                        this.PaymentCardTypes.Add(new KeyValuePair<string, string>(ctypes[i].Split(':')[0], ctypes[i].Split(':')[1]));
                    }
                }
            }
        }
    }
}
