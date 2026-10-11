

namespace RR.Dynamics.Retail.Payment.NBE.Portable
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Dynamics.Retail.PaymentSDK.Portable;
    using Microsoft.Dynamics.Retail.PaymentSDK.Portable.Constants;
    using RR.Utility;

    public class AuthorizeRequest : RequestBase
    {
        public AuthorizeRequest()
            : base()
        {
        }

        public string CardType { get; set; }

        public bool? IsSwipe { get; set; }

        public string CardNumber { get; set; }

        public string Track1 { get; set; }

        public string Track2 { get; set; }

        public decimal? ExpirationYear { get; set; }

        public decimal? ExpirationMonth { get; set; }

        public string CardToken { get; set; }

        public string Last4Digit { get; set; }

        public string EncryptedPin { get; set; }

        public string AdditionalSecurityData { get; set; }

        public string CardVerificationValue { get; set; }

        public string VoiceAuthorizationCode { get; set; }

        public decimal? CashBackAmount { get; set; }

        public string Name { get; set; }

        public string StreetAddress { get; set; }

        public string StreetAddress2 { get; set; }

        public string City { get; set; }

        public string State { get; set; }

        public string PostalCode { get; set; }

        public string Country { get; set; }

        public string Phone { get; set; }

        public string AccountType { get; set; }

        public string UniqueCardId { get; set; }

        public decimal? Amount { get; set; }

        public string CurrencyCode { get; set; }

        public bool? SupportCardTokenization { get; set; }

        public bool? AllowPartialAuthorization { get; set; }

        public string AuthorizationProviderTransactionId { get; set; }

        public string PurchaseLevel { get; set; }

       

        public static AuthorizeRequest ConvertFrom(Request request)
        {
            var authorizeRequest = new AuthorizeRequest();
            var errors = new List<PaymentError>();
            authorizeRequest.ReadBaseProperties(request, errors);

            // Read card data
            Hashtable hashtable = PaymentProperty.ConvertToHashtable(request.Properties);
            authorizeRequest.CardType = PaymentUtilities.GetPropertyStringValue(
                hashtable,
                GenericNamespace.PaymentCard,
                PaymentCardProperties.CardType,
                errors,
                ErrorCode.InvalidRequest);
            if (authorizeRequest.CardType != null
                && !PaymentUtilities.ValidateCardType(authorizeRequest.SupportedTenderTypes, authorizeRequest.CardType))
            {
                errors.Add(new PaymentError(ErrorCode.CardTypeNotSupported, string.Format("Card type is not supported: {0}.", authorizeRequest.CardType)));
            }

            authorizeRequest.IsSwipe = PaymentUtilities.GetPropertyBooleanValue(
                hashtable,
                GenericNamespace.PaymentCard,
                PaymentCardProperties.IsSwipe);
            if (authorizeRequest.IsSwipe ?? false)
            {
                authorizeRequest.Track1 = PaymentUtilities.GetPropertyStringValue(
                    hashtable,
                    GenericNamespace.PaymentCard,
                    PaymentCardProperties.Track1);
                authorizeRequest.Track2 = PaymentUtilities.GetPropertyStringValue(
                    hashtable,
                    GenericNamespace.PaymentCard,
                    PaymentCardProperties.Track2);

                authorizeRequest.CardNumber = PaymentUtilities.ParseTrack1ForCardNumber(authorizeRequest.Track1);
                if (authorizeRequest.CardNumber == null)
                {
                    authorizeRequest.CardNumber = PaymentUtilities.ParseTrack2ForCardNumber(authorizeRequest.Track2);
                }

                if (authorizeRequest.CardNumber == null)
                {
                    errors.Add(new PaymentError(ErrorCode.InvalidCardTrackData, "Invalid card track data."));
                }

                decimal expirationYear, expirationMonth;
                HelperUtilities.ParseTrackDataForExpirationDate(authorizeRequest.Track1 ?? string.Empty, authorizeRequest.Track2 ?? string.Empty, out expirationYear, out expirationMonth);
                authorizeRequest.ExpirationYear = expirationYear;
                authorizeRequest.ExpirationMonth = expirationMonth;
            }
            else
            {
                authorizeRequest.CardToken = PaymentUtilities.GetPropertyStringValue(
                    hashtable,
                    GenericNamespace.PaymentCard,
                    PaymentCardProperties.CardToken);

                if (authorizeRequest.CardToken == null)
                {
                    authorizeRequest.CardNumber = PaymentUtilities.GetPropertyStringValue(
                        hashtable,
                        GenericNamespace.PaymentCard,
                        PaymentCardProperties.CardNumber,
                        errors,
                        ErrorCode.InvalidCardNumber);
                }
                else
                {
                    authorizeRequest.Last4Digit = PaymentUtilities.GetPropertyStringValue(
                        hashtable,
                        GenericNamespace.PaymentCard,
                        PaymentCardProperties.Last4Digits);
                }

                authorizeRequest.ExpirationYear = PaymentUtilities.GetPropertyDecimalValue(
                    hashtable,
                    GenericNamespace.PaymentCard,
                    PaymentCardProperties.ExpirationYear,
                    errors,
                    ErrorCode.InvalidExpirationDate);
                authorizeRequest.ExpirationMonth = PaymentUtilities.GetPropertyDecimalValue(
                    hashtable,
                    GenericNamespace.PaymentCard,
                    PaymentCardProperties.ExpirationMonth,
                    errors,
                    ErrorCode.InvalidExpirationDate);
            }

           

            

            // Don't validate gift cards
            if (authorizeRequest.CardNumber != null
                && !authorizeRequest.CardType.Equals(Microsoft.Dynamics.Retail.PaymentSDK.Portable.CardType.GiftCard.ToString(), StringComparison.OrdinalIgnoreCase)
                && !HelperUtilities.ValidateBankCardNumber(authorizeRequest.CardNumber))
            {
                errors.Add(new PaymentError(ErrorCode.InvalidCardNumber, "Invalid card number."));
            }

            if (authorizeRequest.ExpirationYear.HasValue
                && authorizeRequest.ExpirationMonth.HasValue
                && authorizeRequest.ExpirationYear >= 0M
                && authorizeRequest.ExpirationMonth >= 0M
                && !PaymentUtilities.ValidateExpirationDate(authorizeRequest.ExpirationYear.Value, authorizeRequest.ExpirationMonth.Value))
            {
                errors.Add(new PaymentError(ErrorCode.InvalidExpirationDate, "Invalid expiration date."));
            }

            if (Microsoft.Dynamics.Retail.PaymentSDK.Portable.CardType.Debit.ToString().Equals(authorizeRequest.CardType, StringComparison.OrdinalIgnoreCase))
            {
                authorizeRequest.EncryptedPin = PaymentUtilities.GetPropertyStringValue(
                    hashtable,
                    GenericNamespace.PaymentCard,
                    PaymentCardProperties.EncryptedPin,
                    errors,
                    ErrorCode.CannotVerifyPin);
                authorizeRequest.AdditionalSecurityData = PaymentUtilities.GetPropertyStringValue(
                    hashtable,
                    GenericNamespace.PaymentCard,
                    PaymentCardProperties.AdditionalSecurityData);
            }

            authorizeRequest.CashBackAmount = PaymentUtilities.GetPropertyDecimalValue(
                hashtable,
                GenericNamespace.PaymentCard,
                PaymentCardProperties.CashBackAmount);
            if (authorizeRequest.CashBackAmount.HasValue
                && authorizeRequest.CashBackAmount > 0M
                && !Microsoft.Dynamics.Retail.PaymentSDK.Portable.CardType.Debit.ToString().Equals(authorizeRequest.CardType, StringComparison.OrdinalIgnoreCase))
            {
                errors.Add(new PaymentError(ErrorCode.CashBackNotAvailable, "Cashback is not available."));
            }

            authorizeRequest.CardVerificationValue = PaymentUtilities.GetPropertyStringValue(
                hashtable,
                GenericNamespace.PaymentCard,
                PaymentCardProperties.CardVerificationValue);
            authorizeRequest.VoiceAuthorizationCode = PaymentUtilities.GetPropertyStringValue(
                hashtable,
                GenericNamespace.PaymentCard,
                PaymentCardProperties.VoiceAuthorizationCode);
            authorizeRequest.Name = PaymentUtilities.GetPropertyStringValue(
                hashtable,
                GenericNamespace.PaymentCard,
                PaymentCardProperties.Name);
            authorizeRequest.StreetAddress = PaymentUtilities.GetPropertyStringValue(
                hashtable,
                GenericNamespace.PaymentCard,
                PaymentCardProperties.StreetAddress);
            authorizeRequest.StreetAddress2 = PaymentUtilities.GetPropertyStringValue(
                hashtable,
                GenericNamespace.PaymentCard,
                PaymentCardProperties.StreetAddress2);
            authorizeRequest.City = PaymentUtilities.GetPropertyStringValue(
                hashtable,
                GenericNamespace.PaymentCard,
                PaymentCardProperties.City);
            authorizeRequest.State = PaymentUtilities.GetPropertyStringValue(
                hashtable,
                GenericNamespace.PaymentCard,
                PaymentCardProperties.State);
            authorizeRequest.PostalCode = PaymentUtilities.GetPropertyStringValue(
                hashtable,
                GenericNamespace.PaymentCard,
                PaymentCardProperties.PostalCode);
            authorizeRequest.Country = PaymentUtilities.GetPropertyStringValue(
                hashtable,
                GenericNamespace.PaymentCard,
                PaymentCardProperties.Country);
            authorizeRequest.Phone = PaymentUtilities.GetPropertyStringValue(
                hashtable,
                GenericNamespace.PaymentCard,
                PaymentCardProperties.Phone);
            authorizeRequest.AccountType = PaymentUtilities.GetPropertyStringValue(
                hashtable,
                GenericNamespace.PaymentCard,
                PaymentCardProperties.AccountType);
            authorizeRequest.UniqueCardId = PaymentUtilities.GetPropertyStringValue(
                hashtable,
                GenericNamespace.PaymentCard,
                PaymentCardProperties.UniqueCardId);

            // Read transaction data
            authorizeRequest.Amount = PaymentUtilities.GetPropertyDecimalValue(
                hashtable,
                GenericNamespace.TransactionData,
                TransactionDataProperties.Amount,
                errors,
                ErrorCode.InvalidAmount);
            authorizeRequest.CurrencyCode = PaymentUtilities.GetPropertyStringValue(
                hashtable,
                GenericNamespace.TransactionData,
                TransactionDataProperties.CurrencyCode,
                errors,
                ErrorCode.InvalidRequest);
            if (authorizeRequest.CurrencyCode != null
                && !PaymentUtilities.ValidateCurrencyCode(authorizeRequest.SupportedCurrencies, authorizeRequest.CurrencyCode))
            {
                errors.Add(new PaymentError(ErrorCode.UnsupportedCurrency, string.Format("Currency code is not supported: {0}.", authorizeRequest.CurrencyCode)));
            }

            authorizeRequest.SupportCardTokenization = PaymentUtilities.GetPropertyBooleanValue(
                hashtable,
                GenericNamespace.TransactionData,
                TransactionDataProperties.SupportCardTokenization);
            authorizeRequest.AllowPartialAuthorization = PaymentUtilities.GetPropertyBooleanValue(
                hashtable,
                GenericNamespace.TransactionData,
                TransactionDataProperties.AllowPartialAuthorization);

            authorizeRequest.AuthorizationProviderTransactionId = PaymentUtilities.GetPropertyStringValue(
                hashtable,
                GenericNamespace.TransactionData,
                TransactionDataProperties.AuthorizationProviderTransactionId);

            authorizeRequest.PurchaseLevel = PaymentUtilities.GetPropertyStringValue(
                hashtable,
                GenericNamespace.TransactionData,
                TransactionDataProperties.PurchaseLevel);

            // License >>
            authorizeRequest.SerialNumber = PaymentUtilities.GetPropertyStringValue(hashtable, GenericNamespace.MerchantAccount, LicenseProperties.SerialNumber);
            authorizeRequest.OldSerialNumber = PaymentUtilities.GetPropertyStringValue(hashtable, LicenseProperties.Namespace, LicenseProperties.SerialNumber);
            authorizeRequest.ChannelId = PaymentUtilities.GetPropertyStringValue(hashtable, LicenseProperties.Namespace, LicenseProperties.ChannelId);
            // License <<

            if (errors.Count > 0)
            {
                throw new PaymentException(errors);
            }

            return authorizeRequest;
        }
    }
}