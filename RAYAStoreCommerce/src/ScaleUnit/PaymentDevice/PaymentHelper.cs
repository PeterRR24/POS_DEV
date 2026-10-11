using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

using PaySdk = Microsoft.Dynamics.Retail.PaymentSDK.Portable;
using Constants = Microsoft.Dynamics.Retail.PaymentSDK.Portable.Constants;
using Microsoft.Dynamics.Commerce.HardwareStation.Peripherals.Entities;
using Microsoft.Dynamics.Commerce.HardwareStation;
using Microsoft.Dynamics.Commerce.HardwareStation.CardPayment;
using Microsoft.Dynamics.Commerce.HardwareStation.Peripherals;
using Microsoft.Dynamics.Commerce.Runtime.DataModel;
using System.Globalization;
using RR.Utility;

namespace RR.PaymentDevice
{
    public static class PaymentHelper
    {
        private static readonly PaySdk.ErrorCode[] DoNotThrowOnCodes = new PaySdk.ErrorCode[4]
        {
          PaySdk.ErrorCode.RetrievalTransactionByReferenceInProgress,
          PaySdk.ErrorCode.NoTransactionFoundByTransactionReference,
          PaySdk.ErrorCode.TransactionRetrievalByReferenceNotSupported,
          PaySdk.ErrorCode.DuplicateTransactionResultUnknown
        };

        private static readonly Dictionary<CardType, PaySdk.CardType> cardTypeMapping = new Dictionary<CardType, PaySdk.CardType>()
    { { CardType.GiftCard, PaySdk.CardType.GiftCard },{ CardType.InternationalDebitCard, PaySdk.CardType.Debit } };

        public static TenderInfo GetDummyTender(string cardNumber)
        {
            TenderInfo tenderInfo = new TenderInfo()
            {
                CardNumber = cardNumber,
                CardTypeId = 0,
                CashBackAmount = 0,
                EncryptedPin = string.Empty,
                AdditionalSecurityData = string.Empty,
                ExpirationMonth = DateTime.Today.Month,
                ExpirationYear = DateTime.Today.Year + 1
            };
            return tenderInfo;
        }

        private static PaySdk.Request GetBaseRequest(string paymentConnectorName, string locale, PaySdk.PaymentProperty[] merchantProperties)
        {
            PaySdk.Request request = new PaySdk.Request()
            {
                Locale = locale
            };
            List<PaySdk.PaymentProperty> properties = new List<PaySdk.PaymentProperty>();
            if (merchantProperties[0].Namespace.Equals(Constants.GenericNamespace.Connector, StringComparison.OrdinalIgnoreCase) && merchantProperties[0].Name.Equals(Constants.ConnectorProperties.Properties, StringComparison.OrdinalIgnoreCase))
            {
                foreach (PaySdk.PaymentProperty pp in merchantProperties)
                {
                    if (pp.Name.Equals(Constants.ConnectorProperties.Properties))
                    {
                        PaySdk.Hashtable hashTable = PaySdk.PaymentProperty.ConvertToHashtable(pp.PropertyList);
                        string connectorName;
                        PaySdk.PaymentProperty.GetPropertyValue(hashTable, Constants.GenericNamespace.Connector, Constants.ConnectorProperties.ConnectorName, out connectorName);
                        if (paymentConnectorName == connectorName)
                            properties.AddRange(pp.PropertyList);
                    }
                }
            }
            else
            {
                properties = merchantProperties.ToList();
            }
            request.Properties = properties.ToArray();
            return request;
        }

        //private static PaySdk.Request GetAuthorizationRefundRequest(PaySdk.PaymentProperty[] merchantProperties, TenderInfo tenderInfo, Decimal amount, string currency, string locale, bool isTestMode, string terminalId, string connectorId, ExtensionTransaction extensionTransactionProperties)
        //{
        //    PaySdk.Request request = GetBaseRequest(connectorId, locale, merchantProperties);            
        //    List<PaySdk.PaymentProperty> properties = new List<PaySdk.PaymentProperty>(request.Properties);
        //    GetPaymentCard(properties, tenderInfo);
        //    GetTransactionData(properties, PaySdk.IndustryType.Retail, PaySdk.PurchaseLevel.Level1, true, false, amount, currency, isTestMode, terminalId);
        //    AddExtensionTransactionProperties(properties, extensionTransactionProperties);
        //    request.Properties = properties.ToArray();
        //    return request;
        //}

        public static PaySdk.Request GetAuthorizationRefundRequest(PaySdk.PaymentProperty[] merchantProperties, TenderInfo tenderInfo, Decimal amount, string currency, string locale, bool isTestMode, string terminalId, ExtensionTransaction extensionTransactionProperties)
        {
            PaySdk.Request request = new PaySdk.Request();
            request.Locale = locale;
            List<PaySdk.PaymentProperty> properties = new List<PaySdk.PaymentProperty>(merchantProperties);
            GetPaymentCard(properties, tenderInfo);
            GetTransactionData(properties, PaySdk.IndustryType.Retail, PaySdk.PurchaseLevel.Level1, true, false, amount, currency, isTestMode, terminalId);
            AddExtensionTransactionProperties(properties, extensionTransactionProperties);
            request.Properties = properties.ToArray();
            return request;
        }

        public static PaySdk.Request GetCaptureVoidRequest(PaySdk.PaymentProperty[] merchantProperties, PaySdk.PaymentProperty[] paymentProperties, Decimal amount, string currency, string locale, bool isTestMode, string terminalId, ExtensionTransaction extensionTransactionProperties)
        {
            ThrowIf.Null(paymentProperties, nameof(paymentProperties));
            PaySdk.Request request = new PaySdk.Request
            {
                Locale = locale
            };
            List<PaySdk.PaymentProperty> properties = new List<PaySdk.PaymentProperty>(merchantProperties);
            foreach (PaySdk.PaymentProperty paymentProperty1 in paymentProperties)
            {
                if (!merchantProperties.Contains(paymentProperty1))
                    properties.Add(paymentProperty1);
            }
            GetTransactionData(properties, PaySdk.IndustryType.Retail, PaySdk.PurchaseLevel.Level1, true, false, amount, currency, isTestMode, terminalId);
            AddExtensionTransactionProperties(properties, extensionTransactionProperties);
            request.Properties = properties.ToArray();
            return request;
        }

        public static PaySdk.Request GetGiftCardRequest(PaySdk.PaymentProperty[] merchantProperties, TenderInfo tenderInfo, Decimal? amount, string currency, string locale, bool isTestMode, string terminalId, string connectorName, ExtensionTransaction extensionTransactionProperties)
        {
            PaySdk.Request request = null;
            string payConnectorName = string.Empty;
            Decimal amount1 = amount ?? amount.Value;
            if (merchantProperties[0].Namespace.Equals(Constants.GenericNamespace.Connector, StringComparison.OrdinalIgnoreCase) && merchantProperties[0].Name.Equals(Constants.ConnectorProperties.Properties, StringComparison.OrdinalIgnoreCase))
            {
                bool flag = false;
                foreach (PaySdk.PaymentProperty merchantProperty in merchantProperties)
                {
                    PaySdk.Hashtable hashtable = PaySdk.PaymentProperty.ConvertToHashtable(merchantProperty.PropertyList);
                    PaySdk.PaymentProperty.GetPropertyValue(hashtable, Constants.GenericNamespace.Connector, Constants.ConnectorProperties.ConnectorName, out payConnectorName);
                    if ((string.IsNullOrWhiteSpace(connectorName) || string.IsNullOrWhiteSpace(payConnectorName) || string.Equals(connectorName, payConnectorName, StringComparison.OrdinalIgnoreCase)))
                    {
                        flag = true;
                        request = GetBaseRequest(connectorName, locale, merchantProperty.PropertyList);
                    }
                }
                if (!flag)
                {
                    RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "Payment connector name not found.", "ERROR");
                    throw new CardPaymentException(CardPaymentException.MissingPaymentConnectorName, "Payment connector name not found!", new object[1] { connectorName });
                }

            }
            else
            {
                payConnectorName = GetConnectorName(merchantProperties);
                if (!string.IsNullOrWhiteSpace(connectorName) && !string.IsNullOrWhiteSpace(payConnectorName) && !string.Equals(payConnectorName, connectorName, StringComparison.OrdinalIgnoreCase))
                {
                    RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "Payment connector name not found.", "ERROR");
                    throw new CardPaymentException(CardPaymentException.MissingPaymentConnectorName, "Payment connector name not found!", new object[1] { connectorName });
                }

                request = GetBaseRequest(connectorName, locale, merchantProperties);
            }
            List<PaySdk.PaymentProperty> properties = new List<PaySdk.PaymentProperty>(request.Properties);
            GetPaymentCard(properties, tenderInfo);
            GetTransactionData(properties, PaySdk.IndustryType.Retail, PaySdk.PurchaseLevel.Level1, true, false, amount1, currency, isTestMode, terminalId);
            AddExtensionTransactionProperties(properties, extensionTransactionProperties);
            request.Properties = properties.ToArray();
            return request;
        }

        public static PaySdk.Request GetTokenRequest(PaySdk.PaymentProperty[] merchantProperties, TenderInfo tenderInfo, string locale, ExtensionTransaction extensionTransactionProperties)
        {
            PaySdk.Request request = new PaySdk.Request
            {
                Locale = locale
            };
            List<PaySdk.PaymentProperty> properties = new List<PaySdk.PaymentProperty>(merchantProperties);
            GetPaymentCard(properties, tenderInfo);
            AddExtensionTransactionProperties(properties, extensionTransactionProperties);
            request.Properties = properties.ToArray();
            return request;
        }

        public static PaySdk.Request GetAuthorizeCardTokenRequest(PaySdk.PaymentProperty[] merchantProperties, PaySdk.PaymentProperty[] cardTokenProperties, Decimal amount, string currency, string locale, bool isTestMode, string terminalId, ExtensionTransaction extensionTransactionProperties)
        {
            ThrowIf.NullOrEmpty(merchantProperties, nameof(merchantProperties));
            ThrowIf.NullOrEmpty(cardTokenProperties, nameof(cardTokenProperties));
            ThrowIf.NullOrWhiteSpace(locale, nameof(locale));
            PaySdk.Request request = new PaySdk.Request();
            request.Locale = locale;
            List<PaySdk.PaymentProperty> properties = new List<PaySdk.PaymentProperty>(merchantProperties);
            foreach (PaySdk.PaymentProperty cardTokenProperty in cardTokenProperties)
            {
                if (!(merchantProperties).Contains<PaySdk.PaymentProperty>(cardTokenProperty))
                    properties.Add(cardTokenProperty);
            }
            GetTransactionData(properties, PaySdk.IndustryType.Retail, PaySdk.PurchaseLevel.Level1, true, false, amount, currency, isTestMode, terminalId);
            AddExtensionTransactionProperties(properties, extensionTransactionProperties);
            request.Properties = properties.ToArray();
            return request;
        }

        public static PaySdk.PaymentProperty[] MergePaymentProperties1(PaySdk.PaymentProperty[] merchantProperties, PaySdk.PaymentProperty[] paymentProperties)
        {
            List<PaySdk.PaymentProperty> properties = new List<PaySdk.PaymentProperty>(merchantProperties);
            foreach (PaySdk.PaymentProperty paymentProperty in paymentProperties)
            {
                if (!merchantProperties.Contains(paymentProperty))
                    properties.Add(paymentProperty);
            }
            return properties.ToArray();
        }

        public static List<PaySdk.PaymentProperty> MergePaymentProperties(PaySdk.PaymentProperty[] merchantProperties, PaySdk.PaymentProperty[] paymentProperties)
        {
            List<PaySdk.PaymentProperty> paymentPropertyList = new List<PaySdk.PaymentProperty>(merchantProperties);
            foreach (PaySdk.PaymentProperty paymentProperty1 in paymentProperties)
            {
                PaySdk.PaymentProperty property = paymentProperty1;
                PaySdk.PaymentProperty paymentProperty2 = paymentPropertyList.Find(item => item.Name == property.Name && item.Namespace == property.Namespace);
                if (paymentProperty2 == null)
                    paymentPropertyList.Add(property);
            }
            return paymentPropertyList;
        }

        private static void GetRequiredConnectorProperties1(PaySdk.PaymentProperty[] merchantProperties, PaySdk.PaymentProperty[] responseProperties, out PaySdk.PaymentProperty[] currentMerchantProperties)
        {
            if (merchantProperties == null || merchantProperties.Length == 0)
            {
                RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "The merchant payment properties are empty!", "ERROR");
                throw new CardPaymentException(CardPaymentException.EmptyPaymentProperties, "The merchant payment properties are empty.", Array.Empty<object>());
            }

            currentMerchantProperties = merchantProperties;
            if (!merchantProperties[0].Namespace.Equals(Constants.GenericNamespace.Connector, StringComparison.OrdinalIgnoreCase) || !merchantProperties[0].Name.Equals(Constants.ConnectorProperties.Properties, StringComparison.OrdinalIgnoreCase))
                return;
            PaySdk.Hashtable hashtable1 = PaySdk.PaymentProperty.ConvertToHashtable(responseProperties);
            string empty1 = string.Empty;
            PaySdk.PaymentProperty.GetPropertyValue(hashtable1, Constants.GenericNamespace.MerchantAccount, Constants.MerchantAccountProperties.ServiceAccountId, out empty1);
            foreach (PaySdk.PaymentProperty merchantProperty in merchantProperties)
            {
                PaySdk.Hashtable hashtable2 = PaySdk.PaymentProperty.ConvertToHashtable(merchantProperty.PropertyList);
                string empty2 = string.Empty;
                PaySdk.PaymentProperty.GetPropertyValue(hashtable2, Constants.GenericNamespace.MerchantAccount, Constants.MerchantAccountProperties.ServiceAccountId, out empty2);
                if (empty2.Equals(empty1, StringComparison.Ordinal))
                {
                    currentMerchantProperties = merchantProperty.PropertyList;
                    break;
                }
            }
        }

        public static PaySdk.PaymentProperty[] GetRequiredMerchantProperties(PaySdk.PaymentProperty[] requestPaymentProperties, PaySdk.PaymentProperty[] currentMerchantProperties, string requestMerchantPropertiesXml, string connectorName)
        {
            ThrowIf.NullOrEmpty(requestPaymentProperties, nameof(requestPaymentProperties));
            ThrowIf.NullOrWhiteSpace(connectorName, nameof(connectorName));
            if (currentMerchantProperties == null || currentMerchantProperties.Length == 0)
            {
                switch (requestMerchantPropertiesXml)
                {
                    case "":
                    case null:
                        RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", string.Format("The merchant payment properties are empty.Required connector name: {0}", connectorName), "ERROR");
                        throw new CardPaymentException(CardPaymentException.EmptyPaymentProperties, string.Format("The merchant payment properties are empty.Required connector name: {0}", (object)connectorName), Array.Empty<object>());
                }
            }
            PaySdk.PaymentProperty[] paymentPropertyArray;
            if (!string.IsNullOrWhiteSpace(requestMerchantPropertiesXml))
            {
                paymentPropertyArray = ToLocalProperties(requestMerchantPropertiesXml);
                Microsoft.Dynamics.Retail.Diagnostics.RetailLogger.Log.HardwareStationUsingRequestMerchantProperties(connectorName);
            }
            else
            {
                if (currentMerchantProperties[0].Namespace.Equals(Constants.GenericNamespace.Connector, StringComparison.OrdinalIgnoreCase) && currentMerchantProperties[0].Name.Equals(Constants.ConnectorProperties.Properties, StringComparison.OrdinalIgnoreCase))
                {
                    string serviceAccountId;
                    PaySdk.PaymentProperty.GetPropertyValue(PaySdk.PaymentProperty.ConvertToHashtable(requestPaymentProperties), Constants.GenericNamespace.MerchantAccount, Constants.MerchantAccountProperties.ServiceAccountId, out serviceAccountId);
                    paymentPropertyArray = GetSingleMerchantAccountProperties(currentMerchantProperties, connectorName, serviceAccountId);
                }
                else
                    paymentPropertyArray = currentMerchantProperties;
                Microsoft.Dynamics.Retail.Diagnostics.RetailLogger.Log.HardwareStationUsingLocalMerchantProperties(connectorName);
            }
            return paymentPropertyArray;
        }

        public static void GetRequiredConnector(PaySdk.PaymentProperty[] merchantProperties, PaySdk.PaymentProperty[] responseProperties, PaySdk.IPaymentProcessor processor, out PaySdk.IPaymentProcessor currentProcessor, out PaySdk.PaymentProperty[] currentMerchantProperties)
        {
            if (merchantProperties == null || merchantProperties.Length == 0)
            {
                RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "The merchant payment properties are empty.", "ERROR");
                throw new CardPaymentException(CardPaymentException.EmptyPaymentProperties, "The merchant payment properties are empty.", Array.Empty<object>());
            }
            try
            {

                currentProcessor = processor;
                currentMerchantProperties = merchantProperties;
                if (!merchantProperties[0].Namespace.Equals(Constants.GenericNamespace.Connector, StringComparison.OrdinalIgnoreCase) || !merchantProperties[0].Name.Equals(Constants.ConnectorProperties.Properties, StringComparison.OrdinalIgnoreCase))
                    return;
                PaySdk.Hashtable hashtable1 = PaySdk.PaymentProperty.ConvertToHashtable(responseProperties);
                string empty1 = string.Empty;
                PaySdk.PaymentProperty.GetPropertyValue(hashtable1, Constants.GenericNamespace.MerchantAccount, Constants.MerchantAccountProperties.ServiceAccountId, out empty1);
                foreach (PaySdk.PaymentProperty merchantProperty in merchantProperties)
                {
                    PaySdk.Hashtable hashtable2 = PaySdk.PaymentProperty.ConvertToHashtable(merchantProperty.PropertyList);
                    string empty2 = string.Empty;
                    PaySdk.PaymentProperty.GetPropertyValue(hashtable2, Constants.GenericNamespace.MerchantAccount, Constants.MerchantAccountProperties.ServiceAccountId, out empty2);
                    if (!empty2.Equals(empty1, StringComparison.Ordinal))
                    {
                        //currentProcessor = ((PaymentProcessorWrapper)currentProcessor).Successor;
                        PaySdk.PaymentProperty.GetPropertyValue(hashtable2, Constants.GenericNamespace.Connector, Constants.ConnectorProperties.ConnectorName, out string connectorName);
                        currentProcessor = PaymentProcessor.Get(connectorName);
                    }
                    else
                    {
                        currentMerchantProperties = merchantProperty.PropertyList;
                        //PaySdk.PaymentProperty.GetPropertyValue(hashtable2, Constants.GenericNamespace.Connector, Constants.ConnectorProperties.ConnectorName, out string connectorName);
                        //currentProcessor = PaymentProcessor.Get(connectorName);
                        break;
                    }
                }
            }
            catch (Exception ex)
            {

                RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "GetRequiredConnector - error -"+ ex.Message +ex.StackTrace);
                throw ex;
            }
        }

        public static PaySdk.Request GetRefundCaptureTokenRequest(PaySdk.PaymentProperty[] merchantProperties, PaySdk.PaymentProperty[] captureTokenProperties,
          Decimal amount, string currency, string locale, bool isTestMode, string terminalId, ExtensionTransaction extensionTransactionProperties)
        {
            ThrowIf.NullOrEmpty(merchantProperties, nameof(merchantProperties));
            ThrowIf.NullOrEmpty(captureTokenProperties, nameof(captureTokenProperties));
            ThrowIf.NullOrWhiteSpace(locale, nameof(locale));
            PaySdk.Request request = new PaySdk.Request
            {
                Locale = locale
            };
            List<PaySdk.PaymentProperty> properties = MergePaymentProperties(merchantProperties, captureTokenProperties);
            GetTransactionData(properties, PaySdk.IndustryType.Retail, PaySdk.PurchaseLevel.Level1, true, false, amount, currency, isTestMode, terminalId);
            AddExtensionTransactionProperties(properties, extensionTransactionProperties);
            request.Properties = properties.ToArray();
            return request;
        }

        public static PaySdk.Response InvokeAuthorizationCall(PaySdk.PaymentProperty[] merchantProperties, string paymentConnectorName, TenderInfo tenderInfo, Decimal amount, string currency, string locale, bool isTestMode, string terminalId, ExtensionTransaction extensionTransactionProperties)
        {
            PaySdk.Response response = null;
            ThrowIf.Null<TenderInfo>(tenderInfo, nameof(tenderInfo));
            if (merchantProperties == null || merchantProperties.Length == 0)
            {
                RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "The merchant payment properties are empty.", "ERROR");
                throw new CardPaymentException(CardPaymentException.EmptyPaymentProperties, "The merchant payment properties are empty.", Array.Empty<object>());
            }

            if (merchantProperties[0].Namespace.Equals(Constants.GenericNamespace.Connector, StringComparison.OrdinalIgnoreCase) && merchantProperties[0].Name.Equals(Constants.ConnectorProperties.Properties, StringComparison.OrdinalIgnoreCase))
            {
                bool flag = false;
                foreach (PaySdk.PaymentProperty merchantProperty in merchantProperties)
                {
                    PaySdk.Hashtable hashtable = PaySdk.PaymentProperty.ConvertToHashtable(merchantProperty.PropertyList);
                    string cardSubTypeByTender = GetCardSubTypeByTender(tenderInfo);
                    if (IsSupportedMerchantConnector(hashtable, currency, cardSubTypeByTender, out string connectorName) && (string.IsNullOrWhiteSpace(paymentConnectorName) || string.IsNullOrWhiteSpace(connectorName) || string.Equals(connectorName, paymentConnectorName, StringComparison.OrdinalIgnoreCase)))
                    {
                        flag = true;
                        response = PaymentProcessor.Get(connectorName).Authorize(GetAuthorizationRefundRequest(merchantProperty.PropertyList, tenderInfo, amount, currency, locale, isTestMode, terminalId, extensionTransactionProperties), null);
                        break;
                    }
                }
                if (!flag)
                {
                    RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "Payment connector name not found!", "ERROR");
                    throw new CardPaymentException(CardPaymentException.MissingPaymentConnectorName, "Payment connector name not found!", new object[1] { paymentConnectorName });
                }

            }
            else
            {
                string connectorName = GetConnectorName(merchantProperties);
                if (!string.IsNullOrWhiteSpace(paymentConnectorName) && !string.IsNullOrWhiteSpace(connectorName) && !string.Equals(connectorName, paymentConnectorName, StringComparison.OrdinalIgnoreCase))
                {
                    RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "Payment connector name not found! paymentConnectorName: " + paymentConnectorName, "ERROR");
                    throw new CardPaymentException(CardPaymentException.MissingPaymentConnectorName, "Payment connector name not found!", new object[1] { paymentConnectorName });
                }

                response = PaymentProcessor.Get(connectorName).Authorize(GetAuthorizationRefundRequest(merchantProperties, tenderInfo, amount, currency, locale, isTestMode, terminalId, extensionTransactionProperties), null);
            }
            return response;
        }

        public static PaySdk.Response ChainedRefundCall(PaySdk.PaymentProperty[] merchantProperties, string paymentConnectorName, TenderInfo tenderInfo, Decimal amount, string currency, string locale, bool isTestMode, string terminalId, ExtensionTransaction extensionTransactionProperties)
        {
            PaySdk.Response response = null;
            ThrowIf.Null<TenderInfo>(tenderInfo, nameof(tenderInfo));
            if (merchantProperties == null || merchantProperties.Length == 0)
            {
                RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "The merchant payment properties are empty!", "ERROR");
                throw new CardPaymentException(CardPaymentException.EmptyPaymentProperties, "The merchant payment properties are empty.", Array.Empty<object>());
            }

            if (merchantProperties[0].Namespace.Equals(Constants.GenericNamespace.Connector, StringComparison.OrdinalIgnoreCase) && merchantProperties[0].Name.Equals(Constants.ConnectorProperties.Properties, StringComparison.OrdinalIgnoreCase))
            {
                bool flag = false;
                foreach (PaySdk.PaymentProperty merchantProperty in merchantProperties)
                {
                    PaySdk.Hashtable hashtable = PaySdk.PaymentProperty.ConvertToHashtable(merchantProperty.PropertyList);
                    string cardSubTypeByTender = GetCardSubTypeByTender(tenderInfo);
                    if (IsSupportedMerchantConnector(hashtable, currency, cardSubTypeByTender, out string connectorName) && (string.IsNullOrWhiteSpace(paymentConnectorName) || string.IsNullOrWhiteSpace(connectorName) || string.Equals(connectorName, paymentConnectorName, StringComparison.OrdinalIgnoreCase)))
                    {
                        flag = true;
                        response = PaymentProcessor.Get(connectorName).Refund(GetAuthorizationRefundRequest(merchantProperty.PropertyList, tenderInfo, amount, currency, locale, isTestMode, terminalId, extensionTransactionProperties), null);
                        break;
                    }
                }
                if (!flag)
                {
                    RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "Payment connector name not found! paymentConnectorName: " + paymentConnectorName, "ERROR");
                    throw new CardPaymentException(CardPaymentException.MissingPaymentConnectorName, "Payment connector name not found!", new object[1] { paymentConnectorName });
                }

            }
            else
            {
                string connectorName = GetConnectorName(merchantProperties);
                if (!string.IsNullOrWhiteSpace(paymentConnectorName) && !string.IsNullOrWhiteSpace(connectorName) && !string.Equals(connectorName, paymentConnectorName, StringComparison.OrdinalIgnoreCase))
                {
                    RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "Payment connector name not found!", "ERROR");
                    throw new CardPaymentException(CardPaymentException.MissingPaymentConnectorName, "Payment connector name not found!", new object[1] { paymentConnectorName });
                }

                response = PaymentProcessor.Get(connectorName).Refund(GetAuthorizationRefundRequest(merchantProperties, tenderInfo, amount, currency, locale, isTestMode, terminalId, extensionTransactionProperties), null);
            }
            return response;
        }

        public static PaySdk.PaymentProperty[] ToLocalProperties(string paymentPropertiesXml)
        {
            PaySdk.PaymentProperty[] paymentPropertyArray = null;
            if (!string.IsNullOrWhiteSpace(paymentPropertiesXml))
                paymentPropertyArray = PaySdk.PaymentProperty.ConvertXMLToPropertyArray(paymentPropertiesXml);
            return paymentPropertyArray;
        }

        private static PaySdk.PaymentProperty[] GetSingleMerchantAccountProperties(PaySdk.PaymentProperty[] merchantProperties, string paymentConnectorName, string serviceAccountId)
        {
            ThrowIf.NullOrWhiteSpace(paymentConnectorName, nameof(paymentConnectorName));
            ThrowIf.NullOrWhiteSpace(serviceAccountId, nameof(serviceAccountId));
            ThrowIf.NullOrEmpty(merchantProperties, nameof(merchantProperties));
            PaySdk.PaymentProperty[] paymentPropertyArray = null;
            bool flag = false;
            foreach (PaySdk.PaymentProperty merchantProperty in merchantProperties)
            {
                PaySdk.Hashtable hashtable = PaySdk.PaymentProperty.ConvertToHashtable(merchantProperty.PropertyList);
                string empty = string.Empty;
                PaySdk.PaymentProperty.GetPropertyValue(hashtable, Constants.GenericNamespace.MerchantAccount, Constants.MerchantAccountProperties.ServiceAccountId, out empty);
                PaySdk.PaymentProperty.GetPropertyValue(hashtable, Constants.GenericNamespace.Connector, Constants.ConnectorProperties.ConnectorName, out string str);
                if (!string.IsNullOrWhiteSpace(empty) && empty.Equals(serviceAccountId, StringComparison.OrdinalIgnoreCase) && (string.IsNullOrWhiteSpace(str) || str.Equals(paymentConnectorName, StringComparison.OrdinalIgnoreCase)))
                {
                    paymentPropertyArray = merchantProperty.PropertyList;
                    flag = true;
                    break;
                }
            }
            if (!flag)
            {
                RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "Payment connector name not found!", "ERROR");
                throw new CardPaymentException(CardPaymentException.MissingPaymentConnectorName, "Payment connector name not found!", new object[1] { paymentConnectorName });
            }

            return paymentPropertyArray;
        }

        public static PaySdk.PaymentProperty[] GetMerchantProperties(PaySdk.PaymentProperty[] merchantProperties, PaySdk.PaymentProperty[] paymentProperties, string paymentConnectorName)
        {
            ThrowIf.NullOrEmpty(paymentProperties, nameof(paymentProperties));
            PaySdk.PaymentProperty[] paymentPropertyArray = null;
            PaySdk.Hashtable hashtable = PaySdk.PaymentProperty.ConvertToHashtable(paymentProperties);
            string serviceAccountId = string.Empty;
            PaySdk.PaymentProperty.GetPropertyValue(hashtable, Constants.GenericNamespace.MerchantAccount, Constants.MerchantAccountProperties.ServiceAccountId, out serviceAccountId);
            paymentPropertyArray = GetSingleMerchantAccountProperties(merchantProperties, paymentConnectorName, serviceAccountId);
            return paymentPropertyArray;
        }

        public static string GetConnectorName(PaySdk.PaymentProperty[] merchantProperties)
        {
            PaySdk.Hashtable hashtable = PaySdk.PaymentProperty.ConvertToHashtable(merchantProperties);
            string empty = string.Empty;
            PaySdk.PaymentProperty.GetPropertyValue(hashtable, Constants.GenericNamespace.Connector, Constants.ConnectorProperties.ConnectorName, out empty);
            return !string.IsNullOrWhiteSpace(empty) ? empty : throw new CardPaymentException(CardPaymentException.MissingAssemblyNameInConfiguration, "Payment connector name not found!", Array.Empty<object>());
        }

        private static bool IsSupportedMerchantConnector(PaySdk.Hashtable merchantConnectorInformation, string requestedCurrency, string requestedCardType, out string connectorName)
        {
            ThrowIf.Null<string>(requestedCurrency, nameof(requestedCurrency));
            ThrowIf.Null<string>(requestedCardType, nameof(requestedCardType));
            bool flag1 = false;
            bool flag2 = false;
            if (PaySdk.PaymentProperty.GetPropertyValue(merchantConnectorInformation, Constants.GenericNamespace.MerchantAccount, Constants.MerchantAccountProperties.SupportedCurrencies, out string str1) && !string.IsNullOrWhiteSpace(str1))
                flag1 = str1.ToUpper(CultureInfo.InvariantCulture).Contains(requestedCurrency.ToUpperInvariant());
            if (PaySdk.PaymentProperty.GetPropertyValue(merchantConnectorInformation, Constants.GenericNamespace.MerchantAccount, Constants.MerchantAccountProperties.SupportedTenderTypes, out string str2) && !string.IsNullOrWhiteSpace(str2))
                flag2 = str2.ToUpper(CultureInfo.InvariantCulture).Contains(requestedCardType.ToUpperInvariant());
            PaySdk.PaymentProperty.GetPropertyValue(merchantConnectorInformation, Constants.GenericNamespace.Connector, Constants.ConnectorProperties.ConnectorName, out connectorName);
            return flag2 & flag1;
        }

        private static string GetCardSubTypeByTender(TenderInfo tenderInfo)
        {
            ThrowIf.Null<TenderInfo>(tenderInfo, nameof(tenderInfo));
            string empty = string.Empty;
            Enum.TryParse<CardType>(tenderInfo.CardTypeId.ToString(), out CardType result);
            PaySdk.CardType cardType;
            return result != CardType.InternationalCreditCard ? (!cardTypeMapping.TryGetValue(result, out cardType) ? result.ToString() : cardType.ToString()) : GetCardType(tenderInfo.CardNumber).ToString();
        }

        private static PaySdk.CardType GetCardType(string cardNumber)
        {
            PaySdk.CardType cardType = PaySdk.CardType.Unknown;
            if (!string.IsNullOrWhiteSpace(cardNumber) && cardNumber.Length > 2)
            {
                switch (cardNumber[0])
                {
                    case '3':
                        cardType = cardNumber[1] == '4' || cardNumber[1] == '7' ? PaySdk.CardType.Amex : PaySdk.CardType.Discover;
                        break;
                    case '4':
                        cardType = PaySdk.CardType.Visa;
                        break;
                    case '5':
                        cardType = PaySdk.CardType.MasterCard;
                        break;
                    case '6':
                        cardType = PaySdk.CardType.Discover;
                        break;
                    default:
                        cardType = PaySdk.CardType.Unknown;
                        break;
                }
            }
            return cardType;
        }

        private static void GetPaymentCard(List<PaySdk.PaymentProperty> properties, TenderInfo tenderInfo)
        {
            if (tenderInfo == null) return;

            PaySdk.CardType cardType = tenderInfo.CardTypeId != 7 ? GetCardType(tenderInfo.CardNumber) : PaySdk.CardType.GiftCard;
            if (!string.IsNullOrEmpty(tenderInfo.EncryptedPin))
            {
                cardType = PaySdk.CardType.Debit;
            }
            PaySdk.PaymentProperty paymentProperty = new PaySdk.PaymentProperty(Constants.GenericNamespace.PaymentCard, Constants.PaymentCardProperties.CardType, cardType.ToString());
            properties.Add(paymentProperty);
            paymentProperty = new PaySdk.PaymentProperty(Constants.GenericNamespace.PaymentCard, Constants.PaymentCardProperties.CardNumber, tenderInfo.CardNumber);
            properties.Add(paymentProperty);
            if (tenderInfo.ExpirationMonth > 0 && tenderInfo.ExpirationYear > 0)
            {
                paymentProperty = new PaySdk.PaymentProperty(Constants.GenericNamespace.PaymentCard, Constants.PaymentCardProperties.ExpirationMonth, tenderInfo.ExpirationMonth);
                properties.Add(paymentProperty);
                paymentProperty = new PaySdk.PaymentProperty(Constants.GenericNamespace.PaymentCard, Constants.PaymentCardProperties.ExpirationYear, tenderInfo.ExpirationYear);
                properties.Add(paymentProperty);
            }
            tenderInfo.IsSwipe = !string.IsNullOrWhiteSpace(tenderInfo.Track1) || !string.IsNullOrWhiteSpace(tenderInfo.Track2);
            paymentProperty = new PaySdk.PaymentProperty(Constants.GenericNamespace.PaymentCard, Constants.PaymentCardProperties.IsSwipe, tenderInfo.IsSwipe.ToString());
            properties.Add(paymentProperty);
            if (!string.IsNullOrWhiteSpace(tenderInfo.VoiceAuthorizationCode))
            {
                paymentProperty = new PaySdk.PaymentProperty(Constants.GenericNamespace.PaymentCard, Constants.PaymentCardProperties.VoiceAuthorizationCode, tenderInfo.VoiceAuthorizationCode);
                properties.Add(paymentProperty);
            }
            if (cardType == PaySdk.CardType.Debit)
            {
                if (tenderInfo.CashBackAmount > decimal.Zero)
                {
                    paymentProperty = new PaySdk.PaymentProperty(Constants.GenericNamespace.PaymentCard, Constants.PaymentCardProperties.CashBackAmount, tenderInfo.CashBackAmount);
                    properties.Add(paymentProperty);
                }
                paymentProperty = new PaySdk.PaymentProperty(Constants.GenericNamespace.PaymentCard, Constants.PaymentCardProperties.EncryptedPin, tenderInfo.EncryptedPin);
                properties.Add(paymentProperty);
                paymentProperty = new PaySdk.PaymentProperty(Constants.GenericNamespace.PaymentCard, Constants.PaymentCardProperties.AdditionalSecurityData, tenderInfo.AdditionalSecurityData);
                properties.Add(paymentProperty);
            }
            if (!tenderInfo.IsSwipe)
            {
                paymentProperty = new PaySdk.PaymentProperty(Constants.GenericNamespace.PaymentCard, Constants.PaymentCardProperties.CardVerificationValue, tenderInfo.CCID);
                properties.Add(paymentProperty);
                paymentProperty = new PaySdk.PaymentProperty(Constants.GenericNamespace.PaymentCard, Constants.PaymentCardProperties.Name, tenderInfo.Name);
                properties.Add(paymentProperty);
                paymentProperty = new PaySdk.PaymentProperty(Constants.GenericNamespace.PaymentCard, Constants.PaymentCardProperties.StreetAddress, tenderInfo.Address);
                properties.Add(paymentProperty);
                paymentProperty = new PaySdk.PaymentProperty(Constants.GenericNamespace.PaymentCard, Constants.PaymentCardProperties.City, tenderInfo.City);
                properties.Add(paymentProperty);
                paymentProperty = new PaySdk.PaymentProperty(Constants.GenericNamespace.PaymentCard, Constants.PaymentCardProperties.State, tenderInfo.StateOrProvince);
                properties.Add(paymentProperty);
                paymentProperty = new PaySdk.PaymentProperty(Constants.GenericNamespace.PaymentCard, Constants.PaymentCardProperties.PostalCode, tenderInfo.Zip);
                properties.Add(paymentProperty);
                paymentProperty = new PaySdk.PaymentProperty(Constants.GenericNamespace.PaymentCard, Constants.PaymentCardProperties.Country, tenderInfo.Country);
                properties.Add(paymentProperty);
            }
            else
            {
                paymentProperty = new PaySdk.PaymentProperty(Constants.GenericNamespace.PaymentCard, Constants.PaymentCardProperties.Track1, tenderInfo.Track1);
                properties.Add(paymentProperty);
                paymentProperty = new PaySdk.PaymentProperty(Constants.GenericNamespace.PaymentCard, Constants.PaymentCardProperties.Track2, tenderInfo.Track2);
                properties.Add(paymentProperty);
                paymentProperty = new PaySdk.PaymentProperty(Constants.GenericNamespace.PaymentCard, Constants.PaymentCardProperties.Track3, tenderInfo.Track3);
                properties.Add(paymentProperty);
            }

        }

        private static void GetTransactionData(List<PaySdk.PaymentProperty> properties, PaySdk.IndustryType industryType, PaySdk.PurchaseLevel purchaseLevel, bool allowPartialAuthorization, bool supportCardTokenization, Decimal amount, string currency, bool isTestMode, string terminalId)
        {
            if (isTestMode)
            {
                PaySdk.PaymentProperty paymentProperty = new PaySdk.PaymentProperty(Constants.GenericNamespace.TransactionData, Constants.TransactionDataProperties.IsTestMode, "true");
                properties.Add(paymentProperty);
            }
            PaySdk.PaymentProperty paymentProperty1 = new PaySdk.PaymentProperty(Constants.GenericNamespace.TransactionData, Constants.TransactionDataProperties.IndustryType, industryType.ToString());
            properties.Add(paymentProperty1);
            PaySdk.PaymentProperty paymentProperty2 = new PaySdk.PaymentProperty(Constants.GenericNamespace.TransactionData, Constants.TransactionDataProperties.PurchaseLevel, purchaseLevel.ToString());
            properties.Add(paymentProperty2);
            PaySdk.PaymentProperty paymentProperty3 = new PaySdk.PaymentProperty(Constants.GenericNamespace.TransactionData, Constants.TransactionDataProperties.AllowPartialAuthorization, allowPartialAuthorization.ToString());
            properties.Add(paymentProperty3);
            PaySdk.PaymentProperty paymentProperty4 = new PaySdk.PaymentProperty(Constants.GenericNamespace.TransactionData, Constants.TransactionDataProperties.SupportCardTokenization, supportCardTokenization.ToString());
            properties.Add(paymentProperty4);
            PaySdk.PaymentProperty paymentProperty5 = new PaySdk.PaymentProperty(Constants.GenericNamespace.TransactionData, Constants.TransactionDataProperties.Amount, amount);
            properties.Add(paymentProperty5);
            PaySdk.PaymentProperty paymentProperty6 = new PaySdk.PaymentProperty(Constants.GenericNamespace.TransactionData, Constants.TransactionDataProperties.CurrencyCode, currency);
            properties.Add(paymentProperty6);
            PaySdk.PaymentProperty paymentProperty7 = new PaySdk.PaymentProperty(Constants.GenericNamespace.TransactionData, Constants.TransactionDataProperties.TerminalId, terminalId);
            properties.Add(paymentProperty7);
        }

        public static void AddTransactionDataProperty(List<PaySdk.PaymentProperty> properties, string key, string value)
        {
            PaySdk.PaymentProperty paymentProperty = new PaySdk.PaymentProperty(Constants.GenericNamespace.TransactionData, key, value ?? string.Empty);
            properties.Add(paymentProperty);
        }

        public static void AddTransactionDataProperty(List<PaySdk.PaymentProperty> properties, string key, decimal value)
        {
            PaySdk.PaymentProperty paymentProperty = new PaySdk.PaymentProperty(Constants.GenericNamespace.TransactionData, key, value);
            properties.Add(paymentProperty);
        }

        public static void AddTransactionDataProperty(List<PaySdk.PaymentProperty> properties, string key, DateTime value)
        {
            PaySdk.PaymentProperty paymentProperty = new PaySdk.PaymentProperty(Constants.GenericNamespace.TransactionData, key, value);
            properties.Add(paymentProperty);
        }

        public static void AddTransactionDataProperty(List<PaySdk.PaymentProperty> properties, string key, PaySdk.PaymentProperty[] value)
        {
            PaySdk.PaymentProperty paymentProperty = new PaySdk.PaymentProperty(Constants.GenericNamespace.TransactionData, key, value);
            properties.Add(paymentProperty);
        }

        public static void AddCommerceProperty(ICollection<CommerceProperty> extProperties, string key, string value)
        {
            CommerceProperty comProp = new CommerceProperty { Key = key, Value = value };
            extProperties.Add(comProp);
        }

        public static void AddExtensionTransactionProperties(IList<PaySdk.PaymentProperty> properties, ExtensionTransaction extensionTransactionProperties)
        {
            ThrowIf.Null<IList<PaySdk.PaymentProperty>>(properties, "properties");
            if (extensionTransactionProperties == null || extensionTransactionProperties.ExtensionProperties == null || extensionTransactionProperties.ExtensionProperties.Count == 0)
            {
                return;
            }
            PaySdk.PaymentProperty paymentProperty = null;
            foreach (CommerceProperty extensionProperty in extensionTransactionProperties.ExtensionProperties)
            {
                if (extensionProperty.Value.HasBeenSet)
                {
                    string upperInvariant = extensionProperty.Value.GetPropertyType().Name.ToUpperInvariant();
                    if (upperInvariant == "STRING")
                    {
                        paymentProperty = new PaySdk.PaymentProperty(Constants.GenericNamespace.TransactionData, extensionProperty.Key, extensionProperty.Value.StringValue);
                    }
                    else if (upperInvariant == "DECIMAL")
                    {
                        paymentProperty = new PaySdk.PaymentProperty(Constants.GenericNamespace.TransactionData, extensionProperty.Key, (extensionProperty.Value.DecimalValue.HasValue ? extensionProperty.Value.DecimalValue.Value : decimal.Zero));
                    }
                    else
                    {
                        if (upperInvariant != "DATETIMEOFFSET")
                        {
                            RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "Parameter type not supported use String / Decimal or Datetime", "ERROR");
                            throw new ArgumentException("Parameter type not supported use String/Decimal or Datetime", extensionProperty.Key);
                        }
                        paymentProperty = new PaySdk.PaymentProperty(PaySdk.Constants.GenericNamespace.TransactionData, extensionProperty.Key, (extensionProperty.Value.DateTimeOffsetValue.HasValue ? extensionProperty.Value.DateTimeOffsetValue.Value.DateTime : DateTime.MinValue));
                    }
                    properties.Add(paymentProperty);
                }
            }
        }

        public static void MapAuthorizeResponse(PaySdk.Response response, PaymentInfo paymentInfo, Guid key, string terminalId)
        {
            ThrowIf.Null<PaySdk.Response>(response, "response");
            ThrowIf.Null<PaymentInfo>(paymentInfo, "paymentInfo");
            try
            {
                paymentInfo.Errors = response.Errors;
                if (response.Properties != null)
                {
                    List<PaySdk.PaymentProperty> list = new List<PaySdk.PaymentProperty>(response.Properties);
                    if (key == Guid.Empty)
                    {
                        Microsoft.Dynamics.Retail.Diagnostics.RetailLogger.Log.HardwareStationCardStorageKeyEmpty();
                        RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "key is Empty in MapAuthorizeResponse method.", "ERROR");
                    }
                    else
                    {
                        PaySdk.PaymentProperty paymentProperty = new PaySdk.PaymentProperty(Constants.GenericNamespace.Connector, Constants.ConnectorProperties.CardKey, key.ToString());
                        list.Add(paymentProperty);
                    }
                    if (!string.IsNullOrEmpty(terminalId))
                    {
                        list.Add(new PaySdk.PaymentProperty(Constants.GenericNamespace.AuthorizationResponse, Constants.TransactionDataProperties.TerminalId, terminalId));
                    }
                    else
                    {
                        RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "Terminal Id is null in MapAuthorizeResponse method.");
                    }
                    paymentInfo.PaymentSdkData = PaySdk.PaymentProperty.ConvertPropertyArrayToXML(list.ToArray());
                    PaySdk.Hashtable hashtable = PaySdk.PaymentProperty.ConvertToHashtable(response.Properties);
                    decimal num = new decimal(0, 0, 0, false, 1);
                    PaySdk.PaymentProperty.GetPropertyValue(hashtable, Constants.GenericNamespace.AuthorizationResponse, Constants.AuthorizationResponseProperties.ApprovedAmount, out num);
                    paymentInfo.ApprovedAmount = num;
					
					decimal numSurcharge = new decimal(0, 0, 0, false, 1);
                    PaySdk.PaymentProperty.GetPropertyValue(hashtable, Constants.GenericNamespace.AuthorizationResponse, "TipAmount", out numSurcharge);
                    paymentInfo.TipAmount = numSurcharge;
                }
                else
                {
                    RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "response.Properties is null in MapAuthorizeResponse method.", "ERROR");
                }
                string authorizationResponse = Constants.GenericNamespace.AuthorizationResponse;
                string authorizationResult = Constants.AuthorizationResponseProperties.AuthorizationResult;
                List<string> list1 = new List<string>
                    {
                        PaySdk.AuthorizationResult.Success.ToString(),
                        PaySdk.AuthorizationResult.PartialAuthorization.ToString()
                    };
                paymentInfo.IsApproved = VerifyResponseResult("MapAuthorizeResponse", response, authorizationResponse, authorizationResult, list1, PaymentErrors.UnableToAuthorizePayment, paymentInfo);
            }
            catch (Exception ex)
            {
                RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "MapAuthorizeResponse method in AuthorizePaymentAsync Error: " + ex.Message, "ERROR");
                throw ex;
            }
        }

        public static void MapCaptureResponse(PaySdk.Response response, PaymentInfo paymentInfo)
        {
            ThrowIf.Null<PaySdk.Response>(response, nameof(response));
            ThrowIf.Null<PaymentInfo>(paymentInfo, nameof(paymentInfo));
            paymentInfo.Errors = response.Errors;
            if (response.Properties != null)
                paymentInfo.PaymentSdkData = PaySdk.PaymentProperty.ConvertPropertyArrayToXML(response.Properties);
            PaymentInfo paymentInfo1 = paymentInfo;
            PaySdk.Response response1 = response;
            string captureResponse = Constants.GenericNamespace.CaptureResponse;
            string captureResult = Constants.CaptureResponseProperties.CaptureResult;
            List<string> expectedValues = new List<string>
            {
                PaySdk.CaptureResult.Success.ToString()
            };
            string toCapturePayment = PaymentErrors.UnableToCapturePayment;
            PaymentInfo paymentInfo2 = paymentInfo;
            int num = VerifyResponseResult(nameof(MapCaptureResponse), response1, captureResponse, captureResult, expectedValues, toCapturePayment, paymentInfo2) ? 1 : 0;
            paymentInfo1.IsApproved = num != 0;
        }

        public static void MapRefundResponse(PaySdk.Response response, PaymentInfo paymentInfo)
        {
            ThrowIf.Null(response, nameof(response));
            ThrowIf.Null(paymentInfo, nameof(paymentInfo));
            paymentInfo.Errors = response.Errors;
            if (response.Properties != null)
            {
                paymentInfo.PaymentSdkData = PaySdk.PaymentProperty.ConvertPropertyArrayToXML(response.Properties);
                PaySdk.Hashtable hashtable = PaySdk.PaymentProperty.ConvertToHashtable(response.Properties);
                Decimal num = 0.0M;
                PaySdk.PaymentProperty.GetPropertyValue(hashtable, Constants.GenericNamespace.RefundResponse, Constants.RefundResponseProperties.ApprovedAmount, out num);
                paymentInfo.ApprovedAmount = num;
            }
            PaymentInfo paymentInfo1 = paymentInfo;
            PaySdk.Response response1 = response;
            string refundResponse = Constants.GenericNamespace.RefundResponse;
            string refundResult = Constants.RefundResponseProperties.RefundResult;
            List<string> expectedValues = new List<string>();
            expectedValues.Add(PaySdk.RefundResult.Success.ToString());
            string unableToRefundPayment = PaymentErrors.UnableToRefundPayment;
            PaymentInfo paymentInfo2 = paymentInfo;
            int num1 = VerifyResponseResult(nameof(MapRefundResponse), response1, refundResponse, refundResult, expectedValues, unableToRefundPayment, paymentInfo2) ? 1 : 0;
            paymentInfo1.IsApproved = num1 != 0;
        }

        public static void MapVoidResponse(PaySdk.Response response, PaymentInfo paymentInfo)
        {
            ThrowIf.Null<PaySdk.Response>(response, nameof(response));
            ThrowIf.Null<PaymentInfo>(paymentInfo, nameof(paymentInfo));
            paymentInfo.Errors = response.Errors;
            if (response.Properties != null)
                paymentInfo.PaymentSdkData = PaySdk.PaymentProperty.ConvertPropertyArrayToXML(response.Properties);
            PaymentInfo paymentInfo1 = paymentInfo;
            PaySdk.Response response1 = response;
            string voidResponse = Constants.GenericNamespace.VoidResponse;
            string voidResult = Constants.VoidResponseProperties.VoidResult;
            List<string> expectedValues = new List<string>
            {
                PaySdk.VoidResult.Success.ToString()
            };
            string unableToCancelPayment = PaymentErrors.UnableToCancelPayment;
            PaymentInfo paymentInfo2 = paymentInfo;
            int num = VerifyResponseResult(nameof(MapVoidResponse), response1, voidResponse, voidResult, expectedValues, unableToCancelPayment, paymentInfo2) ? 1 : 0;
            paymentInfo1.IsApproved = num != 0;
        }

        public static void MapTokenResponse(PaySdk.Response response, PaymentInfo paymentInfo)
        {
            ThrowIf.Null<PaySdk.Response>(response, nameof(response));
            ThrowIf.Null<PaymentInfo>(paymentInfo, nameof(paymentInfo));
            paymentInfo.IsApproved = response.Errors == null;
            paymentInfo.Errors = response.Errors;
            if (response.Properties == null)
                return;
            PaySdk.PaymentProperty propertyFromHashtable = PaySdk.PaymentProperty.GetPropertyFromHashtable(PaySdk.PaymentProperty.ConvertToHashtable(response.Properties), Constants.GenericNamespace.PaymentCard, Constants.PaymentCardProperties.UniqueCardId);
            string str = propertyFromHashtable != null ? propertyFromHashtable.StringValue : string.Empty;
            paymentInfo.PaymentSdkData = str + PaySdk.PaymentProperty.ConvertPropertyArrayToXML(response.Properties);
        }
        public static void MapGiftCardResponse(PaySdk.Response response, PaymentInfo paymentInfo, string responseNamespace, string giftCardResponseProperties, string terminalId)
        {
            ThrowIf.Null(response, nameof(response));
            ThrowIf.Null(paymentInfo, nameof(paymentInfo));
            paymentInfo.Errors = response.Errors;
            if (response.Properties != null)
            {
                List<PaySdk.PaymentProperty> paymentPropertyList = new List<PaySdk.PaymentProperty>(response.Properties);
                if (!string.IsNullOrEmpty(terminalId))
                    paymentPropertyList.Add(new PaySdk.PaymentProperty(Constants.GenericNamespace.AuthorizationResponse, Constants.TransactionDataProperties.TerminalId, terminalId));
                paymentInfo.PaymentSdkData = PaySdk.PaymentProperty.ConvertPropertyArrayToXML(paymentPropertyList.ToArray());
                PaySdk.Hashtable hashtable = PaySdk.PaymentProperty.ConvertToHashtable(response.Properties);
                Decimal num = 0.0M;
                PaySdk.PaymentProperty.GetPropertyValue(hashtable, responseNamespace, Constants.ActivateGiftCardResponseProperties.AvailableBalance, out num);
                paymentInfo.AvailableBalanceAmount = num;
            }
            PaymentInfo paymentInfo1 = paymentInfo;
            PaySdk.Response response1 = response;
            string responseNamespace1 = responseNamespace;
            string resultProperty = giftCardResponseProperties;
            List<string> expectedValues = new List<string>();
            expectedValues.Add(PaySdk.ActivateGiftCardResult.Success.ToString());
            string authorizePayment = PaymentErrors.UnableToAuthorizePayment;
            PaymentInfo paymentInfo2 = paymentInfo;
            int num1 = VerifyResponseResult(nameof(MapGiftCardResponse), response1, responseNamespace1, resultProperty, expectedValues, authorizePayment, paymentInfo2) ? 1 : 0;
            paymentInfo1.IsApproved = num1 != 0;
        }

        private static bool VerifyResponseResult(string operationName, PaySdk.Response response, string responseNamespace, string resultProperty, List<string> expectedValues, string errorResourceId, PaymentInfo paymentInfo)
        {
            RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "VerifyResponseResult started from MapAuthorizeResponse method.");
            bool flag = true;
            PaySdk.PaymentProperty propertyFromHashtable = null;
            if (response.Properties != null)
            {
                PaySdk.Hashtable hashtable = PaySdk.PaymentProperty.ConvertToHashtable(response.Properties);
                propertyFromHashtable = PaySdk.PaymentProperty.GetPropertyFromHashtable(hashtable, responseNamespace, resultProperty);
            }
            if (propertyFromHashtable == null || !expectedValues.Where(s => s.Equals(propertyFromHashtable.StringValue, StringComparison.OrdinalIgnoreCase)).Any())
            {
                RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "VerifyErrors started from VerifyResponseResult method.");
                VerifyErrors(operationName, response.Errors, errorResourceId, paymentInfo);
                if (response.Errors.Length > 0)
                {
                    if (response.Errors.Any(a => a.Code.Equals(PaySdk.ErrorCode.NoTransactionFoundByTransactionReference)))
                    {
                        return false;
                    }
                }
                throw new PaymentException(errorResourceId, "{0} failed.", new object[1] { operationName });
            }
            return flag;
        }

        private static void VerifyErrors(string operationName, PaySdk.PaymentError[] errors, string errorResourceId, PaymentInfo paymentInfo)
        {
            StringBuilder stringBuilder = new StringBuilder("Error returned from the payment connector. See error details below: ");
            if (errors != null && (errors.Length != 1 || !errors.Any(pe => DoNotThrowOnCodes.Contains(pe.Code))))
            {
                stringBuilder.Append(string.Join(Environment.NewLine, errors.Select(error => string.Format("ErrorCode: {0}, ErrorMessage: {1}, IsLocalized: {2}", error.Code, error.Message, error.IsLocalized))).ToArray());
                string message = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0} failed. Error(s): {1}", operationName, stringBuilder);
                RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", message, "ERROR");
                throw new PaymentException(errorResourceId, message, paymentInfo, errors);
            }
        }

        public static void SetTransactionProp(ref PaymentInfo paymentInfo, ref PaySdk.Response response)
        {
            if (response.Properties != null)
            {
                ICollection<CommerceProperty> extProperties = new System.Collections.ObjectModel.Collection<CommerceProperty>();
                var props = response.Properties.ToList();
                foreach (var prop in props)
                {
                    if (prop.PropertyList != null)
                    {
                        foreach (var newProp in prop.PropertyList)
                        {
                            extProperties.Add(new CommerceProperty
                            {
                                Key = newProp.Name,
                                Value = newProp.StringValue ?? newProp.DecimalValue.ToString() ?? ""
                            });
                        }
                    }
                    else
                    {
                        extProperties.Add(new CommerceProperty
                        {
                            Key = prop.Name,
                            Value = prop.StringValue ?? prop.DecimalValue.ToString() ?? ""
                        });
                    }
                }
                paymentInfo.ExtensionProperties = extProperties;
            }
        }

        public static void SetTransactionProp(ref ExtensionTransaction extensionTransaction, PaySdk.Response response)
        {
            ICollection<CommerceProperty> extProperties = new System.Collections.ObjectModel.Collection<CommerceProperty>();
            foreach (PaySdk.PaymentProperty property in response.Properties)
            {
                CommerceProperty commerceProperty = null;
                switch (property.ValueType)
                {
                    case PaySdk.DataType.String:
                        commerceProperty = new CommerceProperty(property.Name, (object)property.StringValue ?? "");
                        break;
                    case PaySdk.DataType.Decimal:
                        commerceProperty = new CommerceProperty(property.Name, (object)property.DecimalValue ?? 0);
                        break;
                    //case PaySdk.DataType.DateTime:
                    //    commerceProperty = new CommerceProperty(property.Name, (object)property.DateValue);
                    //    break;
                    case PaySdk.DataType.PropertyList:
                        foreach (PaySdk.PaymentProperty property2 in property.PropertyList)
                        {
                            CommerceProperty commerceProperty2 = null;
                            switch (property2.ValueType)
                            {
                                case PaySdk.DataType.String:
                                    commerceProperty2 = new CommerceProperty(property2.Name, (object)property2.StringValue ?? "");
                                    break;
                                case PaySdk.DataType.Decimal:
                                    commerceProperty2 = new CommerceProperty(property2.Name, (object)property.DecimalValue ?? 0);
                                    break;
                                //case PaySdk.DataType.DateTime:
                                //  commerceProperty2 = new CommerceProperty(property2.Name, (object)property2.DateValue);
                                //  break;
                                default:
                                    break;
                                    //throw new NotSupportedException("Property type not supported");
                            }
                            if (commerceProperty2 != null)
                                extProperties.Add(commerceProperty2);
                        }
                        break;
                    default:
                        break;
                        //throw new NotSupportedException("Property type not supported");

                }
                if (commerceProperty != null)
                    extProperties.Add(commerceProperty);
            }
            if (response.Errors != null && response.Errors.Length > 0)
            {
                RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", response.Errors[0].Message, "ERROR");
                CommerceProperty commerceProperty = new CommerceProperty("Error", response.Errors[0].Message);
                extProperties.Add(commerceProperty);
            }
            extensionTransaction.ExtensionProperties = extProperties;
        }

    }
}
