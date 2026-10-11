namespace RR.Dynamics.Retail.Payment.NBE.Portable
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Dynamics.Retail.PaymentSDK.Portable;
    
    public class RetrievePaymentAcceptResultRequest : RequestBase
    {
        public static RetrievePaymentAcceptResultRequest ConvertFrom(Request request)
        {
            var acceptResultRequest = new RetrievePaymentAcceptResultRequest();
            var errors = new List<PaymentError>();
            acceptResultRequest.ReadBaseProperties(request, errors);
    
            if (errors.Count > 0)
            {
                throw new PaymentException(errors);
            }
    
            return acceptResultRequest;
        }
    }
}

