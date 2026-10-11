using System;
using System.Collections.Generic;
using Microsoft.Dynamics.Retail.PaymentSDK.Portable;
using Microsoft.Dynamics.Retail.PaymentSDK.Portable.Constants;
using RR.Retail.Security;

namespace RR.Dynamics.Retail.Payment.Givex.Portable
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Dynamics.Retail.PaymentSDK.Portable;
    using Microsoft.Dynamics.Retail.PaymentSDK.Portable.Constants;

    using global::RR.Retail.Security;

    internal class LicenseResponse
    {
        internal LicenseResponse(string locale, string connectorName)
        {
            this.Locale = locale;
            this.ConnectorName = connectorName;
        }

        internal string Locale { get; set; }
        internal string ConnectorName { get; set; }

        // Account

        //internal string AccountId { get; set; }
        //internal string ChannelId { get; set; }
        //internal string CustomerCode { get; set; }
        internal string SerialNumber { get; set; }

        // License

        internal string LicenseResult { get; set; }
        internal string LicenseDetails { get; set; }

        // Product

        internal string ProductId { get; set; }
        internal string ProductCode { get; set; }
        internal string ProductName { get; set; }
        internal string ProductVersion { get; set; }

        // Address

        internal string Name { get; set; }
        internal string StreetAddress { get; set; }
        internal string StreetAddress2 { get; set; }
        internal string City { get; set; }
        internal string State { get; set; }
        internal string PostalCode { get; set; }
        internal string Country { get; set; }

        internal static Response ConvertTo(LicenseResponse licenseResponse)
        {
            if (licenseResponse == null)
            {
                throw new ArgumentNullException("licenseResponse");
            }

            List<PaymentProperty> properties = new List<PaymentProperty>();

            properties.Add(new PaymentProperty(GenericNamespace.Connector, ConnectorProperties.ConnectorName, licenseResponse.ConnectorName));

            // PaymentUtilities.AddPropertyIfPresent(properties, LicenseProperties.Namespace, LicenseProperties.AccountId, licenseResponse.AccountId);
            // PaymentUtilities.AddPropertyIfPresent(properties, LicenseProperties.Namespace, LicenseProperties.ChannelId, licenseResponse.ChannelId);
            //  PaymentUtilities.AddPropertyIfPresent(properties, LicenseProperties.Namespace, LicenseProperties.CustomerCode, licenseResponse.CustomerCode);
            PaymentUtilities.AddPropertyIfPresent(properties, LicenseProperties.Namespace, LicenseProperties.SerialNumber, licenseResponse.SerialNumber);

            PaymentUtilities.AddPropertyIfPresent(properties, LicenseProperties.Namespace, LicenseProperties.LicenseResult, licenseResponse.LicenseResult);
            PaymentUtilities.AddPropertyIfPresent(properties, LicenseProperties.Namespace, LicenseProperties.LicenseDetails, licenseResponse.LicenseDetails);

            PaymentUtilities.AddPropertyIfPresent(properties, LicenseProperties.Namespace, LicenseProperties.ProductId, licenseResponse.ProductId);
            //PaymentUtilities.AddPropertyIfPresent(properties, LicenseProperties.Namespace, LicenseProperties.ProductCode, licenseResponse.ProductCode);
            PaymentUtilities.AddPropertyIfPresent(properties, LicenseProperties.Namespace, LicenseProperties.ProductName, licenseResponse.ProductName);
            PaymentUtilities.AddPropertyIfPresent(properties, LicenseProperties.Namespace, LicenseProperties.ProductVersion, licenseResponse.ProductVersion);

            PaymentUtilities.AddPropertyIfPresent(properties, GenericNamespace.PaymentCard, PaymentCardProperties.Name, licenseResponse.Name);
            PaymentUtilities.AddPropertyIfPresent(properties, GenericNamespace.PaymentCard, PaymentCardProperties.StreetAddress, licenseResponse.StreetAddress);
            PaymentUtilities.AddPropertyIfPresent(properties, GenericNamespace.PaymentCard, PaymentCardProperties.StreetAddress2, licenseResponse.StreetAddress2);
            PaymentUtilities.AddPropertyIfPresent(properties, GenericNamespace.PaymentCard, PaymentCardProperties.City, licenseResponse.City);
            PaymentUtilities.AddPropertyIfPresent(properties, GenericNamespace.PaymentCard, PaymentCardProperties.State, licenseResponse.State);
            PaymentUtilities.AddPropertyIfPresent(properties, GenericNamespace.PaymentCard, PaymentCardProperties.PostalCode, licenseResponse.PostalCode);
            PaymentUtilities.AddPropertyIfPresent(properties, GenericNamespace.PaymentCard, PaymentCardProperties.Country, licenseResponse.Country);

            Response response = new Response();
            response.Locale = licenseResponse.Locale;
            response.Properties = properties.ToArray();

            return response;
        }

        internal void Copy(LicenseRequest request)
        {
            if (request == null)
                return;
            this.SerialNumber = request.SerialNumber;

            this.Name = request.Name;
            this.StreetAddress = request.StreetAddress;
            this.StreetAddress2 = request.StreetAddress2;
            this.City = request.City;
            this.State = request.State;
            this.PostalCode = request.PostalCode;
            this.Country = request.Country;
        }
    }
}

