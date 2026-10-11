//namespace System
//{
//    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Enum | AttributeTargets.Interface)]
//    public class SerializableAttribute : Attribute
//    {
//        //this does nothing
//    }
//}
namespace RR.Dynamics.Retail.Payment.NBE.Portable
{
    using System;
    using System.Collections.Generic;
    using System.Composition;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Dynamics.Retail.Diagnostics;
    using Microsoft.Dynamics.Retail.PaymentSDK.Portable;
    using Microsoft.Dynamics.Retail.PaymentSDK.Portable.Constants;
    using Newtonsoft.Json;
    using RR.Utility;
    using Contract;
    using System.Reflection;
    //using Utility;
    //using Microsoft.Dynamics.Commerce.HardwareStation.CardPayment;

    /// <summary>
    /// SampleConnector class.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1724:TypeNamesShouldNotMatchNamespaces", Justification = "SAMPLE CODE ONLY")]
    [Export(typeof(IPaymentProcessor))]
    //[Serializable]
    public class NBEConnector : NBETenderProcessor, IPaymentProcessor, IPaymentProcessorExtension
    {
        private const string Platform = "Portable";
        private const string OperationStarting = "starting";
        private const char PaddingCharacter = '*';
        public string FileV { get; set; }
        /// <summary>
        /// Initializes a new instance of the <see cref="NBEEurope"/> class.
        /// </summary>
        public NBEConnector()
        {
            FileV = Utility.AssemblyDetailsInfo.GetAssemblFileVersion(this.GetType());
            RetailLogger.Log.ExtendedInformationalEvent("Call to NBEConnector. File Version: " + Utility.AssemblyDetailsInfo.GetAssemblFileVersion(this.GetType()));
        }

        #region IPaymentProcessor methods

        /// <summary>
        /// Authorize the request with the payment provider.
        /// </summary>
        /// <param name="request">Request object needed to process the authorize transaction.</param>
        /// <param name="requiredInteractionProperties">Properties required by authorization process.</param>
        /// <returns>
        /// Response object.
        /// </returns>

 
        IEftProcessor eftProcessor = null;
        public Response Authorize(Request request, PaymentProperty[] requiredInteractionProperties)
        {
            string methodName = "Authorize";
            RetailLogger.Log.PaymentConnectorLogOperation(methodName, OperationStarting, this.Name, Platform);
            IDictionary<string, string> methodProp = new Dictionary<string, string>();
            methodProp.Add("Name", this.Name);
            methodProp.Add("MethodName", methodName);
            methodProp.Add("Platform", Platform);
            methodProp.Add("File Version", FileV);

            bool EnableLog = false;
            Response response = new Response();
            // Convert request
            AuthorizeRequest authorizeRequest = null;
            IList<PaymentProperty> properties = new List<PaymentProperty>();
            string LogPrefix = "";
            try
            {
                authorizeRequest = AuthorizeRequest.ConvertFrom(request);
                LogPrefix = authorizeRequest.IndustryType == "Ecommerce" ? "ECOM :: Authorize :: " : "Authorize :: ";
            }
            catch (SampleException ex)
            {
                TransactionProp transactionProp = new TransactionProp();
                transactionProp.ProcessorType = ((int)EftProcessorType.NBE).ToString();
                transactionProp.OperationTypeId = ((int)EftOperationType.Sale).ToString();
                transactionProp.ResponseData = ex.Message;
                SetFailedTransactionProp(request, transactionProp, ref properties);
                RetailLogger.Log.ExtendedErrorEvent(LogPrefix + ex.Message, Name, methodName, Platform);
                RRLogger.Error(LogPrefix + ex.Message, methodProp, authorizeRequest.LogAppInsightsKey);
                return PaymentUtilities.CreateAndLogResponseForReturn(methodName, this.Name, Platform, locale: request == null ? null : request.Locale, properties: null, errors: ex.Errors);
            }

            // Validate merchant account
            List<PaymentError> errors = new List<PaymentError>();
            RRLogger.Information(LogPrefix + "ValidateLicense call is started.", methodProp, authorizeRequest.LogAppInsightsKey);
            // License >>
            DateTime? licenseExpiryDate = null;
            licenseExpiryDate = LicenseValidation.ValidateLicense(authorizeRequest.AccountId, authorizeRequest.ChannelId, authorizeRequest.SerialNumber, authorizeRequest.OldSerialNumber, errors, this.Name);
            // License <<
            RRLogger.Information(LogPrefix + "ValidateLicense call is Completed.", methodProp, authorizeRequest.LogAppInsightsKey);
            if (errors.Count > 0)
            {
                TransactionProp transactionProp = new TransactionProp();
                transactionProp.ProcessorType = ((int)EftProcessorType.NBE).ToString();
                transactionProp.OperationTypeId = ((int)EftOperationType.Sale).ToString();
                transactionProp.Amount = authorizeRequest.Amount.Value.ToString();
                foreach (PaymentError perror in errors)
                {
                    transactionProp.ResponseData += perror.Message;
                    RetailLogger.Log.ExtendedErrorEvent(perror.Message, Name, methodName, Platform);
                    RRLogger.Error(LogPrefix + perror.Message, methodProp, authorizeRequest.LogAppInsightsKey);
                }
                SetFailedTransactionProp(request, transactionProp, ref properties);
                return PaymentUtilities.CreateAndLogResponseForReturn(methodName, this.Name, Platform, authorizeRequest.Locale, errors);
            }

            RRLogger.Information(LogPrefix + "CreateEftProcessor call is started.", methodProp, authorizeRequest.LogAppInsightsKey);
            eftProcessor = CreateEftProcessor(authorizeRequest, errors);
            if (errors.Count > 0)
            {
                TransactionProp transactionProp = new TransactionProp();
                transactionProp.ProcessorType = ((int)EftProcessorType.NBE).ToString();
                transactionProp.OperationTypeId = ((int)EftOperationType.Sale).ToString();
                foreach (PaymentError perror in errors)
                {
                    transactionProp.ResponseData += perror.Message;
                    RetailLogger.Log.ExtendedErrorEvent(perror.Message, Name, methodName, Platform);
                    RRLogger.Error(LogPrefix + perror.Message, methodProp, authorizeRequest.LogAppInsightsKey);
                }
                return PaymentUtilities.CreateAndLogResponseForReturn(methodName, this.Name, Platform, authorizeRequest.Locale, properties, errors);
            }
            EftRequest eftRequest = new EftRequest();
            EftResponse eftResponse = null;
            bool isApproved = false;
            var authorizeResponse = new AuthorizeResponse(authorizeRequest.Locale, authorizeRequest.ServiceAccountId, this.Name);
  
            RRLogger.Information(LogPrefix + "CreateEftProcessor call is Completed.", methodProp, authorizeRequest.LogAppInsightsKey);
            LogHelper.Log(LogTarget.File, Name, FileV, authorizeRequest.AssemblyName, this.GetType().Name, methodName, "CreateEftProcessor call is Completed.", EnableLog);
            eftRequest = new EftRequest();
            eftRequest.Amount = authorizeRequest.Amount.Value;
            eftRequest.Operation = EftOperationType.Sale;
            eftRequest.TransactionId = authorizeRequest.TransactionId;
            RRLogger.Information(LogPrefix + "Transaction Id: " + authorizeRequest.TransactionId, methodProp, authorizeRequest.LogAppInsightsKey);
            eftRequest.Source = EftRequestSource.Eft;            
            eftRequest.StaffId = authorizeRequest.StaffId;
            eftRequest.CardType = EftCardType.CreditCard;
            eftResponse = new EftResponse();
            RRLogger.Information(LogPrefix + "Eft Process call is started.", methodProp, authorizeRequest.LogAppInsightsKey);
            eftResponse = eftProcessor.Process(eftRequest);
            RRLogger.Information(LogPrefix + "Eft Process call is Completed.", methodProp, authorizeRequest.LogAppInsightsKey);
            LogHelper.Log(LogTarget.File, Name, FileV, authorizeRequest.AssemblyName, this.GetType().Name, methodName, "Eft Process call is Completed.", EnableLog);
            isApproved = eftResponse.Status == EftOperationStatus.Approved;

            LogHelper.Log(LogTarget.File, Name, FileV, authorizeRequest.AssemblyName, this.GetType().Name, methodName, "Transaction Status: " + eftResponse.Status, EnableLog);
            LogHelper.Log(LogTarget.File, Name, FileV, authorizeRequest.AssemblyName, this.GetType().Name, methodName, string.Format("ApprovalCode: {0} ApprovedAmount: {1} CardType: {2}", eftResponse.ProviderTransactionId, eftResponse.ApprovedAmount, eftResponse.CardType),EnableLog);
            LogHelper.Log(LogTarget.File, Name, FileV, authorizeRequest.AssemblyName, this.GetType().Name, methodName, "RequestData: " + eftResponse.RequestData, EnableLog);
            LogHelper.Log(LogTarget.File, Name, FileV, authorizeRequest.AssemblyName, this.GetType().Name, methodName, "ResponseData: " + eftResponse.ResponseData, EnableLog);


            RRLogger.Information(LogPrefix + "Transaction Status: " + eftResponse.Status, methodProp, authorizeRequest.LogAppInsightsKey);
            RRLogger.Information(LogPrefix + string.Format("ApprovalCode: {0} ApprovedAmount: {1} CardType: {2}", eftResponse.ProviderTransactionId, eftResponse.ApprovedAmount, eftResponse.CardType), methodProp, authorizeRequest.LogAppInsightsKey);

            if (isApproved)
            {
                authorizeResponse.ApprovedAmount = eftResponse.ApprovedAmount;
                authorizeResponse.AuthorizationResult = AuthorizationResult.Success.ToString();
                LogHelper.Log(LogTarget.File, Name, FileV, authorizeRequest.AssemblyName, this.GetType().Name, methodName, "RequestData: " + eftResponse.RequestData, EnableLog);
                LogHelper.Log(LogTarget.File, Name, FileV, authorizeRequest.AssemblyName, this.GetType().Name, methodName, "ResponseData: " + eftResponse.ResponseData, EnableLog);

                methodProp.Add("RequestData", eftResponse.RequestData);
                methodProp.Add("ResponseData", eftResponse.ResponseData);
                RRLogger.Information(LogPrefix + "Payment Response", methodProp, authorizeRequest.LogAppInsightsKey);
               
            }
            else
            {
                authorizeResponse.ApprovedAmount = 0M;
                authorizeResponse.AuthorizationResult = AuthorizationResult.Failure.ToString();
                string eftErrorMessage = String.Empty;
                EftError eftError = eftResponse.Errors.Last(false);
                foreach (EftError err in eftResponse.Errors.Select())
                {
                    eftErrorMessage = err == null ? "Declined" : err.ErrorMessage;
                    errors.Add(new PaymentError(ErrorCode.Decline, eftErrorMessage));
                    LogHelper.Log(LogTarget.File, Name, FileV, authorizeRequest.AssemblyName, this.GetType().Name, methodName, "ErrorData: " + eftErrorMessage, EnableLog);                  
                }
                LogHelper.Log(LogTarget.File, Name, FileV, authorizeRequest.AssemblyName, this.GetType().Name, methodName, "RequestData: " + eftResponse.RequestData, EnableLog);
                LogHelper.Log(LogTarget.File, Name, FileV, authorizeRequest.AssemblyName, this.GetType().Name, methodName, "ResponseData: " + eftResponse.ResponseData, EnableLog);

                methodProp.Add("RequestData", eftResponse.RequestData);
                methodProp.Add("ResponseData", eftResponse.ResponseData);
                RRLogger.Error(LogPrefix + "Payment Response", methodProp, authorizeRequest.LogAppInsightsKey);
            }
            LogHelper.Log(LogTarget.File, Name, FileV, authorizeRequest.AssemblyName, this.GetType().Name, methodName, "Response Completed", EnableLog);
            authorizeResponse.IsSwipe = authorizeRequest.IsSwipe;
            authorizeResponse.UniqueCardId = authorizeRequest.UniqueCardId;
            authorizeResponse.VoiceAuthorizationCode = authorizeRequest.VoiceAuthorizationCode;
            authorizeResponse.AccountType = authorizeRequest.AccountType;
            authorizeResponse.CurrencyCode = authorizeRequest.CurrencyCode;
            authorizeResponse.TransactionType = TransactionType.Authorize.ToString();
            authorizeResponse.TransactionDateTime = DateTime.UtcNow;
            if (authorizeRequest.CardType != null)
            {
                eftResponse.CardType = CardMap(authorizeRequest.PaymentCardTypes,eftResponse.CardType, methodName);
            }
            authorizeResponse.CardType = eftResponse.CardType;
            authorizeResponse.AvailableBalance = eftResponse.CardBalance;
            
            authorizeResponse.Last4Digit = eftResponse.CardReference;
            // License >>
            ApplyLicense(licenseExpiryDate, authorizeResponse);
            // License <<
            if (authorizeRequest.AllowPrintingProcessorReceipt)
                authorizeResponse.ExternalReceipt = eftResponse.ReceiptTextXml;

            authorizeResponse.ResponseCode = eftResponse.ProviderTransactionId;
            authorizeResponse.ProviderMessage = eftResponse.Status.ToString();
            authorizeResponse.ProviderTransactionId = eftResponse.ProviderTransactionId;
            authorizeResponse.ApprovalCode = eftResponse.ProviderTransactionId;
            authorizeResponse.CardToken = eftResponse.AuthCode;

            response = AuthorizeResponse.ConvertTo(authorizeResponse);
            // PSAUTHTRANSACTION & Receipt
            string OperationTypeId = Convert.ToInt32(EftOperationType.Sale).ToString();
            string ProcessorType = Convert.ToInt32(EftProcessorType.NBE).ToString();
            properties = response.Properties.ToList();
            int Environment = authorizeRequest.TransactionMode == "1" ? 1 : 0;
            properties.Add(new PaymentProperty(GenericNamespace.TransactionData, "ProcessorType", ProcessorType));
            properties.Add(new PaymentProperty(GenericNamespace.TransactionData, "OperationTypeId", OperationTypeId));
            properties.Add(new PaymentProperty(GenericNamespace.TransactionData, "OperationType", eftRequest.Operation.ToString()));
            properties.Add(new PaymentProperty(GenericNamespace.TransactionData, "ResponseAuthTransId", eftResponse.ProviderTransactionId));
            properties.Add(new PaymentProperty(GenericNamespace.TransactionData, "ProviderTransactionId", eftResponse.ProviderTransactionId));
            properties.Add(new PaymentProperty(GenericNamespace.TransactionData, "ReceiptText", eftResponse.ReceiptText));
            properties.Add(new PaymentProperty(GenericNamespace.TransactionData, "RequestData", eftResponse.RequestData));
            properties.Add(new PaymentProperty(GenericNamespace.TransactionData, "ResponseData", eftResponse.ResponseData));
            properties.Add(new PaymentProperty(GenericNamespace.TransactionData, "OurRef", eftResponse.TransactionId));
            properties.Add(new PaymentProperty(GenericNamespace.TransactionData, "Environment", Environment.ToString()));
            properties.Add(new PaymentProperty(GenericNamespace.TransactionData, "DeviceTerminalId", eftResponse.TerminalId));

            properties.Add(new PaymentProperty(GenericNamespace.TransactionData, "Status", ((int)eftResponse.Status).ToString()));
            properties.Add(new PaymentProperty(GenericNamespace.TransactionData, "StatusId", ((int)eftResponse.Status).ToString()));
            properties.Add(new PaymentProperty(GenericNamespace.TransactionData, "RequestAmount", authorizeRequest.Amount.GetValueOrDefault()));
           


            response.Properties = properties.ToArray();
            if (errors.Count > 0)
            {
                foreach (PaymentError perror in errors)
                {
                    RRLogger.Error(LogPrefix + perror.Message, methodProp, authorizeRequest.LogAppInsightsKey);
                }
                response.Errors = errors.ToArray();
            }

            PaymentUtilities.LogOperationResult(methodName, this.Name, Platform, request, response);
            PaymentUtilities.LogResponseBeforeReturn(methodName, this.Name, Platform, response);
            return response;
        }
       

        /// <summary>
        /// Capture the request with the payment provider.
        /// </summary>
        /// <param name="request">Request object needed to process the Capture transaction.</param>
        /// <returns>
        /// Response object.
        /// </returns>
        public Response Capture(Request request)
        {
            string methodName = "Capture";
            RetailLogger.Log.PaymentConnectorLogOperation(methodName, OperationStarting, this.Name, Platform);

            // Convert request
            CaptureRequest captureRequest = null;
            try
            {
                captureRequest = CaptureRequest.ConvertFrom(request);
            }
            catch (SampleException ex)
            {
                return PaymentUtilities.CreateAndLogResponseForReturn(methodName, this.Name, Platform, locale: request == null ? null : request.Locale, properties: null, errors: ex.Errors);
            }

            // Validate merchant account
            List<PaymentError> errors = new List<PaymentError>();
            if (errors.Count > 0)
            {
                return PaymentUtilities.CreateAndLogResponseForReturn(methodName, this.Name, Platform, captureRequest.Locale, errors);
            }

            // Capture and create response
            var captureResponse = new CaptureResponse(captureRequest.Locale, captureRequest.ServiceAccountId, this.Name);
            captureResponse.CardType = captureRequest.CardType;
            captureResponse.CardToken = captureRequest.CardToken;
            captureResponse.Last4Digit = captureRequest.Last4Digit;
            captureResponse.UniqueCardId = captureRequest.UniqueCardId;
            captureResponse.CurrencyCode = captureRequest.CurrencyCode;
            captureResponse.TransactionType = TransactionType.Capture.ToString();
            captureResponse.TransactionDateTime = DateTime.UtcNow;
            captureResponse.ProviderTransactionId = captureRequest.AuthorizationProviderTransactionId;
            captureResponse.CaptureResult = CaptureResult.Success.ToString();
            captureResponse.ProviderMessage = "Approved";

            // Convert response and return
            Response response = CaptureResponse.ConvertTo(captureResponse);
            if (errors.Count > 0)
            {
                response.Errors = errors.ToArray();
            }

            PaymentUtilities.LogOperationResult(methodName, this.Name, Platform, request, response);
            PaymentUtilities.LogResponseBeforeReturn(methodName, this.Name, Platform, response);
            return response;
        }

        /// <summary>
        /// ImmediateCapture the request with the payment provider.
        /// </summary>
        /// <param name="request">Request object needed to process the ImmediateCapture transaction.</param>
        /// <param name="requiredInteractionProperties">Properties required by ImmediateCapture process.</param>
        /// <returns>
        /// Response object.
        /// </returns>
        public Response ImmediateCapture(Request request, PaymentProperty[] requiredInteractionProperties)
        {
            var ex = new NotImplementedException("ImmediateCapture Not Implemented");
            RetailLogger.Log.PaymentConnectorLogException("ImmediateCapture", this.Name, Platform, ex);
            throw ex;
        }

        /// <summary>
        /// Void the request with the payment provider.
        /// </summary>
        /// <param name="request">Request object needed to process the Void transaction.</param>
        /// <returns>
        /// Response object.
        /// </returns>
        public Response Void(Request request)
        {
            string methodName = "Void";
            IDictionary<string, string> methodProp = new Dictionary<string, string>();
            methodProp.Add("Name", this.Name);
            methodProp.Add("MethodName", methodName);
            methodProp.Add("Platform", Platform);
            methodProp.Add("File Version", FileV);

            // Convert request
            VoidRequest voidRequest = null;
            IList<PaymentProperty> properties = new List<PaymentProperty>();
            string LogPrefix = "";
            try
            {
                voidRequest = VoidRequest.ConvertFrom(request);
                LogPrefix = voidRequest.IndustryType == "Ecommerce" ? "ECOM :: Void :: " : "Void :: ";
            }
            catch (SampleException ex)
            {
                TransactionProp transactionProp = new TransactionProp();
                transactionProp.ProcessorType = ((int)EftProcessorType.NBE).ToString();
                transactionProp.OperationTypeId = ((int)EftOperationType.Void).ToString();
                transactionProp.ResponseData = ex.Message;
                SetFailedTransactionProp(request, transactionProp, ref properties);
                RRLogger.Error(LogPrefix + ex.Message, methodProp, voidRequest.LogAppInsightsKey);
                return PaymentUtilities.CreateAndLogResponseForReturn(methodName, this.Name, Platform, locale: request == null ? null : request.Locale, properties: null, errors: ex.Errors);
            }

            // Validate merchant account
            List<PaymentError> errors = new List<PaymentError>();
            RRLogger.Information("CreateEftProcessor call is started.", methodProp, voidRequest.LogAppInsightsKey);
            eftProcessor = CreateEftProcessor(voidRequest,errors);
            if (errors.Count > 0)
            {
                TransactionProp transactionProp = new TransactionProp();
                transactionProp.ProcessorType = ((int)EftProcessorType.NBE).ToString();
                transactionProp.OperationTypeId = ((int)EftOperationType.Void).ToString();
                transactionProp.Amount = voidRequest.AuthorizationApprovedAmount.Value.ToString();
                foreach (PaymentError perror in errors)
                {
                    transactionProp.ResponseData += perror.Message;
                    RRLogger.Error(LogPrefix + perror.Message, methodProp, voidRequest.LogAppInsightsKey);
                }
                SetFailedTransactionProp(request, transactionProp, ref properties);
                return PaymentUtilities.CreateAndLogResponseForReturn(methodName, this.Name, Platform, voidRequest.Locale, properties, errors);
            }
            RRLogger.Information(LogPrefix + "CreateEftProcessor call is completed.", methodProp, voidRequest.LogAppInsightsKey);
            EftRequest eftRequest = new EftRequest();
            eftRequest.Amount = voidRequest.AuthorizationApprovedAmount.Value;
            eftRequest.Operation = EftOperationType.Void;  
            eftRequest.TransactionId = voidRequest.TransactionId;
            RRLogger.Information(LogPrefix + "Transaction Id: " + voidRequest.TransactionId, methodProp, voidRequest.LogAppInsightsKey);
            eftRequest.AuthTransId = voidRequest.AuthorizationProviderTransactionId;
            eftRequest.Source = EftRequestSource.Eft;
            eftRequest.StaffId = voidRequest.StaffId;
            eftRequest.CardType = EftCardType.CreditCard;
          

            RRLogger.Information(LogPrefix + "Eft Process call is started.", methodProp, voidRequest.LogAppInsightsKey);
            EftResponse eftResponse = eftProcessor.Process(eftRequest);
            RRLogger.Information(LogPrefix + "Eft Process call is completed.", methodProp, voidRequest.LogAppInsightsKey);
            bool isApproved = eftResponse.Status == EftOperationStatus.Approved;
            RRLogger.Information(LogPrefix + "Transaction Status: " + eftResponse.Status, methodProp, voidRequest.LogAppInsightsKey);
            RRLogger.Information(LogPrefix + "ResponseCode: " + eftResponse.ResponseCode + " ApprovedAmount: " + eftResponse.ApprovedAmount, methodProp, voidRequest.LogAppInsightsKey);

            var voidResponse = new VoidResponse(voidRequest.Locale, voidRequest.ServiceAccountId, this.Name);
            if (isApproved)
            {
                voidResponse.VoidResult = VoidResult.Success.ToString();
                methodProp.Add("RequestData", eftResponse.RequestData);
                methodProp.Add("ResponseData", eftResponse.ResponseData);
                RRLogger.Information(LogPrefix + "Payment Response", methodProp, voidRequest.LogAppInsightsKey);
            }
            else
            {
                voidResponse.VoidResult = VoidResult.Failure.ToString();
                string eftErrorMessage = string.Empty;
                foreach (EftError err in eftResponse.Errors.Select())
                {
                    eftErrorMessage = err == null ? "Declined" : err.ErrorMessage;
                    errors.Add(new PaymentError(ErrorCode.Decline, eftErrorMessage));
                }
                methodProp.Add("RequestData", eftResponse.RequestData);
                methodProp.Add("ResponseData", eftResponse.ResponseData);
                RRLogger.Error(LogPrefix + "Payment Response", methodProp, voidRequest.LogAppInsightsKey);
            }
            if (voidRequest.CardType != null)
            {
                eftResponse.CardType = CardMap(voidRequest.PaymentCardTypes, eftResponse.CardType, methodName);
            }
            voidResponse.Last4Digit = eftResponse.CardReference;
            voidResponse.ResponseCode = string.IsNullOrEmpty(voidRequest.AuthorizationApprovalCode)?eftResponse.ProviderTransactionId : voidRequest.AuthorizationApprovalCode;
            voidResponse.ProviderMessage = eftResponse.Status.ToString();
            voidResponse.ProviderTransactionId = eftResponse.ProviderTransactionId;
            if (voidRequest.AllowPrintingProcessorReceipt)
                voidResponse.ExternalReceipt = eftResponse.ReceiptTextXml;
           

            Response response = VoidResponse.ConvertTo(voidResponse);
            properties = response.Properties.ToList();
            properties.Add(new PaymentProperty(GenericNamespace.TransactionData, "RequestData", eftResponse.RequestData));
            properties.Add(new PaymentProperty(GenericNamespace.TransactionData, "ResponseData", eftResponse.ResponseData));
           
            string OperationTypeId = Convert.ToInt32(eftRequest.Operation).ToString();
            string ProcessorType = Convert.ToInt32(EftProcessorType.NBE).ToString();

            int Environment = voidRequest.TransactionMode == "1" ? 1 : 0;
            properties.Add(new PaymentProperty(GenericNamespace.TransactionData, "ProcessorType", ProcessorType));
            properties.Add(new PaymentProperty(GenericNamespace.TransactionData, "OperationTypeId", OperationTypeId));
            properties.Add(new PaymentProperty(GenericNamespace.TransactionData, "OperationType", eftRequest.Operation.ToString()));
            properties.Add(new PaymentProperty(GenericNamespace.TransactionData, "ResponseAuthTransId", eftResponse.ProviderTransactionId));
            properties.Add(new PaymentProperty(GenericNamespace.TransactionData, "ProviderTransactionId", eftResponse.ProviderTransactionId));
            properties.Add(new PaymentProperty(GenericNamespace.TransactionData, "ReceiptText", eftResponse.ReceiptText));
            properties.Add(new PaymentProperty(GenericNamespace.TransactionData, "RequestData", eftResponse.RequestData));
            properties.Add(new PaymentProperty(GenericNamespace.TransactionData, "ResponseData", eftResponse.ResponseData));
            properties.Add(new PaymentProperty(GenericNamespace.TransactionData, "OurRef", eftResponse.TransactionId));
            properties.Add(new PaymentProperty(GenericNamespace.TransactionData, "Environment", Environment.ToString()));
            properties.Add(new PaymentProperty(GenericNamespace.TransactionData, "DeviceTerminalId", eftResponse.TerminalId));

            properties.Add(new PaymentProperty(GenericNamespace.TransactionData, "Status", ((int)eftResponse.Status).ToString()));
            properties.Add(new PaymentProperty(GenericNamespace.TransactionData, "StatusId", ((int)eftResponse.Status).ToString()));
            properties.Add(new PaymentProperty(GenericNamespace.TransactionData, "RequestAmount", voidRequest.AuthorizationApprovedAmount.GetValueOrDefault()));


            response.Properties = properties.ToArray();
            if (errors.Count > 0)
            {
                foreach (PaymentError perror in errors)
                {
                    RRLogger.Error(LogPrefix + perror.Message, methodProp, voidRequest.LogAppInsightsKey);
                }
                response.Errors = errors.ToArray();
            }

            PaymentUtilities.LogOperationResult(methodName, this.Name, Platform, request, response);
            PaymentUtilities.LogResponseBeforeReturn(methodName, this.Name, Platform, response);
            return response;
        }

        /// <summary>
        /// Refund the request with the payment provider.
        /// </summary>
        /// <param name="request">Request object needed to process the Refund transaction.</param>
        /// <param name="requiredInteractionProperties">Properties required by Refund process.</param>
        /// <returns>
        /// Response object.
        /// </returns>
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "Test code only")]
        public Response Refund(Request request, PaymentProperty[] requiredInteractionProperties)
        {
            string methodName = "Refund";
            IDictionary<string, string> methodProp = new Dictionary<string, string>();
            methodProp.Add("Name", this.Name);
            methodProp.Add("MethodName", methodName);
            methodProp.Add("Platform", Platform);
            methodProp.Add("File Version", FileV);
            bool EnableLog = false;
            // Convert request
            RefundRequest refundRequest = null;
            IList<PaymentProperty> properties = new List<PaymentProperty>();
            string LogPrefix = "";
            try
            {
                refundRequest = RefundRequest.ConvertFrom(request);
                LogPrefix = refundRequest.IndustryType == "Ecommerce" ? "ECOM :: refund :: " : "refund :: ";
            }
            catch (SampleException ex)
            {
                TransactionProp transactionProp = new TransactionProp();
                transactionProp.ProcessorType = ((int)EftProcessorType.NBE).ToString();
                transactionProp.OperationTypeId = ((int)EftOperationType.Void).ToString();
                transactionProp.ResponseData = ex.Message;
                SetFailedTransactionProp(request, transactionProp, ref properties);
                RRLogger.Error(LogPrefix + ex.Message, methodProp, refundRequest.LogAppInsightsKey);
                return PaymentUtilities.CreateAndLogResponseForReturn(methodName, this.Name, Platform, locale: request == null ? null : request.Locale, properties: null, errors: ex.Errors);
            }

            // Validate merchant account
            List<PaymentError> errors = new List<PaymentError>();
            RRLogger.Information("CreateEftProcessor call is started.", methodProp, refundRequest.LogAppInsightsKey);
            eftProcessor = CreateEftProcessor(refundRequest, errors);
            if (errors.Count > 0)
            {
                TransactionProp transactionProp = new TransactionProp();
                transactionProp.ProcessorType = ((int)EftProcessorType.NBE).ToString();
                transactionProp.OperationTypeId = ((int)EftOperationType.Void).ToString();
                transactionProp.Amount = refundRequest.Amount.Value.ToString();
                foreach (PaymentError perror in errors)
                {
                    transactionProp.ResponseData += perror.Message;
                    RRLogger.Error(LogPrefix + perror.Message, methodProp, refundRequest.LogAppInsightsKey);
                }
                SetFailedTransactionProp(request, transactionProp, ref properties);
                return PaymentUtilities.CreateAndLogResponseForReturn(methodName, this.Name, Platform, refundRequest.Locale, properties, errors);
            }
            RRLogger.Information(LogPrefix + "CreateEftProcessor call is completed.", methodProp, refundRequest.LogAppInsightsKey);
            EftRequest eftRequest = new EftRequest();
            eftRequest.Amount = refundRequest.Amount.Value;
            eftRequest.Operation = EftOperationType.Refund;
            eftRequest.TransactionId = refundRequest.TransactionId;
            RRLogger.Information(LogPrefix + "Transaction Id: " + refundRequest.TransactionId, methodProp, refundRequest.LogAppInsightsKey);
            LogHelper.Log(LogTarget.File, Name, FileV, refundRequest.AssemblyName, this.GetType().Name, methodName, "Card Token: " + refundRequest.CardToken, EnableLog);
            if (!string.IsNullOrEmpty(refundRequest.CardToken))
            {
                eftRequest.AuthCode = refundRequest.CardToken;
                eftRequest.AuthTransId = refundRequest.CaptureProviderTransactionId;
            }
            else
            {
                eftRequest.AuthCode = refundRequest.AuthCode;
                eftRequest.AuthTransId = refundRequest.CaptureProviderTransactionId;
            }

            eftRequest.Source = EftRequestSource.Eft;
            eftRequest.StaffId = refundRequest.StaffId;
            eftRequest.CardType = EftCardType.CreditCard;


            RRLogger.Information(LogPrefix + "Eft Process call is started.", methodProp, refundRequest.LogAppInsightsKey);
            EftResponse eftResponse = eftProcessor.Process(eftRequest);
            RRLogger.Information(LogPrefix + "Eft Process call is completed.", methodProp, refundRequest.LogAppInsightsKey);
            bool isApproved = eftResponse.Status == EftOperationStatus.Approved;
            RRLogger.Information(LogPrefix + "Transaction Status: " + eftResponse.Status, methodProp, refundRequest.LogAppInsightsKey);
            RRLogger.Information(LogPrefix + "ResponseCode: " + eftResponse.ResponseCode + " ApprovedAmount: " + eftResponse.ApprovedAmount, methodProp, refundRequest.LogAppInsightsKey);

            LogHelper.Log(LogTarget.File, Name, FileV, refundRequest.AssemblyName, this.GetType().Name, methodName, "Transaction Status: " + eftResponse.Status, EnableLog);
            LogHelper.Log(LogTarget.File, Name, FileV, refundRequest.AssemblyName, this.GetType().Name, methodName, string.Format("ApprovalCode: {0} ApprovedAmount: {1} CardType: {2}", eftResponse.ProviderTransactionId, eftResponse.ApprovedAmount, eftResponse.CardType), EnableLog);
            LogHelper.Log(LogTarget.File, Name, FileV, refundRequest.AssemblyName, this.GetType().Name, methodName, "RequestData: " + eftResponse.RequestData, EnableLog);
            LogHelper.Log(LogTarget.File, Name, FileV, refundRequest.AssemblyName, this.GetType().Name, methodName, "ResponseData: " + eftResponse.ResponseData, EnableLog);


           
            var refundResponse = new RefundResponse(refundRequest.Locale, refundRequest.ServiceAccountId, this.Name);
            if (isApproved)
            {
                refundResponse.RefundResult = RefundResult.Success.ToString();
                LogHelper.Log(LogTarget.File, Name, FileV, refundRequest.AssemblyName, this.GetType().Name, methodName, "RequestData: " + eftResponse.RequestData, EnableLog);
                LogHelper.Log(LogTarget.File, Name, FileV, refundRequest.AssemblyName, this.GetType().Name, methodName, "ResponseData: " + eftResponse.ResponseData, EnableLog);

                methodProp.Add("RequestData", eftResponse.RequestData);
                methodProp.Add("ResponseData", eftResponse.ResponseData);
                RRLogger.Information(LogPrefix + "Payment Response", methodProp, refundRequest.LogAppInsightsKey);
            }
            else
            {
                refundResponse.RefundResult = VoidResult.Failure.ToString();
                LogHelper.Log(LogTarget.File, Name, FileV, refundRequest.AssemblyName, this.GetType().Name, methodName, "RequestData: " + eftResponse.RequestData, EnableLog);
                LogHelper.Log(LogTarget.File, Name, FileV, refundRequest.AssemblyName, this.GetType().Name, methodName, "ResponseData: " + eftResponse.ResponseData, EnableLog);

                string eftErrorMessage = string.Empty;
                foreach (EftError err in eftResponse.Errors.Select())
                {
                    eftErrorMessage = err == null ? "Declined" : err.ErrorMessage;
                    errors.Add(new PaymentError(ErrorCode.Decline, eftErrorMessage));
                }
                methodProp.Add("RequestData", eftResponse.RequestData);
                methodProp.Add("ResponseData", eftResponse.ResponseData);
                RRLogger.Error(LogPrefix + "Payment Response", methodProp, refundRequest.LogAppInsightsKey);
            }
            if (refundRequest.CardType != null)
            {
                eftResponse.CardType = CardMap(refundRequest.PaymentCardTypes, eftResponse.CardType, methodName);
            }
            refundResponse.Last4Digit = eftResponse.CardReference;
            refundResponse.ResponseCode = string.IsNullOrEmpty(refundRequest.CaptureApprovalCode) ? eftResponse.ProviderTransactionId : refundRequest.CaptureApprovalCode;
            refundResponse.ProviderMessage = eftResponse.Status.ToString();
            refundResponse.ProviderTransactionId = eftResponse.ProviderTransactionId;
            refundResponse.ApprovedAmount = eftResponse.ApprovedAmount;
            if (refundRequest.AllowPrintingProcessorReceipt)
                refundResponse.ExternalReceipt = eftResponse.ReceiptTextXml;


            Response response = RefundResponse.ConvertTo(refundResponse);
            properties = response.Properties.ToList();
            properties.Add(new PaymentProperty(GenericNamespace.TransactionData, "RequestData", eftResponse.RequestData));
            properties.Add(new PaymentProperty(GenericNamespace.TransactionData, "ResponseData", eftResponse.ResponseData));

            string OperationTypeId = Convert.ToInt32(eftRequest.Operation).ToString();
            string ProcessorType = Convert.ToInt32(EftProcessorType.NBE).ToString();

            int Environment = refundRequest.TransactionMode == "1" ? 1 : 0;
            properties.Add(new PaymentProperty(GenericNamespace.TransactionData, "ProcessorType", ProcessorType));
            properties.Add(new PaymentProperty(GenericNamespace.TransactionData, "OperationTypeId", OperationTypeId));
            properties.Add(new PaymentProperty(GenericNamespace.TransactionData, "OperationType", eftRequest.Operation.ToString()));
            properties.Add(new PaymentProperty(GenericNamespace.TransactionData, "ResponseAuthTransId", eftResponse.ProviderTransactionId));
            properties.Add(new PaymentProperty(GenericNamespace.TransactionData, "ProviderTransactionId", eftResponse.ProviderTransactionId));
            properties.Add(new PaymentProperty(GenericNamespace.TransactionData, "ReceiptText", eftResponse.ReceiptText));
            properties.Add(new PaymentProperty(GenericNamespace.TransactionData, "RequestData", eftResponse.RequestData));
            properties.Add(new PaymentProperty(GenericNamespace.TransactionData, "ResponseData", eftResponse.ResponseData));
            properties.Add(new PaymentProperty(GenericNamespace.TransactionData, "OurRef", eftResponse.TransactionId));
            properties.Add(new PaymentProperty(GenericNamespace.TransactionData, "Environment", Environment.ToString()));
            properties.Add(new PaymentProperty(GenericNamespace.TransactionData, "DeviceTerminalId", eftResponse.TerminalId));

            properties.Add(new PaymentProperty(GenericNamespace.TransactionData, "Status", ((int)eftResponse.Status).ToString()));
            properties.Add(new PaymentProperty(GenericNamespace.TransactionData, "StatusId", ((int)eftResponse.Status).ToString()));
            properties.Add(new PaymentProperty(GenericNamespace.TransactionData, "RequestAmount", refundRequest.Amount.GetValueOrDefault()));


            response.Properties = properties.ToArray();
            if (errors.Count > 0)
            {
                foreach (PaymentError perror in errors)
                {
                    RRLogger.Error(LogPrefix + perror.Message, methodProp, refundRequest.LogAppInsightsKey);
                }
                response.Errors = errors.ToArray();
            }

            PaymentUtilities.LogOperationResult(methodName, this.Name, Platform, request, response);
            PaymentUtilities.LogResponseBeforeReturn(methodName, this.Name, Platform, response);
            return response;
        }

        /// <summary>
        /// Reversal the request with the payment provider.
        /// </summary>
        /// <param name="request">Request object needed to process the Reversal transaction.</param>
        /// <returns>
        /// Response object.
        /// </returns>
        public Response Reversal(Request request)
        {
            var ex = new NotImplementedException("Reversal Not Implemented");
            RetailLogger.Log.PaymentConnectorLogException("Reversal", this.Name, Platform, ex);
            throw ex;
        }

        /// <summary>
        /// ReAuthorize the request with the payment provider.
        /// </summary>
        /// <param name="request">Request object needed to process the ReAuthorize transaction.</param>
        /// <param name="requiredInteractionProperties">Properties required by ReAuthorize process.</param>
        /// <returns>
        /// Response object.
        /// </returns>
        public Response Reauthorize(Request request, PaymentProperty[] requiredInteractionProperties)
        {
            var ex = new NotImplementedException("Reauthorize Not Implemented");
            RetailLogger.Log.PaymentConnectorLogException("Reauthorize", this.Name, Platform, ex);
            throw ex;
        }

        /// <summary>
        /// GenerateCardToken get the token for the requested credit card from the payment provider.
        /// </summary>
        /// <param name="request">Request object needed to process the GenerateCardToken transaction.</param>
        /// <param name="requiredInteractionProperties">Properties required by GenerateCardToken process.</param>
        /// <returns>
        /// Response object.
        /// </returns>
        public Response GenerateCardToken(Request request, PaymentProperty[] requiredInteractionProperties)
        {
            var ex = new NotImplementedException("GenerateCardToken Not Implemented");
            RetailLogger.Log.PaymentConnectorLogException("GenerateCardToken", this.Name, Platform, ex);
            throw ex;
        }

        /// <summary>
        /// GetPaymentAcceptPoint gets the payment accepting point from the payment provider, e.g. a payment page URL.
        /// </summary>
        /// <param name="request">Request object needed to process the GetPaymentAcceptPoint transaction.</param>
        /// <returns>Response object.</returns>
        public Response GetPaymentAcceptPoint(Request request)
        {
            string methodName = "GetPaymentAcceptPoint";
            RetailLogger.Log.PaymentConnectorLogOperation(methodName, OperationStarting, this.Name, Platform);

            // Convert request to validate
            GetPaymentAcceptPointRequest acceptPointRequest = null;
            Uri baseAddress = null;
            try
            {
                acceptPointRequest = GetPaymentAcceptPointRequest.ConvertFrom(request);
                baseAddress = new Uri(RequestBase.PaymentAcceptWebUrl); //GetPaymentAcceptBaseAddress(acceptPointRequest.Environment);
            }
            catch (SampleException ex)
            {
                return PaymentUtilities.CreateAndLogResponseForReturn(methodName, this.Name, Platform, locale: request == null ? null : request.Locale, properties: null, errors: ex.Errors);
            }

            // Process and create response
            List<PaymentError> errors = new List<PaymentError>();
            var acceptPointResponse = new GetPaymentAcceptPointResponse(acceptPointRequest.Locale, acceptPointRequest.ServiceAccountId, this.Name);

            // Do not validate merchant account here, because the REST service will validate it.
            using (HttpClient client = new HttpClient())
            {
                // Serialize request content
                string requestJson = Newtonsoft.Json.JsonConvert.SerializeObject(request);
                HttpContent content = new StringContent(requestJson);
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                // Call REST service
                HttpResponseMessage httpResponse = null;
                client.BaseAddress = baseAddress;
                Task getPaymentAcceptPointTask = Task.Run(() => httpResponse = client.PostAsync(RequestBase.GetPaymentAcceptPointRelativeUri, content).Result);
                getPaymentAcceptPointTask.Wait();

                // Handle REST response
                if (httpResponse.IsSuccessStatusCode)
                {
                    // Read and deserialize response content
                    string responseJson = null;
                    Task readAsStringAsyncTask = Task.Run(() => responseJson = httpResponse.Content.ReadAsStringAsync().Result);
                    readAsStringAsyncTask.Wait();

                    Response getPaymentAcceptPointResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<Response>(responseJson);

                    // Prepare response properties
                    if (getPaymentAcceptPointResponse.Errors == null || getPaymentAcceptPointResponse.Errors.Length == 0)
                    {
                        // Read PaymentEntryUrl from reponse
                        var getPaymentAcceptPointResponseProperties = PaymentProperty.ConvertToHashtable(getPaymentAcceptPointResponse.Properties);
                        acceptPointResponse.PaymentAcceptUrl = GetPropertyStringValue(
                            getPaymentAcceptPointResponseProperties,
                            GenericNamespace.TransactionData,
                            TransactionDataProperties.PaymentAcceptUrl,
                            true);
                        acceptPointResponse.PaymentAcceptSubmitUrl = null;
                        acceptPointResponse.PaymentAcceptMessageOrigin = new Uri(acceptPointResponse.PaymentAcceptUrl).GetComponents(UriComponents.SchemeAndServer, UriFormat.UriEscaped);
                    }
                    else
                    {
                        errors.AddRange(getPaymentAcceptPointResponse.Errors);
                    }
                }
                else
                {
                    // GetPaymentAcceptPoint service failure
                    errors.Add(new PaymentError(ErrorCode.ApplicationError, "GetPaymentAcceptPoint failure: Internal server error."));
                }
            }

            // Convert response and return
            Response response = GetPaymentAcceptPointResponse.ConvertTo(acceptPointResponse);
            if (errors.Count > 0)
            {
                response.Errors = errors.ToArray();
            }

            PaymentUtilities.LogResponseBeforeReturn(methodName, this.Name, Platform, response);
            return response;
        }

        /// <summary>
        /// RetrievNBEmentAcceptResult retrieves the payment accepting result from the payment provider after the payment is processed externally.
        /// This method pairs with GetPaymentAcceptPoint.
        /// </summary>
        /// <param name="request">Request object needed to process the RetrievNBEmentAcceptResult transaction.</param>
        /// <returns>Response object.</returns>
        public Response RetrievePaymentAcceptResult(Request request)
        {
            string methodName = "RetrievePaymentAcceptResult";
            RetailLogger.Log.PaymentConnectorLogOperation(methodName, OperationStarting, this.Name, Platform);

            // Convert request to validate
            RetrievePaymentAcceptResultRequest acceptResultRequest = null;
            Uri baseAddress = null;
            try
            {
                acceptResultRequest = RetrievePaymentAcceptResultRequest.ConvertFrom(request);
                baseAddress = new Uri(RequestBase.PaymentAcceptWebUrl);// GetPaymentAcceptBaseAddress(acceptResultRequest.Environment);
            }
            catch (SampleException ex)
            {
                return PaymentUtilities.CreateAndLogResponseForReturn(methodName, this.Name, Platform, locale: request == null ? null : request.Locale, properties: null, errors: ex.Errors);
            }

            // Process and create response
            List<PaymentError> errors = new List<PaymentError>();
            var acceptResultResponse = new RetrievePaymentAcceptResultResponse(acceptResultRequest.Locale, acceptResultRequest.ServiceAccountId, this.Name);

            // Do not validate merchant account here, because the REST service will validate it.
            Response response = null;
            using (HttpClient client = new HttpClient())
            {
                // Serialize request content
                string requestJson = Newtonsoft.Json.JsonConvert.SerializeObject(request);
                HttpContent content = new StringContent(requestJson);
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                // Call REST service
                HttpResponseMessage httpResponse = null;
                client.BaseAddress = baseAddress;
                Task retrievNBEmentAcceptResultTask = Task.Run(() => httpResponse = client.PostAsync(RequestBase.PaymentAcceptWebUrl, content).Result);
                retrievNBEmentAcceptResultTask.Wait();

                // Handle REST response
                if (httpResponse.IsSuccessStatusCode)
                {
                    // Read and deserialize response content
                    string responseJson = null;
                    Task readAsStringAsyncTask = Task.Run(() => responseJson = httpResponse.Content.ReadAsStringAsync().Result);
                    readAsStringAsyncTask.Wait();

                    Response retrievNBEmentAcceptResultResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<Response>(responseJson);

                    // Return the retrieved response as is.
                    response = retrievNBEmentAcceptResultResponse;
                }
                else
                {
                    // RetrievNBEmentAcceptResult service failure
                    errors.Add(new PaymentError(ErrorCode.ApplicationError, "RetrievNBEmentAcceptResult failure: Internal server error."));
                    response = RetrievePaymentAcceptResultResponse.ConvertTo(acceptResultResponse);
                }
            }

            // Return repsonse
            if (errors.Count > 0)
            {
                response.Errors = errors.ToArray();
            }

            PaymentUtilities.LogOperationResult(methodName, this.Name, Platform, request, response);
            PaymentUtilities.LogResponseBeforeReturn(methodName, this.Name, Platform, response);
            return response;
        }

        /// <summary>
        /// ActivateGiftCard the request with the payment provider.
        /// </summary>
        /// <param name="request">Request object needed to process the ActivateGiftCard transaction.</param>
        /// <param name="requiredInteractionProperties">Properties required by ActivateGiftCard process.</param>
        /// <returns>
        /// Response object.
        /// </returns>
        /// <exception cref="System.NotImplementedException">Not implemented exception.</exception>
        public Response ActivateGiftCard(Request request, PaymentProperty[] requiredInteractionProperties)
        {
            string methodName = "ActivateGiftCard";
            RetailLogger.Log.PaymentConnectorLogOperation(methodName, OperationStarting, this.Name, Platform);
            IDictionary<string, string> methodProp = new Dictionary<string, string>();
            methodProp.Add("Name", this.Name);
            methodProp.Add("MethodName", methodName);
            methodProp.Add("Platform", Platform);
            methodProp.Add("File Version", FileV);


            // Convert request
            ActivateGiftCardRequest activateGiftCardRequest = null;
            string LogPrefix = "";
            try
            {
                activateGiftCardRequest = ActivateGiftCardRequest.ConvertFrom(request);
                LogPrefix = activateGiftCardRequest.IndustryType == "Ecommerce" ? "ECOM :: ActivateGC :: " : "ActivateGC :: ";
            }
            catch (SampleException ex)
            {
                RRLogger.Exception(ex, methodProp, activateGiftCardRequest.LogAppInsightsKey);
                return PaymentUtilities.CreateAndLogResponseForReturn(methodName, this.Name, Platform, locale: request == null ? null : request.Locale, properties: null, errors: ex.Errors);
            }

            // Validate merchant account
            List<PaymentError> errors = new List<PaymentError>();
            IList<PaymentProperty> properties = new List<PaymentProperty>();
            RRLogger.Information(LogPrefix + "ValidateLicense call is started.", methodProp, activateGiftCardRequest.LogAppInsightsKey);
            // License >>
            DateTime? licenseExpiryDate = null;
            licenseExpiryDate = LicenseValidation.ValidateLicense(activateGiftCardRequest.AccountId, activateGiftCardRequest.ChannelId, activateGiftCardRequest.SerialNumber, activateGiftCardRequest.OldSerialNumber, errors, this.Name);
            // License <<
            RRLogger.Information(LogPrefix + "ValidateLicense call is completed.", methodProp, activateGiftCardRequest.LogAppInsightsKey);
            if (errors.Count > 0)
            {
                TransactionProp transactionProp = new TransactionProp();
                transactionProp.ProcessorType = ((int)EftProcessorType.NBE).ToString();
                transactionProp.OperationTypeId = ((int)EftOperationType.Activate).ToString();
                transactionProp.Amount = activateGiftCardRequest.Amount.GetValueOrDefault().ToString();
                foreach (PaymentError perror in errors)
                {
                    transactionProp.ResponseData += perror.Message;
                    RRLogger.Error(LogPrefix + perror.Message, methodProp, activateGiftCardRequest.LogAppInsightsKey);
                }
                SetFailedTransactionProp(request, transactionProp, ref properties);
                return PaymentUtilities.CreateAndLogResponseForReturn(methodName, this.Name, Platform, activateGiftCardRequest.Locale, properties, errors);
            }

            Hashtable hashRequestProperties = PaymentProperty.ConvertToHashtable(request.Properties);
            string cardNumber;
            PaymentProperty.GetPropertyValue(hashRequestProperties, GenericNamespace.TransactionData, PaymentCardProperties.CardNumber, out cardNumber);
            string amount;
            PaymentProperty.GetPropertyValue(hashRequestProperties, GenericNamespace.TransactionData, TransactionDataProperties.Amount, out amount);
            RRLogger.Information(LogPrefix +  "CreateEftProcessor call is started.", methodProp, activateGiftCardRequest.LogAppInsightsKey);
            IEftProcessor eftProcessor = CreateEftProcessor(activateGiftCardRequest, errors);
            if (errors.Count > 0)
            {
                TransactionProp transactionProp = new TransactionProp();
                transactionProp.ProcessorType = ((int)EftProcessorType.NBE).ToString();
                transactionProp.OperationTypeId = ((int)EftOperationType.Activate).ToString();
                transactionProp.Amount = activateGiftCardRequest.Amount.GetValueOrDefault().ToString();
                foreach (PaymentError perror in errors)
                {
                    transactionProp.ResponseData += perror.Message;
                    RRLogger.Error(LogPrefix + perror.Message, methodProp, activateGiftCardRequest.LogAppInsightsKey);
                }
                SetFailedTransactionProp(request, transactionProp, ref properties);
                return PaymentUtilities.CreateAndLogResponseForReturn(methodName, this.Name, Platform, activateGiftCardRequest.Locale, properties, errors);
            }
            RRLogger.Information(LogPrefix + "CreateEftProcessor call is completed.", methodProp, activateGiftCardRequest.LogAppInsightsKey);
            EftRequest eftRequest = new EftRequest();

            eftRequest.Amount = activateGiftCardRequest.Amount.GetValueOrDefault();
            eftRequest.Operation = EftOperationType.Activate;
            eftRequest.TransactionId = activateGiftCardRequest.TransactionId;
            eftRequest.Source = EftRequestSource.GiftCard;
            eftRequest.StaffId = activateGiftCardRequest.StaffId;

            RRLogger.Information(LogPrefix + "Eft Process call is started.", methodProp, activateGiftCardRequest.LogAppInsightsKey);
            EftResponse eftResponse = eftProcessor.Process(eftRequest);
            RRLogger.Information(LogPrefix + "Eft Process call is completed.", methodProp, activateGiftCardRequest.LogAppInsightsKey);
            bool isApproved = eftResponse.Status == EftOperationStatus.Approved;
            RRLogger.Information(LogPrefix +  "Transaction Status: " + eftResponse.Status, methodProp, activateGiftCardRequest.LogAppInsightsKey);
            var activateGiftCardResponse = new ActivateGiftCardResponse(activateGiftCardRequest.Locale, activateGiftCardRequest.ServiceAccountId, this.Name);
            if (isApproved)
            {
                activateGiftCardResponse.ActivateGiftCardResult = ActivateGiftCardResult.Success.ToString();
                methodProp.Add("RequestData", eftResponse.RequestData);
                methodProp.Add("ResponseData", eftResponse.ResponseData);
                RRLogger.Information(LogPrefix + "Payment Response", methodProp, activateGiftCardRequest.LogAppInsightsKey);
            }
            else
            {
                activateGiftCardResponse.ActivateGiftCardResult = ActivateGiftCardResult.Failure.ToString();
                EftError eftError = eftResponse.Errors.Last(false);
                string eftErrorMessage = eftError == null ? "Declined" : eftError.ErrorMessage;
                RRLogger.Error(LogPrefix + "ErrorMessage: "+ eftErrorMessage,methodProp, activateGiftCardRequest.LogAppInsightsKey);
                errors.Add(new PaymentError(ErrorCode.Decline, eftErrorMessage));
                methodProp.Add("RequestData", eftResponse.RequestData);
                methodProp.Add("ResponseData", eftResponse.ResponseData);
                RRLogger.Error(LogPrefix + "Payment Response", methodProp, activateGiftCardRequest.LogAppInsightsKey);
            }

            // Authorize and create response
            activateGiftCardResponse.CardType = eftResponse.CardType;
            activateGiftCardResponse.Last4Digit = GetLastFourDigits(eftResponse.CardReference);
            activateGiftCardResponse.GiftCardNumber = GetLastFourDigits(eftResponse.CardReference);
            activateGiftCardResponse.AvailableBalance = eftResponse.CardBalance;
            activateGiftCardResponse.ProviderTransactionId = eftResponse.ProviderTransactionId;

            // License >>
            ApplyLicense(licenseExpiryDate, activateGiftCardResponse);
            // License <<

            // Convert response and return
            Response response = ActivateGiftCardResponse.ConvertTo(activateGiftCardResponse);
            properties = response.Properties.ToList();
            string OperationTypeId = Convert.ToInt32(eftRequest.Operation).ToString();
            string ProcessorType = Convert.ToInt32(EftProcessorType.NBE).ToString();
            int Environment = activateGiftCardRequest.TransactionMode == "1" ? 1 : 0;
            properties.Add(new PaymentProperty(GenericNamespace.TransactionData, "ProcessorType", ProcessorType));
            properties.Add(new PaymentProperty(GenericNamespace.TransactionData, "OperationTypeId", OperationTypeId));
            properties.Add(new PaymentProperty(GenericNamespace.TransactionData, "OperationType", eftRequest.Operation.ToString()));
            properties.Add(new PaymentProperty(GenericNamespace.TransactionData, "ResponseAuthTransId", eftResponse.ProviderTransactionId));
            properties.Add(new PaymentProperty(GenericNamespace.TransactionData, "ProviderTransactionId", eftResponse.ProviderTransactionId));
            properties.Add(new PaymentProperty(GenericNamespace.TransactionData, "ReceiptText", eftResponse.ReceiptText));
            properties.Add(new PaymentProperty(GenericNamespace.TransactionData, "RequestData", eftResponse.RequestData));
            properties.Add(new PaymentProperty(GenericNamespace.TransactionData, "ResponseData", eftResponse.ResponseData));
            properties.Add(new PaymentProperty(GenericNamespace.TransactionData, "OurRef", eftResponse.TransactionId));
            properties.Add(new PaymentProperty(GenericNamespace.TransactionData, "Environment", Environment.ToString()));
            properties.Add(new PaymentProperty(GenericNamespace.TransactionData, "DeviceTerminalId", eftResponse.TerminalId));

            properties.Add(new PaymentProperty(GenericNamespace.TransactionData, "Status", ((int)eftResponse.Status).ToString()));
            properties.Add(new PaymentProperty(GenericNamespace.TransactionData, "StatusId", ((int)eftResponse.Status).ToString()));
            properties.Add(new PaymentProperty(GenericNamespace.TransactionData, "RequestAmount", activateGiftCardRequest.Amount.GetValueOrDefault()));
            


            response.Properties = properties.ToArray();
            if (errors.Count > 0)
            {
                foreach (PaymentError perror in errors)
                {
                    RRLogger.Error(LogPrefix + perror.Message, methodProp, activateGiftCardRequest.LogAppInsightsKey);
                }
                response.Errors = errors.ToArray();
            }

            PaymentUtilities.LogOperationResult(methodName, this.Name, Platform, request, response);
            PaymentUtilities.LogResponseBeforeReturn(methodName, this.Name, Platform, response);
            return response;
        }

        /// <summary>
        /// LoadGiftCard the request with the payment provider.
        /// </summary>
        /// <param name="request">Request object needed to process the LoadGiftCard transaction.</param>
        /// <param name="requiredInteractionProperties">Properties required by LoadGiftCard process.</param>
        /// <returns>
        /// Response object.
        /// </returns>
        /// <exception cref="System.NotImplementedException">Not implemented exception.</exception>
        public Response LoadGiftCard(Request request, PaymentProperty[] requiredInteractionProperties)
        {
            List<PaymentError> errors = new List<PaymentError>();
            IList<PaymentProperty> properties = new List<PaymentProperty>();
            errors.Add(new PaymentError(ErrorCode.NoActionTaken, "Load gift card not supported"));
            string methodName = "LoadGiftCard";
            TransactionProp transactionProp = new TransactionProp();
            transactionProp.ProcessorType = ((int)EftProcessorType.NBE).ToString();
            transactionProp.OperationTypeId = ((int)EftOperationType.Recharge).ToString();
            foreach (PaymentError perror in errors)
            {
                transactionProp.ResponseData += perror.Message;
            }
            SetFailedTransactionProp(request, transactionProp, ref properties);
            return PaymentUtilities.CreateAndLogResponseForReturn(methodName, this.Name, Platform,  request.Locale, properties, errors);
        }

        /// <summary>
        /// BalanceOnGiftCard the request with the payment provider.
        /// </summary>
        /// <param name="request">Request object needed to process the BalanceOnGiftCard transaction.</param>
        /// <param name="requiredInteractionProperties">Properties required by BalanceOnGiftCard process.</param>
        /// <returns>
        /// Response object.
        /// </returns>
        /// <exception cref="System.NotImplementedException">Not implemented exception.</exception>
        public Response BalanceOnGiftCard(Request request, PaymentProperty[] requiredInteractionProperties)
        {
            // RR >>
            string methodName = "BalanceOnGiftCard";
            IDictionary<string, string> methodProp = new Dictionary<string, string>();
            methodProp.Add("Name", this.Name);
            methodProp.Add("MethodName", methodName);
            methodProp.Add("Platform", Platform);
            methodProp.Add("File Version", FileV);


            // Convert request
            BalanceOnGiftCardRequest bogcRequest = null;
            string LogPrefix = "";
            try
            {
                bogcRequest = BalanceOnGiftCardRequest.ConvertFrom(request);
            }
            catch (SampleException ex)
            {
                LogPrefix = bogcRequest.IndustryType == "Ecommerce" ? "ECOM :: BalanceOnGC :: " : "BalanceOnGC :: ";
                RRLogger.Exception(ex, methodProp, bogcRequest.LogAppInsightsKey);
                return PaymentUtilities.CreateAndLogResponseForReturn(methodName, this.Name, Platform, locale: request == null ? null : request.Locale, properties: null, errors: ex.Errors);
            }

            // Validate merchant account
            List<PaymentError> errors = new List<PaymentError>();
            //ValidateMerchantProperties(bogcRequest, errors);
            RRLogger.Information(LogPrefix + "ValidateLicense call is started.", methodProp, bogcRequest.LogAppInsightsKey);
            // License >>
            DateTime? licenseExpiryDate = null;
            licenseExpiryDate = LicenseValidation.ValidateLicense(bogcRequest.AccountId, bogcRequest.ChannelId, bogcRequest.SerialNumber, bogcRequest.OldSerialNumber, errors, this.Name);
            // License <<
            RRLogger.Information(LogPrefix + "ValidateLicense call is completed.", methodProp, bogcRequest.LogAppInsightsKey);
            if (errors.Count > 0)
            {
                return PaymentUtilities.CreateAndLogResponseForReturn(methodName, this.Name, Platform, bogcRequest.Locale, errors);
            }

            RRLogger.Information(LogPrefix + "CreateEftProcessor call is started.", methodProp, bogcRequest.LogAppInsightsKey);
            IEftProcessor eftProcessor = CreateEftProcessor(bogcRequest, errors);
            if (errors.Count > 0)
            {
                return PaymentUtilities.CreateAndLogResponseForReturn(methodName, this.Name, Platform, bogcRequest.Locale, errors);
            }
            RRLogger.Information(LogPrefix+ "CreateEftProcessor call is completed.", methodProp, bogcRequest.LogAppInsightsKey);
            EftRequest eftRequest = new EftRequest();


            eftRequest.CardType = EftCardType.GiftCard;
            eftRequest.Operation = EftOperationType.BalanceEnquiry;
            eftRequest.TransactionId = bogcRequest.TransactionId;
            eftRequest.Source = EftRequestSource.GiftCard;    
            eftRequest.StaffId = bogcRequest.StaffId;
          

            RRLogger.Information(LogPrefix+"Eft Process call is started.", methodProp, bogcRequest.LogAppInsightsKey);
            EftResponse eftResponse = eftProcessor.Process(eftRequest);
            RRLogger.Information(LogPrefix + "Eft Process call is completed.", methodProp, bogcRequest.LogAppInsightsKey);
            bool isApproved = eftResponse.Status == EftOperationStatus.Approved;
            RRLogger.Information(LogPrefix + "Transaction Status: " + eftResponse.Status, methodProp, bogcRequest.LogAppInsightsKey);
            // Create response
            BalanceOnGiftCardResponse bogcResponse = new BalanceOnGiftCardResponse(bogcRequest.Locale, bogcRequest.ServiceAccountId, this.Name);
            if (isApproved)
            {
                bogcResponse.BalanceOnGiftCardResult = BalanceOnGiftCardResult.Success.ToString();
                methodProp.Add("RequestData", eftResponse.RequestData);
                methodProp.Add("ResponseData", eftResponse.ResponseData);
                RRLogger.Information(LogPrefix + "Payment Response", methodProp, bogcRequest.LogAppInsightsKey);
            }
            else
            {
                bogcResponse.BalanceOnGiftCardResult = BalanceOnGiftCardResult.Failure.ToString();
                EftError eftError = eftResponse.Errors.Last(false);
                string eftErrorMessage = eftError == null ? "Declined" : eftError.ErrorMessage;
                RRLogger.Error(LogPrefix +"ErrorMessage: " + eftErrorMessage, methodProp, bogcRequest.LogAppInsightsKey);
                errors.Add(new PaymentError(ErrorCode.Decline, eftErrorMessage));
                methodProp.Add("RequestData", eftResponse.RequestData);
                methodProp.Add("ResponseData", eftResponse.ResponseData);
                RRLogger.Error(LogPrefix + "Payment Response", methodProp, bogcRequest.LogAppInsightsKey);
            }
            // License >>
            ApplyLicense(licenseExpiryDate, bogcResponse);
            // License <<
            bogcResponse.CardType = eftResponse.CardType;
            bogcResponse.Last4Digit = eftResponse.CardReference;
            bogcResponse.ProviderTransactionId = eftResponse.TransactionId;
            bogcResponse.AvailableBalance = eftResponse.CardBalance;
            
            // Conver response and return
            Response response = BalanceOnGiftCardResponse.ConvertTo(bogcResponse);
            if (errors.Count > 0)
            {
                foreach (PaymentError perror in errors)
                {
                    RRLogger.Error( LogPrefix + perror.Message, methodProp, bogcRequest.LogAppInsightsKey);
                }
                response.Errors = errors.ToArray();
            }

            PaymentUtilities.LogOperationResult(methodName, this.Name, Platform, request, response);
            PaymentUtilities.LogResponseBeforeReturn(methodName, this.Name, Platform, response);
            return response;
            // RR <<
        }

        /// <summary>
        /// GetMerchantAccountPropertyMetadata returns the merchant account properties need by the payment provider.
        /// </summary>
        /// <param name="request">Request object.</param>
        /// <returns>
        /// Response object.
        /// </returns>
        public Response GetMerchantAccountPropertyMetadata(Request request)
        {
            string methodName = "GetMerchantAccountPropertyMetadata";
            IDictionary<string, string> methodProp = new Dictionary<string, string>();
            methodProp.Add("Name", this.Name);
            methodProp.Add("MethodName", methodName);
            methodProp.Add("Platform", Platform);
            methodProp.Add("File Version", FileV);

            // Check null request
            List<PaymentError> errors = new List<PaymentError>();
            if (request == null)
            {
                errors.Add(new PaymentError(ErrorCode.InvalidRequest, "Request is null."));
                return PaymentUtilities.CreateAndLogResponseForReturn(methodName, this.Name, Platform, errors);
            }
            AdapterInfo adapterInfo = AdaptersHelper.GetAdapters().FirstOrDefault();
            string macAddress = adapterInfo.MAC;

            // Prepare response
            List<PaymentProperty> properties = new List<PaymentProperty>();
            PaymentProperty property;
            property = new PaymentProperty(
            GenericNamespace.MerchantAccount,
            MerchantAccountProperties.AssemblyName,
            GetAssemblyName(this.GetType()));
            property.SetMetadata("Assembly name:", "The assembly name of the NBE provider", false, true, 0);
            properties.Add(property);

            property = new PaymentProperty(
            GenericNamespace.MerchantAccount,
            NBEMerchantAccountProperty.AssemblyVersion,
            GetAssemblyVersion());
            property.SetMetadata("Assembly version:", "The Assembly Version for NBE provider", false, true, 1);
            properties.Add(property);

            property = new PaymentProperty(
            GenericNamespace.MerchantAccount,
            NBEMerchantAccountProperty.FileVersion,
            Utility.AssemblyDetailsInfo.GetAssemblFileVersion(this.GetType()));
            property.SetMetadata("File version:", "The File Version for NBE provider", false, true, 2);
            properties.Add(property);

            property = new PaymentProperty(
            GenericNamespace.MerchantAccount,
            NBEMerchantAccountProperty.AssemblyCopyright,
            GetExecutingAssemblyAttribute<AssemblyCopyrightAttribute>(a => a.Copyright));
            property.SetMetadata("Copyright:", "The assembly copyright for the NBE provider", false, true, 3);
            properties.Add(property);

            property = new PaymentProperty(
            GenericNamespace.MerchantAccount,
            NBEMerchantAccountProperty.ConnectorName,
            new NBEProcessorIdentifier().Name);
            property.SetMetadata("Connector name:", "The connector name of the NBE provider", false, true, 4);
            properties.Add(property);

            // License >>
            property = new PaymentProperty(
            GenericNamespace.MerchantAccount,
            LicenseProperties.SerialNumber,
            TestData.SerialNumber.ToString());
            property.SetMetadata("Serial number:", "RR serial number", false, true, 5);
            properties.Add(property);
            // License <<

            property = new PaymentProperty(
            GenericNamespace.MerchantAccount,
            MerchantAccountProperties.ServiceAccountId,
            Guid.NewGuid().ToString());
            property.SetMetadata("Service account id:", "The organization subscription Id for NBE provider", false, false, 6);
            properties.Add(property);

            property = new PaymentProperty(
            GenericNamespace.MerchantAccount,
            NBEMerchantAccountProperty.UniqueKey,
            macAddress);
            property.SetMetadata("Unique key:", "The unique key for the NBE provider", false, true, 7);
            property.IsHidden = true;
            properties.Add(property);

            property = new PaymentProperty(
            GenericNamespace.MerchantAccount,
            MerchantAccountProperties.SupportedCurrencies,
            TestData.SupportedCurrencies);
            property.SetMetadata("Supported currencies:", "The supported currencies for the NBE provider", false, false, 8);
            properties.Add(property);

            property = new PaymentProperty(
            GenericNamespace.MerchantAccount,
            MerchantAccountProperties.SupportedTenderTypes,
            TestData.SupportedTenderTypes);
            property.SetMetadata("Supported tender types:", "The supported tender types for the NBE provider", false, false, 9);
            properties.Add(property);

            property = new PaymentProperty(
            GenericNamespace.MerchantAccount,
            NBEMerchantAccountProperty.CardMap,
            TestData.CardMap);
            property.SetMetadata("Card mapping:", "The card mapping received from NBE provider", false, false, 10);
            properties.Add(property);


            property = new PaymentProperty(
            GenericNamespace.MerchantAccount,
            NBEMerchantAccountProperty.Comport,
            TestData.ComPort) ;
            property.SetMetadata("Comport:", "Comport", false, false, 11);
            properties.Add(property);

            property = new PaymentProperty(
         GenericNamespace.MerchantAccount,
           NBEMerchantAccountProperty.Baudrate,
         TestData.BaudRate);
            property.SetMetadata("Baudrate:", "Baudrate", false, false, 12);
            properties.Add(property);

            property = new PaymentProperty(
        GenericNamespace.MerchantAccount,
          NBEMerchantAccountProperty.Stopbit,
         TestData.StopBit);
            property.SetMetadata("Stopbit:", "Stopbit", false, false, 13);
            properties.Add(property);

            property = new PaymentProperty(
        GenericNamespace.MerchantAccount,
          NBEMerchantAccountProperty.parity,
        TestData.Parity);
            property.SetMetadata("Parity:", "Parity", false, false, 14);
            properties.Add(property);

            property = new PaymentProperty(
      GenericNamespace.MerchantAccount,
        NBEMerchantAccountProperty.Timeout,
      TestData.Timeout);
            property.SetMetadata("Timeout:", "Timeout", false, false, 15);
            properties.Add(property);

            property = new PaymentProperty(
            GenericNamespace.MerchantAccount,
            NBEMerchantAccountProperty.TransactionMode,
            TestData.TransactionMode);
            property.SetMetadata("Test mode:", "The MAX Pay testmode", false, false, 16);
            properties.Add(property);

            property = new PaymentProperty(
            GenericNamespace.MerchantAccount,
            NBEMerchantAccountProperty.PaymentAcceptWebUrl,
            TestData.PaymentAcceptWebUrl);
            property.SetMetadata("Payment accept web url:", "The payment accept web url received from NBE provider", false, false, 17);
            properties.Add(property);

            Response response = new Response();
            response.Locale = request.Locale;
            response.Properties = properties.ToArray();
            if (errors.Count > 0)
            {
                foreach (PaymentError perror in errors)
                {
                    //RRLogger.Error(perror.Message, methodProp,  LogAppInsightsKey);
                }
                response.Errors = errors.ToArray();
            }

            PaymentUtilities.LogResponseBeforeReturn(methodName, this.Name, Platform, response);
            return response;
        }

        /// <summary>
        /// ValidateMerchantAccount the passed merchant account properties with the payment provider.
        /// </summary>
        /// <param name="request">Request object to validate.</param>
        /// <returns>
        /// Response object.
        /// </returns>
        public Response ValidateMerchantAccount(Request request)
        {
            string methodName = "ValidateMerchantAccount";
            IDictionary<string, string> methodProp = new Dictionary<string, string>();
            methodProp.Add("Name", this.Name);
            methodProp.Add("MethodName", methodName);
            methodProp.Add("Platform", Platform);
            methodProp.Add("File Version", FileV);


            // Convert request
            ValidateMerchantAccountRequest validateRequest = null;
            try
            {
                validateRequest = ValidateMerchantAccountRequest.ConvertFrom(request);
            }
            catch (SampleException ex)
            {
                return PaymentUtilities.CreateAndLogResponseForReturn(methodName, this.Name, Platform, locale: request == null ? null : request.Locale, properties: null, errors: ex.Errors);
            }

            // Validate merchant account
            List<PaymentError> errors = new List<PaymentError>();
            if (errors.Count > 0)
            {
                return PaymentUtilities.CreateAndLogResponseForReturn(methodName, this.Name, Platform, validateRequest.Locale, errors);
            }

            // Create response
            var validateResponse = new ValidateMerchantAccountResponse(validateRequest.Locale, validateRequest.ServiceAccountId, this.Name);

            // Convert response and return
            Response response = ValidateMerchantAccountResponse.ConvertTo(validateResponse);
            PaymentUtilities.LogResponseBeforeReturn(methodName, this.Name, Platform, response);
            return response;
        }

        /// <summary>
        /// Execute a task on the payment connector. This will be unique to each payment connector and will require customization on client.
        /// </summary>
        /// <param name="request">Request object needed to process the ExecuteTask transaction.</param>
        /// <returns>Response object.</returns>
        public Response ExecuteTask(Request request)
        {
            string methodName = "ExecuteTask";
            IDictionary<string, string> methodProp = new Dictionary<string, string>();
            methodProp.Add("Name", this.Name);
            methodProp.Add("MethodName", methodName);
            methodProp.Add("Platform", Platform);
            methodProp.Add("File Version", FileV);


            // Check null request
            List<PaymentError> errors = new List<PaymentError>();
            if (request == null || request.Properties == null)
            {
                errors.Add(new PaymentError(ErrorCode.InvalidRequest, "Request is null."));
                return PaymentUtilities.CreateAndLogResponseForReturn(methodName, this.Name, Platform, errors);
            }

            // Create hash to find required properties quickly
            Hashtable hashRequestProperties = PaymentProperty.ConvertToHashtable(request.Properties);

            // Get the task name from the request, For parameters please use the TransactionData name space for any parameters you pass for a task.
            string taskName = string.Empty;
            PaymentProperty.GetPropertyValue(hashRequestProperties, GenericNamespace.TransactionData, TransactionDataProperties.TaskName, out taskName);
            Response response = new Response();
            // You can have many different tasks for your payment connector just make sure the client is updated to use your tasks request and response properties.
            switch (taskName.ToLower())
            {
                // License >>
                case "rr.licenseapply":
                    return LicenseValidation.ExecuteLicenseApply(request, this.Name, Platform, this.Name);
                case "rr.licensecheck":
                    return LicenseValidation.ExecuteLicenseCheck(request, this.Name, Platform, this.Name);
                case "rr.licenseparam":
                    return LicenseValidation.ExecuteLicenseParam(request, this.Name, Platform, this.Name);
                default:
                    break;
            }

            return response;
        }

        #endregion
        #region Licence
        // License >>
        private static void ApplyLicense(DateTime? licenseExpiryDate, ResponseBase responseBase)
        {
            if (licenseExpiryDate != null)
                responseBase.LicenseExpireDate = licenseExpiryDate;
        }

        // License <<
        #endregion
        /// <summary>
        /// Gets the token.
        /// </summary>
        /// <param name="cardNumber">The card number.</param>
        /// <returns>The token.</returns>
        private static string GetToken(string cardNumber)
        {
            /* 
             IMPORTANT!!!
             THIS IS SAMPLE CODE ONLY!  
             THE CODE SHOULD BE UPDATED TO OBTAIN THE TOKEN FROM THE APPROPRIATE PAYMENT PROVIDERS.
            */
            var encoder = new UTF8Encoding();
            byte[] bytes = encoder.GetBytes(cardNumber);
            return Convert.ToBase64String(bytes);
        }
        private static string GetLastFourDigits(string cardNumber)
        {
            return cardNumber.Length < 4 ? cardNumber.PadLeft(4, PaddingCharacter) : cardNumber.Substring(cardNumber.Length - 4);
        }

        private static string GetPropertyStringValue(Hashtable propertyHashtable, string propertyNamespace, string propertyName, bool throwExceptionIfNotFound)
        {
            string propertyValue;
            bool found = PaymentProperty.GetPropertyValue(
                            propertyHashtable,
                            propertyNamespace,
                            propertyName,
                            out propertyValue);
            if (!found && throwExceptionIfNotFound)
            {
                ThrowPropertyNotSetException(propertyName);
            }

            return propertyValue;
        }

        private static void ThrowPropertyNotSetException(string propertyName)
        {
            throw new ArgumentException(string.Format("Property '{0}' is null or not set", propertyName));
        }

        private static string GetAssemblyName(Type type)
        {
            string asemblyQualifiedName = type.AssemblyQualifiedName;
            int commaIndex = asemblyQualifiedName.IndexOf(',');
            return asemblyQualifiedName.Substring(commaIndex + 1).Trim();
        }
        private IEftProcessor CreateEftProcessor(RequestBase request,IList<PaymentError> errors)
        {
            try
            {
                EftService eftService = new EftService();
                eftService.ComPort = request.Comport;
                eftService.BaudRate = request.Baudrate;
                eftService.Parity = request.Parity;
                eftService.StopBit = request.Stopbit;
                eftService.Timeout = Convert.ToInt32(request.Timeout);
               
    
                eftService.ServiceType = EftProcessorType.NBE;
                return EftFactory.Create(eftService);
            }
            catch (Exception ex)
            {
                errors.Add(new PaymentError(ErrorCode.InvalidMerchantConfiguration, ex.Message));
                return null;
            }
        }
        public string CardMap(List<KeyValuePair<string, string>> cardTypes, string cardType, string methodName)
        {
            IDictionary<string, string> methodProp = new Dictionary<string, string>();
            methodProp.Add("Name", this.Name);
            methodProp.Add("MethodName", methodName);
            methodProp.Add("Platform", Platform);
            methodProp.Add("File Version", FileV);
            cardType = cardType == null ? string.Empty : cardType;
            cardType = cardTypes.FirstOrDefault(c => c.Key.Equals(cardType, StringComparison.OrdinalIgnoreCase)).Value;

            if (string.IsNullOrEmpty(cardType))
            {
                cardType = "Default";
                cardType = cardTypes.FirstOrDefault(c => c.Key.Equals(cardType, StringComparison.OrdinalIgnoreCase)).Value;
            }
            return cardType;
        }
        private static string GetAssemblyVersion()
        {
            var assembly = typeof(NBEConnector).GetTypeInfo().Assembly;
            var assemblyName = new AssemblyName(assembly.FullName);
            return assemblyName.Version.ToString();
        }
        private static string GetExecutingAssemblyAttribute<T>(Func<T, string> value) where T : Attribute
        {
            var assembly = typeof(NBEConnector).GetTypeInfo().Assembly;
            T attribute = (T)assembly.GetCustomAttribute(typeof(T));
            return value.Invoke(attribute);
        }
        public enum ActivateGiftCardResult
        {
            None = 0,
            Success = 1,
            Failure = 2
        }
        public enum LoadGiftCardResult
        {
            None = 0,
            Success = 1,
            Failure = 2
        }
        public enum BalanceOnGiftCardResult
        {
            None = 0,
            Success = 1,
            Failure = 2
        }
        private void SetFailedTransactionProp(Request request, TransactionProp transactionProp, ref IList<PaymentProperty> properties)
        {
            if (request.Properties != null && transactionProp != null)
            {
                Hashtable hashTable = PaymentProperty.ConvertToHashtable(request.Properties);
                string s_value;
                PaymentProperty.GetPropertyValue(hashTable, GenericNamespace.TransactionData, "EFTTransactionId", out s_value);
                transactionProp.TransactionId = s_value;
                properties.Add(new PaymentProperty(GenericNamespace.TransactionData, "ProcessorType", transactionProp.ProcessorType));
                properties.Add(new PaymentProperty(GenericNamespace.TransactionData, "OperationTypeId", transactionProp.OperationTypeId));
                properties.Add(new PaymentProperty(GenericNamespace.TransactionData, "ProviderTransactionId", transactionProp.TransactionId));
                properties.Add(new PaymentProperty(GenericNamespace.TransactionData, "RequestAmount", transactionProp.Amount));
                properties.Add(new PaymentProperty(GenericNamespace.TransactionData, "ResponseData", transactionProp.ResponseData));
            }
        }
       
    }
    public class Address
    {
        public string State { get; set; }
        public string County { get; set; }
        public string ZipCode { get; set; }
        public string Street { get; set; }
        public string Name { get; set; }
        public string ThreeLetterISORegionName { get; set; }
    }
    public class TransactionProp
    {
        public string ProcessorType { get; set; }
        public string OperationTypeId { get; set; }
        public string TransactionId { get; set; }
        public string ResponseData { get; set; }
        public string Amount { get; set; }
    }

}


