namespace RR.Dynamics.Retail.Payment.NBE.Portable
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Dynamics.Retail.PaymentSDK.Portable;
    using Microsoft.Dynamics.Retail.PaymentSDK.Portable.Constants;
    using RR.Utility;

    /// <summary>
    /// Initializes a new instance of the <see cref="LoadGiftCardResponse" /> class.
    /// Contains attributes associated with a Load Gift Card response.
    /// Also contains a helper method to convert <see cref="LoadGiftCardResponse"/> object into a <see cref="Response"/> object.
    /// </summary>
    /// <param name="locale">The locale value derived from the request.</param>
    /// <param name="serviceAccountId">Service Account ID associated with the Merchant Account derived from the request.</param>
    /// <param name="connectorName">Name of the connector.</param>
    internal class LoadGiftCardResponse : ResponseBase
    {
        internal LoadGiftCardResponse(string locale, string serviceAccountId, string connectorName)
            : base(locale, serviceAccountId, connectorName)
        {
        }

        /// <summary>
        ///  Gets or sets the card type associated with the Balance On Gift Card request.
        /// </summary>
        internal string CardType { get; set; }

        /// <summary>
        ///  Gets or sets the last four digits of the card number associated with the Balance On Gift Card request.
        /// </summary>
        internal string Last4Digit { get; set; }

        /// <summary>
        ///  Gets or sets the Provider Transaction ID for this request/response.
        /// </summary>
        internal string ProviderTransactionId { get; set; }

        /// <summary>
        /// Gets or sets the available balance on the gift card.
        /// </summary>
        internal decimal? AvailableBalance { get; set; }

        /// <summary>
        /// Gets or sets the approval code for the response of the request.
        /// </summary>
        internal string ApprovalCode { get; set; }

        /// <summary>
        /// Gets or sets the result of request after it has been processed.
        /// </summary>
        internal string LoadGiftCardResult { get; set; }

        /// <summary>
        /// Will convert the passed-in <see cref="LoadGiftCardResponse"/> object into a <see cref="Response"/> object.
        /// </summary>
        /// <param name="loadGCResponse"><see cref="LoadGiftCardResponse"/> object to be parsed and converted into a <see cref="Response"/> object.</param>
        /// <returns><see cref="Response"/> object derived from the parsing of the passed-in <see cref="LoadGiftCardResponse"/> object.</returns>
        internal static Response ConvertTo(LoadGiftCardResponse loadGCResponse)
        {
            Response response = new Response();
            loadGCResponse.WriteBaseProperties(response);

            List<PaymentProperty> properties = new List<PaymentProperty>();
            if (response.Properties != null)
            {
                properties.AddRange(response.Properties);
            }

            List<PaymentProperty> loadGCResponseProperties = new List<PaymentProperty>();
            PaymentUtilities.AddPropertyIfPresent(loadGCResponseProperties, GenericNamespace.LoadGiftCardResponse, LoadGiftCardResponseProperties.CardType, loadGCResponse.CardType);
            PaymentUtilities.AddPropertyIfPresent(loadGCResponseProperties, GenericNamespace.LoadGiftCardResponse, LoadGiftCardResponseProperties.Last4Digits, loadGCResponse.Last4Digit);
            PaymentUtilities.AddPropertyIfPresent(loadGCResponseProperties, GenericNamespace.LoadGiftCardResponse, LoadGiftCardResponseProperties.ProviderTransactionId, loadGCResponse.ProviderTransactionId);
            PaymentUtilities.AddPropertyIfPresent(loadGCResponseProperties, GenericNamespace.LoadGiftCardResponse, LoadGiftCardResponseProperties.ApprovalCode, loadGCResponse.ApprovalCode);
            PaymentUtilities.AddPropertyIfPresent(loadGCResponseProperties, GenericNamespace.LoadGiftCardResponse, LoadGiftCardResponseProperties.AvailableBalance, loadGCResponse.AvailableBalance);
            PaymentUtilities.AddPropertyIfPresent(loadGCResponseProperties, GenericNamespace.LoadGiftCardResponse, LoadGiftCardResponseProperties.LoadGiftCardResult, loadGCResponse.LoadGiftCardResult);
            // License >>
            PaymentUtilities.AddPropertyIfPresent(loadGCResponseProperties, LicenseProperties.Namespace, LicenseProperties.LicenseExpireDate, loadGCResponse.LicenseExpireDate);
            // License <<
            properties.Add(new PaymentProperty(GenericNamespace.LoadGiftCardResponse, LoadGiftCardResponseProperties.Properties, loadGCResponseProperties.ToArray()));

            response.Properties = properties.ToArray();
            return response;
        }
    }
}
