using Microsoft.Dynamics.Commerce.HardwareStation;
using Microsoft.Dynamics.Retail.PaymentSDK.Portable;
using System;
using System.Collections.Generic;
using System.Text;
using RR.Utility;

namespace Contoso.Commerce.HardwareStation.Extension.WindowsPrinterSample
{
    public static class PaymentProcessor
    {
        private static IPaymentProcessor Processor;
        public static IPaymentProcessor Get(string paymentConnectorName)
        {
            if(string.IsNullOrEmpty(paymentConnectorName))
            {
                RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "Payment connector name not found! paymentConnectorName: " + paymentConnectorName, "ERROR");
                throw new CardPaymentException(CardPaymentException.MissingPaymentConnectorName, "Payment connector name not found!", new object[1] { paymentConnectorName });
            }
              

            switch (paymentConnectorName)
            {
                case Connectors.NBEConnector:
                    Processor = new RR.Dynamics.Retail.Payment.NBE.Portable.NBEConnector();
                    break;
                case "banqueMisrConnector":
                    Processor = new Microsoft.Dynamics.Retail.SampleConnector.Portable.SampleConnector();
                    break;
                default:
                    RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "Payment connector name not found! paymentConnectorName: " + paymentConnectorName, "ERROR");
                    throw new CardPaymentException(CardPaymentException.MissingPaymentConnectorName, "Payment connector name not found!", new object[1] { paymentConnectorName });
            }
            return Processor;
        }

    }
}
