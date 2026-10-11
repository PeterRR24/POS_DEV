
namespace RR.Dynamics.Retail.Payment.NBE.Portable
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Dynamics.Retail.PaymentSDK.Portable;
    using Microsoft.Dynamics.Retail.PaymentSDK.Portable.Constants;

    public class CaptureRequest : RequestBase
    {
        public CaptureRequest()
            : base()
        {
        }

        public string CardType { get; set; }

        public bool? IsSwipe { get; set; }

        public string CardNumber { get; set; }

        public string Track1 { get; set; }

        public string Track2 { get; set; }

        public string CardToken { get; set; }

        public string Last4Digit { get; set; }

        public string AccountType { get; set; }

        public string UniqueCardId { get; set; }

        public string VoiceAuthorizationCode { get; set; }

        public decimal? Amount { get; set; }

        public string CurrencyCode { get; set; }

        public string PurchaseLevel { get; set; }

        public Level2Data Level2Data { get; set; }

        public IEnumerable<Level3Data> Level3Data { get; set; }

        public string AuthorizationProviderTransactionId { get; set; }

        public string AuthorizationApprovalCode { get; set; }

        public string AuthorizationResponseCode { get; set; }

        public decimal? AuthorizationApprovedAmount { get; set; }

        public decimal? AuthorizationCashbackAmount { get; set; }

        public string AuthorizationResult { get; set; }

        public string AuthorizationProviderMessage { get; set; }

        public DateTime? AuthorizationTransactionDateTime { get; set; }

        public string AuthorizationTransactionType { get; set; }

        public static CaptureRequest ConvertFrom(Request request)
        {
            var captureRequest = new CaptureRequest();
            var errors = new List<PaymentError>();
            captureRequest.ReadBaseProperties(request, errors);

            // Check authorization response
            Hashtable hashtable = PaymentProperty.ConvertToHashtable(request.Properties);
            PaymentProperty authorizationResponsePropertyList = PaymentProperty.GetPropertyFromHashtable(hashtable, GenericNamespace.AuthorizationResponse, AuthorizationResponseProperties.Properties);
            Hashtable authorizationHashtable = null;
            if (authorizationResponsePropertyList == null)
            {
                errors.Add(new PaymentError(ErrorCode.InvalidRequest, "Authorization response is missing."));
                throw new PaymentException(errors);
            }
            else
            {
                authorizationHashtable = PaymentProperty.ConvertToHashtable(authorizationResponsePropertyList.PropertyList);
            }

            // Read card data
            captureRequest.CardType = PaymentUtilities.GetPropertyStringValue(
                authorizationHashtable,
                GenericNamespace.AuthorizationResponse,
                AuthorizationResponseProperties.CardType,
                errors,
                ErrorCode.InvalidRequest);

            captureRequest.IsSwipe = PaymentUtilities.GetPropertyBooleanValue(
                authorizationHashtable,
                GenericNamespace.AuthorizationResponse,
                AuthorizationResponseProperties.IsSwiped);
            if (captureRequest.IsSwipe ?? false)
            {
                captureRequest.Track1 = PaymentUtilities.GetPropertyStringValue(
                    hashtable,
                    GenericNamespace.PaymentCard,
                    PaymentCardProperties.Track1);
                captureRequest.Track2 = PaymentUtilities.GetPropertyStringValue(
                    hashtable,
                    GenericNamespace.PaymentCard,
                    PaymentCardProperties.Track2);
                captureRequest.CardNumber = PaymentUtilities.GetPropertyStringValue(
                    hashtable,
                    GenericNamespace.PaymentCard,
                    PaymentCardProperties.CardNumber);

                if (string.IsNullOrEmpty(captureRequest.CardNumber))
                {
                    captureRequest.CardNumber = PaymentUtilities.ParseTrack1ForCardNumber(captureRequest.Track1);
                    if (captureRequest.CardNumber == null)
                    {
                        captureRequest.CardNumber = PaymentUtilities.ParseTrack2ForCardNumber(captureRequest.Track2);
                    }
                }

                if (string.IsNullOrEmpty(captureRequest.CardNumber))
                {
                    errors.Add(new PaymentError(ErrorCode.InvalidCardTrackData, "Invalid card track data."));
                }
            }
            else
            {
                captureRequest.CardToken = PaymentUtilities.GetPropertyStringValue(
                    authorizationHashtable,
                    GenericNamespace.AuthorizationResponse,
                    AuthorizationResponseProperties.CardToken);
                if (captureRequest.CardToken == null)
                {
                    captureRequest.CardToken = PaymentUtilities.GetPropertyStringValue(
                        hashtable,
                        GenericNamespace.PaymentCard,
                        PaymentCardProperties.CardToken);
                    if (captureRequest.CardToken == null)
                    {
                        captureRequest.CardNumber = PaymentUtilities.GetPropertyStringValue(
                            hashtable,
                            GenericNamespace.PaymentCard,
                            PaymentCardProperties.CardNumber);
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(captureRequest.CardNumber)
                && string.IsNullOrWhiteSpace(captureRequest.CardToken))
            {
                errors.Add(new PaymentError(ErrorCode.InvalidRequest, string.Format("Neither card number nor card token is provided.")));
            }

            captureRequest.Last4Digit = PaymentUtilities.GetPropertyStringValue(
                authorizationHashtable,
                GenericNamespace.AuthorizationResponse,
                AuthorizationResponseProperties.Last4Digits);
            captureRequest.AccountType = PaymentUtilities.GetPropertyStringValue(
                authorizationHashtable,
                GenericNamespace.AuthorizationResponse,
                AuthorizationResponseProperties.AccountType);
            captureRequest.UniqueCardId = PaymentUtilities.GetPropertyStringValue(
                authorizationHashtable,
                GenericNamespace.AuthorizationResponse,
                AuthorizationResponseProperties.UniqueCardId);
            captureRequest.VoiceAuthorizationCode = PaymentUtilities.GetPropertyStringValue(
                authorizationHashtable,
                GenericNamespace.AuthorizationResponse,
                AuthorizationResponseProperties.VoiceAuthorizationCode);

            // Read transaction data
            captureRequest.Amount = PaymentUtilities.GetPropertyDecimalValue(
                hashtable,
                GenericNamespace.TransactionData,
                TransactionDataProperties.Amount,
                errors,
                ErrorCode.InvalidAmount);
            captureRequest.CurrencyCode = PaymentUtilities.GetPropertyStringValue(
                hashtable,
                GenericNamespace.TransactionData,
                TransactionDataProperties.CurrencyCode,
                errors,
                ErrorCode.InvalidRequest);
            captureRequest.PurchaseLevel = PaymentUtilities.GetPropertyStringValue(
                hashtable,
                GenericNamespace.TransactionData,
                TransactionDataProperties.PurchaseLevel);

            captureRequest.Level2Data = PaymentUtilities.GetLevel2Data(hashtable);
            captureRequest.Level3Data = PaymentUtilities.GetLevel3Data(hashtable);

            // Read authorization data
            captureRequest.AuthorizationTransactionType = PaymentUtilities.GetPropertyStringValue(
                authorizationHashtable,
                GenericNamespace.AuthorizationResponse,
                AuthorizationResponseProperties.TransactionType,
                errors,
                ErrorCode.InvalidRequest);
            if (captureRequest.AuthorizationTransactionType != null
                && !TransactionType.Authorize.ToString().Equals(captureRequest.AuthorizationTransactionType, StringComparison.OrdinalIgnoreCase))
            {
                errors.Add(new PaymentError(ErrorCode.InvalidTransaction, "Capture does not support this type of transaction"));
            }

            captureRequest.AuthorizationApprovalCode = PaymentUtilities.GetPropertyStringValue(
                authorizationHashtable,
                GenericNamespace.AuthorizationResponse,
                AuthorizationResponseProperties.ApprovalCode);
            captureRequest.AuthorizationApprovedAmount = PaymentUtilities.GetPropertyDecimalValue(
                authorizationHashtable,
                GenericNamespace.AuthorizationResponse,
                AuthorizationResponseProperties.ApprovedAmount,
                errors,
                ErrorCode.InvalidRequest);
            captureRequest.AuthorizationCashbackAmount = PaymentUtilities.GetPropertyDecimalValue(
                authorizationHashtable,
                GenericNamespace.AuthorizationResponse,
                AuthorizationResponseProperties.CashBackAmount);
            captureRequest.AuthorizationProviderMessage = PaymentUtilities.GetPropertyStringValue(
                authorizationHashtable,
                GenericNamespace.AuthorizationResponse,
                AuthorizationResponseProperties.ProviderMessage);
            captureRequest.AuthorizationProviderTransactionId = PaymentUtilities.GetPropertyStringValue(
                authorizationHashtable,
                GenericNamespace.AuthorizationResponse,
                AuthorizationResponseProperties.ProviderTransactionId,
                errors,
                ErrorCode.InvalidRequest);
            captureRequest.AuthorizationResponseCode = PaymentUtilities.GetPropertyStringValue(
                authorizationHashtable,
                GenericNamespace.AuthorizationResponse,
                AuthorizationResponseProperties.ResponseCode);
            captureRequest.AuthorizationResult = PaymentUtilities.GetPropertyStringValue(
                authorizationHashtable,
                GenericNamespace.AuthorizationResponse,
                AuthorizationResponseProperties.AuthorizationResult,
                errors,
                ErrorCode.InvalidRequest);
            captureRequest.AuthorizationTransactionDateTime = PaymentUtilities.GetPropertyDateTimeValue(
                authorizationHashtable,
                GenericNamespace.AuthorizationResponse,
                AuthorizationResponseProperties.TransactionDateTime);

            if (errors.Count > 0)
            {
                throw new PaymentException(errors);
            }

            return captureRequest;
        }
    }
}
