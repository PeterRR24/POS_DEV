namespace RR.Dynamics.Retail.Payment.NBE.Portable
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Dynamics.Retail.PaymentSDK.Portable;
    using Microsoft.Dynamics.Retail.PaymentSDK.Portable.Constants;
    
    public abstract class ResponseBase
    {
        public ResponseBase(string locale, string serviceAccountId, string connectorName)
        {
            this.Locale = locale;
            this.ServiceAccountId = serviceAccountId;
            this.ConnectorName = connectorName;
        }
    
        public string Locale { get; set; }
            
        public string ServiceAccountId { get; set; }
    
        public string ConnectorName { get; set; }
        // License >>
        internal DateTime? LicenseExpireDate { get; set; }
        // License <<
        protected void WriteBaseProperties(Response response)
        {
            if (response == null)
            {
                throw new ArgumentNullException("response");
            }
    
            response.Locale = this.Locale;
    
            var properties = new List<PaymentProperty>();
            if (response.Properties != null)
            {
                properties.AddRange(response.Properties);
            }
    
            properties.Add(new PaymentProperty(GenericNamespace.MerchantAccount, MerchantAccountProperties.ServiceAccountId, this.ServiceAccountId));
            properties.Add(new PaymentProperty(GenericNamespace.Connector, ConnectorProperties.ConnectorName, this.ConnectorName));
            response.Properties = properties.ToArray();
        }
    }
}

