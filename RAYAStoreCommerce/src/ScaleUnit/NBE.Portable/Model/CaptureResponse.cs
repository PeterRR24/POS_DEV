
namespace RR.Dynamics.Retail.Payment.NBE.Portable
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Dynamics.Retail.PaymentSDK.Portable;
    using Microsoft.Dynamics.Retail.PaymentSDK.Portable.Constants;

    public class CaptureResponse : ResponseBase
    {
        public CaptureResponse(string locale, string serviceAccountId, string connectorName)
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

        public string CurrencyCode { get; set; }

        public string CaptureResult { get; set; }

        public string ProviderMessage { get; set; }

        public string TransactionType { get; set; }

        public DateTime? TransactionDateTime { get; set; }

        public static Response ConvertTo(CaptureResponse captureResponse)
        {
            var response = new Response();
            captureResponse.WriteBaseProperties(response);

            var properties = new List<PaymentProperty>();
            if (response.Properties != null)
            {
                properties.AddRange(response.Properties);
            }

            var captureRespnseProperties = new List<PaymentProperty>();
            PaymentUtilities.AddPropertyIfPresent(captureRespnseProperties, GenericNamespace.CaptureResponse, CaptureResponseProperties.CardType, captureResponse.CardType);
            PaymentUtilities.AddPropertyIfPresent(captureRespnseProperties, GenericNamespace.CaptureResponse, CaptureResponseProperties.Last4Digits, captureResponse.Last4Digit);
            PaymentUtilities.AddPropertyIfPresent(captureRespnseProperties, GenericNamespace.CaptureResponse, CaptureResponseProperties.CardToken, captureResponse.CardToken);
            PaymentUtilities.AddPropertyIfPresent(captureRespnseProperties, GenericNamespace.CaptureResponse, CaptureResponseProperties.UniqueCardId, captureResponse.UniqueCardId);
            PaymentUtilities.AddPropertyIfPresent(captureRespnseProperties, GenericNamespace.CaptureResponse, CaptureResponseProperties.ProviderTransactionId, captureResponse.ProviderTransactionId);
            PaymentUtilities.AddPropertyIfPresent(captureRespnseProperties, GenericNamespace.CaptureResponse, CaptureResponseProperties.ApprovalCode, captureResponse.ApprovalCode);
            PaymentUtilities.AddPropertyIfPresent(captureRespnseProperties, GenericNamespace.CaptureResponse, CaptureResponseProperties.ResponseCode, captureResponse.ResponseCode);
            PaymentUtilities.AddPropertyIfPresent(captureRespnseProperties, GenericNamespace.CaptureResponse, CaptureResponseProperties.CurrencyCode, captureResponse.CurrencyCode);
            PaymentUtilities.AddPropertyIfPresent(captureRespnseProperties, GenericNamespace.CaptureResponse, CaptureResponseProperties.CaptureResult, captureResponse.CaptureResult);
            PaymentUtilities.AddPropertyIfPresent(captureRespnseProperties, GenericNamespace.CaptureResponse, CaptureResponseProperties.ProviderMessage, captureResponse.ProviderMessage);
            PaymentUtilities.AddPropertyIfPresent(captureRespnseProperties, GenericNamespace.CaptureResponse, CaptureResponseProperties.TransactionType, captureResponse.TransactionType);
            PaymentUtilities.AddPropertyIfPresent(captureRespnseProperties, GenericNamespace.CaptureResponse, CaptureResponseProperties.TransactionDateTime, captureResponse.TransactionDateTime);
            properties.Add(new PaymentProperty(GenericNamespace.CaptureResponse, CaptureResponseProperties.Properties, captureRespnseProperties.ToArray()));

            response.Properties = properties.ToArray();
            return response;
        }
    }
}

