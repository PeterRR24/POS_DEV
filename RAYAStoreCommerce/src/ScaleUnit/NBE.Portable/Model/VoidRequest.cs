namespace RR.Dynamics.Retail.Payment.NBE.Portable
{

    using System;
    using System.Collections.Generic;
    using Microsoft.Dynamics.Retail.PaymentSDK.Portable;
    using Microsoft.Dynamics.Retail.PaymentSDK.Portable.Constants;

    internal class VoidRequest : RequestBase
    {
        internal VoidRequest()
            : base()
        {
        }

        /// <summary>
        ///  Gets or sets the card number associated with the Void request. 
        /// </summary>
        internal string CardNumber { get; set; }
        internal string CardType { get; set; }

        internal bool? IsSwipe { get; set; }

        internal string CardToken { get; set; }

        internal string Last4Digit { get; set; }

        internal string AccountType { get; set; }

        internal string UniqueCardId { get; set; }

        internal string VoiceAuthorizationCode { get; set; }

        internal string CurrencyCode { get; set; }

        internal string AuthorizationProviderTransactionId { get; set; }

        internal string AuthorizationApprovalCode { get; set; }

        internal string AuthorizationResponseCode { get; set; }

        internal decimal? AuthorizationApprovedAmount { get; set; }

        internal decimal? AuthorizationCashbackAmount { get; set; }

        internal string AuthorizationResult { get; set; }

        internal string AuthorizationProviderMessage { get; set; }

        internal DateTime? AuthorizationTransactionDateTime { get; set; }

        internal string AuthorizationTransactionType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the issued gift card is requested to be void.
        /// </summary>
        internal bool IsIssuedGiftCard { get; set; }
        internal bool IsLoadGiftCard { get; set; }
        // RR >> for update9
        static string ActivateGiftCardResponse = "ActivateGiftCardResponse";
        static string LoadGiftCardResponse = "LoadGiftCardResponse";
        // RR <<

        internal static VoidRequest ConvertFrom(Request request)
        {
            var voidRequest = new VoidRequest();
            var errors = new List<PaymentError>();
            voidRequest.ReadBaseProperties(request, errors);

            // Check authorization response
            Hashtable hashtable = PaymentProperty.ConvertToHashtable(request.Properties);
            PaymentProperty authorizationResponsePropertyList = PaymentProperty.GetPropertyFromHashtable(hashtable, GenericNamespace.AuthorizationResponse, AuthorizationResponseProperties.Properties);
            PaymentProperty refundResponsePropertyList = PaymentProperty.GetPropertyFromHashtable(hashtable, GenericNamespace.RefundResponse, AuthorizationResponseProperties.Properties);
            // RR >>
            // For App update 3 and onwards
            //PaymentProperty activateGiftCardReponsePropertyList = PaymentProperty.GetPropertyFromHashtable(hashtable, GenericNamespace.ActivateGiftCardResponse, AuthorizationResponseProperties.Properties);
            //PaymentProperty loadGiftCardReponsePropertyList = PaymentProperty.GetPropertyFromHashtable(hashtable, GenericNamespace.LoadGiftCardResponse, AuthorizationResponseProperties.Properties);
            // For update9 only

            PaymentProperty activateGiftCardReponsePropertyList = PaymentProperty.GetPropertyFromHashtable(hashtable, ActivateGiftCardResponse, AuthorizationResponseProperties.Properties);
            PaymentProperty loadGiftCardReponsePropertyList = PaymentProperty.GetPropertyFromHashtable(hashtable, LoadGiftCardResponse, AuthorizationResponseProperties.Properties);
            // RR <<
            Hashtable authorizationHashtable = null;

            bool isGiftCardTransaction = false;

            // Authorization Response can be missing if it is a Gift Card transation. Otherwise, throw an exception.
            if (authorizationResponsePropertyList == null && activateGiftCardReponsePropertyList == null && loadGiftCardReponsePropertyList == null && refundResponsePropertyList == null)
            {
                voidRequest.CardType = PaymentUtilities.GetPropertyStringValue(
                    hashtable,
                    GenericNamespace.PaymentCard,
                    PaymentCardProperties.CardType);

                isGiftCardTransaction = voidRequest.CardType != null && voidRequest.CardType.Equals(Microsoft.Dynamics.Retail.PaymentSDK.Portable.CardType.GiftCard.ToString(), StringComparison.OrdinalIgnoreCase);
                if (!isGiftCardTransaction)
                {
                    errors.Add(new PaymentError(ErrorCode.InvalidRequest, "Authorization response is missing."));
                    throw new SampleException(errors);
                }
            }
            else if (activateGiftCardReponsePropertyList != null)
            {
                voidRequest.IsIssuedGiftCard = true;

                voidRequest.CardType = PaymentUtilities.GetPropertyStringValue(
                    hashtable,
                    ActivateGiftCardResponse /* GenericNamespace.ActivateGiftCardResponse */,
                    PaymentCardProperties.CardType);
                // RR >>
                voidRequest.AuthorizationProviderTransactionId = PaymentUtilities.GetPropertyStringValue(
                   hashtable,
                   ActivateGiftCardResponse /* GenericNamespace.ActivateGiftCardResponse */,
                   "ProviderTransactionId" /* ActivateGiftCardResponseProperties.ProviderTransactionId */,
                   errors,
                   ErrorCode.InvalidRequest);
                voidRequest.CurrencyCode = PaymentUtilities.GetPropertyStringValue(
                 hashtable,
                 GenericNamespace.TransactionData,
                 TransactionDataProperties.CurrencyCode,
                 errors,
                 ErrorCode.InvalidRequest);
                // RR <<
                isGiftCardTransaction = voidRequest.CardType != null && voidRequest.CardType.Equals(Microsoft.Dynamics.Retail.PaymentSDK.Portable.CardType.GiftCard.ToString(), StringComparison.OrdinalIgnoreCase);
            }
            // RR >>
            else if (loadGiftCardReponsePropertyList != null)
            {
                voidRequest.IsLoadGiftCard = true;
                voidRequest.CardType = PaymentUtilities.GetPropertyStringValue(
                    hashtable,
                    LoadGiftCardResponse /* GenericNamespace.LoadGiftCardResponse */,
                    PaymentCardProperties.CardType);

                voidRequest.AuthorizationProviderTransactionId = PaymentUtilities.GetPropertyStringValue(
                   hashtable,
                   LoadGiftCardResponse /* GenericNamespace.LoadGiftCardResponse */,
                   "ProviderTransactionId" /*LoadGiftCardResponseProperties.ProviderTransactionId*/,
                   errors,
                   ErrorCode.InvalidRequest);
                
                isGiftCardTransaction = voidRequest.CardType != null && voidRequest.CardType.Equals(Microsoft.Dynamics.Retail.PaymentSDK.Portable.CardType.GiftCard.ToString(), StringComparison.OrdinalIgnoreCase);
            }
            else if (refundResponsePropertyList != null)
            {
                Hashtable refundHashtable = PaymentProperty.ConvertToHashtable(refundResponsePropertyList.PropertyList);

                // Read card data
                voidRequest.CardType = PaymentUtilities.GetPropertyStringValue(
                    refundHashtable,
                    GenericNamespace.RefundResponse,
                    RefundResponseProperties.CardType,
                    errors,
                    ErrorCode.InvalidRequest);

                isGiftCardTransaction = voidRequest.CardType != null && voidRequest.CardType.Equals(Microsoft.Dynamics.Retail.PaymentSDK.Portable.CardType.GiftCard.ToString(), StringComparison.OrdinalIgnoreCase);

                voidRequest.IsSwipe = PaymentUtilities.GetPropertyBooleanValue(
                    refundHashtable,
                    GenericNamespace.PaymentCard,
                    PaymentCardProperties.IsSwipe);
                voidRequest.CardToken = PaymentUtilities.GetPropertyStringValue(
                        refundHashtable,
                        GenericNamespace.RefundResponse,
                        RefundResponseProperties.CardToken);
                voidRequest.Last4Digit = PaymentUtilities.GetPropertyStringValue(
                    refundHashtable,
                    GenericNamespace.RefundResponse,
                    RefundResponseProperties.Last4Digits);
                voidRequest.AccountType = PaymentUtilities.GetPropertyStringValue(
                    refundHashtable,
                    GenericNamespace.PaymentCard,
                    PaymentCardProperties.AccountType);
                voidRequest.UniqueCardId = PaymentUtilities.GetPropertyStringValue(
                    refundHashtable,
                    GenericNamespace.RefundResponse,
                    RefundResponseProperties.UniqueCardId);
                voidRequest.VoiceAuthorizationCode = PaymentUtilities.GetPropertyStringValue(
                    refundHashtable,
                    GenericNamespace.PaymentCard,
                    PaymentCardProperties.VoiceAuthorizationCode);

                // Read authorization data
                voidRequest.AuthorizationTransactionType = PaymentUtilities.GetPropertyStringValue(
                    refundHashtable,
                    GenericNamespace.RefundResponse,
                    RefundResponseProperties.TransactionType,
                    errors,
                    ErrorCode.InvalidRequest);
                if (voidRequest.AuthorizationTransactionType != null
                    && !TransactionType.Refund.ToString().Equals(voidRequest.AuthorizationTransactionType, StringComparison.OrdinalIgnoreCase))
                {
                    errors.Add(new PaymentError(ErrorCode.InvalidTransaction, "Void does not support this type of transaction"));
                }

                voidRequest.AuthorizationApprovalCode = PaymentUtilities.GetPropertyStringValue(
                    refundHashtable,
                    GenericNamespace.RefundResponse,
                    RefundResponseProperties.ApprovalCode);
                voidRequest.AuthorizationApprovedAmount = PaymentUtilities.GetPropertyDecimalValue(
                    refundHashtable,
                    GenericNamespace.RefundResponse,
                    RefundResponseProperties.ApprovedAmount,
                    errors,
                    ErrorCode.InvalidRequest);
                voidRequest.CurrencyCode = PaymentUtilities.GetPropertyStringValue(
                    refundHashtable,
                    GenericNamespace.RefundResponse,
                    RefundResponseProperties.CurrencyCode,
                    errors,
                    ErrorCode.InvalidRequest);
                voidRequest.AuthorizationProviderMessage = PaymentUtilities.GetPropertyStringValue(
                    refundHashtable,
                    GenericNamespace.RefundResponse,
                    RefundResponseProperties.ProviderMessage);
                voidRequest.AuthorizationProviderTransactionId = PaymentUtilities.GetPropertyStringValue(
                    refundHashtable,
                    GenericNamespace.RefundResponse,
                    RefundResponseProperties.ProviderTransactionId,
                    errors,
                    ErrorCode.InvalidRequest);
                voidRequest.AuthorizationResponseCode = PaymentUtilities.GetPropertyStringValue(
                    refundHashtable,
                    GenericNamespace.RefundResponse,
                    RefundResponseProperties.ResponseCode);
                voidRequest.AuthorizationResult = PaymentUtilities.GetPropertyStringValue(
                    refundHashtable,
                    GenericNamespace.RefundResponse,
                    RefundResponseProperties.RefundResult,
                    errors,
                    ErrorCode.InvalidRequest);
                voidRequest.AuthorizationTransactionDateTime = PaymentUtilities.GetPropertyDateTimeValue(
                    refundHashtable,
                    GenericNamespace.RefundResponse,
                    RefundResponseProperties.TransactionDateTime);
            }
            else
            {
                authorizationHashtable = PaymentProperty.ConvertToHashtable(authorizationResponsePropertyList.PropertyList);

                // Read card data
                voidRequest.CardType = PaymentUtilities.GetPropertyStringValue(
                    authorizationHashtable,
                    GenericNamespace.AuthorizationResponse,
                    AuthorizationResponseProperties.CardType,
                    errors,
                    ErrorCode.InvalidRequest);

                isGiftCardTransaction = voidRequest.CardType != null && voidRequest.CardType.Equals(Microsoft.Dynamics.Retail.PaymentSDK.Portable.CardType.GiftCard.ToString(), StringComparison.OrdinalIgnoreCase);

                voidRequest.IsSwipe = PaymentUtilities.GetPropertyBooleanValue(
                    authorizationHashtable,
                    GenericNamespace.AuthorizationResponse,
                    AuthorizationResponseProperties.IsSwiped);
                voidRequest.CardToken = PaymentUtilities.GetPropertyStringValue(
                        authorizationHashtable,
                        GenericNamespace.AuthorizationResponse,
                        AuthorizationResponseProperties.CardToken);
                voidRequest.Last4Digit = PaymentUtilities.GetPropertyStringValue(
                    authorizationHashtable,
                    GenericNamespace.AuthorizationResponse,
                    AuthorizationResponseProperties.Last4Digits);
                voidRequest.AccountType = PaymentUtilities.GetPropertyStringValue(
                    authorizationHashtable,
                    GenericNamespace.AuthorizationResponse,
                    AuthorizationResponseProperties.AccountType);
                voidRequest.UniqueCardId = PaymentUtilities.GetPropertyStringValue(
                    authorizationHashtable,
                    GenericNamespace.AuthorizationResponse,
                    AuthorizationResponseProperties.UniqueCardId);
                voidRequest.VoiceAuthorizationCode = PaymentUtilities.GetPropertyStringValue(
                    authorizationHashtable,
                    GenericNamespace.AuthorizationResponse,
                    AuthorizationResponseProperties.VoiceAuthorizationCode);

                // Read authorization data
                voidRequest.AuthorizationTransactionType = PaymentUtilities.GetPropertyStringValue(
                    authorizationHashtable,
                    GenericNamespace.AuthorizationResponse,
                    AuthorizationResponseProperties.TransactionType,
                    errors,
                    ErrorCode.InvalidRequest);
                if (voidRequest.AuthorizationTransactionType != null
                    && !TransactionType.Authorize.ToString().Equals(voidRequest.AuthorizationTransactionType, StringComparison.OrdinalIgnoreCase))
                {
                    errors.Add(new PaymentError(ErrorCode.InvalidTransaction, "Void does not support this type of transaction"));
                }

                voidRequest.AuthorizationApprovalCode = PaymentUtilities.GetPropertyStringValue(
                    authorizationHashtable,
                    GenericNamespace.AuthorizationResponse,
                    AuthorizationResponseProperties.ApprovalCode);
                voidRequest.AuthorizationApprovedAmount = PaymentUtilities.GetPropertyDecimalValue(
                    authorizationHashtable,
                    GenericNamespace.AuthorizationResponse,
                    AuthorizationResponseProperties.ApprovedAmount,
                    errors,
                    ErrorCode.InvalidRequest);
                voidRequest.CurrencyCode = PaymentUtilities.GetPropertyStringValue(
                    authorizationHashtable,
                    GenericNamespace.AuthorizationResponse,
                    AuthorizationResponseProperties.CurrencyCode,
                    errors,
                    ErrorCode.InvalidRequest);
                voidRequest.AuthorizationCashbackAmount = PaymentUtilities.GetPropertyDecimalValue(
                    authorizationHashtable,
                    GenericNamespace.AuthorizationResponse,
                    AuthorizationResponseProperties.CashBackAmount);
                voidRequest.AuthorizationProviderMessage = PaymentUtilities.GetPropertyStringValue(
                    authorizationHashtable,
                    GenericNamespace.AuthorizationResponse,
                    AuthorizationResponseProperties.ProviderMessage);
                voidRequest.AuthorizationProviderTransactionId = PaymentUtilities.GetPropertyStringValue(
                    authorizationHashtable,
                    GenericNamespace.AuthorizationResponse,
                    AuthorizationResponseProperties.ProviderTransactionId,
                    errors,
                    ErrorCode.InvalidRequest);
                voidRequest.AuthorizationResponseCode = PaymentUtilities.GetPropertyStringValue(
                    authorizationHashtable,
                    GenericNamespace.AuthorizationResponse,
                    AuthorizationResponseProperties.ResponseCode);
                voidRequest.AuthorizationResult = PaymentUtilities.GetPropertyStringValue(
                    authorizationHashtable,
                    GenericNamespace.AuthorizationResponse,
                    AuthorizationResponseProperties.AuthorizationResult,
                    errors,
                    ErrorCode.InvalidRequest);
                voidRequest.AuthorizationTransactionDateTime = PaymentUtilities.GetPropertyDateTimeValue(
                    authorizationHashtable,
                    GenericNamespace.AuthorizationResponse,
                    AuthorizationResponseProperties.TransactionDateTime);
            }

            // If it is a Gift Card type transaction, attempt to set the Card Number (if not already set) using the PaymentCard properties.
            // If Card Number cannot be set using the PaymentCard properties, attempt to set it using the Card Token.
            if (isGiftCardTransaction)
            {
                voidRequest.CardNumber = voidRequest.CardNumber ?? PaymentUtilities.GetPropertyStringValue(
                    hashtable,
                    GenericNamespace.PaymentCard,
                    PaymentCardProperties.CardNumber);

                // RR >>

                voidRequest.CardToken = voidRequest.CardToken ?? PaymentUtilities.GetPropertyStringValue(
                        hashtable,
                        GenericNamespace.PaymentCard,
                        PaymentCardProperties.CardToken);
                if (voidRequest.AuthorizationApprovedAmount == null || voidRequest.AuthorizationApprovedAmount == decimal.Zero)
                {
                    voidRequest.AuthorizationApprovedAmount = PaymentUtilities.GetPropertyDecimalValue(
                       hashtable,
                       GenericNamespace.TransactionData,
                       "Amount",
                       errors,
                       ErrorCode.InvalidRequest);

                    voidRequest.CardToken = voidRequest.CardToken ?? PaymentUtilities.GetPropertyStringValue(
                       hashtable,
                        GenericNamespace.TransactionData,
                       "SecureCardData");
                }
                // RR <<

                if (voidRequest.IsIssuedGiftCard)
                {
                    voidRequest.CardNumber = voidRequest.CardNumber ?? PaymentUtilities.GetPropertyStringValue(
                    hashtable,
                    ActivateGiftCardResponse /*GenericNamespace.ActivateGiftCardResponse*/,
                    "GiftCardNumber" /*ActivateGiftCardResponseProperties.GiftCardNumber*/);
                }

                // If Card Number is not available through PaymentCard.CardNumber property, set it through the CardToken instead.
                //if (string.IsNullOrEmpty(voidRequest.CardNumber))
                //{
                //    voidRequest.CardToken = voidRequest.CardToken ?? PaymentUtilities.GetPropertyStringValue(
                //        hashtable,
                //        GenericNamespace.PaymentCard,
                //        PaymentCardProperties.CardToken);

                //    if (string.IsNullOrEmpty(voidRequest.CardToken))
                //    {
                //        errors.Add(new PaymentError(ErrorCode.InvalidRequest, "Card Number and Card Token missing."));
                //        throw new SampleException(errors);
                //    }
                //    else
                //    {
                //        UTF8Encoding encoder = new UTF8Encoding();
                //        byte[] cardNumberBytes = Convert.FromBase64String(voidRequest.CardToken);
                //        voidRequest.CardNumber = encoder.GetString(cardNumberBytes, 0, cardNumberBytes.Length);
                //    }
                //}
            }

            if (errors.Count > 0)
            {
                throw new SampleException(errors);
            }

            return voidRequest;
        }
    }
}

