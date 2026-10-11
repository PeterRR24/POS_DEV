namespace RR.Dynamics.Retail.Payment.NBE.Portable
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Dynamics.Retail.PaymentSDK.Portable;
    using Microsoft.Dynamics.Retail.PaymentSDK.Portable.Constants;
    using RR.Utility;

    /// <summary>
    /// Initializes a new instance of the <see cref="ActivateGiftCardRequest" /> class.
    /// Contains attributes associated with a Activate Gift Card request.
    /// Also contains a helper method to convert <see cref="Request"/> object into a <see cref="ActivateGiftCardRequest"/> object.
    /// </summary>
    internal class ActivateGiftCardRequest : RequestBase
    {
        internal ActivateGiftCardRequest()
            : base()
        {
        }

        /// <summary>
        ///  Gets or sets the card type associated with the Activate Gift Card request. It should be <see cref="CardType.GiftCard"/>.
        /// </summary>
        internal string CardType { get; set; }

        /// <summary>
        ///  Gets or sets whether the request was a swipe for the Activate Gift Card request.
        /// </summary>
        internal bool? IsSwipe { get; set; }

        /// <summary>
        ///  Gets or sets the card number associated with the Activate Gift Card request. 
        /// </summary>
        internal string CardNumber { get; set; }

        /// <summary>
        ///  Gets or sets the Track 1 data associated with the Activate Gift Card request.
        /// </summary>
        internal string Track1 { get; set; }

        /// <summary>
        ///  Gets or sets the Track 2 data associated with the Activate Gift Card request.
        /// </summary>
        internal string Track2 { get; set; }

        /// <summary>
        ///  Gets or sets the card token string associated with the Activate Gift Card request.
        /// </summary>
        internal string CardToken { get; set; }

        /// <summary>
        ///  Gets or sets the last four digits of the card number associated with the Activate Gift Card request.
        /// </summary>
        internal string Last4Digit { get; set; }

        /// <summary>
        ///  Gets or sets the amount associated with the Activate Gift Card request.
        /// </summary>
        internal decimal? Amount { get; set; }

        /// <summary>
        ///  Gets or sets the currency code associated with the Activate Gift Card request.
        /// </summary>
        internal string CurrencyCode { get; set; }

        /// <summary>
        /// Will convert the original <see cref="Request"/> into a <see cref="ActivateGiftCardRequest"/> object while performing necessary validation.
        /// Method will throw <see cref="SampleException"/> if validation fails during processing of <see cref="Request"/> object passed.
        /// </summary>
        /// <param name="request">Original request needing to be converted to <see cref="ActivateGiftCardRequest"/>.</param>
        /// <returns><see cref="ActivateGiftCardRequest"/> object representing the passed-in <see cref="Request"/> object.</returns>
        internal static ActivateGiftCardRequest ConvertFrom(Request request)
        {
            var activateGiftCardRequest = new ActivateGiftCardRequest();
            var errors = new List<PaymentError>();
            activateGiftCardRequest.ReadBaseProperties(request, errors);

            // Read card data
            Hashtable hashtable = PaymentProperty.ConvertToHashtable(request.Properties);
            activateGiftCardRequest.CardType = PaymentUtilities.GetPropertyStringValue(
                hashtable,
                GenericNamespace.PaymentCard,
                PaymentCardProperties.CardType,
                errors,
                ErrorCode.InvalidRequest);

            // Ensure CardType==GiftCard
            if (activateGiftCardRequest.CardType != null && !activateGiftCardRequest.CardType.Equals(Microsoft.Dynamics.Retail.PaymentSDK.Portable.CardType.GiftCard.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                errors.Add(new PaymentError(ErrorCode.CardTypeNotSupported, string.Format("Card type is not supported: {0}.", activateGiftCardRequest.CardType)));
            }

            activateGiftCardRequest.IsSwipe = PaymentUtilities.GetPropertyBooleanValue(
                hashtable,
                GenericNamespace.PaymentCard,
                PaymentCardProperties.IsSwipe);

            if (activateGiftCardRequest.IsSwipe ?? false)
            {
                activateGiftCardRequest.Track1 = PaymentUtilities.GetPropertyStringValue(
                    hashtable,
                    GenericNamespace.PaymentCard,
                    PaymentCardProperties.Track1);
                activateGiftCardRequest.Track2 = PaymentUtilities.GetPropertyStringValue(
                    hashtable,
                    GenericNamespace.PaymentCard,
                    PaymentCardProperties.Track2);

                activateGiftCardRequest.CardNumber = PaymentUtilities.ParseTrack1ForCardNumber(activateGiftCardRequest.Track1);
                if (activateGiftCardRequest.CardNumber == null)
                {
                    activateGiftCardRequest.CardNumber = PaymentUtilities.ParseTrack2ForCardNumber(activateGiftCardRequest.Track2);
                }

                if (activateGiftCardRequest.CardNumber == null)
                {
                    errors.Add(new PaymentError(ErrorCode.InvalidCardTrackData, "Invalid card track data."));
                }
            }
            else
            {
                activateGiftCardRequest.CardToken = PaymentUtilities.GetPropertyStringValue(
                    hashtable,
                    GenericNamespace.PaymentCard,
                    PaymentCardProperties.CardToken);

                if (activateGiftCardRequest.CardToken == null)
                {
                    activateGiftCardRequest.CardNumber = PaymentUtilities.GetPropertyStringValue(
                        hashtable,
                        GenericNamespace.PaymentCard,
                        PaymentCardProperties.CardNumber,
                        errors,
                        ErrorCode.InvalidCardNumber);
                }
                else
                {
                    activateGiftCardRequest.Last4Digit = PaymentUtilities.GetPropertyStringValue(
                        hashtable,
                        GenericNamespace.PaymentCard,
                        PaymentCardProperties.Last4Digits);
                }
            }

            // Read transaction data
            activateGiftCardRequest.Amount = PaymentUtilities.GetPropertyDecimalValue(
                hashtable,
                GenericNamespace.TransactionData,
                TransactionDataProperties.Amount,
                errors,
                ErrorCode.InvalidAmount);
            activateGiftCardRequest.CurrencyCode = PaymentUtilities.GetPropertyStringValue(
                hashtable,
                GenericNamespace.TransactionData,
                TransactionDataProperties.CurrencyCode,
                errors,
                ErrorCode.InvalidRequest);

            if (activateGiftCardRequest.CurrencyCode != null
                && !PaymentUtilities.ValidateCurrencyCode(activateGiftCardRequest.SupportedCurrencies, activateGiftCardRequest.CurrencyCode))
            {
                errors.Add(new PaymentError(ErrorCode.UnsupportedCurrency, string.Format("Currency code is not supported: {0}.", activateGiftCardRequest.CurrencyCode)));
            }


            // License >>
            activateGiftCardRequest.SerialNumber = PaymentUtilities.GetPropertyStringValue(hashtable, GenericNamespace.MerchantAccount, LicenseProperties.SerialNumber);
            activateGiftCardRequest.OldSerialNumber = PaymentUtilities.GetPropertyStringValue(hashtable, LicenseProperties.Namespace, LicenseProperties.SerialNumber);
            activateGiftCardRequest.ChannelId = PaymentUtilities.GetPropertyStringValue(hashtable, LicenseProperties.Namespace, LicenseProperties.ChannelId);
            // License <<

            if (errors.Count > 0)
            {
                throw new SampleException(errors);
            }

            return activateGiftCardRequest;
        }
    }
}

