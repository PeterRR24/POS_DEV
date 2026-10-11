namespace RR.Dynamics.Retail.Payment.NBE.Portable
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Dynamics.Retail.PaymentSDK.Portable;
    using Microsoft.Dynamics.Retail.PaymentSDK.Portable.Constants;
    using RR.Utility;

    public class RefundResponse : ResponseBase
    {
        public RefundResponse(string locale, string serviceAccountId, string connectorName)
            : base(locale, serviceAccountId, connectorName)
        {
        }

        public string CardType { get; set; }

        public string Last4Digit { get; set; }

        public string CardToken { get; set; }

        public string UniqueCardId { get; set; }

        public string ProviderTransactionId { get; set; }

        public string ApprovalCode { get; set; }

        public string ResponseCode { get; set; }

        public decimal? ApprovedAmount { get; set; }

        public string CurrencyCode { get; set; }

        public string RefundResult { get; set; }

        public string ProviderMessage { get; set; }

        public string TransactionType { get; set; }

        public DateTime? TransactionDateTime { get; set; }
        // RR >>
        internal string ExternalReceipt { get; set; }
        // RR <<
        public static Response ConvertTo(RefundResponse refundResponse)
        {
            var response = new Response();
            refundResponse.WriteBaseProperties(response);

            var properties = new List<PaymentProperty>();
            if (response.Properties != null)
            {
                properties.AddRange(response.Properties);
            }

            var refundRespnseProperties = new List<PaymentProperty>();
            PaymentUtilities.AddPropertyIfPresent(refundRespnseProperties, GenericNamespace.RefundResponse, RefundResponseProperties.CardType, refundResponse.CardType);
            PaymentUtilities.AddPropertyIfPresent(refundRespnseProperties, GenericNamespace.RefundResponse, RefundResponseProperties.Last4Digits, refundResponse.Last4Digit);
            PaymentUtilities.AddPropertyIfPresent(refundRespnseProperties, GenericNamespace.RefundResponse, RefundResponseProperties.CardToken, refundResponse.CardToken);
            PaymentUtilities.AddPropertyIfPresent(refundRespnseProperties, GenericNamespace.RefundResponse, RefundResponseProperties.UniqueCardId, refundResponse.UniqueCardId);
            PaymentUtilities.AddPropertyIfPresent(refundRespnseProperties, GenericNamespace.RefundResponse, RefundResponseProperties.ProviderTransactionId, refundResponse.ProviderTransactionId);
            PaymentUtilities.AddPropertyIfPresent(refundRespnseProperties, GenericNamespace.RefundResponse, RefundResponseProperties.ApprovalCode, refundResponse.ApprovalCode);
            PaymentUtilities.AddPropertyIfPresent(refundRespnseProperties, GenericNamespace.RefundResponse, RefundResponseProperties.ResponseCode, refundResponse.ResponseCode);
            PaymentUtilities.AddPropertyIfPresent(refundRespnseProperties, GenericNamespace.RefundResponse, RefundResponseProperties.ApprovedAmount, refundResponse.ApprovedAmount);
            PaymentUtilities.AddPropertyIfPresent(refundRespnseProperties, GenericNamespace.RefundResponse, RefundResponseProperties.CurrencyCode, refundResponse.CurrencyCode);
            PaymentUtilities.AddPropertyIfPresent(refundRespnseProperties, GenericNamespace.RefundResponse, RefundResponseProperties.RefundResult, refundResponse.RefundResult);
            PaymentUtilities.AddPropertyIfPresent(refundRespnseProperties, GenericNamespace.RefundResponse, RefundResponseProperties.ProviderMessage, refundResponse.ProviderMessage);
            PaymentUtilities.AddPropertyIfPresent(refundRespnseProperties, GenericNamespace.RefundResponse, RefundResponseProperties.TransactionType, refundResponse.TransactionType);
            PaymentUtilities.AddPropertyIfPresent(refundRespnseProperties, GenericNamespace.RefundResponse, RefundResponseProperties.TransactionDateTime, refundResponse.TransactionDateTime);
            // License >>
            PaymentUtilities.AddPropertyIfPresent(refundRespnseProperties, LicenseProperties.Namespace, LicenseProperties.LicenseExpireDate, refundResponse.LicenseExpireDate);
            // License <<
            // RR >>
            PaymentUtilities.AddPropertyIfPresent(refundRespnseProperties, GenericNamespace.RefundResponse, RefundResponseProperties.ExternalReceipt, refundResponse.ExternalReceipt);
            // RR <<
            properties.Add(new PaymentProperty(GenericNamespace.RefundResponse, RefundResponseProperties.Properties, refundRespnseProperties.ToArray()));

            response.Properties = properties.ToArray();
            return response;
        }
    }
}

