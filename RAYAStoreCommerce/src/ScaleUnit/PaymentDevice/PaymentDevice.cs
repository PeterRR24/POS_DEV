
namespace RR.PaymentDevice
{
    using System;
    using System.Linq;
    using System.Text;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.Dynamics.Commerce.HardwareStation;
    using Microsoft.Dynamics.Commerce.HardwareStation.CardPayment;
    using Microsoft.Dynamics.Commerce.HardwareStation.PeripheralRequests;
    using Microsoft.Dynamics.Commerce.HardwareStation.Peripherals;
    using Microsoft.Dynamics.Commerce.HardwareStation.Peripherals.Entities;
    using Microsoft.Dynamics.Commerce.HardwareStation.Peripherals.PaymentTerminal;
    using Microsoft.Dynamics.Commerce.Runtime.Handlers;
    using Microsoft.Dynamics.Commerce.Runtime.Messages;
    using Microsoft.Dynamics.Commerce.Runtime.DataModel;
    using PaySdk = Microsoft.Dynamics.Retail.PaymentSDK.Portable;
    using Constants = Microsoft.Dynamics.Retail.PaymentSDK.Portable.Constants;

    using RR.Utility;
    using System.Text.RegularExpressions;
    using Newtonsoft.Json;
    using Microsoft.Dynamics.Retail.Diagnostics;
    using Microsoft.Dynamics.Retail.Diagnostics.Extensions;
    using Microsoft.Dynamics.Retail.PaymentSDK.Portable.Constants;

    /// <summary>
    /// A sample class that handles all messages that a Payment Device is responsible for.
    /// </summary>
    /// <remarks>
    /// This class intentionally implements a no-op Payment device handler as a way to demonstrate the capabilities that this handler provides
    /// and also the information that it requires to work with the hardware station infrastructure.
    /// The functionalities supported by this handler are the following:
    /// - Device management:
    ///   LockPaymentTerminalDeviceRequest, ReleasePaymentTerminalDeviceRequest, OpenPaymentTerminalDeviceRequest,
    ///   ClosePaymentTerminalDeviceRequest, BeginTransactionPaymentTerminalDeviceRequest, EndTransactionPaymentTerminalDeviceRequest,
    ///   CancelOperationPaymentTerminalDeviceRequestCancel Operation and UpdateLineItemsPaymentTerminalDeviceRequest
    /// - Payment operations:
    ///   AuthorizePaymentTerminalDeviceRequest, CapturePaymentTerminalDeviceRequest, VoidPaymentTerminalDeviceRequest, RefundPaymentTerminalDeviceRequest
    /// - Gift operations:
    ///   GetGiftCardBalancePaymentTerminalRequest, AddBalanceToGiftCardPaymentTerminalRequest, ActivateGiftCardPaymentTerminalRequest
    /// - Duplicated Payment Protection:
    ///   GetTransactionReferencePaymentTerminalDeviceRequest, GetTransactionByTransactionReferencePaymentTerminalDeviceRequest
    /// - Misc operations:
    ///   GetPrivateTenderPaymentTerminalDeviceRequest, ExecuteTaskPaymentTerminalDeviceRequest, FetchTokenPaymentTerminalDeviceRequest
    /// In the case of unsupported operations by the Payment Device, the request must still be supported, but an exception should be thrown.
    /// A real implementation should replace all mock and hard-coded information with properties retrieved from the actual device.
    /// </remarks>
#pragma warning disable CS0618 // Type or member is obsolete. JUSTIFICATION: TODO: transform to asynchronous handler 
    public class DeviceHandler : INamedRequestHandler, IDisposable
#pragma warning restore CS0618 // Type or member is obsolete
    {
        // Name of the Device configured on the hardware profile.
        private const string DeviceName = "RRPDNBE";
        private const int TaskDelayInMilliSeconds = 20;
        private const string DummyCardNumber = "4111111111111111";
        private const string DummyCardNoMasked = "4111********";

        //private SettingsInfo terminalSettings;
        private CancellationTokenSource timeoutTask;
        private PaySdk.IPaymentProcessor processor;
        private PaySdk.IPaymentProcessorExtension processorExt;
        private PaySdk.PaymentProperty[] merchantProperties;
        //private PaySdk.PaymentProperty[] allMerchantProperties;
        //private KeyValuePair<string, string> keySettings;
        private DeviceInstanceVars div;
        private TenderInfo tenderInfo;
        //private bool isDevicePair;
        private string VSCACounterDPP = "";
        private string SensePassPairCode { get; set; }
		
        /// <summary>
        /// Gets the collection of supported request types by this handler.
        /// </summary>
        public IEnumerable<Type> SupportedRequestTypes
        {
            get
            {
                return new[]
                {
                    // PaymentTerminalDeviceRequests
                    typeof(LockPaymentTerminalDeviceRequest),
                    typeof(ReleasePaymentTerminalDeviceRequest),
                    typeof(OpenPaymentTerminalDeviceRequest),
                    typeof(ClosePaymentTerminalDeviceRequest),
                    typeof(BeginTransactionPaymentTerminalDeviceRequest),
                    typeof(EndTransactionPaymentTerminalDeviceRequest),
                    typeof(CancelOperationPaymentTerminalDeviceRequest),
                    typeof(UpdateLineItemsPaymentTerminalDeviceRequest),
                    typeof(AuthorizePaymentTerminalDeviceRequest),
                    typeof(CapturePaymentTerminalDeviceRequest),
                    typeof(VoidPaymentTerminalDeviceRequest),
                    typeof(RefundPaymentTerminalDeviceRequest),
                    typeof(GetPrivateTenderPaymentTerminalDeviceRequest),
                    typeof(ExecuteTaskPaymentTerminalDeviceRequest),
                    typeof(FetchTokenPaymentTerminalDeviceRequest),
                    typeof(ActivateGiftCardPaymentTerminalRequest),
                    typeof(AddBalanceToGiftCardPaymentTerminalRequest),
                    typeof(GetGiftCardBalancePaymentTerminalRequest),
                    typeof(CashoutGiftCardPaymentTerminalRequest),
                    typeof(GetTransactionReferencePaymentTerminalDeviceRequest),
                    typeof(GetTransactionByTransactionReferencePaymentTerminalDeviceRequest),
                    // CardPaymentRequests
                    typeof(AuthorizePaymentCardPaymentRequest),
                    typeof(AuthorizeCardTokenCardPaymentRequest),
                    typeof(CapturePaymentCardPaymentRequest),
                    typeof(VoidPaymentCardPaymentRequest),
                    typeof(RefundCaptureTokenCardPaymentRequest),
                };
            }
        }

        /// <summary>
        /// Gets the unique name for this request handler.
        /// </summary>
        public string HandlerName
        {
            get
            {
                return DeviceName;
            }
        }

        /// <summary>
        /// Represents the entry point of the request handler.
        /// </summary>
        /// <param name="request">The incoming request message.</param>
        /// <returns>The outgoing response message.</returns>
        public Response Execute(Request request)
        {
            ThrowIf.Null(request, nameof(request));

            Response response;

            Type requestType = request.GetType();

            if (requestType == typeof(OpenPaymentTerminalDeviceRequest))
            {
                response = this.Open((OpenPaymentTerminalDeviceRequest)request);
            }
            else if (requestType == typeof(ClosePaymentTerminalDeviceRequest))
            {
                response = this.Close();
            }
            else if (requestType == typeof(LockPaymentTerminalDeviceRequest))
            {
                response = this.Lock((LockPaymentTerminalDeviceRequest)request);
            }
            else if (requestType == typeof(ReleasePaymentTerminalDeviceRequest))
            {
                response = this.UnLock((ReleasePaymentTerminalDeviceRequest)request);
            }
            else if (requestType == typeof(BeginTransactionPaymentTerminalDeviceRequest))
            {
                response = this.BeginTransaction((BeginTransactionPaymentTerminalDeviceRequest)request);
            }
            else if (requestType == typeof(EndTransactionPaymentTerminalDeviceRequest))
            {
                response = this.EndTransaction();
            }
            else if (requestType == typeof(CancelOperationPaymentTerminalDeviceRequest))
            {
                response = this.CancelOperation();
            }
            else if (requestType == typeof(UpdateLineItemsPaymentTerminalDeviceRequest))
            {
                response = this.UpdateLineItems((UpdateLineItemsPaymentTerminalDeviceRequest)request);
            }
            else if (requestType == typeof(AuthorizePaymentTerminalDeviceRequest))
            {
                response = this.AuthorizePayment((AuthorizePaymentTerminalDeviceRequest)request);
            }
            else if (requestType == typeof(CapturePaymentTerminalDeviceRequest))
            {
                response = this.CapturePayment((CapturePaymentTerminalDeviceRequest)request);
            }
            else if (requestType == typeof(VoidPaymentTerminalDeviceRequest))
            {
                response = this.VoidPayment((VoidPaymentTerminalDeviceRequest)request);
            }
            else if (requestType == typeof(RefundPaymentTerminalDeviceRequest))
            {
                response = this.RefundPayment((RefundPaymentTerminalDeviceRequest)request);
            }
            else if (requestType == typeof(GetPrivateTenderPaymentTerminalDeviceRequest))
            {
                response = this.GetPrivateTender((GetPrivateTenderPaymentTerminalDeviceRequest)request);
            }
            else if (requestType == typeof(ExecuteTaskPaymentTerminalDeviceRequest))
            {
                response = this.ExecuteTask((ExecuteTaskPaymentTerminalDeviceRequest)request);
            }
            else if (requestType == typeof(FetchTokenPaymentTerminalDeviceRequest))
            {
                response = this.FetchToken((FetchTokenPaymentTerminalDeviceRequest)request);
            }
            
           
            else if (requestType == typeof(GetTransactionReferencePaymentTerminalDeviceRequest))
            {
                response = this.GetTransactionReferenceId((GetTransactionReferencePaymentTerminalDeviceRequest)request);
            }
            else if (requestType == typeof(GetTransactionByTransactionReferencePaymentTerminalDeviceRequest))
            {
                response = this.GetTransactionByTransactionReference((GetTransactionByTransactionReferencePaymentTerminalDeviceRequest)request);
            }
            else if (requestType == typeof(AuthorizeCardTokenCardPaymentRequest))
            {
                return this.AuthorizeCardTokenCardPayment((AuthorizeCardTokenCardPaymentRequest)request);
            }
            else if (requestType == typeof(AuthorizePaymentCardPaymentRequest))
            {
                return this.AuthorizePaymentCardPayment((AuthorizePaymentCardPaymentRequest)request);
            }
            else if (requestType == typeof(RefundCaptureTokenCardPaymentRequest))
            {
                return this.RefundCaptureTokenCardPayment((RefundCaptureTokenCardPaymentRequest)request);
            }
            else if (requestType == typeof(VoidPaymentCardPaymentRequest))// Multiple connector
            {
                return this.VoidCardPayment((VoidPaymentCardPaymentRequest)request);
            }
            else if (requestType == typeof(CapturePaymentCardPaymentRequest))
            {
                return this.CapturePaymentCardPayment((CapturePaymentCardPaymentRequest)request);
            }
            else
            {
                RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", $"Request '{requestType.FullName}' is not supported.");
                throw new NotSupportedException($"Request '{requestType.FullName}' is not supported.");
            }

            return response;
        }

        /// <summary>
        /// The dispose implementation.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose the Native and Managed resources.
        /// </summary>
        /// <param name="disposeAll">Whether to dispose both Native and Managed resources.</param>
        protected virtual void Dispose(bool disposeAll)
        {
        }

        /// <summary>
        /// Resets the card payment info.
        /// </summary>
        private void ResetCardPaymentInfo()
        {
            this.merchantProperties = null;
            div.PaymentConnectorName = null;
            div.IsTestMode = false;
            div.TerminalSettings = null;
            this.timeoutTask = null;
        }

        /// <summary>
        /// Gets an array of payment properties.
        /// </summary>
        /// <param name="paymentConnectorName">The payment connector name to use.</param>
        /// <returns></returns>
        private static PaySdk.PaymentProperty[] GetPaymentProperties(string paymentConnectorName)
        {
            // Several below values are hardcoded, real implementations should take this data from real device.
            PaySdk.PaymentProperty assemblyNameProperty = new PaySdk.PaymentProperty();
            assemblyNameProperty.Namespace = PaySdk.Constants.GenericNamespace.MerchantAccount;
            assemblyNameProperty.Name = PaySdk.Constants.MerchantAccountProperties.AssemblyName;
            assemblyNameProperty.ValueType = PaySdk.DataType.String;
            assemblyNameProperty.StoredStringValue = typeof(DeviceHandler).Assembly.GetName().Name;

            PaySdk.PaymentProperty connectorNameProperty = new PaySdk.PaymentProperty();
            connectorNameProperty.Namespace = PaySdk.Constants.GenericNamespace.Connector;
            connectorNameProperty.Name = PaySdk.Constants.ConnectorProperties.ConnectorName;
            connectorNameProperty.StoredStringValue = paymentConnectorName;

            PaySdk.PaymentProperty[] properties = new PaySdk.PaymentProperty[] { assemblyNameProperty, connectorNameProperty };

            return properties;
        }

        /// <summary>
        /// Construct the Payment Device class and open the connection from it.
        /// </summary>
        /// <returns>The response.</returns>
        /// <param name="request">Open payment terminal device request.</param>
        [Obsolete]
        private NullResponse Open(OpenPaymentTerminalDeviceRequest request)
        {
            ThrowIf.Null(request, nameof(request));

            // Open the device for payments
            Utilities.WaitAsyncTask(() => Task.Run(async () =>
            {
                await this.OpenAsync(request.DeviceName, request.TerminalSettings, request.DeviceConfig).ConfigureAwait(false);
            }));

            return new NullResponse();
            //return NullResponse.Instance;
        }

        /// <summary>
        /// Close the connection to the payment terminal.
        /// </summary>
        /// <returns>The response.</returns>
        /// 
        [Obsolete]
        private NullResponse Close()
        {
            // Close the device for payments
            Utilities.WaitAsyncTask(() => Task.Run(async () =>
            {
                await this.CloseAsync().ConfigureAwait(false);
            }));

            return new NullResponse();
            //return NullResponse.Instance;
        }

        /// <summary>
        /// Construct the Payment Device class and lock the connection from it.
        /// </summary>
        /// <returns>The response.</returns>
        /// <param name="request">Lock payment terminal device request.</param>
        private LockPaymentTerminalDeviceResponse Lock(LockPaymentTerminalDeviceRequest request)
        {
            ThrowIf.Null(request, nameof(request));

            // Lock the device for payments
            Utilities.WaitAsyncTask(() => Task.Run(async () =>
            {
                await Task.Delay(TaskDelayInMilliSeconds).ConfigureAwait(false);
            }));
            string token = new Guid().ToString();

            return new LockPaymentTerminalDeviceResponse(token);
        }
        [Obsolete]
        private NullResponse UnLock(ReleasePaymentTerminalDeviceRequest request)
        {
            ThrowIf.Null(request, nameof(request));

            // Lock the device for payments
            Utilities.WaitAsyncTask(() => Task.Run(async () =>
            {
                await Task.Delay(TaskDelayInMilliSeconds).ConfigureAwait(false);
            }));

            return new NullResponse();
           // return NullResponse.Instance;
        }

        /// <summary>
        ///  Begins the transaction.
        /// </summary>
        /// <param name="request">The begin transaction request.</param>
        /// <returns>The response.</returns>
        [Obsolete]
        private NullResponse BeginTransaction(BeginTransactionPaymentTerminalDeviceRequest request)
        {
            ThrowIf.Null(request, nameof(request));
            //Utilities.WaitAsyncTask(() => Task.Run(async () =>
            //{

            //}));
            RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "BeginTransaction call is completed");
            if (string.IsNullOrEmpty(request.MerchantInformation)) return new NullResponse();//return NullResponse.Instance; 
            PaySdk.PaymentProperty[] merchantProperties = PaymentHelper.ToLocalProperties(request.MerchantInformation); //  PaySdk.PaymentProperty.ConvertXMLToPropertyArray(request.MerchantInformation);
            if (request.ExtensionTransactionProperties != null && request.ExtensionTransactionProperties.ExtensionProperties != null && request.ExtensionTransactionProperties.ExtensionProperties.Count > 0)
            {
                CommerceProperty prop = request.ExtensionTransactionProperties.ExtensionProperties.FirstOrDefault(a => a.Key == "TerminalId");
                div.RRTerminalId = prop != null ? prop.Value.StringValue : string.Empty;
                prop = request.ExtensionTransactionProperties.ExtensionProperties.FirstOrDefault(a => a.Key == "CartId");
                div.RRCartId = prop != null ? prop.Value.StringValue : string.Empty;
                prop = request.ExtensionTransactionProperties.ExtensionProperties.FirstOrDefault(a => a.Key == "RRStaffId");
                if (prop != null)
                {
                    if (!string.IsNullOrEmpty(prop.Value.StringValue))
                        div.RRStaffId = prop != null ? prop.Value.StringValue : string.Empty;
                }

                
                prop = request.ExtensionTransactionProperties.ExtensionProperties.FirstOrDefault(a => a.Key == "LogAppInsightsKey");
                div.LogAppInsightsKey = prop != null ? prop.Value.StringValue : string.Empty;

                prop = request.ExtensionTransactionProperties.ExtensionProperties.FirstOrDefault(a => a.Key == "StoreNumber");
                div.StoreNumber = prop != null ? prop.Value.StringValue : "";

               

                // Terminal Properties - IsLogEnabled
                prop = request.ExtensionTransactionProperties.ExtensionProperties.FirstOrDefault(a => a.Key == "ISLOGENABLED");
                div.IsLogEnable = prop != null ? prop.Value.StringValue : "0";
                RRPaymentDeviceLogger.IsLogEnabled = div.IsLogEnable == "1";
                RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "ISLOGENABLED: " + div.IsLogEnable);

                // Terminal Properties - BaseCounter
                prop = request.ExtensionTransactionProperties.ExtensionProperties.FirstOrDefault(a => a.Key == "BASECOUNTER");
                if (prop != null)
                {
                    if (!string.IsNullOrEmpty(prop.Value.StringValue))
                    {
                        div.Basecounter = prop.Value.StringValue;
                        DeviceStaticVars.Basecounter = div.Basecounter;
                    }
                }


                prop = request.ExtensionTransactionProperties.ExtensionProperties.FirstOrDefault(a => a.Key == "EnableMinimalReceiptData");
                div.EnableMinimalReceiptData = prop != null ? prop.Value.StringValue : string.Empty;

               
                prop = request.ExtensionTransactionProperties.ExtensionProperties.FirstOrDefault(a => a.Key == "IsPaygistixApp");
                div.IsPaygistixApp = prop != null ? prop.Value.StringValue : string.Empty;
                
                prop = request.ExtensionTransactionProperties.ExtensionProperties.FirstOrDefault(a => a.Key == "CartCurrencyCode");
                div.CartCurrencyCode = prop != null ? prop.Value.StringValue : string.Empty;


                if (string.IsNullOrEmpty(div.KeySettings.Key))
                {
                    string ipAddress = string.Empty,
                        port = string.Empty;
                    prop = request.ExtensionTransactionProperties.ExtensionProperties.FirstOrDefault(a => a.Key == "IPAddress");
                    if (prop != null)
                    {
                        if (!string.IsNullOrEmpty(prop.Value.StringValue))
                        {
                            ipAddress = prop.Value.StringValue;
                        }
                    }
                    prop = request.ExtensionTransactionProperties.ExtensionProperties.FirstOrDefault(a => a.Key == "Port");
                    if (prop != null)
                    {
                        if (!string.IsNullOrEmpty(prop.Value.StringValue))
                        {
                            port = prop.Value.StringValue;
                        }
                    }
                    if (!string.IsNullOrEmpty(ipAddress))
                    {
                        div.KeySettings = new KeyValuePair<string, string>(ipAddress, port);
                    }
                }
               
            }
            if (!string.IsNullOrEmpty(div.KeySettings.Key))
            {
                merchantProperties = merchantProperties.Concat(new PaySdk.PaymentProperty[] {
                                    new PaySdk.PaymentProperty(Constants.GenericNamespace.MerchantAccount, "IpAddress", div.KeySettings.Key ) }).ToArray();
            }
            if (!string.IsNullOrEmpty(div.KeySettings.Value) && (div.KeySettings.Value != "0"))
            {
                merchantProperties = merchantProperties.Concat(new PaySdk.PaymentProperty[] {
                            new PaySdk.PaymentProperty(Constants.GenericNamespace.MerchantAccount, "Port", div.KeySettings.Value) }).ToArray();
            }
            Utilities.WaitAsyncTask(() => this.BeginTransactionAsync(request.PaymentConnectorName, merchantProperties, request.InvoiceNumber, request.IsTestMode));
            RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "BeginTransaction call is completed");
            return new NullResponse();
            //return NullResponse.Instance;
        }
        private bool SetTerminalPropertiesFromExtensionProperties(ICollection<CommerceProperty> extensionTransactionProperties)
        {
            if (extensionTransactionProperties != null && extensionTransactionProperties.Count > 0)
            {
                CommerceProperty prop = extensionTransactionProperties.FirstOrDefault(a => a.Key == "TerminalId");
                div.RRTerminalId = prop != null ? prop.Value.StringValue : string.Empty;

                // Terminal Properties - BaseCounter
                prop = extensionTransactionProperties.FirstOrDefault(a => a.Key == "BASECOUNTER");
                if (prop != null)
                {
                    if (!string.IsNullOrEmpty(prop.Value.StringValue))
                    {
                        div.Basecounter = prop.Value.StringValue;
                    }
                }
                if (string.IsNullOrEmpty(div.Basecounter) && !string.IsNullOrEmpty(DeviceStaticVars.Basecounter))
                    div.Basecounter = DeviceStaticVars.Basecounter;
                if (!string.IsNullOrEmpty(div.Basecounter))
                {
                    DeviceStaticVars.Basecounter = div.Basecounter;
                }

                // Terminal Properties - IsLogEnabled
                prop = extensionTransactionProperties.FirstOrDefault(a => a.Key == "ISLOGENABLED");
                div.IsLogEnable = prop != null ? prop.Value.StringValue : "0";
                // Terminal Properties - EnableLineDisplay
                prop = extensionTransactionProperties.FirstOrDefault(a => a.Key == "ISLINEDISPLAY");
                div.EnableLineDisplay = prop != null ? prop.Value.StringValue : "";

            }
            else
            {
                if (string.IsNullOrEmpty(div.Basecounter) && !string.IsNullOrEmpty(DeviceStaticVars.Basecounter))
                    div.Basecounter = DeviceStaticVars.Basecounter;
            }
            if (!string.IsNullOrEmpty(div.KeySettings.Key))
            {
                merchantProperties = merchantProperties.Concat(new PaySdk.PaymentProperty[] {
                                    new PaySdk.PaymentProperty(Constants.GenericNamespace.MerchantAccount, "IpAddress", div.KeySettings.Key ) }).ToArray();
            }
            if (!string.IsNullOrEmpty(div.KeySettings.Value) && (div.KeySettings.Value != "0"))
            {
                merchantProperties = merchantProperties.Concat(new PaySdk.PaymentProperty[] {
                            new PaySdk.PaymentProperty(Constants.GenericNamespace.MerchantAccount, "Port", div.KeySettings.Value) }).ToArray();
            }

            return true;
        }
        /// <summary>
        /// Ends the transaction.
        /// </summary>
        /// <returns>The response.</returns>
        [Obsolete]
        private NullResponse EndTransaction()
        {
            RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "EndTransaction call is started");
            Utilities.WaitAsyncTask(() => this.EndTransactionAsync());

            if (this.timeoutTask != null)
            {
                this.timeoutTask.Cancel();
            }

            //this.ResetCardPaymentInfo();
            RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "EndTransaction call is completed");
            return new NullResponse();
            //return NullResponse.Instance;
        }

        /// <summary>
        /// Cancel current operation on payment terminal.
        /// </summary>
        /// <returns>The response.</returns>
        [Obsolete]
        private NullResponse CancelOperation()
        {
            RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "CancelOperation call is started");
            // Cancel current operation on payment terminal
            Utilities.WaitAsyncTask(() => Task.Run(async () =>
            {
                await this.CancelOperationAsync().ConfigureAwait(false);
            }));
            RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "CancelOperation call is completed");
            return new NullResponse();
            //return NullResponse.Instance;
        }

        /// <summary>
        /// Update the line items.
        /// </summary>
        /// <param name="request">The update line items request.</param>
        /// <returns>The response.</returns>
        [Obsolete]
        private NullResponse UpdateLineItems(UpdateLineItemsPaymentTerminalDeviceRequest request)
        {
            ThrowIf.Null(request, nameof(request));
            RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "UpdateLineItems call is started");
            //Utilities.WaitAsyncTask(() => this.UpdateLineItemsAsync(request));


            Task.Run(async delegate
#pragma warning disable AvoidBlockingCallsAnalyzer // Avoid blocking asynchronous execution.
            {
                await this.UpdateLineItemsAsync(request).ConfigureAwait(continueOnCapturedContext: false);
            }).Wait(119000);
#pragma warning restore AvoidBlockingCallsAnalyzer // Avoid blocking asynchronous execution.


            RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "UpdateLineItems call is completed");
            return new NullResponse();
           // return NullResponse.Instance;
        }

        /// <summary>
        /// Authorize payment.
        /// </summary>
        /// <param name="request">The authorize payment request.</param>
        /// <returns>The authorize payment response.</returns>
        private AuthorizePaymentTerminalDeviceResponse AuthorizePayment(AuthorizePaymentTerminalDeviceRequest request)
        {
            ThrowIf.Null(request, nameof(request));
            RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "AuthorizePayment call is started");
            PaymentInfo paymentInfo = Utilities.WaitAsyncTask(() => this.AuthorizePaymentAsync(request.PaymentConnectorName, request.Amount, request.Currency, request.VoiceAuthorization, request.IsManualEntry, request.TenderInfo, request.ExtensionTransactionProperties));
            RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "AuthorizePayment call is completed");
            return new AuthorizePaymentTerminalDeviceResponse(paymentInfo);
        }

        /// <summary>
        /// Capture payment.
        /// </summary>
        /// <param name="request">The capture payment request.</param>
        /// <returns>The capture payment response.</returns>
        private CapturePaymentTerminalDeviceResponse CapturePayment(CapturePaymentTerminalDeviceRequest request)
        {
            ThrowIf.Null(request, nameof(request));
            RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "CapturePayment call is started");
            PaySdk.PaymentProperty[] paymentProperties = null;
            if (!string.IsNullOrWhiteSpace(request.PaymentPropertiesXml))
            {
                paymentProperties = PaySdk.PaymentProperty.ConvertXMLToPropertyArray(request.PaymentPropertiesXml);
            }
            PaymentInfo paymentInfo = Utilities.WaitAsyncTask(() => this.CapturePaymentAsync(request.Amount, request.Currency, paymentProperties, request.ExtensionTransactionProperties));
            RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "CapturePayment call is completed");
            return new CapturePaymentTerminalDeviceResponse(paymentInfo);
        }

        /// <summary>
        /// Void payment.
        /// </summary>
        /// <param name="request">The void payment request.</param>
        /// <returns>The void payment response.</returns>
        private VoidPaymentTerminalDeviceResponse VoidPayment(VoidPaymentTerminalDeviceRequest request)
        {
            RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "VoidPayment call is started");
            ThrowIf.Null(request, nameof(request));
            PaySdk.PaymentProperty[] paymentProperties = null;
            if (!string.IsNullOrWhiteSpace(request.PaymentPropertiesXml))
            {
                paymentProperties = PaySdk.PaymentProperty.ConvertXMLToPropertyArray(request.PaymentPropertiesXml);
            }
            PaymentInfo paymentInfo = Utilities.WaitAsyncTask(() => this.VoidPaymentAsync(request.PaymentConnectorName, request.Amount, request.Currency, request.TenderInfo, paymentProperties, request.ExtensionTransactionProperties));
            RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "VoidPayment call is completed");
            return new VoidPaymentTerminalDeviceResponse(paymentInfo);
        }

        /// <summary>
        /// Make refund payment.
        /// </summary>
        /// <param name="request">Request for refund.</param>
        /// <returns>A task that can await until the refund has completed.</returns>
        private RefundPaymentTerminalDeviceResponse RefundPayment(RefundPaymentTerminalDeviceRequest request)
        {
            RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "RefundPayment call is started");
            ThrowIf.Null(request, nameof(request));

            PaymentInfo paymentInfo = Utilities.WaitAsyncTask(() => this.RefundPaymentAsync(request.PaymentConnectorId, request.Amount, request.Currency, request.IsManualEntry, request.TenderInfo, request.ExtensionTransactionProperties));
            RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "RefundPayment call is completed");
            return new RefundPaymentTerminalDeviceResponse(paymentInfo);
        }

        /// <summary>
        /// Executes Task.
        /// </summary>
        /// <param name="request">The reuqest.</param>
        /// <returns>The response.</returns>
        private ExecuteTaskPaymentTerminalDeviceResponse ExecuteTask(ExecuteTaskPaymentTerminalDeviceRequest request)
        {
            RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "ExecuteTask call is started");
            ThrowIf.Null(request, nameof(request));

            ExtensionTransaction extensionTransactionProp = Utilities.WaitAsyncTask(() => this.ExecuteTaskAsync(request.Token, request.Task, request.ExtensionTransactionProperties));
            RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "ExecuteTask call is completed");
            return new ExecuteTaskPaymentTerminalDeviceResponse(extensionTransactionProp);
        }

        /// <summary>
        /// Gets the private tender.
        /// </summary>
        /// <param name="request">The get private tender request.</param>
        /// <returns>Returns the private tender response.</returns>
        private GetPrivateTenderPaymentTerminalDeviceResponse GetPrivateTender(GetPrivateTenderPaymentTerminalDeviceRequest request)
        {
            ThrowIf.Null(request, nameof(request));
            return new GetPrivateTenderPaymentTerminalDeviceResponse(this.tenderInfo);
        }

        /// <summary>
        /// Fetch token.
        /// </summary>
        /// <param name="request">The fetch token request.</param>
        /// <returns>The fetch token response.</returns>
        private FetchTokenPaymentTerminalDeviceResponse FetchToken(FetchTokenPaymentTerminalDeviceRequest request)
        {
            RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "FetchToken call is started");
            ThrowIf.Null(request, nameof(request));

            PaymentInfo paymentInfo = Utilities.WaitAsyncTask(() => this.FetchTokenAsync(request.IsManualEntry, request.ExtensionTransactionProperties));
            RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "FetchToken call is completed");
            return new FetchTokenPaymentTerminalDeviceResponse(paymentInfo);
        }

      
        /// <summary>
        /// Gets Trasnaction Reference ID.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>The response.</returns>
        private GetTransactionReferencePaymentTerminalDeviceResponse GetTransactionReferenceId(GetTransactionReferencePaymentTerminalDeviceRequest request)
        {
            ThrowIf.Null(request, nameof(request));
            RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "GetTransactionReferenceId call is started");
            string refId = Utilities.WaitAsyncTask(() => this.GetPaymentTransactionReferenceAsync(request));
            RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "GetTransactionReferenceId call is completed");
            //string id = $"{request.PosTerminalId}_{request.EftTerminalId}_{Guid.NewGuid()}";
            return new GetTransactionReferencePaymentTerminalDeviceResponse(refId);
        }

        /// <summary>
        /// Gets payment transaction by the Transaction Reference ID.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>The response.</returns>
        private GetTransactionByTransactionReferencePaymentTerminalDeviceResponse GetTransactionByTransactionReference(GetTransactionByTransactionReferencePaymentTerminalDeviceRequest request)
        {
            RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "GetTransactionByTransactionReference call is started");
            ThrowIf.Null(request, nameof(request));
            PaymentInfo paymentInfo = Utilities.WaitAsyncTask(() => this.GetTransactionByTransactionReferenceAsync(request));
            RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "GetTransactionByTransactionReference call is completed");
            return new GetTransactionByTransactionReferencePaymentTerminalDeviceResponse(paymentInfo);
        }

        /// <summary>
        /// Authorize CardToken Card Payment.
        /// </summary>
        /// <param name="request">The authorized card token card payment request.</param>
        /// <returns>The authorized payment response.</returns>
        public AuthorizeCardTokenCardPaymentResponse AuthorizeCardTokenCardPayment(AuthorizeCardTokenCardPaymentRequest request)
        {
            ThrowIf.Null(request, nameof(request));
            PaySdk.PaymentProperty[] cardTokenProperties = null;
            if (!string.IsNullOrWhiteSpace(request.CardTokenXml))
            {
                cardTokenProperties = PaySdk.PaymentProperty.ConvertXMLToPropertyArray(request.CardTokenXml);
            }
            RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "AuthorizeCardTokenCardPayment call is started");
            PaymentInfo paymentInfo = Utilities.WaitAsyncTask(() => this.AuthorizeCardTokenCardPaymentAsync(this.merchantProperties, cardTokenProperties, request.Amount, request.Currency, div.TerminalSettings.Locale, div.IsTestMode, div.TerminalSettings.TerminalId, request.ExtensionTransactionProperties));
            RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "AuthorizeCardTokenCardPayment call is completed");
            return new AuthorizeCardTokenCardPaymentResponse(paymentInfo);
        }

        /// <summary>
        /// Authorize Card Payment.
        /// </summary>
        /// <param name="request">The authorized card payment request.</param>
        /// <returns>The authorized payment response.</returns>
        public AuthorizePaymentCardPaymentResponse AuthorizePaymentCardPayment(AuthorizePaymentCardPaymentRequest request)
        {
            RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "AuthorizePaymentCardPayment call is started");
            ThrowIf.Null(request, nameof(request));
            PaymentInfo paymentInfo = Utilities.WaitAsyncTask(() => this.AuthorizePaymentCardPaymentAsync(request));
            RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "AuthorizePaymentCardPayment call is completed");
            return new AuthorizePaymentCardPaymentResponse(paymentInfo);
        }

        /// <summary>
        /// Capture Card Payment.
        /// </summary>
        /// <param name="request">The capture card payment request.</param>
        /// <returns>The capture payment response.</returns>
        public CapturePaymentCardPaymentResponse CapturePaymentCardPayment(CapturePaymentCardPaymentRequest request)
        {
            RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "CapturePaymentCardPayment call is started");
            ThrowIf.Null(request, nameof(request));
            PaymentInfo paymentInfo = Utilities.WaitAsyncTask(() => this.CapturePaymentCardPaymentAsync(request));
            RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "CapturePaymentCardPayment call is completed");
            return new CapturePaymentCardPaymentResponse(paymentInfo);
        }

        /// <summary>
        /// Void payment.
        /// </summary>
        /// <param name="request">The void payment request.</param>
        /// <returns>The void payment response.</returns>
        public VoidPaymentCardPaymentResponse VoidCardPayment(VoidPaymentCardPaymentRequest request)
        {
            RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "VoidCardPayment call is started");
            ThrowIf.Null(request, nameof(request));
            PaySdk.PaymentProperty[] paymentProperties = null;
            try
            {
                if (!string.IsNullOrWhiteSpace(request.PaymentPropertiesXml))
                {
                    paymentProperties = PaySdk.PaymentProperty.ConvertXMLToPropertyArray(request.PaymentPropertiesXml);
                }
            }
            catch (Exception ex)
            {
                RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "VoidCardPayment Error loading PaymentProperties -" + ex.Message + ex.StackTrace);
                RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "VoidCardPayment PaymentPropertiesXml -" + request.PaymentPropertiesXml);
                throw ex;
            }
            PaymentInfo paymentInfo = Utilities.WaitAsyncTask(() => this.VoidPaymentAsync(request.PaymentConnectorName, request.Amount, request.Currency, request.TenderInfo, paymentProperties, request.ExtensionTransactionProperties));
            RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "VoidCardPayment call is completed");
            return new VoidPaymentCardPaymentResponse(paymentInfo);
        }

        /// <summary>
        /// Refund CaptureToken Card Payment.
        /// </summary>
        /// <param name="request">The refund payment request.</param>
        /// <returns>The refund payment response.</returns>
        public RefundCaptureTokenCardPaymentResponse RefundCaptureTokenCardPayment(RefundCaptureTokenCardPaymentRequest request)
        {
            RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "RefundCaptureTokenCardPayment call is started");
            ThrowIf.Null(request, nameof(request));
            PaySdk.PaymentProperty[] captureTokenProperties = null;
            if (!string.IsNullOrWhiteSpace(request.CaptureTokenXml))
            {
                captureTokenProperties = PaySdk.PaymentProperty.ConvertXMLToPropertyArray(request.CaptureTokenXml);
            }
            PaymentInfo paymentInfo = Utilities.WaitAsyncTask(() => this.RefundCaptureTokenRequestAsync(this.merchantProperties, captureTokenProperties, request.Amount, request.Currency, div.TerminalSettings.Locale, div.IsTestMode, div.TerminalSettings.TerminalId, request.ExtensionTransactionProperties));
            RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "RefundCaptureTokenCardPayment call is completed");
            return new RefundCaptureTokenCardPaymentResponse(paymentInfo);
        }

        /// <summary>
        /// Construct the Payment Device class and open the connection from it.
        /// </summary>
        /// <param name="peripheralName">Name of peripheral device.</param>
        /// <param name="terminalSettings">The terminal settings for the peripheral device.</param>
        /// <param name="deviceConfig">Device Configuration parameters.</param>
        /// <returns>A task that can be awaited until the connection is opened.</returns>
        [SuppressMessage("Microsoft.Usage", "CA1801", Justification = "Suppressed on adding CA for backward compatibility")]
        private async Task OpenAsync(string peripheralName, SettingsInfo terminalSettings, PeripheralConfiguration deviceConfig)
        {
            //await Task.Factory.StartNew(() => {
            //    this.div = new DeviceInstanceVars();
            //    div.Locale = terminalSettings.Locale;
            //    div.TerminalSettings = terminalSettings;
            //    if (deviceConfig != null)
            //        SetKeySettings(deviceConfig.ExtensionProperties);
            //}).ConfigureAwait(false);
            this.div = new DeviceInstanceVars();
            div.Locale = terminalSettings.Locale;
            div.TerminalSettings = terminalSettings;
            if (deviceConfig != null)
                SetKeySettings(deviceConfig.ExtensionProperties);
            await Task.Delay(100).ConfigureAwait(false);
        }

        /// <summary>
        /// Close the connection to the payment terminal.
        /// </summary>
        /// <returns>A task that can be awaited until the connection is closed.</returns>
        private async Task CloseAsync()
        {
            // Close the device for payments
            await Task.Delay(TaskDelayInMilliSeconds).ConfigureAwait(false);
        }

        /// <summary>
        ///  Begins the transaction.
        /// </summary>
        /// <param name="paymentConnectorName">The payment connector name for payment.</param>
        /// <param name="merchantPaymentPropertiesXml">The merchant provider payment properties.</param>
        /// <param name="isTestMode">The boolean value indicating whether the payment is in test mode.</param>
        /// <returns>A task that can be awaited until the operation is completed.</returns>
        private async Task BeginTransactionAsync(string paymentConnectorName, PaySdk.PaymentProperty[] merchantProperties, string invoiceNumber, bool isTestMode)
        {
            //await Task.Run(() => {

            //}).ConfigureAwait(false);
            RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "BeginTransactionAsync call is started");
            List<PaySdk.PaymentProperty> properties = new List<PaySdk.PaymentProperty>();
            string connectorName = string.Empty, assemblyName = string.Empty;
            PaySdk.Hashtable hashTable = new PaySdk.Hashtable();
            List<string> assemblies = new List<string>();
            div.PaymentProcessors = new string[] { };
            foreach (PaySdk.PaymentProperty pp in merchantProperties)
            {
                if (pp.Name.Equals(Constants.ConnectorProperties.Properties))
                {
                    hashTable = PaySdk.PaymentProperty.ConvertToHashtable(pp.PropertyList);
                    PaySdk.PaymentProperty.GetPropertyValue(hashTable, Constants.GenericNamespace.Connector, Constants.ConnectorProperties.ConnectorName, out connectorName);
                    PaySdk.PaymentProperty.GetPropertyValue(hashTable, Constants.GenericNamespace.MerchantAccount, Constants.MerchantAccountProperties.AssemblyName, out assemblyName);
                    assemblies.Add(assemblyName);
                    
                    if (paymentConnectorName == connectorName)
                        properties.AddRange(pp.PropertyList);
                    div.PaymentProcessors = div.PaymentProcessors.Concat(new string[] { connectorName }).ToArray();
                }
                else
                {
                    properties.Add(pp);
                }
            }

            div.PaymentConnectorName = paymentConnectorName ?? string.Empty;
            div.AllMerchantProperties = merchantProperties;
            if (string.IsNullOrEmpty(div.Basecounter) && !string.IsNullOrEmpty(DeviceStaticVars.Basecounter))
                div.Basecounter = DeviceStaticVars.Basecounter;
            //if (assemblies.Count > 1 && div.PaymentProcessors == null)
            //{
            //    Microsoft.Dynamics.Commerce.SDKManager.Portable.PaymentProcessorManager.Create(assemblies.ToArray());
            //    div.PaymentProcessors = Microsoft.Dynamics.Retail.SDKManager.Portable.PaymentProcessorManager.GetSupportedProcessors();
            //}

            if (properties != null)
                hashTable = PaySdk.PaymentProperty.ConvertToHashtable(properties.ToArray());

            string _connectorName = div.PaymentConnectorName;
            if (string.IsNullOrEmpty(_connectorName))
                PaySdk.PaymentProperty.GetPropertyValue(hashTable, Constants.GenericNamespace.MerchantAccount, Constants.ConnectorProperties.ConnectorName, out _connectorName);
            PaySdk.PaymentProperty property = PaySdk.PaymentProperty.GetPropertyFromHashtable(hashTable, Constants.GenericNamespace.Connector, Constants.ConnectorProperties.ConnectorName);
            if (property == null)
            {
                property = new PaySdk.PaymentProperty(Constants.GenericNamespace.Connector, Constants.ConnectorProperties.ConnectorName, _connectorName);
                properties.Add(property);
            }
            div.PaymentConnectorName = _connectorName;
           
           
            this.merchantProperties = properties.ToArray();

            //Add file read new properties in alllmerchantproperties
            if (div.PaymentProcessors != null && div.PaymentProcessors.Length > 1)
            {
                properties = new List<PaySdk.PaymentProperty>();
                foreach (PaySdk.PaymentProperty pp in div.AllMerchantProperties)
                {
                    if (pp.Name.Equals(Constants.ConnectorProperties.Properties))
                    {
                        hashTable = PaySdk.PaymentProperty.ConvertToHashtable(pp.PropertyList);
                        PaySdk.PaymentProperty.GetPropertyValue(hashTable, Constants.GenericNamespace.Connector, Constants.ConnectorProperties.ConnectorName, out string s_value);
                        if (paymentConnectorName == s_value)
                        {
                            pp.PropertyList = pp.PropertyList.Union(this.merchantProperties).ToArray();
                        }

                    }
                }
            }

            div.IsTestMode = isTestMode;
            this.tenderInfo = PaymentHelper.GetDummyTender(DummyCardNumber);

            RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "BeginTransactionAsync call is completed");
            await Task.Delay(TaskDelayInMilliSeconds).ConfigureAwait(false);
        }

        /// <summary>
        ///  Ends the transaction.
        /// </summary>
        /// <returns>A task that can be awaited until the operation has completed.</returns>
        private async Task EndTransactionAsync()
        {
            RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "EndTransactionAsync call is started");
            if (div.PaymentConnectorName != null)
            {
                //multiple Connectors line diaplay call always send to default connector
                if (div.PaymentProcessors != null && div.PaymentProcessors.Length > 1)
                {
                    this.processor = null;
                }
                //
               
            }
            RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "EndTransactionAsync call is completed");
            await Task.Delay(TaskDelayInMilliSeconds).ConfigureAwait(false);
        }

        /// <summary>
        /// Cancel current operation on payment terminal.
        /// </summary>
        /// <returns>A task that can be awaited until the cancel operation has completed.</returns>
        /// <summary>
        /// Cancel current operation on payment terminal.
        /// </summary>
        /// <returns>A task that can be awaited until the cancel operation has completed.</returns>
        private async Task CancelOperationAsync()
        {
            RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "CancelOperationAsync call is started");
            if (div.PaymentConnectorName != null)
            {
                Task<object> t = Task<object>.Factory.StartNew(() =>
                {
                    //multiple Connectors send request to two connectors
                    PaySdk.Hashtable hashTable = new PaySdk.Hashtable();
                    PaySdk.PaymentProperty[] PaymentProperties = null;
                    string connectorName = "";
                    if (div.PaymentProcessors != null && div.PaymentProcessors.Length > 1)
                    {
                        List<PaySdk.PaymentProperty> prop = new List<PaySdk.PaymentProperty>();
                        foreach (PaySdk.PaymentProperty pp in div.AllMerchantProperties)
                        {
                            if (pp.Name.Equals(Constants.ConnectorProperties.Properties))
                            {
                                hashTable = PaySdk.PaymentProperty.ConvertToHashtable(pp.PropertyList);
                                PaySdk.PaymentProperty.GetPropertyValue(hashTable, Constants.GenericNamespace.Connector, Constants.ConnectorProperties.ConnectorName, out string s_value);
                                if (div.PaymentConnectorName != s_value)
                                {
                                    prop.AddRange(pp.PropertyList);
                                    connectorName = s_value;
                                }
                            }
                            else
                            {
                                prop.Add(pp);
                            }
                        }
                        PaymentProperties = prop.ToArray();
                        this.processor = null;
                        if (!connectorName.Equals(Connectors.ElavonFuseboxConnector) && !connectorName.Equals(Connectors.CardConnectConnector))
                        {
                            if (this.processor == null)
                            {
                                this.processor = PaymentProcessor.Get(connectorName);
                            }
                        }
                        this.processorExt = this.processor as PaySdk.IPaymentProcessorExtension;
                        PaySdk.Request request = new PaySdk.Request
                        {
                            Locale = div.Locale,
                            Properties = PaymentProperties
                        };

                        List<PaySdk.PaymentProperty> properties = request.Properties.ToList();
                        properties.Add(new PaySdk.PaymentProperty(Constants.GenericNamespace.TransactionData, Constants.TransactionDataProperties.TaskName, "canceltransaction"));
                        if (connectorName.Equals(Connectors.CitconConnector) || connectorName.Equals(Connectors.YuansferConnector) || connectorName.Equals(Connectors.ElavonFuseboxConnector))
                        {
                            AppStorage storage = new AppStorage(connectorName, div.Basecounter);
                            properties.Add(new PaySdk.PaymentProperty(Constants.GenericNamespace.TransactionData, "EFTTransactionId", DeviceStaticVars.PreviousCounter ?? string.Empty));
                        }
                        request.Properties = properties.ToArray();

                        if (!connectorName.Equals(Connectors.ElavonFuseboxConnector) && !connectorName.Equals(Connectors.CardConnectConnector))
                        {
                            this.processorExt.ExecuteTask(request);
                        }
                    }

                  
                        return null;
                });
                await t.ConfigureAwait(false);
            }
            RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "CancelOperationAsync call is completed");
            await Task.Delay(TaskDelayInMilliSeconds).ConfigureAwait(false);
        }

        /// <summary>
        /// Update the line items on the current open session.  This method will compare against previous lines specified and make the appropriate device calls.
        /// </summary>
        /// <param name="request">The update line items request.</param>
        /// <returns>A task that can be awaited until the text is displayed on the screen.</returns>
        [SuppressMessage("Microsoft.Usage", "CA1801", Justification = "Suppressed on adding CA for backward compatibility")]
        private async Task UpdateLineItemsAsync(UpdateLineItemsPaymentTerminalDeviceRequest request)
        {
            RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "UpdateLineItemsAsync call is started");
            string[] lineDisplayConnectorArray = new string[] { Connectors.NBEConnector};
            string connectorName = string.Empty;
            if (div!= null)
            {
                connectorName = div.PaymentConnectorName;
            }
            else
            {
                RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "UpdateLineItemsAsync div is null");
            }
            try
            {
                if (lineDisplayConnectorArray.Contains(div.PaymentConnectorName) ||
                (div.PaymentConnectorName.Equals(Connectors.AdyenConnector) && DeviceStaticVars.IsDeviceRegistered == true))
                {
                    div.CartData = await this.ConvertToCartStringAsync(request.SubTotalAmount, request.TotalAmount, request.TaxAmount, request.DiscountAmount, request.Items).ConfigureAwait(false);
                    div.TotalAmount = Math.Abs(Convert.ToDecimal(request.TotalAmount));
                    await UpdateLineDisplay().ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                RRPaymentDeviceLogger.WriteFileContent("Error sending call to " + connectorName + ". Error - " + ex.Message, "ERROR");
            }
            RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "UpdateLineItemsAsync call is completed.");

            return;
        }

        /// <summary>
        /// Authorize payment.
        /// </summary>
        /// <param name="paymentConnectorName">The payment connector name.</param>
        /// <param name="amount">The amount.</param>
        /// <param name="currency">The currency.</param>
        /// <param name="tenderInfo">Tender information.</param>
        /// <param name="extensionTransactionProperties">Optional extension transaction properties.</param>
        /// <returns>A task that can await until the authorization has completed.</returns>
        [SuppressMessage("Microsoft.Usage", "CA1801", Justification = "Suppressed on adding CA for backward compatibility")]
        private async Task<PaymentInfo> AuthorizePaymentAsync(string paymentConnectorName, decimal amount, string currency, string voiceAuthorization, bool isManualEntry, TenderInfo tenderInfo, ExtensionTransaction extensionTransactionProperties)
        {
            string methodName = "AuthorizePaymentAsync";
            string s_value = string.Empty;
            RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "AuthorizePaymentAsync call is started");
            string connectorName = string.IsNullOrEmpty(paymentConnectorName) ? div.PaymentConnectorName : paymentConnectorName;
            div.IsSwipe = !isManualEntry;

            if (tenderInfo != null)
            {
                this.tenderInfo.CardNumber = tenderInfo.CardNumber;
                this.tenderInfo.CardTypeId = tenderInfo.CardTypeId;
            }
            // set voice auth code
            if (!string.IsNullOrEmpty(voiceAuthorization))
                this.tenderInfo.VoiceAuthorizationCode = voiceAuthorization;

            

            PaySdk.Response response = new PaySdk.Response();
            PaymentInfo paymentInfo = new PaymentInfo();
            SetExtensionTransactionProp(ref extensionTransactionProperties, response);
            
            GetMerchantProp(extensionTransactionProperties, response, ref paymentInfo);
            if (response.Errors != null)
            {
                paymentInfo.Errors = response.Errors;
                return await Task.FromResult<PaymentInfo>(paymentInfo).ConfigureAwait(false);
            }
            if (div.IsGetSignatureCall && !div.IsSignatureCapture)
            {
                paymentInfo.IsApproved = false;
                paymentInfo.PaymentSdkData = "EnableCustomerAccountSignatureFalse";
                return await Task.FromResult<PaymentInfo>(paymentInfo).ConfigureAwait(false);
            }
            if (div.IsGetMerchantProp)
            {
                paymentInfo.IsApproved = false;
                return await Task.FromResult<PaymentInfo>(paymentInfo).ConfigureAwait(false);
            }
            // handle pay other connector <<
            connectorName = GetOtherConnector(connectorName, extensionTransactionProperties);
            // pay other connector >>



            // multiple Connectors <<
            PaySdk.Hashtable hashTable = new PaySdk.Hashtable();
            PaySdk.PaymentProperty[] PaymentProperties = null;
            if (div.PaymentProcessors != null && div.PaymentProcessors.Length > 1)
            {
                List<PaySdk.PaymentProperty> properties = new List<PaySdk.PaymentProperty>();
                foreach (PaySdk.PaymentProperty pp in div.AllMerchantProperties)
                {
                    if (pp.Name.Equals(Constants.ConnectorProperties.Properties))
                    {
                        hashTable = PaySdk.PaymentProperty.ConvertToHashtable(pp.PropertyList);
                        PaySdk.PaymentProperty.GetPropertyValue(hashTable, Constants.GenericNamespace.Connector, Constants.ConnectorProperties.ConnectorName, out s_value);
                        if (connectorName == s_value)
                            properties.AddRange(pp.PropertyList);
                    }
                    else
                    {
                        properties.Add(pp);
                    }
                }
                PaymentProperties = properties.ToArray();
            }
            else
            {
                PaymentProperties = this.merchantProperties;
            }
            
            this.tenderInfo = GetCustomerAddress(extensionTransactionProperties);
            //PaySdk.Request authRequest = PaymentHelper.GetAuthorizationRefundRequest(PaymentProperties, tenderInfo, amount, currency, div.TerminalSettings.Locale, div.IsTestMode, div.TerminalSettings.TerminalId, extensionTransactionProperties);
            try
            {
                //response = PaymentProcessor.Get(connectorName).Authorize(authRequest, null);
                RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "InvokeAuthorizationCall call is started");
                response = PaymentHelper.InvokeAuthorizationCall(PaymentProperties, connectorName, this.tenderInfo, amount, currency, div.TerminalSettings.Locale, true, div.TerminalSettings.TerminalId, extensionTransactionProperties);
                RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "InvokeAuthorizationCall call is completed");
            }
            catch (Exception ex)
            {
                RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "Error sending call to " + connectorName + ". Error - " + ex.Message, "ERROR");
                throw ex;
            }
            if (tenderInfo != null)
            {
                this.tenderInfo.Name = "";
                this.tenderInfo.Address = "";
                this.tenderInfo.City = "";
                this.tenderInfo.Country = "";
                this.tenderInfo.StateOrProvince = "";
                this.tenderInfo.Zip = "";
            }
            hashTable = new PaySdk.Hashtable();
            Guid cardStorageKey = Guid.NewGuid();

            if (response.Errors == null)
            {
                if (response.Properties != null)
                    hashTable = PaySdk.PaymentProperty.ConvertToHashtable(response.Properties);
                decimal ApprovedAmount, CashbackAmount;
                PaySdk.PaymentProperty.GetPropertyValue(hashTable, Constants.GenericNamespace.AuthorizationResponse, Constants.AuthorizationResponseProperties.ApprovedAmount, out ApprovedAmount);
                PaySdk.PaymentProperty.GetPropertyValue(hashTable, Constants.GenericNamespace.AuthorizationResponse, Constants.AuthorizationResponseProperties.CashBackAmount, out CashbackAmount);
                paymentInfo.CashbackAmount = CashbackAmount;
                if (div.TotalAmount > ApprovedAmount)
                {
                    await UpdateLineDisplay().ConfigureAwait(false);
                    div.TotalAmount -= ApprovedAmount;// for multi tender
                }
                if (div.PaymentConnectorName == Connectors.CayanConnector)
                {
                    PaySdk.PaymentProperty.GetPropertyValue(hashTable, Constants.GenericNamespace.TransactionData, "SaveCardTokenAfterSaleXML", out s_value);
                    if (!string.IsNullOrEmpty(s_value))// cayan wines requirments, remove SaveCardTokenAfterSaleXML from auth response and set to paymentInfo.SignatureData.
                    {
                        paymentInfo.SignatureData = s_value;
                        response.Properties = response.Properties.Where(prop => prop.Name != "SaveCardTokenAfterSaleXML").ToArray();
                    }
                }
                RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "Authorize succeeded with Amount " + ApprovedAmount.ToString() + ".");
                var propObj = response.Properties.FirstOrDefault(x => x.Name == "SignatureData");
                try
                {
                    if (propObj != null)
                    {
                        if (string.IsNullOrEmpty(propObj.StringValue) == false)
                        {
                            string signatureFormat = string.Empty;
                            PaySdk.PaymentProperty.GetPropertyValue(hashTable, GenericNamespace.TransactionData, "SignatureFormat", out signatureFormat);
                            if (string.IsNullOrEmpty(signatureFormat) == true)
                                signatureFormat = string.Empty;

                            propObj.StringValue = SignatureHelper.ConvertImageBase64(propObj.StringValue, div.PaymentConnectorName, signatureFormat);

                            RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "SignatureData to MapAuthorizeResponse in AuthorizePaymentAsync method present.");
                        }
                        else
                        {
                            RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "SignatureData is blank to MapAuthorizeResponse in AuthorizePaymentAsync method.");
                        }

                    }
                    else
                    {
                        RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "SignatureData is empty in AuthorizePaymentAsync method come from connector SignatureData.");
                    }
                }
                catch (Exception ex1)
                {
                    if (propObj != null)
                        propObj.StringValue = "";
                    RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "Failed to Convert Image Base64 in AuthorizePaymentAsync method Exception: " + ex1.Message, "ERROR");
                }
                try
                {
                    if (div.TerminalSettings == null)
                    {
                        RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "div.TerminalSettings is null in AuthorizePaymentAsync method.");
                        div.TerminalSettings = new SettingsInfo();
                    }
                    PaymentHelper.MapAuthorizeResponse(response, paymentInfo, cardStorageKey, div.TerminalSettings.TerminalId);
                }
                catch (Exception ex)
                {
                    //RetailLogger.Log.ExtendedErrorEvent(ex.Message, this.GetType().Name, methodName);
                    RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "Failed to MapAuthorizeResponse in AuthorizePaymentAsync method Exception: " + ex.Message, "ERROR");
                    //throw ex;                    
                }
                if (div.PaymentConnectorName == Connectors.MexicoConnector || div.PaymentConnectorName == Connectors.MonerisConnector)
                {
                    PaymentHelper.SetTransactionProp(ref paymentInfo, ref response);
                }
            }
            else
            {
                RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "Authorize failed.");
                SetFailedTransactionProp(ref paymentInfo, ref response);

                PaymentHelper.MapAuthorizeResponse(response, paymentInfo, cardStorageKey, div.TerminalSettings.TerminalId);

                //if (connectorName == Connectors.WorldpayUKConnector || connectorName == Connectors.MonerisConnector || connectorName == Connectors.TriPOSConnector)
                //    PaymentHelper.MapAuthorizeResponse(response, paymentInfo, cardStorageKey, div.TerminalSettings.TerminalId);
            }
            paymentInfo.Errors = response.Errors;

            if (paymentInfo.IsApproved)
            {
                //TemporaryCardMemoryStorage<string> cardStorage = new TemporaryCardMemoryStorage<string>(DateTime.UtcNow, this.tenderInfo.CardNumber);
                //cardStorage.StorageInfo = paymentInfo.PaymentSdkData;
                //cardCache.Add(cardStorageKey, cardStorage);
                hashTable = PaySdk.PaymentProperty.ConvertToHashtable(response.Properties);
                PaySdk.PaymentProperty.GetPropertyValue(hashTable, Constants.GenericNamespace.AuthorizationResponse, Constants.PaymentCardProperties.Last4Digits, out s_value);
                paymentInfo.CardNumberMasked = string.Format("{0}{1}", DummyCardNoMasked, s_value);
                if ((div.PaymentConnectorName == Connectors.AdyenConnector || div.PaymentConnectorName == Connectors.RRVerifoneConnector || div.PaymentConnectorName == Connectors.VerifoneEngageConnector) && string.IsNullOrEmpty(s_value))// in first auth request card number always blank
                {
                    PaySdk.PaymentProperty.GetPropertyValue(hashTable, Constants.GenericNamespace.TransactionData, "SignatureData", out s_value);
                    paymentInfo.SignatureData = s_value;
                }
                if (div.PaymentConnectorName == Connectors.WestPacPaymentConnector)
                {
                    PaySdk.PaymentProperty.GetPropertyValue(hashTable, Constants.GenericNamespace.TransactionData, "SignatureRequest", out s_value);
                    paymentInfo.SignatureData = s_value;

                    if (paymentInfo.SignatureData == "True")
                    {
                        PaySdk.PaymentProperty.GetPropertyValue(hashTable, Constants.GenericNamespace.TransactionData, "SignatureMerchantCopy", out s_value);
                        paymentInfo.PaymentSdkData = s_value;
                    }
                }
                if (div.PaymentConnectorName == Connectors.ValitorConnector)
                {
                    PaySdk.PaymentProperty.GetPropertyValue(hashTable, Constants.GenericNamespace.TransactionData, "SignatureRequest", out s_value);
                    if (s_value == "Y")
                        paymentInfo.SignatureData = s_value;
                }
            }
            if (div.PaymentConnectorName == Connectors.WestPacPaymentConnector)
            {
                if (paymentInfo.Errors != null)
                {
                    paymentInfo.PaymentSdkData = paymentInfo.Errors[1].Message;
                }
            }

            this.tenderInfo.CardNumber = DummyCardNumber;
            this.tenderInfo.CardTypeId = 0;

            if (!string.IsNullOrEmpty(voiceAuthorization))
                this.tenderInfo.VoiceAuthorizationCode = null;

            RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "AuthorizePaymentAsync execution completed.");
            return await Task.FromResult(paymentInfo).ConfigureAwait(false);
        }

        /// <summary>
        /// Make settlement of a payment.
        /// </summary>
        /// <param name="amount">The amount.</param>
        /// <param name="currency">The currency.</param>
        /// <param name="paymentPropertiesXml">The payment properties of the authorization response.</param>
        /// <param name="extensionTransactionProperties">Optional extension transaction properties.</param>
        /// <returns>A task that can await until the settlement has completed.</returns>
        [SuppressMessage("Microsoft.Usage", "CA1801", Justification = "Suppressed on adding CA for backward compatibility")]
        private async Task<PaymentInfo> CapturePaymentAsync(decimal amount, string currency, PaySdk.PaymentProperty[] paymentProperties, ExtensionTransaction extensionTransactionProperties)
        {
            string connectorName = string.Empty;
            RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "CapturePaymentAsync call is started");
            PaySdk.PaymentProperty[] paymentMerchantProperties = this.merchantProperties;
            if (paymentProperties != null)
            {
                connectorName = PaymentHelper.GetConnectorName(paymentProperties);
            }
            else
            {
                RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "CapturePaymentAsync - paymentProperties are null");
            }
            if (div==null)
            {
                RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "CapturePaymentAsync - div is null");
            }
            else
            {
                RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "CapturePaymentAsync - div present");
            }
            if (this.merchantProperties.Length==0)
            {
                RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "CapturePaymentAsync - merchantProperties count zero");
            }
            if (div != null && div.AllMerchantProperties.Length==0)
            {
                RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "CapturePaymentAsync - div.AllMerchantProperties count zero");
            }

            if (connectorName != div.PaymentConnectorName)
            {
                this.processor = PaymentProcessor.Get(connectorName);
                //paymentMerchantProperties = PaymentHelper.GetMerchantProperties(div.AllMerchantProperties, paymentProperties, connectorName);
                foreach (PaySdk.PaymentProperty pp in div.AllMerchantProperties)
                {
                    if (pp.Name.Equals(Constants.ConnectorProperties.Properties))
                    {
                        PaySdk.Hashtable hashTable = PaySdk.PaymentProperty.ConvertToHashtable(pp.PropertyList);
                        PaySdk.PaymentProperty.GetPropertyValue(hashTable, Constants.GenericNamespace.Connector, Constants.ConnectorProperties.ConnectorName, out string s_value);
                        if (connectorName == s_value)
                            paymentMerchantProperties = pp.PropertyList;
                    }
                }
            }
            else
                this.processor = PaymentProcessor.Get(div.PaymentConnectorName);
            RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "CapturePaymentAsync - paymentprocessor initialized");
            PaymentInfo paymentInfo = new PaymentInfo();
            // Handle multiple chain connectors by returning single instance used in capture.
            PaySdk.IPaymentProcessor currentProcessor = null;
            PaySdk.PaymentProperty[] paymentPropertyArray = null;
            PaymentHelper.GetRequiredConnector(paymentMerchantProperties, paymentProperties, this.processor, out currentProcessor, out paymentPropertyArray);
            RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "CapturePaymentAsync - currentProcessor initialized");
            PaySdk.Request captureRequest = PaymentHelper.GetCaptureVoidRequest(paymentPropertyArray, paymentProperties, amount, currency, div.TerminalSettings.Locale, div.IsTestMode, div.TerminalSettings.TerminalId, extensionTransactionProperties);
            RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "Capture call is started");
            PaySdk.Response response = currentProcessor.Capture(captureRequest);
            RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "Capture call is completed");
            PaymentHelper.MapCaptureResponse(response, paymentInfo);
            if (response.Errors == null)
            {
                PaySdk.Hashtable hashTable = new PaySdk.Hashtable();
                if (response.Properties != null)
                    hashTable = PaySdk.PaymentProperty.ConvertToHashtable(response.Properties);
                string s_value = string.Empty;
                PaySdk.PaymentProperty.GetPropertyValue(hashTable, Constants.GenericNamespace.CaptureResponse, Constants.PaymentCardProperties.Last4Digits, out s_value);
                paymentInfo.CardNumberMasked = string.Format("{0}{1}", DummyCardNoMasked, s_value);
            }
            RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "CapturePaymentAsync call is completed");
            return await Task.FromResult(paymentInfo).ConfigureAwait(false);
        }

        /// <summary>
        /// Make reversal/void a payment.
        /// </summary>
        /// <param name="paymentConnectorName">The payment connector name.</param>
        /// <param name="amount">The amount.</param>
        /// <param name="currency">The currency.</param>
        /// <param name="tenderInfo">The tender information.</param>
        /// <param name="paymentPropertiesXml">The payment properties of the authorization response.</param>
        /// <param name="extensionTransactionProperties">Optional extension transaction properties.</param>
        /// <returns>A task that can await until the void has completed.</returns>
        [SuppressMessage("Microsoft.Usage", "CA1801", Justification = "Suppressed on adding CA for backward compatibility")]
        private async Task<PaymentInfo> VoidPaymentAsync(string paymentConnectorName, decimal amount, string currency, TenderInfo tenderInfo, PaySdk.PaymentProperty[] paymentProperties, ExtensionTransaction extensionTransactionProperties)
        {

            //string methodName = "VoidPaymentAsync";
            div.IsSwipe = false;
            RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "VoidPaymentAsync call is started");
            string s_value, connectorName = string.IsNullOrEmpty(paymentConnectorName) ? div.PaymentConnectorName : paymentConnectorName; ;
            // for setting gift card card type id to tenderInfo
            if (tenderInfo != null)
            {
                this.tenderInfo.CardNumber = tenderInfo.CardNumber;
                this.tenderInfo.CardTypeId = tenderInfo.CardTypeId;
            }
            //Gift
            PaySdk.Hashtable hashTable = new PaySdk.Hashtable();
            PaySdk.PaymentProperty[] MerchantProperties = null;
            if (div.PaymentProcessors != null && div.PaymentProcessors.Length > 1)
            {
                CommerceProperty paymentConnectorIdProperty = new CommerceProperty();
                if (extensionTransactionProperties != null) // avoid error conditions where extensionTransactionProperties is null
                {
                    paymentConnectorIdProperty = extensionTransactionProperties.ExtensionProperties.FirstOrDefault(a => a.Key == "paymentConnectorId" && a.Value.StringValue != null);
                    connectorName = paymentConnectorIdProperty == null ? connectorName : paymentConnectorIdProperty.Value.StringValue;
                }
                // handle pay other connector >>
                var payProp = paymentProperties.FirstOrDefault(a => a.Name == "ConnectorName");
                if (payProp != null)
                {
                    var strConnectorName = payProp.StringValue;
                    connectorName = strConnectorName;
                }
                // handle pay other connector >>
                List<PaySdk.PaymentProperty> properties = new List<PaySdk.PaymentProperty>();
                foreach (PaySdk.PaymentProperty pp in div.AllMerchantProperties)
                {
                    if (pp.Name.Equals(Constants.ConnectorProperties.Properties))
                    {
                        hashTable = PaySdk.PaymentProperty.ConvertToHashtable(pp.PropertyList);
                        PaySdk.PaymentProperty.GetPropertyValue(hashTable, Constants.GenericNamespace.Connector, Constants.ConnectorProperties.ConnectorName, out s_value);
                        if (connectorName == s_value)
                            properties.AddRange(pp.PropertyList);
                    }
                    else
                    {
                        properties.Add(pp);
                    }
                }
                MerchantProperties = properties.ToArray();
                this.processor = null;
            }
            else
            {
                MerchantProperties = this.merchantProperties;
            }
            if (this.processor == null)
            {
                this.processor = PaymentProcessor.Get(connectorName);
            }
            PaySdk.Response response = new PaySdk.Response();
            PaymentInfo paymentInfo = new PaymentInfo();
            SetExtensionTransactionProp(ref extensionTransactionProperties, response);
            if (connectorName.Equals(Connectors.VerifoneSCAConnector))
            {
                AppStorage storage = new AppStorage(div.PaymentConnectorName, div.Basecounter);
                string macData = storage.GetValueFromJsonFile();
                List<PaySdk.PaymentProperty> properties = new List<PaySdk.PaymentProperty>();
                DeviceData devicedata = JsonConvert.DeserializeObject<DeviceData>(macData);
                properties.Add(new PaySdk.PaymentProperty(Constants.GenericNamespace.TransactionData, "MacKey", devicedata == null ? "" : Convert.ToBase64String(devicedata.MackKey)));
                properties.Add(new PaySdk.PaymentProperty(Constants.GenericNamespace.TransactionData, "MacLabel", devicedata == null ? "" : devicedata.MacLabel));
                properties.Add(new PaySdk.PaymentProperty(Constants.GenericNamespace.TransactionData, "IsLogEnabled", div.IsLogEnable ?? string.Empty));
                properties.Add(new PaySdk.PaymentProperty(Constants.GenericNamespace.TransactionData, "EnableLineDisplay", div.EnableLineDisplay ?? string.Empty));
                properties.Add(new PaySdk.PaymentProperty(Constants.GenericNamespace.TransactionData, "LogAppInsightsKey", div.LogAppInsightsKey ?? string.Empty));
                MerchantProperties = MerchantProperties.Union(properties).ToArray();
            }
		if (connectorName.Equals(Connectors.PceftConnector))
         {
             bool IsRestAPI = false;
             foreach (PaySdk.PaymentProperty pp in div.AllMerchantProperties)
             {
                 if (pp.Name.Equals(Constants.ConnectorProperties.Properties))
                 {
                     hashTable = PaySdk.PaymentProperty.ConvertToHashtable(pp.PropertyList);
                     PaySdk.PaymentProperty.GetPropertyValue(hashTable, Constants.GenericNamespace.MerchantAccount, "IsRestAPI", out s_value);
                     if (s_value == "1")
                         IsRestAPI = true;
                 }
             }
             if (IsRestAPI)
             {
                 AppStorage storage = new AppStorage(div.PaymentConnectorName, div.Basecounter);
                 string SecretKey = storage.GetValueFromJsonFile();
                 List<PaySdk.PaymentProperty> properties = new List<PaySdk.PaymentProperty>();
                 if (!string.IsNullOrEmpty(SecretKey))
                 {
                     Newtonsoft.Json.Linq.JObject jObject = Newtonsoft.Json.Linq.JObject.Parse(SecretKey);
                     string secrateKey = Convert.ToString(jObject["SecretKey"]);
                     properties.Add(new PaySdk.PaymentProperty(Constants.GenericNamespace.TransactionData, "SecretKey", secrateKey ?? string.Empty));
                     MerchantProperties = MerchantProperties.Union(properties).ToArray();
                 }
             }
         }
            if (response.Errors != null)
            {
                paymentInfo.Errors = response.Errors;
                return await Task.FromResult<PaymentInfo>(paymentInfo).ConfigureAwait(false);
            }

            // Handle multiple chain connectors by returning single instance used in capture.
            PaymentHelper.GetRequiredConnector(MerchantProperties, paymentProperties, this.processor, out PaySdk.IPaymentProcessor currentProcessor, out PaySdk.PaymentProperty[] currentMerchantProperties);

            PaySdk.Request request = PaymentHelper.GetCaptureVoidRequest(currentMerchantProperties, paymentProperties, amount, currency, div.TerminalSettings.Locale, div.IsTestMode, div.TerminalSettings.TerminalId, extensionTransactionProperties);
            //RetailLogger.Log.ExtendedInformationalEvent("Void call is started.", this.GetType().Name, methodName);
            RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "Void call is started");
            response = currentProcessor.Void(request);
            RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "Void call is Completed");
            if (response.Errors == null)
            {
                //to convert SignatureData to Base64 and bitmap added 
                var propObj = response.Properties.FirstOrDefault(x => x.Name == "SignatureData");
                try
                {
                    if (propObj != null)
                    {
                        if (string.IsNullOrEmpty(propObj.StringValue) == false)
                        {
                            string signatureFormat = string.Empty;
                            PaySdk.PaymentProperty.GetPropertyValue(hashTable, GenericNamespace.TransactionData, "SignatureFormat", out signatureFormat);
                            if (string.IsNullOrEmpty(signatureFormat) == true)
                                signatureFormat = string.Empty;

                            propObj.StringValue = SignatureHelper.ConvertImageBase64(propObj.StringValue, div.PaymentConnectorName, signatureFormat);
                            RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "SignatureData in VoidPaymentAsync method is present.");
                        }
                    }
                    else
                    {
                        RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "SignatureData is empty in VoidPaymentAsync method come from connector SignatureData.");
                    }
                    //to convert SignatureData to Base64 and bitmap added 
                }
                catch (Exception ex)
                {
                    if (propObj != null)
                        propObj.StringValue = "";
                    RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "Failed to Convert Image Base64 in VoidPaymentAsync method Exception: " + ex.Message, "ERROR");
                }
                PaymentHelper.MapVoidResponse(response, paymentInfo);
                if (div.PaymentConnectorName.Equals(Connectors.CayanConnector) || div.PaymentConnectorName.Equals(Connectors.AdyenConnector) || div.PaymentConnectorName.Equals(Connectors.CloverConnector))
                {
                    await UpdateLineDisplay().ConfigureAwait(false);
                    div.TotalAmount += amount;
                }
              
            }
            else
            {
                RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "VoidPaymentAsync failed.");
                SetFailedTransactionProp(ref paymentInfo, ref response);
                if (connectorName == Connectors.WorldpayUKConnector || connectorName == Connectors.MonerisConnector || connectorName == Connectors.TriPOSConnector || connectorName == Connectors.EpayConnector || connectorName == Connectors.ChaseConnector)
                    PaymentHelper.MapVoidResponse(response, paymentInfo);
            }

            this.tenderInfo.CardNumber = DummyCardNumber;
            this.tenderInfo.CardTypeId = 0;

            paymentInfo.Errors = response.Errors;
            RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "VoidPaymentAsync call is Completed.");
            return await Task.FromResult(paymentInfo).ConfigureAwait(false);
        }

        /// <summary>
        /// Make refund payment.
        /// </summary>
        /// <param name="amount">The amount.</param>
        /// <param name="currency">The currency.</param>
        /// <param name="isManualEntry">If manual credit card entry is required.</param>
        /// <param name="extensionTransactionProperties">Optional extension transaction properties.</param>
        /// <returns>A task that can await until the refund has completed.</returns>
        [SuppressMessage("Microsoft.Usage", "CA1801", Justification = "Suppressed on adding CA for backward compatibility")]
        private async Task<PaymentInfo> RefundPaymentAsync(string paymentConnectorName, decimal amount, string currency, bool isManualEntry, TenderInfo tenderInfo, ExtensionTransaction extensionTransactionProperties)
        {
            RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "RefundPaymentAsync call is started");
            string s_value = string.Empty;
            bool returnResponse = false;
            div.IsSwipe = !isManualEntry;
            string connectorName = string.IsNullOrEmpty(paymentConnectorName) ? div.PaymentConnectorName : paymentConnectorName;
            // for setting gift card card type id to tenderInfo
            if (tenderInfo != null)
            {
                this.tenderInfo.CardNumber = tenderInfo.CardNumber;
                this.tenderInfo.CardTypeId = tenderInfo.CardTypeId;
            }
            PaySdk.Response response = new PaySdk.Response();
            PaymentInfo paymentInfo = new PaymentInfo();

            // handle pay other connector >>
            if (extensionTransactionProperties != null)
            {
                var payProp = extensionTransactionProperties.ExtensionProperties.FirstOrDefault(cp => cp.Key == "PayOtherConnector");
                if (payProp != null)
                {
                    s_value = payProp.Value.StringValue;
                    if (!string.IsNullOrEmpty(s_value))
                    {
                        connectorName = s_value;
                    }
                }
            }
           
            PaySdk.Hashtable hashTable = new PaySdk.Hashtable();
            PaySdk.PaymentProperty[] PaymentProperties = null;
            if (div.PaymentProcessors != null && div.PaymentProcessors.Length > 1)
            {
                List<PaySdk.PaymentProperty> properties = new List<PaySdk.PaymentProperty>();
                foreach (PaySdk.PaymentProperty pp in div.AllMerchantProperties)
                {
                    if (pp.Name.Equals(Constants.ConnectorProperties.Properties))
                    {
                        hashTable = PaySdk.PaymentProperty.ConvertToHashtable(pp.PropertyList);
                        PaySdk.PaymentProperty.GetPropertyValue(hashTable, Constants.GenericNamespace.Connector, Constants.ConnectorProperties.ConnectorName, out s_value);
                        if (connectorName == s_value)
                            properties.AddRange(pp.PropertyList);
                    }
                    else
                    {
                        properties.Add(pp);
                    }
                }
                PaymentProperties = properties.ToArray();
            }
            else
            {
                PaymentProperties = this.merchantProperties;
            }
            

            CheckPayment(ref returnResponse, ref paymentInfo, extensionTransactionProperties, PaymentProperties, connectorName);
            if (returnResponse)
            {
                return await Task.FromResult<PaymentInfo>(paymentInfo).ConfigureAwait(false);
            }
            
            SetExtensionTransactionProp(ref extensionTransactionProperties, response);
            
            if (response.Errors != null)
            {
                paymentInfo.Errors = response.Errors;
                return await Task.FromResult<PaymentInfo>(paymentInfo).ConfigureAwait(false);
            }
            if (div.IsGetSignatureCall && !div.IsSignatureCapture)
            {
                paymentInfo.IsApproved = false;
                paymentInfo.PaymentSdkData = "EnableCustomerAccountSignatureFalse";
            }

            //RetailLogger.Log.ExtendedInformationalEvent("Refund call is started.", this.GetType().Name, methodName);
            //PaySdk.Request refundRequest = PaymentHelper.GetAuthorizationRefundRequest(PaymentProperties, tenderInfo, amount, currency, div.TerminalSettings.Locale, div.IsTestMode, div.TerminalSettings.TerminalId, extensionTransactionProperties);
            //response = PaymentProcessor.Get(connectorName).Refund(refundRequest, null);
            RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "Refund call is started.");
            response = PaymentHelper.ChainedRefundCall(PaymentProperties, connectorName, this.tenderInfo, amount, currency, div.TerminalSettings.Locale, div.IsTestMode, div.TerminalSettings.TerminalId, extensionTransactionProperties);
            RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "Refund call is completed.");
            if (response.Errors == null)
            {
                hashTable = new PaySdk.Hashtable();
                if (response.Properties != null)
                    hashTable = PaySdk.PaymentProperty.ConvertToHashtable(response.Properties);
                if (div.PaymentConnectorName.Equals(Connectors.AdyenConnector) || div.PaymentConnectorName.Equals(Connectors.CayanConnector) || div.PaymentConnectorName.Equals(Connectors.CloverConnector))  // set Tender state properties for line display handlings
                {
                    PaySdk.PaymentProperty.GetPropertyValue(hashTable, Constants.GenericNamespace.RefundResponse, Constants.RefundResponseProperties.ApprovedAmount, out decimal ApprovedAmount);
                    if (Math.Abs(div.TotalAmount) > Math.Abs(ApprovedAmount))
                    {
                        await UpdateLineDisplay().ConfigureAwait(false);
                        div.TotalAmount -= ApprovedAmount;// for multi tender
                    }
                }
                var propObj = response.Properties.FirstOrDefault(x => x.Name == "SignatureData");
                try
                {
                    //to convert SignatureData to Base64 and bitmap added 
                    if (propObj != null)
                    {
                        if (string.IsNullOrEmpty(propObj.StringValue) == false)
                        {
                            string signatureFormat = string.Empty;
                            PaySdk.PaymentProperty.GetPropertyValue(hashTable, GenericNamespace.TransactionData, "SignatureFormat", out signatureFormat);
                            if (string.IsNullOrEmpty(signatureFormat) == true)
                                signatureFormat = string.Empty;

                            propObj.StringValue = SignatureHelper.ConvertImageBase64(propObj.StringValue, div.PaymentConnectorName, signatureFormat);
                            RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "SignatureData to MapAuthorizeResponse in RefundPaymentAsync method is present.");
                        }
                    }
                    else
                    {
                        RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "SignatureData is empty in RefundPaymentAsync method come from connector SignatureData.");
                    }
                    //to convert SignatureData to Base64 and bitmap added 
                }
                catch (Exception ex)
                {
                    if (propObj != null)
                        propObj.StringValue = "";
                    RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "Failed to Convert Image Base64 in RefundPaymentAsync method Exception: " + ex.Message, "ERROR");
                }
                PaymentHelper.MapRefundResponse(response, paymentInfo);
                PaySdk.PaymentProperty.GetPropertyValue(hashTable, Constants.GenericNamespace.RefundResponse, Constants.PaymentCardProperties.Last4Digits, out s_value);
                paymentInfo.CardNumberMasked = string.Format("{0}{1}", DummyCardNoMasked, s_value);
                
            }
           
            else
            {
                RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "RefundPaymentAsync failed.");
                SetFailedTransactionProp(ref paymentInfo, ref response);
                if (connectorName == Connectors.WorldpayUKConnector || connectorName == Connectors.MonerisConnector || connectorName == Connectors.TriPOSConnector)
                    PaymentHelper.MapRefundResponse(response, paymentInfo);
            }

            this.tenderInfo.CardNumber = DummyCardNumber;
            this.tenderInfo.CardTypeId = 0;

            paymentInfo.Errors = response.Errors;
            RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "RefundPaymentAsync call is Completed.");
            return await Task.FromResult(paymentInfo).ConfigureAwait(false);
        }
        public void SetProperties(ExtensionTransaction extensionTransactionProperties)
        {
            if (extensionTransactionProperties != null && extensionTransactionProperties.ExtensionProperties != null && extensionTransactionProperties.ExtensionProperties.Count > 0)
            {
                CommerceProperty prop = new CommerceProperty();
                if (div.TerminalId == null)
                {
                    prop = extensionTransactionProperties.ExtensionProperties.FirstOrDefault(a => a.Key == "TerminalId");
                    div.RRTerminalId = prop != null ? prop.Value.StringValue : string.Empty;
                }
                if (div.RRCartId == null)
                {
                    prop = extensionTransactionProperties.ExtensionProperties.FirstOrDefault(a => a.Key == "CartId");
                    div.RRCartId = prop != null ? prop.Value.StringValue : string.Empty;
                }
                if (div.RRStaffId == null)
                {
                    prop = extensionTransactionProperties.ExtensionProperties.FirstOrDefault(a => a.Key == "RRStaffId");
                    if (prop != null)
                    {
                        if (!string.IsNullOrEmpty(prop.Value.StringValue))
                            div.RRStaffId = prop != null ? prop.Value.StringValue : string.Empty;
                    }
                }
                if (div.LogAppInsightsKey == null)
                {
                    prop = extensionTransactionProperties.ExtensionProperties.FirstOrDefault(a => a.Key == "LogAppInsightsKey");
                    div.LogAppInsightsKey = prop != null ? prop.Value.StringValue : string.Empty;
                }
                if (div.StoreNumber == null)
                {
                    prop = extensionTransactionProperties.ExtensionProperties.FirstOrDefault(a => a.Key == "StoreNumber");
                    div.StoreNumber = prop != null ? prop.Value.StringValue : "";
                }
                
                if (div.IsLogEnable == null)
                {
                    prop = extensionTransactionProperties.ExtensionProperties.FirstOrDefault(a => a.Key == "ISLOGENABLED");
                    div.IsLogEnable = prop != null ? prop.Value.StringValue : "0";
                }
                if (div.EnableLineDisplay == null)
                {
                    prop = extensionTransactionProperties.ExtensionProperties.FirstOrDefault(a => a.Key == "ISLINEDISPLAY");
                    div.EnableLineDisplay = prop != null ? prop.Value.StringValue : "";
                }

                

                if (string.IsNullOrEmpty(div.Basecounter))
                {
                    prop = extensionTransactionProperties.ExtensionProperties.FirstOrDefault(a => a.Key == "BASECOUNTER");
                    if (prop != null)
                    {
                        if (!string.IsNullOrEmpty(prop.Value.StringValue))
                        {
                            div.Basecounter = prop.Value.StringValue;
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Fetch token for credit card.
        /// </summary>
        /// <param name="isManualEntry">The value indicating whether credit card should be entered manually.</param>
        /// <param name="extensionTransactionProperties">Optional extension transaction properties.</param>
        /// <returns>A task that can await until the token generation has completed.</returns>
        [SuppressMessage("Microsoft.Usage", "CA1801", Justification = "Suppressed on adding CA for backward compatibility")]
        private async Task<PaymentInfo> FetchTokenAsync(bool isManualEntry, ExtensionTransaction extensionTransactionProperties)
        {
            RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "FetchTokenAsync call is started");
            //string methodName = "FetchTokenAsync";
            PaySdk.Response response = new PaySdk.Response();
            PaymentInfo paymentInfo = new PaymentInfo();
            PaySdk.PaymentProperty[] defaultMerchantProperties = this.merchantProperties;
            if (string.IsNullOrEmpty(div.PaymentConnectorName))
            {
                PaySdk.Hashtable hashTable = PaySdk.PaymentProperty.ConvertToHashtable(merchantProperties);
                PaySdk.PaymentProperty.GetPropertyValue(hashTable, Constants.GenericNamespace.MerchantAccount, "ConnectorName", out string _connector);
                div.PaymentConnectorName = _connector;
            }
            
           
            paymentInfo.CardNumberMasked = this.tenderInfo.CardNumber;
            paymentInfo.CashbackAmount = this.tenderInfo.CashBackAmount;
            paymentInfo.CardType = (CardType)this.tenderInfo.CardTypeId;

            if (this.processor == null)
            {
                this.processor = PaymentProcessor.Get(div.PaymentConnectorName);
            }
            this.tenderInfo = GetCustomerAddress(extensionTransactionProperties);
            // Generate card token
            PaySdk.Request request = PaymentHelper.GetTokenRequest(defaultMerchantProperties, this.tenderInfo, div.TerminalSettings.Locale, extensionTransactionProperties);
            //Microsoft.Dynamics.Retail.Diagnostics.RetailLogger.Log.ExtendedInformationalEvent("GenerateCardToken call is started.", this.GetType().Name, methodName);
            RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "GenerateCardToken call is started.");
            response = this.processor.GenerateCardToken(request, null);
            //Microsoft.Dynamics.Retail.Diagnostics.RetailLogger.Log.ExtendedInformationalEvent("GenerateCardToken call is completed.", this.GetType().Name, methodName);
            RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "GenerateCardToken call is completed.");
            PaymentHelper.MapTokenResponse(response, paymentInfo);
            if (tenderInfo != null)
            {
                this.tenderInfo.Name = "";
                this.tenderInfo.Address = "";
                this.tenderInfo.City = "";
                this.tenderInfo.Country = "";
                this.tenderInfo.StateOrProvince = "";
                this.tenderInfo.Zip = "";
            }
            
            RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "FetchTokenAsync call is completed.");
            return await Task.FromResult<PaymentInfo>(paymentInfo).ConfigureAwait(false);
        }

        
        
        /// <summary>
        /// ExecuteTaskAsync.
        /// </summary>
        /// <param name="token">Token of peripheral device.</param>
        /// <param name="task">Task Name of peripheral device.</param>
        /// <param name="extensionTransactionProperties">Optional extension transaction properties.</param>
        /// <returns>A task that can be awaited until the connection is opened.</returns>
        public async Task<ExtensionTransaction> ExecuteTaskAsync(string token, string task, ExtensionTransaction extensionTransactionProperties)
        {
            RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "ExecuteTaskAsync call started. Task: "+ task);
            //string methodName = "ExecuteTaskAsync";
            ExtensionTransaction extensionTransaction = new ExtensionTransaction();
            ICollection<CommerceProperty> extProperties = new System.Collections.ObjectModel.Collection<CommerceProperty>();
            CommerceProperty comProp;
            PaySdk.Response response = new PaySdk.Response();
            PaySdk.Request request = new PaySdk.Request();
            
            // handle pay other connector
            string connectorName = div.PaymentConnectorName;
            string s_value = "";
             bool IsIncrementTransactionId = true;
            
           
            request.Locale = div.Locale;
            
            List<PaySdk.PaymentProperty> properties = request.Properties.ToList();
            
            if (response.Errors != null)
            {
                return await Task.FromResult<ExtensionTransaction>(extensionTransaction).ConfigureAwait(false);
            }
            PaymentHelper.AddExtensionTransactionProperties(properties, extensionTransactionProperties);
            properties.Add(new PaySdk.PaymentProperty(Constants.GenericNamespace.TransactionData, Constants.TransactionDataProperties.TaskName, task));
            request.Properties = properties.ToArray();
            this.processorExt = this.processor as PaySdk.IPaymentProcessorExtension;
            
			
            
            PaymentHelper.SetTransactionProp(ref extensionTransaction, response);
            RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "ExecuteTaskAsync call is completed.");
            return await Task.FromResult<ExtensionTransaction>(extensionTransaction).ConfigureAwait(false);
        }

        private async Task<string> GetPaymentTransactionReferenceAsync(GetTransactionReferencePaymentTerminalDeviceRequest request)
        {
            RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "GetPaymentTransactionReferenceAsync call started.");
            Microsoft.Dynamics.Commerce.Runtime.ThrowIf.Null<GetTransactionReferencePaymentTerminalDeviceRequest>(request, "request");
            string referenceID = string.Empty;
            switch (div.PaymentConnectorName)
            {

                case Connectors.NBEConnector:
                
                    referenceID = GetConnectorTransID();
                    break;
          
                default:
                    referenceID = string.Empty;
                    break;
            }
            RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "GetPaymentTransactionReferenceAsync called generated referenceID: " + referenceID);
            RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "GetPaymentTransactionReferenceAsync call completed.");
            return await Task.FromResult(referenceID).ConfigureAwait(false);
        }

        internal Task<PaymentInfo> GetTransactionByTransactionReferenceAsync(GetTransactionByTransactionReferencePaymentTerminalDeviceRequest request)
        {
            RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "GetTransactionByTransactionReferenceAsync call is started.");
            ThrowIf.Null(request, "request");
            var getTransByTransRef = new GetTransactionByTransactionReferencePaymentTerminalDeviceRequest(request.LockToken, request.TransactionReferenceData);
            PaymentInfo paymentInfo = null;
            if(div == null)
            {
                RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "GetTransactionByTransactionReferenceAsync div is null.");
            }
            switch (div.PaymentConnectorName)
            {
                case Connectors.NBEConnector:
                
                    div.ConnectorTransId = request.TransactionReferenceData.IdFromConnector;
                    break;
                
                default:
                    div.ConnectorTransId = string.Empty;
                    break;
            }
            if (string.IsNullOrEmpty(div.ConnectorTransId))
            {
                return Task.FromResult(paymentInfo);
            }
            RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "GetTransactionByTransactionReferenceAsync called generated TransId: " + div.ConnectorTransId);
            System.Globalization.RegionInfo ri = new System.Globalization.RegionInfo(div.TerminalSettings.Locale);
            ExtensionTransaction extensionTransaction = new ExtensionTransaction() { ExtensionProperties = new List<CommerceProperty>() };
            var strTRD = Newtonsoft.Json.JsonConvert.SerializeObject(request.TransactionReferenceData);
            if (strTRD == null)
            {
                RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "GetTransactionByTransactionReferenceAsync Unable to convert TransactionReferenceData.");
            }
            extensionTransaction.ExtensionProperties.Add(new CommerceProperty
            {
                Key = "TransactionReferenceData",
                Value = strTRD
            });
            extensionTransaction.ExtensionProperties.Add(new CommerceProperty
            {
                Key = "GetTransaction",
                Value = "true"
            });
            
            if (request.TransactionReferenceData.Command == Constants.SupportedCorrelationCommands.Sale || (div.TranIdToCheckCancel != div.ConnectorTransId && div.PaymentProcessors != null && div.PaymentProcessors.Length > 1))
            {
                try
                {
                    RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "GetTransactionByTransactionReferenceAsync starting Authorize method.");
                    paymentInfo = Utilities.WaitAsyncTask(() => this.AuthorizePaymentAsync(div.PaymentConnectorName, request.TransactionReferenceData.Amount, ri.ISOCurrencySymbol, "", false, null, extensionTransaction));
                    RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "GetTransactionByTransactionReferenceAsync Authorize method completed.");
                    var isNoTransactionFound = false;
                    if (!paymentInfo.IsApproved)
                    {
                        RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "GetTransactionByTransactionReferenceAsync payment not approved.");
                        if (paymentInfo.Errors.Length > 0)
                        {
                            if (paymentInfo.Errors.Any(a => a.Code.Equals(PaySdk.ErrorCode.NoTransactionFoundByTransactionReference)))
                            {
                                isNoTransactionFound = true;
                            }
                        }
                        if (div.PaymentProcessors != null && div.PaymentProcessors.Length > 1)
                        {
                            paymentInfo = null;
                        }
                    }
                    if (!isNoTransactionFound)
                    {
                        if (paymentInfo.ApprovedAmount == decimal.Zero)
                            paymentInfo = null;
                    }
                }
                catch (Exception ex)
                {
                    RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "Error in AuthorizePaymentAsync method call Error: " + ex.Message, "ERROR");
                    paymentInfo = null;
                }

                //duplicate check for multiple Connectors
                if (paymentInfo == null)
                {
                    PaySdk.Hashtable hashTable = new PaySdk.Hashtable();
                    string connectorName = "";
                    if (div.PaymentProcessors != null && div.PaymentProcessors.Length > 1)
                    {
                        foreach (PaySdk.PaymentProperty pp in div.AllMerchantProperties)
                        {
                            if (pp.Name.Equals(Constants.ConnectorProperties.Properties))
                            {
                                hashTable = PaySdk.PaymentProperty.ConvertToHashtable(pp.PropertyList);
                                PaySdk.PaymentProperty.GetPropertyValue(hashTable, Constants.GenericNamespace.Connector, Constants.ConnectorProperties.ConnectorName, out string s_value);
                                if (div.PaymentConnectorName != s_value)
                                    connectorName = s_value;
                            }
                        }
                        if (!string.IsNullOrEmpty(connectorName))
                        {
                            RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "GetTransactionByTransactionReferenceAsync starting Authorize for other connectors.");
                            paymentInfo = Utilities.WaitAsyncTask(() => this.AuthorizePaymentAsync(connectorName, request.TransactionReferenceData.Amount, ri.ISOCurrencySymbol, "", false, null, extensionTransaction));
                            if (paymentInfo.ApprovedAmount == decimal.Zero)
                                paymentInfo = null;
                        }
                    }
                }
                //handle multiple Connectors
            }
            else if (request.TransactionReferenceData.Command == Constants.SupportedCorrelationCommands.Refund)
            {
                // getting issue with refund from MS 
                //paymentInfo = Utilities.WaitAsyncTask(() => this.RefundPaymentAsync(request.TransactionReferenceData.Amount, currency, false, null, extensionTransaction));
            }
            RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "GetTransactionByTransactionReferenceAsync call is completed.");
            return Task.FromResult(paymentInfo);
        }

        private Task<PaymentInfo> AuthorizeCardTokenCardPaymentAsync(PaySdk.PaymentProperty[] merchantProperties, PaySdk.PaymentProperty[] cardTokenProperties, decimal amount, string currency, string locale, bool isTestMode, string terminalId, ExtensionTransaction extensionTransactionProperties)
        {
            string methodName = "AuthorizeCardTokenCardPaymentAsync";
            string s_value = string.Empty;
            RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", methodName + " call is started.");
            string connectorName = div.PaymentConnectorName;
            var payProp = cardTokenProperties.FirstOrDefault(a => a.Name == "ConnectorName");
            if (payProp != null)
            {
                var strConnectorName = payProp.StringValue;
                connectorName = strConnectorName;
            }
            if (this.processor == null || div.PaymentConnectorName != connectorName)
            {
                this.processor = PaymentProcessor.Get(connectorName);
            }
            // handle pay other connector >>

            //multiple Connectors   if MerchantPropertiesXml not present in response
            PaySdk.Hashtable hashTable = new PaySdk.Hashtable();

            if (div.PaymentProcessors != null && div.PaymentProcessors.Length > 1)
            {
                List<PaySdk.PaymentProperty> properties = new List<PaySdk.PaymentProperty>();
                foreach (PaySdk.PaymentProperty pp in div.AllMerchantProperties)
                {
                    if (pp.Name.Equals(Constants.ConnectorProperties.Properties))
                    {
                        hashTable = PaySdk.PaymentProperty.ConvertToHashtable(pp.PropertyList);
                        PaySdk.PaymentProperty.GetPropertyValue(hashTable, Constants.GenericNamespace.Connector, Constants.ConnectorProperties.ConnectorName, out s_value);
                        if (connectorName == s_value)
                            properties.AddRange(pp.PropertyList);
                    }
                    else
                    {
                        properties.Add(pp);
                    }
                }
                merchantProperties = properties.ToArray();
                //this.processor = null;
            }
            else
            {
                merchantProperties = this.merchantProperties;
            }

            PaymentInfo paymentInfo = new PaymentInfo();
            //PaySdk.Hashtable hashTable = new PaySdk.Hashtable();
            PaymentHelper.GetRequiredConnector(merchantProperties, cardTokenProperties, this.processor, out PaySdk.IPaymentProcessor currentProcessor, out PaySdk.PaymentProperty[] currentMerchantProperties);
            PaySdk.Response response = new PaySdk.Response();
            if (Connectors.HeartlandConnector.Equals(connectorName))
            {
                div.ConnectorTransId = string.Empty;
            }
            SetExtensionTransactionProp(ref extensionTransactionProperties, response, true);
            if (response.Errors != null)
            {
                paymentInfo.Errors = response.Errors;
                return Task.FromResult<PaymentInfo>(paymentInfo);
            }
            PaySdk.Request authorizeCardTokenRequest = PaymentHelper.GetAuthorizeCardTokenRequest(currentMerchantProperties, cardTokenProperties, amount, currency, locale, isTestMode, terminalId, extensionTransactionProperties);
            //RetailLogger.Log.ExtendedInformationalEvent("Authorize call is started.", this.GetType().Name, methodName);
            response = currentProcessor.Authorize(authorizeCardTokenRequest, null);
            Guid cardStorageKey = Guid.NewGuid();
            if (response.Errors == null)
            {
                hashTable = new PaySdk.Hashtable();
                if (response.Properties != null)
                    hashTable = PaySdk.PaymentProperty.ConvertToHashtable(response.Properties);
                try
                {
                    PaymentHelper.MapAuthorizeResponse(response, paymentInfo, cardStorageKey, div.TerminalSettings.TerminalId);
                }
                catch (Exception ex)
                {
                    //RetailLogger.Log.ExtendedErrorEvent(ex.Message, this.GetType().Name, methodName);
                    RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "Failed to MapAuthorizeResponse in AuthorizeCardTokenCardPaymentAsync method Exception: " + ex.Message, "ERROR");
                    //throw ex;
                }
                PaySdk.PaymentProperty.GetPropertyValue(hashTable, Constants.GenericNamespace.AuthorizationResponse, Constants.PaymentCardProperties.Last4Digits, out s_value);
                paymentInfo.CardNumberMasked = string.Format("{0}{1}", DummyCardNoMasked, s_value);
            }
            paymentInfo.Errors = response.Errors;

            //RetailLogger.Log.ExtendedInformationalEvent("Authorize call is completed.", this.GetType().Name, methodName);
            RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", methodName + " call is completed.");
            return Task.FromResult<PaymentInfo>(paymentInfo);
        }

        public Task<PaymentInfo> AuthorizePaymentCardPaymentAsync(AuthorizePaymentCardPaymentRequest request)
        {
            string methodName = "AuthorizePaymentCardPaymentAsync";
            string s_value = string.Empty;
            RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "AuthorizePaymentCardPaymentAsync call is started.");
            // handle pay other connector >>
            string connectorName = string.IsNullOrEmpty(request.PaymentConnectorName) ? div.PaymentConnectorName : request.PaymentConnectorName;
            //multiple Connectors
            PaySdk.Hashtable hashTable = new PaySdk.Hashtable();
            PaySdk.PaymentProperty[] PaymentProperties = null;
            if (div.PaymentProcessors != null && div.PaymentProcessors.Length > 1)
            {
                List<PaySdk.PaymentProperty> properties = new List<PaySdk.PaymentProperty>();
                foreach (PaySdk.PaymentProperty pp in div.AllMerchantProperties)
                {
                    if (pp.Name.Equals(Constants.ConnectorProperties.Properties))
                    {
                        hashTable = PaySdk.PaymentProperty.ConvertToHashtable(pp.PropertyList);
                        PaySdk.PaymentProperty.GetPropertyValue(hashTable, Constants.GenericNamespace.Connector, Constants.ConnectorProperties.ConnectorName, out s_value);
                        if (connectorName == s_value)
                            properties.AddRange(pp.PropertyList);
                    }
                    else
                    {
                        properties.Add(pp);
                    }
                }
                PaymentProperties = properties.ToArray();
                this.processor = null;
            }
            else
            {
                PaymentProperties = this.merchantProperties;
            }
            //
            if (this.processor == null)
            {
                this.processor = PaymentProcessor.Get(connectorName);
            }
            PaymentInfo paymentInfo = new PaymentInfo();
            hashTable = new PaySdk.Hashtable();
            //CardPaymentManager.GetRequiredConnector(this.merchantProperties, request., this.processor, out currentProcessor, out currentMerchantProperties);
            PaySdk.Response response = new PaySdk.Response();
            var extensionTransactionProperties = request.ExtensionTransactionProperties;
            SetExtensionTransactionProp(ref extensionTransactionProperties, response, true);
            if (response.Errors != null)
            {
                paymentInfo.Errors = response.Errors;
                return Task.FromResult<PaymentInfo>(paymentInfo);
            }

            request.TenderInfo.ExpirationMonth = request.TenderInfo.ExpirationMonth == 0 ? this.tenderInfo.ExpirationMonth : request.TenderInfo.ExpirationMonth;
            request.TenderInfo.ExpirationYear = request.TenderInfo.ExpirationYear == 0 ? this.tenderInfo.ExpirationYear : request.TenderInfo.ExpirationYear;

            response = PaymentHelper.InvokeAuthorizationCall(PaymentProperties, request.PaymentConnectorName, request.TenderInfo, request.Amount, request.Currency, div.TerminalSettings.Locale, div.IsTestMode, div.TerminalSettings.TerminalId, extensionTransactionProperties);
            //RetailLogger.Log.ExtendedInformationalEvent("InvokeAuthorizationCall call is started.", this.GetType().Name, methodName);
            RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "AuthorizePaymentCardPaymentAsync call is completed.");
            Guid cardStorageKey = Guid.NewGuid();
            if (response.Errors == null)
            {
                hashTable = new PaySdk.Hashtable();
                if (response.Properties != null)
                    hashTable = PaySdk.PaymentProperty.ConvertToHashtable(response.Properties);
                try
                {
                    PaymentHelper.MapAuthorizeResponse(response, paymentInfo, cardStorageKey, div.TerminalSettings.TerminalId);
                }
                catch (Exception ex)
                {
                    //RetailLogger.Log.ExtendedErrorEvent(ex.Message, this.GetType().Name, methodName);
                    RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "Failed to MapAuthorizeResponse in AuthorizePaymentCardPaymentAsync method Exception: " + ex.Message, "ERROR");
                    //throw ex;
                }
                paymentInfo.Errors = response.Errors;
                PaySdk.PaymentProperty.GetPropertyValue(hashTable, Constants.GenericNamespace.AuthorizationResponse, Constants.PaymentCardProperties.Last4Digits, out s_value);
                paymentInfo.CardNumberMasked = string.Format("{0}{1}", DummyCardNoMasked, s_value);
            }
            else
            {
                SetFailedTransactionProp(ref paymentInfo, ref response);
            }
            paymentInfo.Errors = response.Errors;
            //RetailLogger.Log.ExtendedInformationalEvent("InvokeAuthorizationCall  call is completed.", this.GetType().Name, methodName);
            RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "AuthorizePaymentCardPaymentAsync call is completed.");
            return Task.FromResult<PaymentInfo>(paymentInfo);
        }

        public Task<PaymentInfo> CapturePaymentCardPaymentAsync(CapturePaymentCardPaymentRequest request)
        {
            //string methodName = "CapturePaymentCardPaymentAsync";
            RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "CapturePaymentCardPaymentAsync call started.");
            string s_value = string.Empty;
            PaySdk.PaymentProperty[] capturePaymentMerchantProps = PaymentHelper.ToLocalProperties(request.MerchantPropertiesXml);
            RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "CapturePaymentCardPaymentAsync captureMercahntProperties loaded.");
            PaySdk.PaymentProperty[] cardTokenProperties = PaymentHelper.ToLocalProperties(request.PaymentPropertiesXml);
            RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "CapturePaymentCardPaymentAsync paymentProperties loaded.");
            // handle pay other connector >>
            if (div ==null)
            {
                RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "CapturePaymentCardPaymentAsync div is null.");
            }
            string connectorName = div.PaymentConnectorName;
            var payProp = cardTokenProperties.FirstOrDefault(a => a.Name == "ConnectorName");
            if (payProp != null)
            {
                var strConnectorName = payProp.StringValue;
                connectorName = strConnectorName;
            }
            // handle pay other connector >>

            //multiple Connectors   if MerchantPropertiesXml not present in response
            PaySdk.Hashtable hashTable = new PaySdk.Hashtable();
            if (string.IsNullOrEmpty(request.MerchantPropertiesXml))
            {
                if (div.PaymentProcessors != null && div.PaymentProcessors.Length > 1)
                {
                    List<PaySdk.PaymentProperty> properties = new List<PaySdk.PaymentProperty>();
                    foreach (PaySdk.PaymentProperty pp in div.AllMerchantProperties)
                    {
                        if (pp.Name.Equals(Constants.ConnectorProperties.Properties))
                        {
                            hashTable = PaySdk.PaymentProperty.ConvertToHashtable(pp.PropertyList);
                            PaySdk.PaymentProperty.GetPropertyValue(hashTable, Constants.GenericNamespace.Connector, Constants.ConnectorProperties.ConnectorName, out s_value);
                            if (connectorName == s_value)
                                properties.AddRange(pp.PropertyList);
                        }
                        else
                        {
                            properties.Add(pp);
                        }
                    }
                    capturePaymentMerchantProps = properties.ToArray();
                    this.processor = null;
                }
                else
                {
                    capturePaymentMerchantProps = this.merchantProperties;
                }
            }
            RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "CapturePaymentCardPaymentAsync merchant Properties loaded.");
            // adding preoperties from constant file\
			
			
			
            try
            {
                var propList = capturePaymentMerchantProps.ToList();
                AppStorage storage = new AppStorage(connectorName, div.Basecounter);
                var connectorProp = storage.GetTerminalProperties();
                if (connectorProp.Count > 0)
                {
                    propList.AddRange(connectorProp);
                    capturePaymentMerchantProps = propList.ToArray();
                }
            }
            catch (Exception ex)
            {
                RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "Error in CapturePaymentCardPaymentAsync storage.GetTerminalProperties method Error: " + ex.Message, "ERROR");
            }
            //================>>
            if (this.processor == null || div.PaymentConnectorName != connectorName)
            {
                this.processor = PaymentProcessor.Get(connectorName);
            }
            RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "CapturePaymentCardPaymentAsync PaymentProcessor.Get completed.");

            PaymentInfo paymentInfo = new PaymentInfo();
            PaymentHelper.GetRequiredConnector(capturePaymentMerchantProps, cardTokenProperties, this.processor, out PaySdk.IPaymentProcessor currentProcessor, out PaySdk.PaymentProperty[] currentMerchantProperties);
            RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "CapturePaymentCardPaymentAsync GetRequiredConnector completed.");
            PaySdk.Response response = new PaySdk.Response();
            var extensionTransactionProperties = request.ExtensionTransactionProperties;
            bool IsIncrementTransactionId = true;
            if (div.PaymentConnectorName == Connectors.CardConnectConnector) // do not increment invoice id in card connect
                IsIncrementTransactionId = false;
            if (connectorName != div.PaymentConnectorName)
            {
                CommerceProperty comProp = new CommerceProperty();
                comProp.Key = "PayOtherConnector";
                comProp.Value = connectorName;
                if (extensionTransactionProperties != null)
                {
                    if (extensionTransactionProperties.ExtensionProperties != null)
                        extensionTransactionProperties.ExtensionProperties.Add(comProp);
                }
            }
            RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "CapturePaymentCardPaymentAsync SetExtensionTransactionProp started.");
            SetExtensionTransactionProp(ref extensionTransactionProperties, response, true, IsIncrementTransactionId);
            RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "CapturePaymentCardPaymentAsync SetExtensionTransactionProp completed.");
            if (response.Errors != null)
            {
                paymentInfo.Errors = response.Errors;
                return Task.FromResult<PaymentInfo>(paymentInfo);
            }

            // Device IP & Port <<
            if (!string.IsNullOrEmpty(div.KeySettings.Key))
            {
                currentMerchantProperties = currentMerchantProperties.Concat(new PaySdk.PaymentProperty[] {
                                    new PaySdk.PaymentProperty(Constants.GenericNamespace.MerchantAccount, "IpAddress", div.KeySettings.Key ) }).ToArray();
            }
            if (!string.IsNullOrEmpty(div.KeySettings.Value) && (div.KeySettings.Value != "0"))
            {
                currentMerchantProperties = currentMerchantProperties.Concat(new PaySdk.PaymentProperty[] {
                            new PaySdk.PaymentProperty(Constants.GenericNamespace.MerchantAccount, "Port", div.KeySettings.Value) }).ToArray();
            }
            // Device IP & Port >>

            PaySdk.Request authorizeCardTokenRequest = PaymentHelper.GetCaptureVoidRequest(currentMerchantProperties, cardTokenProperties, request.Amount, request.Currency, div.Locale, div.IsTestMode, div.TerminalSettings.TerminalId, extensionTransactionProperties);
            //RetailLogger.Log.ExtendedInformationalEvent("Capture call is started.", this.GetType().Name, methodName);
            RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "Capture call is started.");
            response = currentProcessor.Capture(authorizeCardTokenRequest);

            if (response.Errors == null)
            {

                hashTable = new PaySdk.Hashtable();
                if (response.Properties != null)
                    hashTable = PaySdk.PaymentProperty.ConvertToHashtable(response.Properties);
                PaymentHelper.MapCaptureResponse(response, paymentInfo);
                PaySdk.PaymentProperty.GetPropertyValue(hashTable, Constants.GenericNamespace.CaptureResponse, Constants.PaymentCardProperties.Last4Digits, out s_value);
                paymentInfo.CardNumberMasked = string.Format("{0}{1}", DummyCardNoMasked, s_value);
            }
            //RetailLogger.Log.ExtendedInformationalEvent("Capture call is completed.", this.GetType().Name, methodName);
            RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "Capture call is completed.");
            RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "CapturePaymentCardPaymentAsync call completed.");
            return Task.FromResult<PaymentInfo>(paymentInfo);
        }

        public Task<PaymentInfo> RefundCaptureTokenRequestAsync(PaySdk.PaymentProperty[] merchantProperties, PaySdk.PaymentProperty[] captureTokenProperties, decimal amount, string currency, string locale, bool isTestMode, string terminalId, ExtensionTransaction extensionTransactionProperties)
        {
            RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "RefundCaptureTokenRequestAsync call started.");
            //string methodName = "RefundCaptureTokenRequestAsync";
            string s_value = string.Empty;
            div.IsSwipe = true; // done for Moneris need to check for others
            // handle pay other connector >>
            string connectorName = div.PaymentConnectorName;
            var payProp = captureTokenProperties.FirstOrDefault(a => a.Name == "ConnectorName");
            if (payProp != null)
            {
                var strConnectorName = payProp.StringValue;
                connectorName = strConnectorName;
            }
            //multiple Connectors
            PaySdk.Hashtable hashTable = new PaySdk.Hashtable();
            PaySdk.PaymentProperty[] PaymentProperties = null;
            if (div.PaymentProcessors != null && div.PaymentProcessors.Length > 1)
            {
                List<PaySdk.PaymentProperty> properties = new List<PaySdk.PaymentProperty>();
                foreach (PaySdk.PaymentProperty pp in div.AllMerchantProperties)
                {
                    if (pp.Name.Equals(Constants.ConnectorProperties.Properties))
                    {
                        hashTable = PaySdk.PaymentProperty.ConvertToHashtable(pp.PropertyList);
                        PaySdk.PaymentProperty.GetPropertyValue(hashTable, Constants.GenericNamespace.Connector, Constants.ConnectorProperties.ConnectorName, out s_value);
                        if (connectorName == s_value)
                            properties.AddRange(pp.PropertyList);
                    }
                    else
                    {
                        properties.Add(pp);
                    }
                }
                PaymentProperties = properties.ToArray();
                this.processor = null;
            }
            else
            {
                PaymentProperties = this.merchantProperties;
            }
           
            // handle pay other connector >>
            if (this.processor == null)
            {
                this.processor = PaymentProcessor.Get(connectorName);
            }
            PaymentInfo paymentInfo = new PaymentInfo();
            hashTable = new PaySdk.Hashtable();
            PaymentHelper.GetRequiredConnector(PaymentProperties, captureTokenProperties, this.processor, out PaySdk.IPaymentProcessor currentProcessor, out PaySdk.PaymentProperty[] currentMerchantProperties);
            PaySdk.Response response = new PaySdk.Response();
            SetExtensionTransactionProp(ref extensionTransactionProperties, response, true);
            if (response.Errors != null)
            {
                paymentInfo.Errors = response.Errors;
                return Task.FromResult<PaymentInfo>(paymentInfo);
            }
            PaySdk.Request refundCaptureTokenRequest = PaymentHelper.GetRefundCaptureTokenRequest(currentMerchantProperties, captureTokenProperties, amount, currency, locale, isTestMode, terminalId, extensionTransactionProperties);
            //RetailLogger.Log.ExtendedInformationalEvent("RefundCapture call is started.", this.GetType().Name, methodName);
            RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "Refund call is started.");
            response = currentProcessor.Refund(refundCaptureTokenRequest, null);
            RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "Refund call is completed.");

            if (response.Errors == null)
            {
                hashTable = new PaySdk.Hashtable();
                if (response.Properties != null)
                    hashTable = PaySdk.PaymentProperty.ConvertToHashtable(response.Properties);

                var propObj = response.Properties.FirstOrDefault(x => x.Name == "SignatureData");
                try
                {
                    //to convert SignatureData to Base64 and bitmap added 
                    if (propObj != null)
                    {
                        if (string.IsNullOrEmpty(propObj.StringValue) == false)
                        {
                            string signatureFormat = string.Empty;
                            PaySdk.PaymentProperty.GetPropertyValue(hashTable, GenericNamespace.TransactionData, "SignatureFormat", out signatureFormat);
                            if (string.IsNullOrEmpty(signatureFormat) == true)
                                signatureFormat = string.Empty;

                            propObj.StringValue = SignatureHelper.ConvertImageBase64(propObj.StringValue, div.PaymentConnectorName, signatureFormat);
                            RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "SignatureData to MapAuthorizeResponse in RefundPaymentAsync method is present.");
                        }
                    }
                    else
                    {
                        RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "SignatureData is empty in RefundPaymentAsync method come from connector SignatureData.");
                    }
                    //to convert SignatureData to Base64 and bitmap added 
                }
                catch (Exception ex)
                {
                    if (propObj != null)
                        propObj.StringValue = "";
                    RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "Failed to Convert Image Base64 in RefundPaymentAsync method Exception: " + ex.Message, "ERROR");
                }

                PaymentHelper.MapRefundResponse(response, paymentInfo);
                PaySdk.PaymentProperty.GetPropertyValue(hashTable, Constants.GenericNamespace.RefundResponse, Constants.PaymentCardProperties.Last4Digits, out s_value);
                paymentInfo.CardNumberMasked = string.Format("{0}{1}", DummyCardNoMasked, s_value);
            }
            else if (response.Errors != null)
            {
                PaymentHelper.MapRefundResponse(response, paymentInfo);
                PaySdk.PaymentProperty.GetPropertyValue(hashTable, Constants.GenericNamespace.RefundResponse, Constants.PaymentCardProperties.Last4Digits, out s_value);
                paymentInfo.CardNumberMasked = string.Format("{0}{1}", DummyCardNoMasked, s_value);
                paymentInfo.Errors = response.Errors;
            }
            //RetailLogger.Log.ExtendedInformationalEvent("RefundCapture  call is completed.", this.GetType().Name, methodName);
            RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "RefundCaptureTokenRequestAsync call completed.");
            return Task.FromResult<PaymentInfo>(paymentInfo);
        }

        private string GetConnectorTransID()
        {
            try
            {
                if (string.IsNullOrEmpty(div.Basecounter))
                    div.Basecounter = DeviceStaticVars.Basecounter;
                AppStorage storage = new AppStorage(div.PaymentConnectorName, div.Basecounter, div.TerminalId);
                div.ConnectorTransId = storage.GetCounter();
                div.TranIdToCheckCancel = div.ConnectorTransId;
                RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "GetConnectorTransID called TransID: " + div.ConnectorTransId);
            }
            catch (Exception ex)
            {
                RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "Failed to GetConnectorTransID Exception: " + ex.Message, "ERROR");
                throw ex;
            }
            return div.ConnectorTransId;
        }

        private string GetTransactionCounter(PaySdk.Response response)
        {
            RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "GetTransactionCounter call is started.");
            //string methodName = "GetTransactionCounter";
            string transcounter = string.Empty;
            List<PaySdk.PaymentError> errors = new List<PaySdk.PaymentError>();
            try
            {
                string[] transCounterConnectorArray = new string[]
                {
                    Connectors.NBEConnector
                };
                if (transCounterConnectorArray.Contains(div.PaymentConnectorName))
                {
                    AppStorage storage = new AppStorage(div.PaymentConnectorName, div.Basecounter,div.TerminalId);
                    transcounter = div.ConnectorTransId;
                    if (string.IsNullOrEmpty(transcounter))
                    {
                        transcounter = storage.GetCounter();
                    }
                    RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "GetTransactionCounter called transcounter: " + transcounter);
                    if (string.IsNullOrEmpty(DeviceStaticVars.PreviousCounter))
                    {
                        DeviceStaticVars.PreviousCounter = transcounter;
                    }
                    else if (!string.IsNullOrEmpty(DeviceStaticVars.PreviousCounter))
                    {
                        if (Convert.ToInt64(DeviceStaticVars.PreviousCounter) == Convert.ToInt64(transcounter) && string.IsNullOrEmpty(div.ConnectorTransId))
                        {
                            RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "The payment provider rejected the transaction because the Transaction id is a duplicate of a previous transaction!!!");
                            throw new PaymentException("The payment provider rejected the transaction because the Transaction id is a duplicate of a previous transaction!!!");
                        }
                        else
                        {
                            DeviceStaticVars.PreviousCounter = transcounter;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "Error in GetTransactionCounter method Exception: " + ex.Message, "ERROR");
                errors.Add(new PaySdk.PaymentError(PaySdk.ErrorCode.Decline, ex.Message, true));
                response.Errors = errors.ToArray();
            }
            RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "GetTransactionCounter call is completed.");
            return transcounter;
        }

        private void SetKeySettings(ICollection<CommerceProperty> extProperties)
        {
            if (extProperties != null && extProperties.Count > 0)
            {
                CommerceProperty comProperty;
                comProperty = extProperties.FirstOrDefault(cp => cp.Key == "IpAddress" && cp.Value.StringValue != null);
                string _ipAddress = comProperty != null ? comProperty.Value.StringValue : string.Empty;
                comProperty = extProperties.FirstOrDefault(cp => cp.Key == "Port" && cp.Value.IntegerValue != null);
                string _port = comProperty != null ? comProperty.Value.IntegerValue.ToString() : string.Empty;
                div.KeySettings = new KeyValuePair<string, string>(_ipAddress, _port);
            }
        }

        private async Task UpdateLineDisplay()
        {
            RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "UpdateLineDisplay call is started");
            if (div.PaymentConnectorName.Equals(Connectors.NBEConnector))
            {
                if (this.processor == null)
                {
                    this.processor = PaymentProcessor.Get(div.PaymentConnectorName);
                }
                if (this.processor.Name == "SensePassConnector") { return; }

                this.processorExt = this.processor as PaySdk.IPaymentProcessorExtension;

                PaySdk.Request request = new PaySdk.Request
                {
                    Locale = div.Locale,
                    Properties = this.merchantProperties
                };

                List<PaySdk.PaymentProperty> properties = request.Properties.ToList();
                if (div.PaymentConnectorName.Equals(Connectors.CayanConnector))
                {
                    properties.Add(new PaySdk.PaymentProperty(Constants.GenericNamespace.TransactionData, "EFTTerminalId", div.TerminalSettings.TerminalId));
                }
                properties.Add(new PaySdk.PaymentProperty(Constants.GenericNamespace.TransactionData, "RRTerminalId", string.IsNullOrEmpty(div.RRTerminalId) ? string.Empty : div.RRTerminalId));
                properties.Add(new PaySdk.PaymentProperty(Constants.GenericNamespace.TransactionData, Constants.TransactionDataProperties.TaskName, "linedisplay"));
                properties.Add(new PaySdk.PaymentProperty(Constants.GenericNamespace.TransactionData, "LineData", div.CartData));
                properties.Add(new PaySdk.PaymentProperty(Constants.GenericNamespace.TransactionData, "DisplayOption", "DisplayItem"));
                properties.Add(new PaySdk.PaymentProperty(Constants.GenericNamespace.TransactionData, "EnableLineDisplay", div.EnableLineDisplay ?? string.Empty));
                properties.Add(new PaySdk.PaymentProperty(Constants.GenericNamespace.TransactionData, "DeviceSerialNumber", div.DeviceSerialNumber ?? string.Empty));
                properties.Add(new PaySdk.PaymentProperty(Constants.GenericNamespace.TransactionData, "EventType", div.eventTypeName ?? string.Empty));
                properties.Add(new PaySdk.PaymentProperty(Constants.GenericNamespace.TransactionData, "CartCurrencyCode", div.CartCurrencyCode ?? string.Empty));

                

                request.Properties = properties.ToArray();
                RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "UpdateLineDisplay Execute started.");
                if (div.PaymentConnectorName.Equals(Connectors.HeartlandConnector))
                {
                    this.processor.GenerateCardToken(request, null);
                }
                else
                {
                    this.processorExt.ExecuteTask(request);
                }
                RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "UpdateLineDisplay Execute completed.");
            }
            
            RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "UpdateLineDisplay call is completed.");
        }

        private Task<string> ConvertToCartStringAsync(string subTotalAmount, string totalAmount, string taxAmount, string discountAmount, IEnumerable<ItemInfo> items)
        {
            List<CartLine> cartLines = new List<CartLine>();
            foreach (ItemInfo item in items)
            {
                cartLines.Add(new CartLine()
                {
                    Description = string.IsNullOrEmpty(item.Description) ? item.LineItemId.ToString() : item.Description,
                    LineDiscount = Convert.ToDecimal(item.Discount),
                    Quantity = item.Quantity,
                    Price = Convert.ToDecimal(item.UnitPrice),
                    IsVoided = item.IsVoided,
                    LineNumber = item.LineItemId,
                    ExtendedPrice = Convert.ToDecimal(item.ExtendedPriceWithTax)
                });
            }
            Cart cart = new Cart()
            {
                CartLines = cartLines,
                TaxAmount = Convert.ToDecimal(taxAmount),
                DiscountAmount = Convert.ToDecimal(discountAmount),
                SubtotalAmountWithoutTax = Convert.ToDecimal(subTotalAmount),
                TotalAmount = Convert.ToDecimal(totalAmount)
            };

            string strCart = Newtonsoft.Json.JsonConvert.SerializeObject(cart);

            return Task.FromResult<string>(strCart);
        }

        private string GetOtherConnector(string currentConnector, ExtensionTransaction extensionTransactionProperties)
        {
            string connectorName = currentConnector;
            if (extensionTransactionProperties != null)
            {
                var payProp = extensionTransactionProperties.ExtensionProperties.FirstOrDefault(cp => cp.Key == "PayOtherConnector");
                if (payProp != null)
                {
                    string s_value = payProp.Value.StringValue;
                    if (!string.IsNullOrEmpty(s_value))
                    {
                        connectorName = s_value;
                    }
                }
            }
            return connectorName;
        }

        private TenderInfo GetCustomerAddress(ExtensionTransaction extensionTransactionProperties)
        {
            if (extensionTransactionProperties != null)
            {
                foreach (CommerceProperty property in extensionTransactionProperties.ExtensionProperties)
                {
                    if (property.Key.Equals("Name"))
                    {
                        this.tenderInfo.Name = property.Value.StringValue;
                    }
                    if (property.Key.Equals("StreetAddress"))
                    {
                        this.tenderInfo.Address = property.Value.StringValue;
                    }
                    if (property.Key.Equals("City"))
                    {
                        this.tenderInfo.City = property.Value.StringValue;
                    }
                    if (property.Key.Equals("Country"))
                    {
                        this.tenderInfo.Country = property.Value.StringValue;
                    }
                    if (property.Key.Equals("State"))
                    {
                        this.tenderInfo.StateOrProvince = property.Value.StringValue;
                    }
                    if (property.Key.Equals("ZipCode") || property.Key.Equals("PostalCode"))
                    {
                        this.tenderInfo.Zip = property.Value.StringValue;
                    }
                    //if (property.Key.Equals("Phone"))
                    //{
                    //    this.tenderInfo. = property.Value.StringValue;
                    //}
                }
            }
            return tenderInfo;
        }

        private void SetExtensionTransactionProp(ref ExtensionTransaction extensionTransactionProperties, PaySdk.Response response, bool IsRefToken = false, bool IsIncrementTransactionId = true)
        {
            RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "SetExtensionTransactionProp call started.");
            //string methodName = "SetExtensionTransactionProp";
            string transcounter = string.Empty;
            ExtensionTransaction extensionTransaction = new ExtensionTransaction();
            ICollection<CommerceProperty> extProperties = new System.Collections.ObjectModel.Collection<CommerceProperty>();
            //CommerceProperty comProp;
            List<PaySdk.PaymentError> errors = new List<PaySdk.PaymentError>();

            string connectorName = "";
            string s_value = "";
            string IsFSA = "";
            string HTAMemberNumber = "";
            if (extensionTransactionProperties != null)
            {
                if (extensionTransactionProperties.ExtensionProperties == null)
                {
                    extensionTransactionProperties.ExtensionProperties = new System.Collections.ObjectModel.Collection<CommerceProperty>();
                }
                var payProp = extensionTransactionProperties.ExtensionProperties.FirstOrDefault(cp => cp.Key == "PayOtherConnector");
                if (payProp != null)
                {
                    s_value = payProp.Value.StringValue;
                    if (!string.IsNullOrEmpty(s_value))
                    {
                        connectorName = s_value;
                    }
                }
                
                payProp = extensionTransactionProperties.ExtensionProperties.FirstOrDefault(cp => cp.Key == "HTAMemberNumber");
                if (payProp != null)
                {
                    s_value = payProp.Value.StringValue;
                    if (!string.IsNullOrEmpty(s_value))
                    {
                        HTAMemberNumber = s_value;
                    }
                }
				
                if (string.IsNullOrEmpty(div.Basecounter))
                {
                    SetTerminalPropertiesFromExtensionProperties(extensionTransactionProperties.ExtensionProperties);
                }
            }
            // handle pay other connector
            if (this.tenderInfo.EncryptedPin == "EndDisplayItem" || this.tenderInfo.EncryptedPin == "LineDisplay" || connectorName != div.PaymentConnectorName)
                DeviceStaticVars.PreviousCounter = "";
            
            
                string counter = GetTransactionCounter(response);
                PaymentHelper.AddCommerceProperty(extProperties, "EFTTransactionId", (this.IsIncrementTransactionId(ref extensionTransactionProperties, IsIncrementTransactionId) ? counter == null ? "" : counter : string.Empty));
           

            //comProp = new CommerceProperty();
            //comProp.Key = "IsRefToken";
            //comProp.Value = IsRefToken ? "true" : "false";
            //extProperties.Add(comProp);
            PaymentHelper.AddCommerceProperty(extProperties, "IsRefToken", (IsRefToken ? "true" : "false"));

            //comProp = new CommerceProperty();
            //comProp.Key = "RRTerminalId";
            //comProp.Value = string.IsNullOrEmpty(div.RRTerminalId) ? string.Empty : div.RRTerminalId;
            //extProperties.Add(comProp);
            PaymentHelper.AddCommerceProperty(extProperties, "RRTerminalId", (div.RRTerminalId ?? string.Empty));

            //comProp = new CommerceProperty();
            //comProp.Key = "RRCartId";
            //comProp.Value = string.IsNullOrEmpty(div.RRCartId) ? string.Empty : div.RRCartId;
            //extProperties.Add(comProp);
            PaymentHelper.AddCommerceProperty(extProperties, "RRCartId", (div.RRCartId ?? string.Empty));

            //comProp = new CommerceProperty();
            //comProp.Key = "RRStaffId";
            //comProp.Value = string.IsNullOrEmpty(div.RRStaffId) ? string.Empty : div.RRStaffId;
            //extProperties.Add(comProp);
            PaymentHelper.AddCommerceProperty(extProperties, "RRStaffId", (div.RRStaffId ?? string.Empty));

           
            //comProp = new CommerceProperty();
            //comProp.Key = "IsLogEnabled";
            //comProp.Value = string.IsNullOrEmpty(div.IsLogEnable) ? string.Empty : div.IsLogEnable;
            //extProperties.Add(comProp);
            PaymentHelper.AddCommerceProperty(extProperties, "IsLogEnabled", (div.IsLogEnable ?? string.Empty));

            //comProp = new CommerceProperty();
            //comProp.Key = "EnableLineDisplay";
            //comProp.Value = string.IsNullOrEmpty(div.EnableLineDisplay) ? string.Empty : div.EnableLineDisplay;
            //extProperties.Add(comProp);
            PaymentHelper.AddCommerceProperty(extProperties, "EnableLineDisplay", (div.EnableLineDisplay ?? string.Empty));

            //comProp = new CommerceProperty();
            //comProp.Key = "DeviceSerialNumber";
            //comProp.Value = string.IsNullOrEmpty(div.DeviceSerialNumber) ? string.Empty : div.DeviceSerialNumber;
            //extProperties.Add(comProp);
            PaymentHelper.AddCommerceProperty(extProperties, "DeviceSerialNumber", (div.DeviceSerialNumber ?? string.Empty));

           
            //comProp = new CommerceProperty();
            //comProp.Key = "ByPassPinForDebitTransactions";
            //comProp.Value = string.IsNullOrEmpty(div.ByPassPinForDebitTransactions) ? string.Empty : div.ByPassPinForDebitTransactions;
            //extProperties.Add(comProp);
            PaymentHelper.AddCommerceProperty(extProperties, "ByPassPinForDebitTransactions", (div.ByPassPinForDebitTransactions ?? string.Empty));

            

            //comProp = new CommerceProperty();
            //comProp.Key = "LogAppInsightsKey";
            //comProp.Value = string.IsNullOrEmpty(div.LogAppInsightsKey) ? string.Empty : div.LogAppInsightsKey;
            //extProperties.Add(comProp);
            PaymentHelper.AddCommerceProperty(extProperties, "LogAppInsightsKey", (div.LogAppInsightsKey ?? string.Empty));

           

            //comProp = new CommerceProperty();
            //comProp.Key = "StoreNumber";
            //comProp.Value = string.IsNullOrEmpty(div.StoreNumber) ? string.Empty : div.StoreNumber;
            //extProperties.Add(comProp);
            PaymentHelper.AddCommerceProperty(extProperties, "StoreNumber", (div.StoreNumber ?? string.Empty));

          
          
            
            if (extensionTransactionProperties == null)
            {
                extensionTransaction.ExtensionProperties = extProperties;
                extensionTransactionProperties = extensionTransaction;
            }
            else
            {
                foreach (CommerceProperty comProperty in extProperties)
                {
                    extensionTransactionProperties.ExtensionProperties.Add(comProperty);
                }
            }
            RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "SetExtensionTransactionProp call completed.");
        }

        private void SetFailedTransactionProp(ref PaymentInfo paymentInfo, ref PaySdk.Response response)
        {
            RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "SetFailedTransactionProp call started.");
            EnableErrorsLocalization(ref response); // NOTE :: do not localize failed transaction property
            string[] failTransConnectorArray = new string[] {
                Connectors.NBEConnector
            };
            if (response.Properties != null && failTransConnectorArray.Contains(div.PaymentConnectorName))
            {
                ICollection<CommerceProperty> extProperties = new System.Collections.ObjectModel.Collection<CommerceProperty>();
                string amount = string.Empty;
                foreach (PaySdk.PaymentProperty property in response.Properties)
                {
                    CommerceProperty commerceProperty = null;
                    switch (property.ValueType)
                    {
                        case PaySdk.DataType.String:
                            commerceProperty = new CommerceProperty(property.Name, (object)property.StringValue ?? "");
                            break;
                        case PaySdk.DataType.Decimal:
                            amount = property.DecimalValue.ToString();
                            commerceProperty = new CommerceProperty(property.Name, amount);
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
                                        amount = property.DecimalValue.ToString();
                                        commerceProperty = new CommerceProperty(property.Name, amount);
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
                var isGetTransactionFoundNotFound = false;
                if (response.Errors != null && response.Errors.Length > 0)
                {
                    string errorMessage = string.Empty;
                    for (int i = 0; i < response.Errors.Length; i++)
                    {
                        if (string.IsNullOrEmpty(response.Errors[i].Message) == false)
                            RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "Error in SetFailedTransactionProp method Error: " + response.Errors[i].Message, "ERROR");
                        if (string.IsNullOrEmpty(errorMessage))
                            errorMessage = response.Errors[i].Message;
                        else
                            errorMessage += " " + response.Errors[i].Message;
                        if (response.Errors[i].Code.Equals(PaySdk.ErrorCode.NoTransactionFoundByTransactionReference))
                        {
                            isGetTransactionFoundNotFound = true;
                        }
                    }
                    CommerceProperty commerceProperty = new CommerceProperty("ErrorMessage", errorMessage);
                    extProperties.Add(commerceProperty);
					
					paymentInfo.ExtensionProperties = extProperties;
					List<PaySdk.PaymentError> errorList = new List<PaySdk.PaymentError>();
					errorList.AddRange(response.Errors);
					if (!isGetTransactionFoundNotFound)
						errorList.Add(new PaySdk.PaymentError(PaySdk.ErrorCode.Decline, "<RR>" + Base64Encode(JsonConvert.SerializeObject(extProperties)) + "</RR>"));
					response.Errors = errorList.ToArray();
                }
				else
				{
					RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "SetGCTransactionProp method");
                    CommerceProperty commerceProperty1 = new CommerceProperty("RRGCMessage", "<RR>" + Base64Encode(JsonConvert.SerializeObject(extProperties)) + "</RR>");
                    extProperties.Add(commerceProperty1);
                    paymentInfo.ExtensionProperties = extProperties;
				}
            }
            RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "SetFailedTransactionProp call completed.");
        }

        private static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

       
        private void GetMerchantProp(ExtensionTransaction extensionTransactionProperties, PaySdk.Response response, ref PaymentInfo paymentInfo)
        {
            //string methodName = "GetMerchantProp";
            div.IsGetMerchantProp = false;
            List<PaySdk.PaymentError> errors = new List<PaySdk.PaymentError>();
            try
            {
                string s_value = string.Empty;
                string connectorName = div.PaymentConnectorName;
                PaySdk.Hashtable hashTable = new PaySdk.Hashtable();
                PaySdk.PaymentProperty[] PaymentProperties = null;

                if (extensionTransactionProperties != null)
                {
                    var payProp = extensionTransactionProperties.ExtensionProperties.FirstOrDefault(cp => cp.Key == "PayOtherConnector");
                    if (payProp != null)
                    {
                        s_value = payProp.Value.StringValue;
                        if (!string.IsNullOrEmpty(s_value))
                        {
                            connectorName = s_value;
                        }
                    }
                }

                if (div.PaymentProcessors != null && div.PaymentProcessors.Length > 1)
                {
                    List<PaySdk.PaymentProperty> properties = new List<PaySdk.PaymentProperty>();
                    foreach (PaySdk.PaymentProperty pp in div.AllMerchantProperties)
                    {
                        if (pp.Name.Equals(Constants.ConnectorProperties.Properties))
                        {
                            hashTable = PaySdk.PaymentProperty.ConvertToHashtable(pp.PropertyList);
                            PaySdk.PaymentProperty.GetPropertyValue(hashTable, Constants.GenericNamespace.Connector, Constants.ConnectorProperties.ConnectorName, out s_value);
                            if (connectorName == s_value)
                                properties.AddRange(pp.PropertyList);
                        }
                        else
                        {
                            properties.Add(pp);
                        }
                    }
                    PaymentProperties = properties.ToArray();
                }
                else
                {
                    PaymentProperties = this.merchantProperties;
                }

                if (PaymentProperties.Length > 0 && extensionTransactionProperties != null && extensionTransactionProperties.ExtensionProperties.FirstOrDefault(cp => cp.Key == "GetMerchantProp") != null)
                {
                    div.IsGetMerchantProp = true;

                    s_value = extensionTransactionProperties.ExtensionProperties.FirstOrDefault(cp => cp.Key == "GetMerchantProp").Value.StringValue;
                    if (s_value == "1")
                    {
                        hashTable = PaySdk.PaymentProperty.ConvertToHashtable(PaymentProperties);
                        PaySdk.PaymentProperty.GetPropertyValue(hashTable, Constants.GenericNamespace.MerchantAccount, "TokenForSale", out string TokenForSale);
                        PaySdk.PaymentProperty.GetPropertyValue(hashTable, Constants.GenericNamespace.MerchantAccount, "CustomerNotPresent", out string CustomerNotPresent);
                        PaySdk.PaymentProperty.GetPropertyValue(hashTable, Constants.GenericNamespace.MerchantAccount, "UseCameraForScan", out string UseCameraforScan);
                        PaySdk.PaymentProperty.GetPropertyValue(hashTable, Constants.GenericNamespace.MerchantAccount, "DeviceType", out string DeviceType);
                        PaySdk.PaymentProperty.GetPropertyValue(hashTable, GenericNamespace.MerchantAccount, "EnablePinNumberValidation", out string EnablePinNumberValidation); //Factor4
                        PaySdk.PaymentProperty.GetPropertyValue(hashTable, Constants.GenericNamespace.MerchantAccount, "DeviceName", out string strDeviceName);
                        ExtensionTransaction extensionTransaction = new ExtensionTransaction();
                        ICollection<CommerceProperty> extProperties = new System.Collections.ObjectModel.Collection<CommerceProperty>();
                        CommerceProperty comProp;
                        comProp = new CommerceProperty();
                        comProp.Key = "TokenForSale";
                        comProp.Value = TokenForSale;

                        extProperties.Add(comProp);

                        comProp = new CommerceProperty();
                        comProp.Key = "CustomerNotPresent";
                        comProp.Value = CustomerNotPresent;

                        extProperties.Add(comProp);

                        comProp = new CommerceProperty();
                        comProp.Key = "UseCameraForScan";
                        comProp.Value = UseCameraforScan;

                        extProperties.Add(comProp);

                        comProp = new CommerceProperty();
                        comProp.Key = "DeviceType";
                        comProp.Value = DeviceType;

                        extProperties.Add(comProp);

                        comProp = new CommerceProperty();
                        comProp.Key = "DeviceName";
                        comProp.Value = strDeviceName;

                        extProperties.Add(comProp);

                        comProp = new CommerceProperty();
                        comProp.Key = "EnablePinNumberValidation";
                        comProp.Value = EnablePinNumberValidation;

                        extProperties.Add(comProp);

                        extensionTransaction.ExtensionProperties = extProperties;
                        paymentInfo.ExtensionProperties = extensionTransaction.ExtensionProperties;
                    }
                }
            }
            catch (Exception ex)
            {
                //RetailLogger.Log.ExtendedErrorEvent(ex.Message, this.GetType().Name, methodName);
                RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "Error in GetMerchantProp method Exception: " + ex.Message, "ERROR");
                errors.Add(new PaySdk.PaymentError(PaySdk.ErrorCode.Decline, ex.Message, true));
                response.Errors = errors.ToArray();
                return;
            }
        }

        private bool IsIncrementTransactionId(ref ExtensionTransaction extensionTransactionProperties, bool isIncrementTransactionId)
        {
            if (isIncrementTransactionId == false)
                return false;
           
            else
            {
                return true;
            }
        }

        private void EnableErrorsLocalization(ref PaySdk.Response response)
        {
            if (response.Errors != null)
            {
                PaySdk.PaymentError[] errorList = new PaySdk.PaymentError[] { };
                List<string> uniqueErrors = new List<string>();
                if (!div.PaymentConnectorName.Equals(Connectors.Shift4Connector) &&
                    !div.PaymentConnectorName.Equals(Connectors.MonerisConnector))
                {
                    PaySdk.ErrorCode errorCode = PaySdk.ErrorCode.Decline;
                    // get all errors from connector and merge them in one error message
                    foreach (PaySdk.PaymentError error in response.Errors)
                    {
                        // adding error message to list of errors and skip duplicate.
                        if (!uniqueErrors.Contains(error.Message))
                            uniqueErrors.Add(error.Message);
                        if (error.Code == PaySdk.ErrorCode.NoTransactionFoundByTransactionReference)
                        {
                            errorCode = PaySdk.ErrorCode.NoTransactionFoundByTransactionReference;
                        }
                    }
                    errorList = errorList.Concat(new PaySdk.PaymentError[]
                    { new PaySdk.PaymentError(errorCode, string.Join(" ", uniqueErrors), true) }).ToArray();
                }
                else
                {
                    errorList = response.Errors;
                }
                response.Errors = errorList.ToArray();
            }
        }


        // Added for AssemblyPayment Pairing device Process. 
        private void PairAssemblyPaymentDevice(string operation, ExtensionTransaction extensionTransactionProperties, PaySdk.PaymentProperty[] merchantProperties)
        {
            if (!string.IsNullOrEmpty(operation))
            {
                if (operation == "CheckStatusOperation")
                {
                    this.tenderInfo.EncryptedPin = "CheckPairStatus";

                    AppStorage storage = new AppStorage(div.PaymentConnectorName, div.Basecounter);
                    var connectorProp = storage.GetTerminalProperties();
                    if (connectorProp != null)
                    {
                        if (!string.IsNullOrEmpty(connectorProp[0].StringValue) && !string.IsNullOrEmpty(connectorProp[1].StringValue))
                        {
                            div.AssemblyPosId = connectorProp[0].StringValue + "#" + connectorProp[1].StringValue;

                        }
                        else
                        {
                            string posId = string.Empty;
                            string eftPoasaddress = string.Empty;
                            PaySdk.Hashtable hashTable = PaySdk.PaymentProperty.ConvertToHashtable(this.merchantProperties);
                            PaySdk.PaymentProperty.GetPropertyValue(hashTable, Constants.GenericNamespace.MerchantAccount, "PosID", out posId);
                            PaySdk.PaymentProperty.GetPropertyValue(hashTable, Constants.GenericNamespace.MerchantAccount, "EftPosAddress", out eftPoasaddress);

                            div.AssemblyPosId = posId + "#" + eftPoasaddress;
                        }
                    }
                    else
                    {
                        string posId = string.Empty;
                        string eftPoasaddress = string.Empty;
                        PaySdk.Hashtable hashTable = PaySdk.PaymentProperty.ConvertToHashtable(this.merchantProperties);
                        PaySdk.PaymentProperty.GetPropertyValue(hashTable, Constants.GenericNamespace.MerchantAccount, "PosID", out posId);
                        PaySdk.PaymentProperty.GetPropertyValue(hashTable, Constants.GenericNamespace.MerchantAccount, "EftPosAddress", out eftPoasaddress);

                        div.AssemblyPosId = posId + "#" + eftPoasaddress;
                    }

                }
                else if (operation == "PairOperation" || operation == "SaveOperation" ||
                    operation == "UnPairOperation" || operation == "CodeOperation" ||
                    operation == "CancleCodeOperation" || operation == "EftConfirmCodeOperation" ||
                    operation == "RefreshOperation")
                {
                    if (operation == "PairOperation")
                    {
                        this.tenderInfo.EncryptedPin = "PairDevice";
                    }
                    else if (operation == "SaveOperation")
                    {
                        this.tenderInfo.EncryptedPin = "SaveProperties";
                    }
                    else if (operation == "CodeOperation")
                    {
                        this.tenderInfo.EncryptedPin = "CodeMatch";
                    }
                    else if (operation == "CancleCodeOperation")
                    {
                        this.tenderInfo.EncryptedPin = "CodeDoesNotMatch";
                    }
                    else if (operation == "EftConfirmCodeOperation")
                    {
                        this.tenderInfo.EncryptedPin = "EftConfirmCodeProperties";
                    }
                    else if (operation == "RefreshOperation")
                    {
                        this.tenderInfo.EncryptedPin = "RefreshStatus";
                    }
                    else
                    {
                        this.tenderInfo.EncryptedPin = "UnPairDevice";
                    }

                    foreach (CommerceProperty property in extensionTransactionProperties.ExtensionProperties)
                    {
                        if (property.Key == "PosId")
                        {
                            this.tenderInfo.AdditionalSecurityData = property.Value.StringValue;
                        }
                        if (property.Key == "EftPosAddress")
                        {
                            this.tenderInfo.Address = property.Value.StringValue;
                        }
                    }
                    div.AssemblyPosId = this.tenderInfo.AdditionalSecurityData + "#" + this.tenderInfo.Address;
                }
                else if (operation == "SettelmentOperation")
                {
                    this.tenderInfo.EncryptedPin = "SettelmentReport";
                }
                else if (operation == "AssemblySettelmentOperation")
                {
                    this.tenderInfo.EncryptedPin = "AssemblySettelmentReport";
                }
                if (operation == "SaveOperation")
                {

                    bool iscontainsLetter = Regex.IsMatch(this.tenderInfo.Address, @"[~`!@#$%^&*()-+=|\{}':;,<>/?a - zA - Z  ]");
                    bool iscontainSpace = Regex.IsMatch(this.tenderInfo.AdditionalSecurityData, @"[~`!@#$%^&*()-+=|\{}':;,<>/?   ]");
                    AppStorage storage = new AppStorage(div.PaymentConnectorName, div.Basecounter);

                    if (!string.IsNullOrEmpty(this.tenderInfo.AdditionalSecurityData.Trim()) && !string.IsNullOrEmpty(this.tenderInfo.Address.Trim()))
                    {
                        if (iscontainsLetter || iscontainSpace)
                        {
                            div.AssemblyPosId = "Settings cannot be saved. Please enter valid Address";
                        }
                        else
                        {
                            div.AssemblyPosId = this.tenderInfo.AdditionalSecurityData + "#" + this.tenderInfo.Address;
                            storage.SaveProperies(this.tenderInfo.AdditionalSecurityData, this.tenderInfo.Address);
                        }

                    }
                    else if (string.IsNullOrEmpty(this.tenderInfo.AdditionalSecurityData.Trim()) || string.IsNullOrEmpty(this.tenderInfo.Address.Trim()))
                    {
                        div.AssemblyPosId = "Settings cannot be saved";
                    }

                }
            }
        }

        // Pos Extenstion Request for just get value, Block Request Going To Devide 
        private void CheckPayment(ref bool returnResponse, ref PaymentInfo paymentInfo, ExtensionTransaction extensionTransactionProperties, PaySdk.PaymentProperty[] paymentProperties, string otherConnectorName)
        {
            //string methodName = "CheckRefundPayment";
            string cvalue = string.Empty;

            IList<PaySdk.PaymentProperty> properties = new List<PaySdk.PaymentProperty>();
            PaySdk.PaymentProperty property = new PaySdk.PaymentProperty();
            List<PaySdk.PaymentError> errors = new List<PaySdk.PaymentError>();
            List<CommerceProperty> comPropList = new List<CommerceProperty>();
            CommerceProperty comProp;
            try
            {
                PaymentHelper.AddExtensionTransactionProperties(properties, extensionTransactionProperties);
                if (properties != null && properties.Count > 0)
                {
                    property = properties.FirstOrDefault(prop => prop.Name == "CheckIsReturnUsingCardToken");
                    if ((property != null && property.StringValue == "1"))
                    {
                        if (otherConnectorName != "SynchronyConnector")
                        {
                            AppStorage appStorage = new AppStorage(div.PaymentConnectorName, div.Basecounter);
                            cvalue = appStorage.GetSingleNodeValue("IsReturnUsingCardToken");
                        }
                        else
                        {
                            cvalue = "1";
                        }
                        if (string.IsNullOrEmpty(cvalue))
                        {
                            property = paymentProperties.FirstOrDefault(prop => prop.Name == "IsReturnUsingCardToken");
                            if (property != null)
                            {
                                cvalue = property.StringValue;
                            }
                        }
                        if (cvalue == "1")
                        {
                            comProp = new CommerceProperty();
                            comProp.Key = "IsReturnUsingCardToken";
                            comProp.Value = cvalue;
                            comPropList.Add(comProp);
                        }
                        cvalue = string.Empty;
                        //adding ReturnUsingTokenOnly for shift4
                        property = paymentProperties.FirstOrDefault(prop => prop.Name == "PayOnlyReturnToken" || prop.Name == "ReturnUsingTokenOnly");
                        if (property != null)
                        {
                            cvalue = property.StringValue;
                        }
                        if (cvalue == "1")
                        {
                            comProp = new CommerceProperty();
                            comProp.Key = "ReturnUsingTokenOnly";
                            comProp.Value = cvalue;
                            comPropList.Add(comProp);
                        }
                        paymentInfo.IsApproved = true;
                        paymentInfo.ExtensionProperties = comPropList.Count > 0 ? comPropList : null;
                        returnResponse = true;
                    }
                }
            }
            catch (Exception ex)
            {
                //RetailLogger.Log.ExtendedErrorEvent(ex.Message, this.GetType().Name, methodName);
                RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "Error in CheckPayment method Exception: " + ex.Message, "ERROR");
                errors.Add(new PaySdk.PaymentError(PaySdk.ErrorCode.Decline, ex.Message, true));
                paymentInfo.Errors = errors.ToArray();
            }
        }

        private bool CkeckCustomerAccountSignatureEnable(ExtensionTransaction extensionTransactionProperties)
        {
            //string methodName = "CkeckCustomerAccountSignatureEnable";
            //RetailLogger.Log.ExtendedInformationalEvent("CkeckCustomerAccountSignatureEnable call is started.", this.GetType().Name, methodName);
            string cvalue = string.Empty;
            bool result = false;
            PaySdk.PaymentProperty property = new PaySdk.PaymentProperty();
            AppStorage appStorage = new AppStorage(div.PaymentConnectorName, div.Basecounter);
            cvalue = appStorage.GetSingleNodeValue("EnableCustomerAccountSignature");
            if (string.IsNullOrEmpty(cvalue))
            {
                if (div.PaymentConnectorName == Connectors.CardConnectConnector || div.PaymentConnectorName == Connectors.CayanConnector || div.PaymentConnectorName.Equals(Connectors.HeartlandConnector) || div.PaymentConnectorName.Equals(Connectors.PaymentLogisticsConnector))
                {
                    CommerceProperty extprop = extensionTransactionProperties.ExtensionProperties.FirstOrDefault(a => a.Key == "CardConnectEnableOnAccuntSignMPOS");
                    if (extprop != null)
                        cvalue = extprop.Value.DecimalValue.ToString();
                    if (string.IsNullOrEmpty(cvalue) || cvalue == "2")
                    {
                        property = this.merchantProperties.FirstOrDefault(prop => prop.Name == "EnableCustomerAccountSignature");
                        if (property != null)
                        {
                            cvalue = property.StringValue;
                        }
                    }
                }
                else
                {
                    property = this.merchantProperties.FirstOrDefault(prop => prop.Name == "EnableCustomerAccountSignature");
                    if (property != null)
                    {
                        cvalue = property.StringValue;
                    }
                }
            }
            result = cvalue == "1" ? true : false;
            return result;
        }

        private bool CheckIsPinPadSupportSignatureEnable()
        {
            //string methodName = "CheckIsPinPadSupportSignatureEnable";
            //RetailLogger.Log.ExtendedInformationalEvent("CheckIsPinPadSupportSignatureEnable call is started.", this.GetType().Name, methodName);
            string cvalue = string.Empty;
            bool result = false;
            PaySdk.PaymentProperty property = new PaySdk.PaymentProperty();
            AppStorage appStorage = new AppStorage(div.PaymentConnectorName, div.Basecounter);
            cvalue = appStorage.GetSingleNodeValue("IsPinPadSupportSignature");
            if (string.IsNullOrEmpty(cvalue))
            {
                if (!string.IsNullOrEmpty(div.IsPinPadSupportSign) && (div.IsPinPadSupportSign == "0" || div.IsPinPadSupportSign == "1") && div.PaymentConnectorName == Connectors.CardConnectConnector)
                {
                    cvalue = div.IsPinPadSupportSign;
                }
                else
                {
                    property = this.merchantProperties.FirstOrDefault(prop => prop.Name == "IsPinPadSupportSignature");
                    if (property != null)
                    {
                        cvalue = property.StringValue;
                    }
                }
            }
            result = cvalue == "1" ? true : false;
            return result;
        }

        private static string GetAssemblyVersion()
        {
            var assembly = typeof(DeviceHandler).Assembly;
            return assembly.GetName().Version.ToString();
        }

        private string ReplaceBetweenString(string value, string a, string b, string replaceValue)
        {
            string updatedString = string.Empty;
            int posA = value.IndexOf(a);
            int posB = value.LastIndexOf(b);
            if (posA == -1)
            {
                return "";
            }
            if (posB == -1)
            {
                return "";
            }
            int adjustedPosA = posA + a.Length;
            if (adjustedPosA >= posB)
            {
                return "";
            }
            string betweenString = value.Substring(adjustedPosA, posB - adjustedPosA);
            updatedString = value.Replace(betweenString, replaceValue);
            return updatedString;
        }

        private string GetCounterFromDate(string terminalId)
        {
            return terminalId + DateTime.Now.ToString("yyyyMMddhhmmssmm");
        }

        //private string CalculateMD5Hash(string input)
        //{
        //    // Use input string to calculate MD5 hash
        //    using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
        //    {
        //        byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
        //        byte[] hashBytes = md5.ComputeHash(inputBytes);

        //        // Convert the byte array to hexadecimal string
        //        StringBuilder sb = new StringBuilder();
        //        for (int i = 0; i < hashBytes.Length; i++)
        //        {
        //            sb.Append(hashBytes[i].ToString("x2"));
        //        }
        //        return sb.ToString();
        //    }

        //}

    }
}
