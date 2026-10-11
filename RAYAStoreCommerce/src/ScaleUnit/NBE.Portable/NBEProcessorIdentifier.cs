 
namespace RR.Dynamics.Retail.Payment.NBE.Portable
{
    using Microsoft.Dynamics.Retail.PaymentSDK.Portable;

    /// <summary>
    /// Identification information of the Sample payment processor.
    /// </summary>
    public class NBEProcessorIdentifier : IProcessorIdentifier
    {
        /// <summary>
        /// Gets the name of payment connector.
        /// </summary>
        public string Name
        {
            get { return "NBEConnector"; }
        }

        /// <summary>
        /// Gets the copyright text for payment connector.
        /// </summary>
        public string Copyright
        {
            get { return "Copyright © Retail Realm Distribution"; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [primary processor].  
        /// </summary>
        /// <remarks>In the case where the processor drives a customer facing device the primary processor should have priority 
        /// (over other processors).</remarks>
        /// <value>
        ///   <c>True</c> if [primary processor]; otherwise, <c>false</c>.
        /// </value>
        public bool PrimaryProcessor
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the supported country codes.
        /// Processor operates in the following country codes: ISO 3166-1 alpha-2.
        /// </summary>
        public ArrayList SupportedCountries
        {
            get
            {
                return new ArrayList(new string[] { "US" });
            }
        }

        public bool isCountrySupported(string locale)
        {
            System.Globalization.CultureInfo ci = new System.Globalization.CultureInfo(locale);
            //var ri = new System.Globalization.RegionInfo(c.Name);
            string countryCode = new System.Globalization.RegionInfo(ci.Name).TwoLetterISORegionName;
            return SupportedCountries.IndexOf(countryCode) > 0;
        }
    }
}

