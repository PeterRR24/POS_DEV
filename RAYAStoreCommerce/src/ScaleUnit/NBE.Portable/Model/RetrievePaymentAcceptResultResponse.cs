namespace RR.Dynamics.Retail.Payment.NBE.Portable
{
    using Microsoft.Dynamics.Retail.PaymentSDK.Portable;
    
    public class RetrievePaymentAcceptResultResponse : ResponseBase
    {
        public RetrievePaymentAcceptResultResponse(string locale, string serviceAccountId, string connectorName)
            : base(locale, serviceAccountId, connectorName)
        {
        }
    
        public static Response ConvertTo(RetrievePaymentAcceptResultResponse acceptResultResponse)
        {
            var response = new Response();
            acceptResultResponse.WriteBaseProperties(response);
            return response;
        }
    }
}

