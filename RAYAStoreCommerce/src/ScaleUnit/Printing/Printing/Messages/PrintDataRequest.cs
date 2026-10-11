using System.Runtime.Serialization;

using Microsoft.Dynamics.Commerce.Runtime;
using Microsoft.Dynamics.Commerce.Runtime.Messages;
using Microsoft.Dynamics.Commerce.Runtime.DataModel;

namespace RR.Commerce.Runtime.Printing
{
    [DataContract]
    public sealed class GetPRPrinterDataRequest : Request
    {
        public GetPRPrinterDataRequest(long printerId)
        {
            this.PrinterId = printerId;
        }

        [DataMember]
        public long PrinterId { get; }
    }

    [DataContract]
    public sealed class GetPRTerminalDataRequest : Request
    {
        public GetPRTerminalDataRequest(string teminalId)
        {
            this.TeminalId = teminalId;
        }

        [DataMember]
        public string TeminalId { get; }
    }
    
    [DataContract]
    public sealed class GetStorePrinterDataRequest : Request
    {
        public GetStorePrinterDataRequest(long storeRecId)
        {
            this.StoreRecId = storeRecId;
        }

        [DataMember]
        public long StoreRecId { get; }
    }

}
