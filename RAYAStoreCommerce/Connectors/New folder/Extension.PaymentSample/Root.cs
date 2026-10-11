using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BanqueMisrDeviceName
{
    

    public class Response
    {
  
        public string cardNumber { get; set; }
        public string responseMessage { get; set; }
        public string authNumber { get; set; }
        public string merchantId { get; set; }
        public string terminalId { get; set; }
        public string transactionDate { get; set; }
        public string batchNumber { get; set; }
        public string stan { get; set; }
    }

   
}
