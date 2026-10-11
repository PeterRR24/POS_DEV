
namespace RR.Dynamics.Retail.Payment.NBE.Portable
{
    using System.Collections.Generic;
    using Microsoft.Dynamics.Retail.PaymentSDK.Portable;
    using Microsoft.Dynamics.Retail.PaymentSDK.Portable.Constants;

    public class GenerateCardTokenResponse : ResponseBase
    {
        public GenerateCardTokenResponse(string locale, string serviceAccountId, string connectorName)
            : base(locale, serviceAccountId, connectorName)
        {
        }

        public string CardType { get; set; }

        public string Last4Digit { get; set; }

        public string CardToken { get; set; }

        public string UniqueCardId { get; set; }

        public decimal? ExpirationYear { get; set; }

        public decimal? ExpirationMonth { get; set; }

        public string Name { get; set; }

        public string StreetAddress { get; set; }

        public string StreetAddress2 { get; set; }

        public string City { get; set; }

        public string State { get; set; }

        public string PostalCode { get; set; }

        public string Country { get; set; }

        public string Phone { get; set; }

        public string AccountType { get; set; }

        public IList<PaymentProperty> OtherCardProperties { get; set; }

        public static Response ConvertTo(GenerateCardTokenResponse tokenizeResponse)
        {
            var response = new Response();
            tokenizeResponse.WriteBaseProperties(response);

            var properties = new List<PaymentProperty>();
            if (response.Properties != null)
            {
                properties.AddRange(response.Properties);
            }

            PaymentUtilities.AddPropertyIfPresent(properties, GenericNamespace.PaymentCard, PaymentCardProperties.CardType, tokenizeResponse.CardType);
            PaymentUtilities.AddPropertyIfPresent(properties, GenericNamespace.PaymentCard, PaymentCardProperties.Last4Digits, tokenizeResponse.Last4Digit);
            PaymentUtilities.AddPropertyIfPresent(properties, GenericNamespace.PaymentCard, PaymentCardProperties.CardToken, tokenizeResponse.CardToken);
            PaymentUtilities.AddPropertyIfPresent(properties, GenericNamespace.PaymentCard, PaymentCardProperties.UniqueCardId, tokenizeResponse.UniqueCardId);
            PaymentUtilities.AddPropertyIfPresent(properties, GenericNamespace.PaymentCard, PaymentCardProperties.ExpirationYear, tokenizeResponse.ExpirationYear);
            PaymentUtilities.AddPropertyIfPresent(properties, GenericNamespace.PaymentCard, PaymentCardProperties.ExpirationMonth, tokenizeResponse.ExpirationMonth);
            PaymentUtilities.AddPropertyIfPresent(properties, GenericNamespace.PaymentCard, PaymentCardProperties.Name, tokenizeResponse.Name);
            PaymentUtilities.AddPropertyIfPresent(properties, GenericNamespace.PaymentCard, PaymentCardProperties.StreetAddress, tokenizeResponse.StreetAddress);
            PaymentUtilities.AddPropertyIfPresent(properties, GenericNamespace.PaymentCard, PaymentCardProperties.StreetAddress2, tokenizeResponse.StreetAddress2);
            PaymentUtilities.AddPropertyIfPresent(properties, GenericNamespace.PaymentCard, PaymentCardProperties.City, tokenizeResponse.City);
            PaymentUtilities.AddPropertyIfPresent(properties, GenericNamespace.PaymentCard, PaymentCardProperties.State, tokenizeResponse.State);
            PaymentUtilities.AddPropertyIfPresent(properties, GenericNamespace.PaymentCard, PaymentCardProperties.PostalCode, tokenizeResponse.PostalCode);
            PaymentUtilities.AddPropertyIfPresent(properties, GenericNamespace.PaymentCard, PaymentCardProperties.Country, tokenizeResponse.Country);
            PaymentUtilities.AddPropertyIfPresent(properties, GenericNamespace.PaymentCard, PaymentCardProperties.Phone, tokenizeResponse.Phone);
            PaymentUtilities.AddPropertyIfPresent(properties, GenericNamespace.PaymentCard, PaymentCardProperties.AccountType, tokenizeResponse.AccountType);
            properties.AddRange(tokenizeResponse.OtherCardProperties);

            response.Properties = properties.ToArray();
            return response;
        }
    }
}

