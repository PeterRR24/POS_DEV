namespace RR.Dynamics.Retail.Payment.NBE.Portable
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Dynamics.Retail.PaymentSDK.Portable;
    using Microsoft.Dynamics.Retail.PaymentSDK.Portable.Constants;

    public class VoidResponse : ResponseBase
    {
        public VoidResponse(string locale, string serviceAccountId, string connectorName)
            : base(locale, serviceAccountId, connectorName)
        {
        }

        public string CardType { get; set; }

        public string Last4Digit { get; set; }

        public string UniqueCardId { get; set; }

        public string ProviderTransactionId { get; set; }

        public string ResponseCode { get; set; }

        public string CurrencyCode { get; set; }

        public string VoidResult { get; set; }

        public string ProviderMessage { get; set; }

        public string TransactionType { get; set; }

        public DateTime? TransactionDateTime { get; set; }
        // RR >>
        internal string ExternalReceipt { get; set; }
        // RR <<

        public static Response ConvertTo(VoidResponse voidResponse)
        {
            var response = new Response();
            voidResponse.WriteBaseProperties(response);

            var properties = new List<PaymentProperty>();
            if (response.Properties != null)
            {
                properties.AddRange(response.Properties);
            }

            var voidRespnseProperties = new List<PaymentProperty>();
            PaymentUtilities.AddPropertyIfPresent(voidRespnseProperties, GenericNamespace.VoidResponse, VoidResponseProperties.CardType, voidResponse.CardType);
            PaymentUtilities.AddPropertyIfPresent(voidRespnseProperties, GenericNamespace.VoidResponse, VoidResponseProperties.Last4Digits, voidResponse.Last4Digit);
            PaymentUtilities.AddPropertyIfPresent(voidRespnseProperties, GenericNamespace.VoidResponse, VoidResponseProperties.UniqueCardId, voidResponse.UniqueCardId);
            PaymentUtilities.AddPropertyIfPresent(voidRespnseProperties, GenericNamespace.VoidResponse, VoidResponseProperties.ProviderTransactionId, voidResponse.ProviderTransactionId);
            PaymentUtilities.AddPropertyIfPresent(voidRespnseProperties, GenericNamespace.VoidResponse, VoidResponseProperties.ResponseCode, voidResponse.ResponseCode);
            PaymentUtilities.AddPropertyIfPresent(voidRespnseProperties, GenericNamespace.VoidResponse, VoidResponseProperties.CurrencyCode, voidResponse.CurrencyCode);
            PaymentUtilities.AddPropertyIfPresent(voidRespnseProperties, GenericNamespace.VoidResponse, VoidResponseProperties.VoidResult, voidResponse.VoidResult);
            PaymentUtilities.AddPropertyIfPresent(voidRespnseProperties, GenericNamespace.VoidResponse, VoidResponseProperties.ProviderMessage, voidResponse.ProviderMessage);
            PaymentUtilities.AddPropertyIfPresent(voidRespnseProperties, GenericNamespace.VoidResponse, VoidResponseProperties.TransactionType, voidResponse.TransactionType);
            PaymentUtilities.AddPropertyIfPresent(voidRespnseProperties, GenericNamespace.VoidResponse, VoidResponseProperties.TransactionDateTime, voidResponse.TransactionDateTime);
            // RR >>
            PaymentUtilities.AddPropertyIfPresent(voidRespnseProperties, GenericNamespace.VoidResponse, VoidResponseProperties.ExternalReceipt, voidResponse.ExternalReceipt);
            // RR <<

            properties.Add(new PaymentProperty(GenericNamespace.VoidResponse, VoidResponseProperties.Properties, voidRespnseProperties.ToArray()));

            response.Properties = properties.ToArray();
            return response;
        }
    }
}

