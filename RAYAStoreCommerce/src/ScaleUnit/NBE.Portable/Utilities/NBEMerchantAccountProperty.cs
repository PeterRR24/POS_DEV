/*
SAMPLE CODE NOTICE

THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED, 
OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.  
THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.  
NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
*/
namespace RR.Dynamics.Retail.Payment.NBE.Portable
{
    using Microsoft.Dynamics.Retail.PaymentSDK.Portable;
    using Microsoft.Dynamics.Retail.PaymentSDK.Portable.Constants;
    using RR.Utility;

    /// <summary>
    /// Merchant account properties of the payment connector.
    /// </summary>
    public static class NBEMerchantAccountProperty
    {

        /// <summary>
        /// ConnectorName property.
        /// </summary>

        public static string ConnectorName = LicenseProperties.ConnectorName;

        public const string Comport = "Comport";
        public const string Baudrate = "Baudrate";
        public const string parity = "parity";
        public const string Stopbit = "Stopbit";
        
        
        /// <summary>
        /// CardMap property.
        /// </summary>
        public const string CardMap = "CardMap";

        /// <summary>
        /// Assembly Version property.
        /// </summary>
        public const string AssemblyVersion = "AssemblyVersion";

        /// <summary>
        /// Assembly Copyright property.
        /// </summary>
        public const string AssemblyCopyright = "AssemblyCopyright";

        /// <summary>
        /// PaymentAccepTestUrl property.
        /// </summary>
        public const string PaymentAcceptWebUrl = "PaymentAcceptWebUrl";

        /// <summary>
        /// UniqueKey property.
        /// </summary>
        public const string UniqueKey = "UniqueKey";

        /// <summary>
        /// Assembly Version property.
        /// </summary>
        public const string FileVersion = "FileVersion";

        /// <summary>
        /// SerialNumber property
        /// </summary>
        public const string SerialNumber = "SerialNumber";
        /// <summary>
        /// TransactionMode property.
        /// </summary>
        public const string TransactionMode = "TransactionMode";
        /// <summary>
        /// Timeout property.
        /// </summary>
        public const string Timeout = "Timeout";


        private static ArrayList arrayList { get; } = new ArrayList()
        {
                MerchantAccountProperties.AssemblyName,
                MerchantAccountProperties.ServiceAccountId,
                ConnectorName,
               // ApiUrl,
               // UserId,
               //  Password,
               // TerminalId, 
               //  SaleDirectPhysicalProductId,
                //SaleDirectDigitalProductId,
                //RedeemDirectProductId,
                //StatusDirectProductId,
                MerchantAccountProperties.SupportedCurrencies,
                MerchantAccountProperties.SupportedTenderTypes,
                CardMap,
                AssemblyVersion,
                FileVersion,
                AssemblyCopyright,
                TransactionMode,
                SerialNumber,
                UniqueKey,
                PaymentAcceptWebUrl,
                Timeout
        };

        /// <summary>
        /// Gets the merchant account properties array.
        /// </summary>
        public static ArrayList ArrayList
        {
            get
            {
                return arrayList;
            }
        }

        public static string Parity { get; internal set; }
    }
}
