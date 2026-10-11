using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using RR.Dynamics.Retail.Payment.NBE;
using RR.Dynamics.Retail.Payment.NBE.Contract;


namespace RR.Dynamics.Retail.Payment.NBE
{
    internal class BuildMessage
    {
        public static string RequestData(EftRequest request)
        {
            string data = String.Empty;
            string TrxType = String.Empty;
            
            data = "<TOPOS>" + "\n";
            switch (request.Operation)
            {
                case EftOperationType.Sale:
                    TrxType = "00";
                    break;
                case EftOperationType.Void:
                    TrxType = "02";
                    break;
                case EftOperationType.Refund:
                    TrxType = "20";
                    break;
                
            }

        if (!String.IsNullOrEmpty(TrxType))
        data += "<TrxType>" + TrxType + "</TrxType>" + "\n";

        if (!String.IsNullOrEmpty(request.Amount.ToString()))
        data += "<Amount>" + Convert.ToDouble(request.Amount * 100) + "</Amount>" + "\n";
        
        data += "<RefNum>" + request.AuthTransId + "</RefNum>" + "\n";
          
        data += "<RcptNum>" + request.AuthCode + "</RcptNum>" + "\n";
        data += "</TOPOS>";
        return data;
      
        }
        public static string TestCommRequestData(EftRequest request)
        {
            string data = String.Empty;

            data = "<CONNTOPOS>" + "\n";
            data += "<ConnTest>" + "RRTest" + "</ConnTest>" + "\n";
            data += "</CONNTOPOS>";

            return data;
        }
    }
}
