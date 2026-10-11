
namespace RR.Dynamics.Retail.Payment.NBE.Portable
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Dynamics.Retail.PaymentSDK.Portable;

    /// <summary>
    /// Sample exception class for input errors and payment errors.
    /// </summary>
    public class PaymentException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PaymentException"/> class.
        /// </summary>
        /// <param name="code">The error code.</param>
        /// <param name="message">The error message.</param>
        public PaymentException(ErrorCode code, string message)
        {
            this.Errors = new List<PaymentError>();
            this.Errors.Add(new PaymentError(code, message));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PaymentException"/> class.
        /// </summary>
        /// <param name="errors">The errors.</param>
        public PaymentException(List<PaymentError> errors)
        {
            this.Errors = errors;
        }

        /// <summary>
        /// Gets or sets the errors.
        /// </summary>
        public List<PaymentError> Errors { get; set; }
    }
}
