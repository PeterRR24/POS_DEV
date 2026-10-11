
using RR.Dynamics.Retail.Payment.NBE.Contract;
using System;  
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RR.Dynamics.Retail.Payment.NBE;


namespace RR.Dynamics.Retail.Payment.NBE
{
    internal class NBEDescriptor : EftProcessorDescriptor
    {
        public override string Id { get { return "NBE"; } }
        public override string Name { get { return "NBE"; } } 
        public override bool IsGiftCardProcessor { get { return true; } }
        public override int NeedProductDescription { get { return 0; } }
        public override string CreditCardTypes { get { return "Mastercard;Visa;Discover;Amex;Check"; } }
        public override string CreditCardCurrencies(string creditCardType) { return "USD"; }
        public override bool IsTokenListSupported { get { return false; } }
        public override bool HasUserInterface { get { return true; } }
    }
    public class NBEEft : IEftProcessor
    {
        private EftService _service;
        private string _requestData = String.Empty;
        private string _responseData = String.Empty;
       
        internal NBEEft(EftService service)
        {
            _service = service;
        }

        public EftService Service { get { return _service; } }
        public EftProcessorDescriptor GetDescriptor() { return new NBEDescriptor(); }
        public EftRequest CreateRequest()
        {
            return new EftRequest();
        }

        private EftResponse HandleResponse(EftRequest request, EftResponse eftResponse, EftMessage response)
        {

            eftResponse.RequestData = _requestData;
            eftResponse.ResponseData = _responseData;

            if (response != null)
            {
                eftResponse.TransactionId = request.TransactionId;
                eftResponse.AuthTransId = response.RefNum;
                eftResponse.ApprovedAmount = request.Amount;
                eftResponse.ProviderTransactionId = response.RefNum;
                eftResponse.MerchantId = response.MerchID;
                eftResponse.ResponseCode = response.ResponseCode;
                eftResponse.CardType = request.CardType.ToString();
                eftResponse.AuthCode = response.ReceiptNum;
                if (response.CardMask != null)
                {
                    eftResponse.CardReference = response.CardMask.Length > 4 ? response.CardMask.Substring(response.CardMask.Length - 4, 4) : response.CardMask;
                   //eftResponse.CardExpiry = response.CARD.EXPIRY_MONTH + response.CARD.EXPIRY_YEAR;
                }

                eftResponse.TerminalId = response.TermID;
                HandleResponseCode(request, eftResponse, response);
            }
            return eftResponse;

        }
        private EftResponse HandleResponseCode(EftRequest request, EftResponse eftResponse, EftMessage response)
        {
            switch (response.ResponseCode)
            {
                case "00":
                    eftResponse.TransStatus = EftTransactionStatus.Success;
                    break;
                default:
                    eftResponse.TransStatus = EftTransactionStatus.Failure;
                    eftResponse.Status = EftOperationStatus.Error;
                    eftResponse.Errors.Add(EftErrorSeverity.Error, string.IsNullOrEmpty(response.TrxStatusMsg) ? "Error" : response.TrxStatusMsg);
                    break;
            }
            return eftResponse;
        }
        public EftResponse Process(EftRequest request)
        {
            EftResponse eftResponse = new EftResponse();
            try
            {
                EftMessage response = null;
                string OperationType = string.Empty;
                if (ValidateRequest(request, eftResponse))
                {
                    response = SendRequest(request, eftResponse);
                }
                if (response != null)
                {
                    eftResponse = HandleResponse(request, eftResponse, response);
                }
            }
            catch (Exception ex)
            {
                eftResponse.Errors.Add(EftErrorSeverity.Critical, ex.Message);
            }
            // Final Status
            eftResponse.Status = EftOperationStatus.Error;
            switch (eftResponse.TransStatus)
            {
                case EftTransactionStatus.Success:
                    eftResponse.Status = EftOperationStatus.Approved;
                    break;
                case EftTransactionStatus.Declined:
                case EftTransactionStatus.Referral:
                    eftResponse.Status = EftOperationStatus.Declined;
                    break;
            }
            return eftResponse;
        }
        public bool isTimeoutError(EftResponse response)
        {
            if (response.Errors.Select() != null)
            {
                EftError err = response.Errors.Last(false);
                if (err.ErrorMessage == "The operation has timed out")
                    return true;
                else
                    return false;
            }
            else
                return false;
        }
        private bool ValidateRequest(EftRequest request, EftResponse response)
        {
            if (!IsOperationSupported(request.Operation))
            {
                //response.Errors.Add(EftErrorSeverity.Critical, ResEftText.Text("OPRNOTSUPPT"));                
                return false;
            }
            return true;
        }
        private EftMessage SendRequest(EftRequest request, EftResponse eftResponse)
        {
            EftMessage transResp = null;
            try
            {
                string requestToProcess = GenerateRequest(request);
                _requestData = requestToProcess;
                if (!String.IsNullOrEmpty(requestToProcess))
                {
                    string responseReceived = ProcessRequest(requestToProcess);
                    _responseData = responseReceived;
                    eftResponse.RequestData = _requestData;
                    eftResponse.ResponseData = _responseData;

                    if (!string.IsNullOrEmpty(responseReceived))
                    {
                        transResp = new EftMessage();
                        transResp = EftMessage.Unpack(responseReceived);// extracted response and assigned to corresponding class
                    }
                    else
                    {
                        if(eftResponse.Errors==null)
                        eftResponse.Errors.Add(EftErrorSeverity.Critical, "Response is blank!!!");
                        eftResponse.RequestData = _requestData;
                        eftResponse.ResponseData = _responseData;
                    }
                }
            }
            catch (Exception ex)
            {
                eftResponse.RequestData = _requestData;
                eftResponse.ResponseData = _responseData;
                eftResponse.TransStatus = EftTransactionStatus.Failure;
                eftResponse.Status = EftOperationStatus.Error;
                eftResponse.TransactionId = request.TransactionId;
                eftResponse.Errors.Add(EftErrorSeverity.Critical, ex.Message, "");
            }
            return transResp;
        }

        private string GenerateRequest(EftRequest request)
        {
            string XmlData = string.Empty;
            switch (request.Operation)
            {
                case EftOperationType.Sale:
                case EftOperationType.Void:
                case EftOperationType.Refund:                  
                    XmlData = BuildMessage.RequestData(request);
                    break;
                case EftOperationType.TestCommunication:
                    XmlData = BuildMessage.TestCommRequestData(request);
                    break;
            }
            return XmlData;
        }

        #region ProcessRequest

        private string ProcessRequest(string requestToProcess)
        {
            Utility.RRSerialport Rrsp = new Utility.RRSerialport();
            Utility.SerialportRequest Rrspreq = new Utility.SerialportRequest();
            Rrspreq.Timeout = _service.Timeout;
            Rrspreq.ComPort = _service.ComPort;
            Rrspreq.BaudRate = _service.BaudRate;
            Rrspreq.Parity = _service.Parity;
            Rrspreq.StopBit = _service.StopBit;
            Rrspreq.requestToProcess = requestToProcess;

            string  _responseData  = Rrsp.ProcessRequest(Rrspreq);
            return _responseData; 
        }

       

        

        #endregion
       
       
        
        #region static
        internal static bool IsOperationSupported(EftOperationType operation)
        {
            switch (operation)
            {
                case EftOperationType.TestCommunication:
                case EftOperationType.Sale:
                case EftOperationType.Refund:
                case EftOperationType.Void:
                    return true;
            }
            return false;
        }
        #endregion
    }

    internal class SerialPort
    {
        internal void Write(string requestToProcess)
        {
            throw new NotImplementedException();
        }
    }

    public class SerialDataReceivedEventArgs
    {
    }
}

