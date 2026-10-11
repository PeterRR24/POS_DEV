using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

using Microsoft.Dynamics.Commerce.Runtime;

namespace RR.Commerce.Runtime.Utilities
{
    public class LicenseException : CommerceException
    {
        [DataMember]
        public string ErrorDetail { get; protected set; }

        public override string Message
        {
            get { return this.ErrorDetail; }
        }

        public LicenseException(string message) : base(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidOperation.ToString(), message)
        {
            this.ErrorDetail = message;
            this.LocalizedMessage = message;
        }
    }
}
