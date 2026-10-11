namespace RR.Dynamics.Retail.Payment.NBE.Portable
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Dynamics.Retail.PaymentSDK.Portable;
    using Microsoft.Dynamics.Retail.PaymentSDK.Portable.Constants;
    using RR.Utility;

    /// <summary>
    /// Contains attributes associated with a Load Gift Card request. 
    /// Also contains a helper method to convert <see cref="Request"/> object into a <see cref="LoadGiftCardRequest"/> object.
    /// </summary>
    internal class LoadGiftCardRequest : RequestBase
    {
        internal LoadGiftCardRequest()
            : base()
        {
        }

        /// <summary>
        /// Gets or sets Card type for load gift card request.
        /// </summary>
        internal string CardType { get; set; }

        /// <summary>
        /// Gets or sets Is Swipe for load gift card request.
        /// </summary>
        internal bool? IsSwipe { get; set; }

        /// <summary>
        /// Gets or sets Card number for load gift card request.
        /// </summary>
        internal string CardNumber { get; set; }

        /// <summary>
        /// Gets or sets Track 1 for load gift card request.
        /// </summary>
        internal string Track1 { get; set; }

        /// <summary>
        /// Gets or sets Track 2 for load gift card request.
        /// </summary>
        internal string Track2 { get; set; }

        /// <summary>
        /// Gets or sets Card token for load gift card request.
        /// </summary>
        internal string CardToken { get; set; }

        /// <summary>
        /// Gets or sets Last 4 digits of card for load gift card request.
        /// </summary>
        internal string Last4Digit { get; set; }

        /// <summary>
        /// Gets or sets Amount for load gift card request.
        /// </summary>
        internal decimal? Amount { get; set; }

        /// <summary>
        /// Gets or sets Currency code for load gift card request.
        /// </summary>
        internal string CurrencyCode { get; set; }

        /// <summary>
        /// Will convert the original <see cref="Request"/> into a <see cref="LoadGiftCardRequest"/> object while performing necessary validation.
        /// Method will throw <see cref="SampleException"/> if validation fails during processing of <see cref="Request"/> object passed.
        /// </summary>
        /// <param name="request">Original request needing to be converted to <see cref="LoadGiftCardRequest"/>.</param>
        /// <returns><see cref="LoadGiftCardRequest"/> object representing the passed-in <see cref="Request"/> object.</returns>
        internal static LoadGiftCardRequest ConvertFrom(Request request)
        {
            LoadGiftCardRequest loadGCRequest = new LoadGiftCardRequest();
            List<PaymentError> errors = new List<PaymentError>();
            loadGCRequest.ReadBaseProperties(request, errors);

            /*
             * READ CARD DATA.
             */
            Hashtable hashtable = PaymentProperty.ConvertToHashtable(request.Properties);
            loadGCRequest.CardType = PaymentUtilities.GetPropertyStringValue(
                hashtable,
                GenericNamespace.PaymentCard,
                PaymentCardProperties.CardType,
                errors,
                ErrorCode.InvalidRequest);

            // Ensure CardType==GiftCard
            if (loadGCRequest.CardType != null && !loadGCRequest.CardType.Equals(Microsoft.Dynamics.Retail.PaymentSDK.Portable.CardType.GiftCard.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                errors.Add(new PaymentError(ErrorCode.CardTypeNotSupported, string.Format("Card type is not supported: {0}.", loadGCRequest.CardType)));
            }

            loadGCRequest.IsSwipe = PaymentUtilities.GetPropertyBooleanValue(
                hashtable,
                GenericNamespace.PaymentCard,
                PaymentCardProperties.IsSwipe);

            if (loadGCRequest.IsSwipe ?? false)
            {
                // Extract and assign values from original request.
                loadGCRequest.Track1 = PaymentUtilities.GetPropertyStringValue(
                    hashtable,
                    GenericNamespace.PaymentCard,
                    PaymentCardProperties.Track1);
                loadGCRequest.Track2 = PaymentUtilities.GetPropertyStringValue(
                    hashtable,
                    GenericNamespace.PaymentCard,
                    PaymentCardProperties.Track2);
                loadGCRequest.Track2 = PaymentUtilities.GetPropertyStringValue(
                    hashtable,
                    GenericNamespace.PaymentCard,
                    PaymentCardProperties.EncryptedPin);

                loadGCRequest.CardNumber = PaymentUtilities.ParseTrack1ForCardNumber(loadGCRequest.Track1);
                if (loadGCRequest.CardNumber == null)
                {
                    loadGCRequest.CardNumber = PaymentUtilities.ParseTrack2ForCardNumber(loadGCRequest.Track2);
                }

                if (loadGCRequest.CardNumber == null)
                {
                    errors.Add(new PaymentError(ErrorCode.InvalidCardTrackData, "Invalid card track data."));
                }
            }
            else
            {
                loadGCRequest.CardToken = PaymentUtilities.GetPropertyStringValue(
                    hashtable,
                    GenericNamespace.PaymentCard,
                    PaymentCardProperties.CardToken);

                if (loadGCRequest.CardToken == null)
                {
                    loadGCRequest.CardNumber = PaymentUtilities.GetPropertyStringValue(
                        hashtable,
                        GenericNamespace.PaymentCard,
                        PaymentCardProperties.CardNumber,
                        errors,
                        ErrorCode.InvalidCardNumber);
                }
                else
                {
                    loadGCRequest.Last4Digit = PaymentUtilities.GetPropertyStringValue(
                        hashtable,
                        GenericNamespace.PaymentCard,
                        PaymentCardProperties.Last4Digits);
                }
            }

            /*
             * READ TRANSACTION DATA.
             */
            loadGCRequest.Amount = PaymentUtilities.GetPropertyDecimalValue(
                hashtable,
                GenericNamespace.TransactionData,
                TransactionDataProperties.Amount,
                errors,
                ErrorCode.InvalidAmount);

            loadGCRequest.CurrencyCode = PaymentUtilities.GetPropertyStringValue(
                hashtable,
                GenericNamespace.TransactionData,
                TransactionDataProperties.CurrencyCode,
                errors,
                ErrorCode.InvalidRequest);

            if (loadGCRequest.CurrencyCode != null
                && !PaymentUtilities.ValidateCurrencyCode(loadGCRequest.SupportedCurrencies, loadGCRequest.CurrencyCode))
            {
                errors.Add(new PaymentError(ErrorCode.UnsupportedCurrency, string.Format("Currency code is not supported: {0}.", loadGCRequest.CurrencyCode)));
            }

            // License >>
            loadGCRequest.SerialNumber = PaymentUtilities.GetPropertyStringValue(hashtable, GenericNamespace.MerchantAccount, LicenseProperties.SerialNumber);
            loadGCRequest.OldSerialNumber = PaymentUtilities.GetPropertyStringValue(hashtable, LicenseProperties.Namespace, LicenseProperties.SerialNumber);
            loadGCRequest.ChannelId = PaymentUtilities.GetPropertyStringValue(hashtable, LicenseProperties.Namespace, LicenseProperties.ChannelId);
            // License <<

            if (errors.Count > 0)
            {
                throw new SampleException(errors);
            }

            return loadGCRequest;
        }
    }
}
