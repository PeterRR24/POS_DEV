namespace RR.Dynamics.Retail.Payment.NBE.Portable
{
    using System.Collections.Generic;
    using Microsoft.Dynamics.Retail.PaymentSDK.Portable;
    using Microsoft.Dynamics.Retail.PaymentSDK.Portable.Constants;

    public class GetPaymentAcceptPointResponse : ResponseBase
    {
        public GetPaymentAcceptPointResponse(string locale, string serviceAccountId, string connectorName)
            : base(locale, serviceAccountId, connectorName)
        {
        }

        public string PaymentAcceptUrl { get; set; }

        public string PaymentAcceptSubmitUrl { get; set; }

        public string PaymentAcceptMessageOrigin { get; set; }

        public static Response ConvertTo(GetPaymentAcceptPointResponse acceptPointResponse)
        {
            var response = new Response();
            acceptPointResponse.WriteBaseProperties(response);

            var properties = new List<PaymentProperty>();
            if (response.Properties != null)
            {
                properties.AddRange(response.Properties);
            }

            PaymentUtilities.AddPropertyIfPresent(properties, GenericNamespace.TransactionData, TransactionDataProperties.PaymentAcceptUrl, acceptPointResponse.PaymentAcceptUrl);
            PaymentUtilities.AddPropertyIfPresent(properties, GenericNamespace.TransactionData, TransactionDataProperties.PaymentAcceptSubmitUrl, acceptPointResponse.PaymentAcceptSubmitUrl);
            PaymentUtilities.AddPropertyIfPresent(properties, GenericNamespace.TransactionData, TransactionDataProperties.PaymentAcceptMessageOrigin, acceptPointResponse.PaymentAcceptMessageOrigin);
            //PaymentUtilities.AddPropertyIfPresent(properties, GenericNamespace.MerchantAccount , Shift4MerchantAccountProperty.APISerialNumber , req)

            response.Properties = properties.ToArray();
            return response;
        }
    }
}

