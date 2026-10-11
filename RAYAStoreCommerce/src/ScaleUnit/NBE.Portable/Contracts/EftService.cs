using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RR.Dynamics.Retail.Payment.NBE.Contract
{
    public enum EftProcessorType
    {
        Poscashier = 55,
        NBE = 56
    }

    public enum EftGatewayControl
    {
        None = 0,
        MSRPinpad = 1,
        Application = 5
    }

    public enum IntegrationType
    {
        AX = 0,
        EnterprisePos = 1,
        ModernPos = 2,
        RMS = 5,
        Dinerware = 3
    }
    public enum EftTransactionMode
    {
        Test = 0,
        Live = 1
    }

    public enum EftProtocol
    {
        Http = 0,
        Https = 1,
        Tcp = 2
    }

    public enum EftOperationStatus
    {
        NA = 0,
        Approved = 1,
        Declined = 2,
        Error = 3,
    }

    public class EftService
    {
        public EftProcessorType ServiceType { get; set; }
        public IntegrationType IntegrationType { get; set; }
        public EftTransactionMode TransactionMode { get; set; }
        public string ComPort { get; set; }
        public string BaudRate { get; set; }
        public string Parity { get; set; }
        public string StopBit { get; set; }
        public int Timeout { get; set; }
       
        
    }
}
