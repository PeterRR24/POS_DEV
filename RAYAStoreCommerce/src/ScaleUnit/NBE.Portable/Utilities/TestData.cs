/*
SAMPLE CODE NOTICE

THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED, 
OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.  
THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.  
NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
*/
namespace RR.Dynamics.Retail.Payment.NBE.Portable
{
    using System;

    /// <summary>
    /// Pre-defined test data for the payment connector.
    /// </summary>
    public static class TestData
    {
        /// <summary>
        /// The supported currencies.
        /// </summary>
        public const string SupportedCurrencies = "AUD;BRL;CAD;CHF;CNY;CZK;DKK;EUR;GBP;HKD;HUF;INR;JPY;KPW;KRW;MXN;NOK;NZD;PLN;SEK;SGD;TWD;USD;ZAR";

        /// <summary>
        /// The supported tender types.
        /// </summary>
        public const string SupportedTenderTypes = "AMEXPRESS;DISCOVER;EUROCARD;GIFTCARD;LOYALTY;MAESTRO;MASTER;VISA;VISAELEC";

        /// <summary>
        /// The PaymentAcceptWebUrl.
        /// 
        /// </summary>
        public const string PaymentAcceptWebUrl = @"http://localhost:3973";

        public const string ComPort = "";
        public const string BaudRate = "";
        public const string Parity = "";
        public const string StopBit = "";
        public const string Timeout = "120000";


        /// <summary>
        /// The SerialNumber.
        /// </summary>
        public const string SerialNumber = "";
        /// <summary>
        /// TransactionMode property.
        /// </summary>
        public const string TransactionMode = "1";

        /// <summary>
        /// The CardMap.
        /// </summary>//Unknown;DEBIT CARD;BANKCARD;MASTERCARD;VISA;AMERICAN EXPRESS;DINERS CLUB;JCB;PRIVATE LABEL CARD;OTHER 
        public const string CardMap = "Default:VISA;Visa:VISA;Mastercard:MASTER;Discover:DISCOVER;Amex:AMEXPRESS;Diners Club:VISA;JCB:VISA;Carte Blanche:VISA;GiftCard:GiftCard;Debit:Debit;";
    }
}
