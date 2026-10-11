using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace RR.Dynamics.Retail.Payment.NBE
{
    public class EftMessage
    {
        public string RefNum { get; set; }
        public string ReceiptNum { get; set; }
        public string MerchID { get; set; }
        public string TermID { get; set; }
        public string CardMask { get; set; }
        public string TrxStatusMsg { get; set; }
        public string ResponseCode { get; set; }
        public static EftMessage Unpack(string data)
        {
            EftMessage _eftMessage = new EftMessage();

                //string xmlheader = "<?xml version='1.0' encoding='utf-8'?>";
                // data = "<FROMPOS><AuthNum></AuthNum><RefNum>125011000013</RefNum><ReceiptNum>000013</ReceiptNum><MerchID>001700000002</MerchID><TermID>91127814</TermID><BatchID>000001</BatchID><CardMask>521624******3340</CardMask><CardVerified>11</CardVerified><TrxStatusMsg>INVALID CARD</TrxStatusMsg><ResponseCode>14</ResponseCode><TrxDate>07/09/2021</TrxDate><Trxtime>12:59:51</Trxtime></FROMPOS>";
                var xmldoc = EftCommon.LoadXml(data);
                if (xmldoc.Element("FROMPOS")!=null)
                {
                    if(xmldoc.Element("FROMPOS").Element("RefNum") != null)
                    _eftMessage.RefNum = xmldoc.Element("FROMPOS").Element("RefNum").Value;
                    if (xmldoc.Element("FROMPOS").Element("ReceiptNum") != null)
                        _eftMessage.ReceiptNum = xmldoc.Element("FROMPOS").Element("ReceiptNum").Value;
                    if (xmldoc.Element("FROMPOS").Element("MerchID") != null)
                        _eftMessage.MerchID = xmldoc.Element("FROMPOS").Element("MerchID").Value;
                    if (xmldoc.Element("FROMPOS").Element("TermID") != null)
                        _eftMessage.TermID = xmldoc.Element("FROMPOS").Element("TermID").Value;
                    if (xmldoc.Element("FROMPOS").Element("CardMask") != null)
                        _eftMessage.CardMask = xmldoc.Element("FROMPOS").Element("CardMask").Value;
                    if (xmldoc.Element("FROMPOS").Element("TrxStatusMsg") != null)
                        _eftMessage.TrxStatusMsg = xmldoc.Element("FROMPOS").Element("TrxStatusMsg").Value;
                    if (xmldoc.Element("FROMPOS").Element("ResponseCode") != null)
                        _eftMessage.ResponseCode = xmldoc.Element("FROMPOS").Element("ResponseCode").Value;
                }
                if(xmldoc.Element("CONNFROMPOS") != null)
                {
                    if (xmldoc.Element("CONNFROMPOS").Element("TrxStatusMsg") != null)
                        _eftMessage.TrxStatusMsg = xmldoc.Element("CONNFROMPOS").Element("TrxStatusMsg").Value;
                    if (xmldoc.Element("CONNFROMPOS").Element("TrxStatusMsg") != null)
                        _eftMessage.ResponseCode = xmldoc.Element("CONNFROMPOS").Element("ResponseCode").Value;
                    if (string.IsNullOrEmpty(_eftMessage.TrxStatusMsg))
                        _eftMessage.ResponseCode = "00";
                }

            return _eftMessage;
        }
    }
    public static class EftCommon
    {
        public static XDocument LoadXml(string strXml)
        {
            XDocument doc = XDocument.Parse(strXml);
            return doc;
        }
        public static string GetSingleNodeValue(XDocument doc, string elementName)
        {
            XElement node = doc.Root.Element(elementName);
            if (node == null)
                return "";
            else
                return node.Value;
        }
        public static string GetSingleNodeXML(XDocument doc, string elementName)
        {
            XElement node = doc.Root.Element(elementName);
            if (node == null)
                return "";
            else
                return node.ToString();
        }
        public static DateTime GetSecureCardExpiry(string reqExpDate)
        {
            DateTime dtExpToken;

            int reqMn = Convert.ToInt32(reqExpDate.Substring(0, 2));
            int reqYr = Convert.ToInt32("20" + reqExpDate.Substring(reqExpDate.Length - 2, 2));

            int day = DateTime.DaysInMonth(reqYr, reqMn);
            dtExpToken = new DateTime(reqYr, reqMn, day);
            return dtExpToken;
        }
    }
}
