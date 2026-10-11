
namespace RR.Dynamics.Retail.Payment.NBE.Portable
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Dynamics.Retail.PaymentSDK.Portable;
    using Microsoft.Dynamics.Retail.PaymentSDK.Portable.Constants;
    
    public class GetPaymentAcceptPointRequest : RequestBase
    {
        public static GetPaymentAcceptPointRequest ConvertFrom(Request request)
        {
            var acceptPointRequest = new GetPaymentAcceptPointRequest();
            var errors = new List<PaymentError>();
            acceptPointRequest.ReadBaseProperties(request, errors);
    
            if (errors.Count > 0)
            {
                throw new PaymentException(errors);
            }
    
            return acceptPointRequest;
        }
    }
}

