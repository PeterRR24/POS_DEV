namespace RR.Dynamics.Retail.Payment.NBE.Portable
{
    using Microsoft.Dynamics.Retail.PaymentSDK.Portable;
    
    public class ValidateMerchantAccountResponse : ResponseBase
    {
        public ValidateMerchantAccountResponse(string locale, string serviceAccountId, string connectorName)
            : base(locale, serviceAccountId, connectorName)
        {
        }
    
        public static Response ConvertTo(ValidateMerchantAccountResponse validateResponse)
        {
            var response = new Response();
            validateResponse.WriteBaseProperties(response);
            return response;
        }
    }
}

