namespace RR.Dynamics.Retail.Payment.NBE.Portable
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Dynamics.Retail.PaymentSDK.Portable;
    using Microsoft.Dynamics.Retail.PaymentSDK.Portable.Constants;
    
    public class ValidateMerchantAccountRequest : RequestBase
    {
        public static ValidateMerchantAccountRequest ConvertFrom(Request request)
        {
            var validateRequest = new ValidateMerchantAccountRequest();
            var errors = new List<PaymentError>();
            validateRequest.ReadBaseProperties(request, errors);
    
            foreach (PaymentProperty property in request.Properties)
            {
                if (property.Namespace != GenericNamespace.MerchantAccount && property.Namespace != GenericNamespace.TransactionData)
                {
                    errors.Add(new PaymentError(ErrorCode.InvalidMerchantProperty, string.Format("Invalid namespace {0} for property {1}", property.Namespace, property.Name)));
                }
                else if (!NBEMerchantAccountProperty.ArrayList.Contains(property.Name))
                {
                    errors.Add(new PaymentError(ErrorCode.InvalidMerchantProperty, string.Format("Invalid property {0}", property.Name)));
                }
            }
    
            if (errors.Count > 0)
            {
                throw new PaymentException(errors);
            }
    
            return validateRequest;
        }
    }
}

