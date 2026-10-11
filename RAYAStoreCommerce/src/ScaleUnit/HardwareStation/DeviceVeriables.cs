using System;
using System.Collections.Generic;
using System.Text;

namespace Contoso.Commerce.HardwareStation.Extension.WindowsPrinterSample
{
    public static class DeviceStaticVars
    {
        internal static bool IsDeviceRegistered { get; set; }
        internal static string PreviousCounter { get; set; }
        internal static string Basecounter { get; set; }

    }

    public class DeviceInstanceVars
    {
        internal Microsoft.Dynamics.Retail.PaymentSDK.Portable.PaymentProperty[] MerchantProperties { get; set; }
        internal Microsoft.Dynamics.Retail.PaymentSDK.Portable.PaymentProperty[] AllMerchantProperties { get; set; }
        internal Microsoft.Dynamics.Commerce.HardwareStation.CardPayment.SettingsInfo TerminalSettings { get; set; }
        internal KeyValuePair<string, string> KeySettings { get; set; }
        internal string[] PaymentProcessors { get; set; }
        internal string PaymentConnectorName { get; set; }

        internal string eventTypeName { get; set; }
        internal bool IsTestMode { get; set; }
        internal string RRCartId { get; set; }
        internal string CartData { get; set; }
        internal string Locale { get; set; }
        internal decimal TotalAmount { get; set; }
        internal bool IsSwipe { get; set; }
        internal bool IsSignatureCapture { get; set; }
        internal bool IsGetSignatureCall { get; set; }
        internal string ConnectorTransId { get; set; }
        internal string AssemblyPosId { get; set; }
        internal string RRTerminalId { get; set; }
        internal string RRStaffId { get; set; }
        internal string Basecounter { get; set; }
        internal string IsLogEnable { get; set; }
        internal string LogAppInsightsKey { get; set; }
        internal string EnableLineDisplay { get; set; }
        internal string FileV { get; set; }
        internal string DeviceSerialNumber { get; set; }
        
        internal string IsPinPadSupportSign { get; set; }
        internal string IsPinPadSupportDebitTransaction { get; set; }
        internal string ByPassPinForDebitTransactions { get; set; }
        internal bool IsGetMerchantProp { get; set; }
        
        internal string TranIdToCheckCancel { get; set; }
        internal string TerminalId { get; set; }
       
        internal bool IsDevicePair { get; set; }
       
        internal string EnableMinimalReceiptData { get; set; } // Shift4
		internal string IsPaygistixApp { get; set; }
        internal string StoreNumber { get; set; }
        internal string CartCurrencyCode { get; set; }
		 internal string PCEFTCloudUserName { get; set; }
		 internal string PCEFTCloudPassward { get; set; }
		 internal string PCEFTCloudPairCode { get; set; }
        public DeviceInstanceVars()
        {
            //MerchantProperties = new Microsoft.Dynamics.Retail.PaymentSDK.Portable.PaymentProperty[] { };
            //AllMerchantProperties = new Microsoft.Dynamics.Retail.PaymentSDK.Portable.PaymentProperty[] { };
            //KeySettings = new KeyValuePair<string, string>();
        }
    }

    public class DeviceData
    {
        public string MacLabel { get; set; }
        public byte[] MackKey { get; set; }

    }

    public class CartLine
    {
        public decimal LineNumber { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal ExtendedPrice { get; set; }
        public decimal Price { get; set; }
        public decimal Quantity { get; set; }
        public string Description { get; set; }
        public string ItemId { get; set; }
        public string LineId { get; set; }
        public decimal LineDiscount { get; set; }
        public bool IsVoided { get; set; }
        
    }

    public class Cart
    {
        public IList<CartLine> CartLines { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal SubtotalAmountWithoutTax { get; set; }
        public decimal TotalAmount { get; set; }
        public Cart()
        {
            CartLines = new List<CartLine>();
            DiscountAmount = 0m;
            TaxAmount = 0m;
            SubtotalAmountWithoutTax = 0m;
            TotalAmount = 0m;
        }
    }

}
