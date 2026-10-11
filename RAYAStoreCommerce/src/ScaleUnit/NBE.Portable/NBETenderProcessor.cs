
namespace RR.Dynamics.Retail.Payment.NBE.Portable
{
    using System;
    using Microsoft.Dynamics.Retail.PaymentSDK;

    /// <summary>
    /// Tender interface for the Sample payment processor.
    /// </summary>
    public class NBETenderProcessor : NBEProcessorIdentifier
    {
        /// <summary>
        /// Optional: Returns a tender for when one has been made available by the customer (using a customer facing device).
        /// </summary>
        /// <param name="hostSessionId">The host session id.</param>
        /// <returns>
        /// The tender information.
        /// </returns>
        //public ITenderInfo GetTender(object hostSessionId)
        //{
        //    throw new NotImplementedException();
        //}
    }
}

