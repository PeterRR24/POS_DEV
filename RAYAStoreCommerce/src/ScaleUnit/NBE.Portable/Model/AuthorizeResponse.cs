

namespace RR.Dynamics.Retail.Payment.NBE.Portable
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Dynamics.Retail.PaymentSDK.Portable;
    using Microsoft.Dynamics.Retail.PaymentSDK.Portable.Constants;
    using RR.Utility;

    public class AuthorizeResponse : ResponseBase
    {
        public AuthorizeResponse(string locale, string serviceAccountId, string connectorName)
            : base(locale, serviceAccountId, connectorName)
        {
        }

        public string CardType { get; set; }

        public bool? IsSwipe { get; set; }

        public string Last4Digit { get; set; }

        public string CardToken { get; set; }

        public string UniqueCardId { get; set; }

        public string VoiceAuthorizationCode { get; set; }

        public decimal? CashBackAmount { get; set; }

        public string AccountType { get; set; }

        public string ProviderTransactionId { get; set; }

        public string ApprovalCode { get; set; }

        public string ResponseCode { get; set; }

        public decimal? ApprovedAmount { get; set; }

        public string CurrencyCode { get; set; }

        public string AuthorizationResult { get; set; }

        public string ProviderMessage { get; set; }

        public string AVSResult { get; set; }

        public string AVSDetail { get; set; }

        public string CVV2Result { get; set; }

        public decimal? AvailableBalance { get; set; }

        public string TransactionType { get; set; }

        public DateTime? TransactionDateTime { get; set; }
        // RR >>
        internal string ExternalReceipt { get; set; }
        // RR <<

        public static Response ConvertTo(AuthorizeResponse authorizeResponse)
        {
            var response = new Response();
            authorizeResponse.WriteBaseProperties(response);

            var properties = new List<PaymentProperty>();
            if (response.Properties != null)
            {
                properties.AddRange(response.Properties);
            }

            var authorizationRespnseProperties = new List<PaymentProperty>();
            PaymentUtilities.AddPropertyIfPresent(authorizationRespnseProperties, GenericNamespace.AuthorizationResponse, AuthorizationResponseProperties.CardType, authorizeResponse.CardType);
            PaymentUtilities.AddPropertyIfPresent(authorizationRespnseProperties, GenericNamespace.AuthorizationResponse, AuthorizationResponseProperties.IsSwiped, authorizeResponse.IsSwipe);
            PaymentUtilities.AddPropertyIfPresent(authorizationRespnseProperties, GenericNamespace.AuthorizationResponse, AuthorizationResponseProperties.Last4Digits, authorizeResponse.Last4Digit);
            PaymentUtilities.AddPropertyIfPresent(authorizationRespnseProperties, GenericNamespace.AuthorizationResponse, AuthorizationResponseProperties.CardToken, authorizeResponse.CardToken);
            PaymentUtilities.AddPropertyIfPresent(authorizationRespnseProperties, GenericNamespace.AuthorizationResponse, AuthorizationResponseProperties.UniqueCardId, authorizeResponse.UniqueCardId);
            PaymentUtilities.AddPropertyIfPresent(authorizationRespnseProperties, GenericNamespace.AuthorizationResponse, AuthorizationResponseProperties.VoiceAuthorizationCode, authorizeResponse.VoiceAuthorizationCode);
            PaymentUtilities.AddPropertyIfPresent(authorizationRespnseProperties, GenericNamespace.AuthorizationResponse, AuthorizationResponseProperties.CashBackAmount, authorizeResponse.CashBackAmount);
            PaymentUtilities.AddPropertyIfPresent(authorizationRespnseProperties, GenericNamespace.AuthorizationResponse, AuthorizationResponseProperties.AccountType, authorizeResponse.AccountType);
            PaymentUtilities.AddPropertyIfPresent(authorizationRespnseProperties, GenericNamespace.AuthorizationResponse, AuthorizationResponseProperties.ProviderTransactionId, authorizeResponse.ProviderTransactionId);
            PaymentUtilities.AddPropertyIfPresent(authorizationRespnseProperties, GenericNamespace.AuthorizationResponse, AuthorizationResponseProperties.ApprovalCode, authorizeResponse.ApprovalCode);
            PaymentUtilities.AddPropertyIfPresent(authorizationRespnseProperties, GenericNamespace.AuthorizationResponse, AuthorizationResponseProperties.ResponseCode, authorizeResponse.ResponseCode);
            PaymentUtilities.AddPropertyIfPresent(authorizationRespnseProperties, GenericNamespace.AuthorizationResponse, AuthorizationResponseProperties.ApprovedAmount, authorizeResponse.ApprovedAmount);
            PaymentUtilities.AddPropertyIfPresent(authorizationRespnseProperties, GenericNamespace.AuthorizationResponse, AuthorizationResponseProperties.CurrencyCode, authorizeResponse.CurrencyCode);
            PaymentUtilities.AddPropertyIfPresent(authorizationRespnseProperties, GenericNamespace.AuthorizationResponse, AuthorizationResponseProperties.AuthorizationResult, authorizeResponse.AuthorizationResult);
            PaymentUtilities.AddPropertyIfPresent(authorizationRespnseProperties, GenericNamespace.AuthorizationResponse, AuthorizationResponseProperties.ProviderMessage, authorizeResponse.ProviderMessage);
            PaymentUtilities.AddPropertyIfPresent(authorizationRespnseProperties, GenericNamespace.AuthorizationResponse, AuthorizationResponseProperties.AVSResult, authorizeResponse.AVSResult);
            PaymentUtilities.AddPropertyIfPresent(authorizationRespnseProperties, GenericNamespace.AuthorizationResponse, AuthorizationResponseProperties.AVSDetail, authorizeResponse.AVSDetail);
            PaymentUtilities.AddPropertyIfPresent(authorizationRespnseProperties, GenericNamespace.AuthorizationResponse, AuthorizationResponseProperties.CVV2Result, authorizeResponse.CVV2Result);
            PaymentUtilities.AddPropertyIfPresent(authorizationRespnseProperties, GenericNamespace.AuthorizationResponse, AuthorizationResponseProperties.AvailableBalance, authorizeResponse.AvailableBalance);
            PaymentUtilities.AddPropertyIfPresent(authorizationRespnseProperties, GenericNamespace.AuthorizationResponse, AuthorizationResponseProperties.TransactionType, authorizeResponse.TransactionType);
            PaymentUtilities.AddPropertyIfPresent(authorizationRespnseProperties, GenericNamespace.AuthorizationResponse, AuthorizationResponseProperties.TransactionDateTime, authorizeResponse.TransactionDateTime);
            // RR >>
            PaymentUtilities.AddPropertyIfPresent(authorizationRespnseProperties, GenericNamespace.AuthorizationResponse, AuthorizationResponseProperties.ExternalReceipt, authorizeResponse.ExternalReceipt);
            // RR <<
            // License >>
            PaymentUtilities.AddPropertyIfPresent(authorizationRespnseProperties, LicenseProperties.Namespace, LicenseProperties.LicenseExpireDate, authorizeResponse.LicenseExpireDate);
            // License <<
            properties.Add(new PaymentProperty(GenericNamespace.AuthorizationResponse, AuthorizationResponseProperties.Properties, authorizationRespnseProperties.ToArray()));

            response.Properties = properties.ToArray();
            return response;
        }
    }
}

