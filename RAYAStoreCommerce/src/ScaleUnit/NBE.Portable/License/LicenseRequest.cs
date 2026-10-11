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

    internal class LicenseRequest
    {
        internal LicenseRequest()
        {
        }

        internal string Locale { get; set; }

        // Account

        internal string AccountId { get; set; }

        internal string TenantId { get; set; }

        internal string ProductId { get; set; }

        internal string SerialNumber { get; set; }

        internal string SerialMaster { get; set; }

        // Address

        internal string Name { get; set; }

        internal string StreetAddress { get; set; }

        internal string StreetAddress2 { get; set; }

        internal string City { get; set; }

        internal string State { get; set; }

        internal string PostalCode { get; set; }

        internal string Country { get; set; }

        internal static LicenseRequest ConvertFrom(Request request, bool forCreate)
        {
            LicenseRequest licenseRequest = new LicenseRequest();
            List<PaymentError> errors = new List<PaymentError>();

            if (request == null)
            {
                throw new SampleException(ErrorCode.InvalidRequest, "Request is null.");
            }

            if (request.Properties == null || request.Properties.Length == 0)
            {
                throw new SampleException(ErrorCode.InvalidRequest, "Request properties is null or empty.");
            }

            if (String.IsNullOrWhiteSpace(request.Locale))
            {
                throw new SampleException(ErrorCode.InvalidRequest, "Locale is null or whitespaces.");
            }
            else
            {
                licenseRequest.Locale = request.Locale;
            }

            Hashtable hashtable = PaymentProperty.ConvertToHashtable(request.Properties);

            licenseRequest.AccountId = PaymentUtilities.GetPropertyStringValue(
                hashtable,
                GenericNamespace.MerchantAccount,
                GivexMerchantAccountProperty.UniqueKey,
                errors,
                ErrorCode.InvalidMerchantProperty);

            if (forCreate)
            {
                licenseRequest.TenantId = PaymentUtilities.GetPropertyStringValue(
                    hashtable,
                    LicenseProperties.Namespace,
                    LicenseProperties.TenantId,
                    errors,
                    ErrorCode.InvalidRequest);

                licenseRequest.ProductId = PaymentUtilities.GetPropertyStringValue(
                    hashtable,
                    LicenseProperties.Namespace,
                    LicenseProperties.ProductId,
                    errors,
                    ErrorCode.InvalidRequest);

                licenseRequest.SerialMaster = PaymentUtilities.GetPropertyStringValue(
                    hashtable,
                    LicenseProperties.Namespace,
                    LicenseProperties.SerialNumber,
                    errors,
                    ErrorCode.InvalidRequest);
            }

            licenseRequest.SerialNumber = PaymentUtilities.GetPropertyStringValue(
                hashtable,
                GenericNamespace.MerchantAccount,
                LicenseProperties.SerialNumber);

            licenseRequest.Name = PaymentUtilities.GetPropertyStringValue(
                hashtable,
                GenericNamespace.PaymentCard,
                PaymentCardProperties.Name);
            licenseRequest.StreetAddress = PaymentUtilities.GetPropertyStringValue(
                hashtable,
                GenericNamespace.PaymentCard,
                PaymentCardProperties.StreetAddress);
            licenseRequest.StreetAddress2 = PaymentUtilities.GetPropertyStringValue(
                hashtable,
                GenericNamespace.PaymentCard,
                PaymentCardProperties.StreetAddress2);
            licenseRequest.City = PaymentUtilities.GetPropertyStringValue(
                hashtable,
                GenericNamespace.PaymentCard,
                PaymentCardProperties.City);
            licenseRequest.State = PaymentUtilities.GetPropertyStringValue(
                hashtable,
                GenericNamespace.PaymentCard,
                PaymentCardProperties.State);
            licenseRequest.PostalCode = PaymentUtilities.GetPropertyStringValue(
                hashtable,
                GenericNamespace.PaymentCard,
                PaymentCardProperties.PostalCode);
            licenseRequest.Country = PaymentUtilities.GetPropertyStringValue(
                hashtable,
                GenericNamespace.PaymentCard,
                PaymentCardProperties.Country);

            if (errors.Count > 0)
            {
                throw new SampleException(errors);
            }

            return licenseRequest;
        }
    }
}
