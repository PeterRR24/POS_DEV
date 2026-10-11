namespace RR.PaymentDevice
{
    using Microsoft.Dynamics.Commerce.HardwareStation;
    using Microsoft.Dynamics.Retail.Diagnostics;
    using Microsoft.Dynamics.Retail.PaymentSDK.Portable;
    using RR.Utility;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Xml;
    using System.Xml.Linq;
    public class AppStorage
    {
        private const Int64 initialCounter = 0;
        private string limitCounter = "999999999999999999";
        private string _baseCounter = null;
        private string constantFileFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, AppDomain.CurrentDomain.RelativeSearchPath ?? string.Empty) + Path.DirectorySeparatorChar;
        private string counterFileFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + Path.DirectorySeparatorChar;
        private string fileName = string.Empty;
        private string fileNameConstant = string.Empty;
        private string fileNameCommon = string.Empty;
        private string connectorName = string.Empty;
        private int TransactionIdLength = 0;
        private int TransactionCounterLength = 0;
        private string axBaseCounter = string.Empty;
        private static string PayLogisticsSignaturePath = string.Empty;
        private string triPOSCustomWorkFlow = string.Empty;
        private string terminalId = string.Empty;
        public string sfileName = string.Empty;
        //public string extPOSName = string.Empty;
        //public string sDeviceID = string.Empty;
        public string extPOSName = string.Empty;
        public string sDeviceID = string.Empty;
        public string sPairingCode = string.Empty;

        public AppStorage(string connectorName, string bCounter,string TriPOSCustomWorkFlow = "0", string TerminalId = "")
        {
            axBaseCounter = bCounter;
            this.connectorName = connectorName;
            triPOSCustomWorkFlow = TriPOSCustomWorkFlow;
            terminalId = TerminalId;
            
        }
        public static string GetPayLogisticsSignaturePath()
        {
            return PayLogisticsSignaturePath;
        }
        public string GiftTerminalId
        {
            get
            {
                return GetSingleNodeValue("GiftTerminalId");
            }
        }
        // For Assembly Payment Provider
        public string PosId
        {
            get
            {
                return GetSingleNodeValue("PosId");
            }
        }

        public string EftPosaddress
        {
            get
            {
                return GetSingleNodeValue("PosEftAddress");
            }
        }
        // For Assembly Payment Provider//
        private string BaseCounter
        {
            get
            {
                if (_baseCounter == null)
                    _baseCounter = GetBaseCounter();
                return _baseCounter;
            }
        }

        private string BaseCounterSensepass
        {
            get
            {
                if (_baseCounter == null)
                    _baseCounter = GetBaseCounterSensepass();
                return _baseCounter;
            }
        }

        public string GetCounter(int counterLength = 0)
        {
            if (counterLength != 0)
            {
                this.TransactionIdLength = counterLength;
                limitCounter = limitCounter.Substring(0, counterLength);
            }

       
            
                return axBaseCounter + DateTime.Now.ToString("yyMMddHHmmssfff");
            
			
        }

        private void WriteTextToFile(string fileName, string text)
        {
            using (var fileStream = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Write, 4096, FileOptions.WriteThrough))
            {
                byte[] bytes = System.Text.Encoding.UTF8.GetBytes(text);
                fileStream.Write(bytes, 0, bytes.Length);
            }
        }

        public string GetSavedCounter()
        {
            return getFileContent(this.fileName);
        }

        public string GetConstantFile()
        {
            string result = string.Empty;
            XDocument xmlFile = XDocument.Load(Path.Combine(this.constantFileFolderPath, this.fileNameConstant));
            if (xmlFile != null)
            {
                XElement node = (from connector in xmlFile.Descendants(this.connectorName)
                                 select connector).Single();
                result = node.ToString();
            }
            return result;
        }

        public string GetSingleNodeValue(string elementName)
        {
            string result = string.Empty;
            if (!File.Exists(Path.Combine(this.constantFileFolderPath, this.fileNameConstant)))
            {
                return result;
            }
            //if (Connectors.Shift4Connector == this.connectorName && !File.Exists(Path.Combine(this.constantFileFolderPath, this.fileNameConstant)))
            //{
            //    return result;
            //}
            XDocument xmlFile = XDocument.Load(Path.Combine(this.constantFileFolderPath, this.fileNameConstant));
            if (xmlFile != null)
            {
                XElement node = (from connector in xmlFile.Descendants(this.connectorName)
                                 select connector).Single();
                result = node.Element(elementName) != null ? node.Element(elementName).Value : string.Empty;
            }
            return result;
        }

        private string getFileContent(string constFileName)
        {
            string result = string.Empty;
            //RetailLogger.Log.ExtendedInformationalEvent("GetFileContent Counter File Path", constFileName);
            if (File.Exists(constFileName))
                result = File.ReadAllText(constFileName);
            if (Connectors.TriPOSConnector == this.connectorName)
            {
                try
                {
                    Int64 val = Convert.ToInt64(result);
                }
                catch (Exception ex)
                {
                    RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "failed to getFileContent in method Exception: " + ex.Message, "ERROR");
                    result = string.Empty;
                }
            }
            return result;
        }

        private string GetBaseCounter()
        {
            Int64 baseCounter = 0;
            string stringBaseCounter = GetSingleNodeValue("BaseCounter");
            try
            {
                if (!string.IsNullOrEmpty(stringBaseCounter))
                {
                    baseCounter = Convert.ToInt64(stringBaseCounter);
                }
                else if (!string.IsNullOrEmpty(axBaseCounter))
                {
                    baseCounter = Convert.ToInt64(axBaseCounter);
                    stringBaseCounter = axBaseCounter;
                }
                else
                {
                    RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "Base Counter is empty.", "ERROR");
                    throw new Exception("Base Counter is empty.");
                }
            }
            catch (Exception ex)
            {
                RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "failed to GetBaseCounter in method Exception: " + ex.Message, "ERROR");
                throw ex;
            }
            RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "GetBaseCounter in method BaseCounter: " + stringBaseCounter);
            return stringBaseCounter;
        }

        private string GetBaseCounterSensepass()
        {
            Int64 baseCounter = 0;
            string stringBaseCounter = "";
            try
            {
                if (!string.IsNullOrEmpty(axBaseCounter))
                {
                    baseCounter = Convert.ToInt64(axBaseCounter);
                    stringBaseCounter = axBaseCounter;
                }
                else
                {
                    RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "Base Counter is empty.", "ERROR");
                    //throw new Exception("Base Counter is empty.");
                }
            }
            catch (Exception ex)
            {
                RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "failed to GetBaseCounter in method Exception: " + ex.Message, "ERROR");
                throw ex;
            }
            RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "GetBaseCounter in method BaseCounter: " + stringBaseCounter);
            return stringBaseCounter;
        }

        public string CalculateSecondsAmtrack()
        {
            int year = DateTime.Now.Year;

            DateTime dt = new DateTime(year, 1, 1, 0, 0, 0, 0, DateTimeKind.Local); //from current year/1/1 00:00:00 to now

            DateTime dtNow = DateTime.Now;

            TimeSpan result = dtNow.Subtract(dt);

            int seconds = Convert.ToInt32(result.TotalSeconds);
            string sec1 = seconds.ToString().PadLeft(8, '0'); // total 8 digit seconds from 1 Jan to current date
                                                              

            string terminalId = this.terminalId; //4 digit terminal id
            string uniqueId = terminalId + sec1;//sec1 + terminalId;
            return uniqueId;

        }
		public static string calculateSeconds()
        {
            int year = DateTime.Now.Year;

            DateTime dt = new DateTime(year, 1, 1, 0, 0, 0, 0, DateTimeKind.Local);//from current year/1/1 00:00:00 to now
            System.Threading.Thread.Sleep(500);
            DateTime dtNow = DateTime.Now;

            TimeSpan result = dtNow.Subtract(dt);

            int seconds = Convert.ToInt32(result.TotalSeconds);
            string sec1 = seconds.ToString().PadLeft(8, '0');
            //string yearTwod = Convert.ToString(Convert.ToInt32(year.ToString().Substring(2, 2)) - 22); // starting with 22 not working so taken from 10
            string yearTwod = Convert.ToString(Convert.ToInt32(year.ToString().Substring(2, 2)) - 22).PadLeft(2, '0'); // starting with 22 not working so taken from 10
            return yearTwod + sec1;
        }
        public string EnableCustomerAccountSignature()
        {
            return GetSingleNodeValue("EnableCustomerAccountSignature");
        }

        public List<PaymentProperty> GetTerminalProperties()
        {
            List<PaymentProperty> list = new List<PaymentProperty>();
            if ((Connectors.Shift4Connector == this.connectorName && !File.Exists(Path.Combine(this.constantFileFolderPath, this.fileNameConstant))) || this.connectorName == Connectors.CloverConnector)
            {
                return list;
            }
            if(string.IsNullOrEmpty(this.fileNameConstant))
                return list;
            //XDocument xmlFile = XDocument.Load(Path.Combine(this.constantFileFolderPath, this.fileNameConstant));
            //list = PaymentUtility.GetExtraTerminalProperties(this.connectorName, xmlFile);
            return list;
        }

        // added for save AssemblyPayment merchant property into Constant file.
        public void SaveProperies(string posId, string EftPosaddress)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(Path.Combine(this.constantFileFolderPath, this.fileNameConstant));
                XmlNode root = doc.DocumentElement;
                XmlNode myNode = root.SelectSingleNode("PosId");
                myNode.InnerText = posId;
                XmlNode myNode1 = root.SelectSingleNode("PosEftAddress");
                myNode1.InnerText = EftPosaddress;
                doc.Save(Path.Combine(this.constantFileFolderPath, this.fileNameConstant));
            }
            catch (Exception ex)
            {
                RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "failed to SaveProperies in method Exception: " + ex.Message, "ERROR");
            }
        }

        public string GetValueFromText(string element)
        {
            string result = "0";
            string prop = string.Empty;

            if (File.Exists(this.fileNameCommon))
            {
                prop = File.ReadAllText(this.fileNameCommon);
                result = Between(prop, element + "=", ",");
            }
            return result;
        }

        public bool SetValueToText(string element, string value)
        {
            bool result = false;
            try
            {
                if (!Directory.Exists(Path.GetDirectoryName(this.fileNameCommon)))
                    Directory.CreateDirectory(Path.GetDirectoryName(this.fileNameCommon));
                string text = element + "=" + value + ",";
                File.WriteAllText(this.fileNameCommon, text);
                result = true;
            }
            catch (Exception ex)
            {
                RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "failed to SetValueToText in method Exception: " + ex.Message, "ERROR");
            }
            return result;
        }
        public bool SetValueToJsonFile(string data)
        {
            bool result = false;
            try
            {
                if (!Directory.Exists(Path.GetDirectoryName(this.fileNameCommon)))
                    Directory.CreateDirectory(Path.GetDirectoryName(this.fileNameCommon));

                File.WriteAllText(this.fileNameCommon, data);
                result = true;
            }
            catch (Exception ex)
            {
                RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "failed to SetValueToJsonFile in method Exception: " + ex.Message, "ERROR");
            }
            return result;
        }
        public string GetValueFromJsonFile()
        {
            string result = "";
            try
            {
                if (!Directory.Exists(Path.GetDirectoryName(this.fileNameCommon)))
                    Directory.CreateDirectory(Path.GetDirectoryName(this.fileNameCommon));

                result = File.ReadAllText(this.fileNameCommon);

            }
            catch (Exception ex)
            {
                RRPaymentDeviceLogger.WriteFileContent("RRPDLogger", "failed to GetValueFromJsonFile in method Exception: " + ex.Message, "ERROR");
            }
            return result;
        }
        /// <summary>
        /// Get string value between [first] a and [last] b.
        /// </summary>
        private static string Between(string value, string a, string b)
        {
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
            return value.Substring(adjustedPosA, posB - adjustedPosA);
        }
    }
}
