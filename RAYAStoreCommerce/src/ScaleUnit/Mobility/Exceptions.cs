using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

using Microsoft.Dynamics.Commerce.Runtime;
using Microsoft.Dynamics.Commerce.Runtime.Framework.Exceptions;

namespace RR.Commerce.Runtime.Mobility
{
    public class NotFoundException : CommerceException
    {

        [DataMember]
        public string ErrorDetail { get; protected set; }

        public override string Message
        {
            get { return this.ErrorDetail; }
        }

        public NotFoundException(): this(null)
        {
        }

        public NotFoundException(string message) : base(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_ObjectNotFound.ToString(), message)
        {
            this.ErrorDetail = message;
        }
    }

    public class ValidationException : CommerceException
    {

        [DataMember]
        public string ErrorDetail { get; protected set; }

        public override string Message
        {
            get { return this.ErrorDetail; }
        }

        public ValidationException(DataValidationErrors error) : this(error, null)
        {
        }

        public ValidationException(DataValidationErrors error, string message) : base(error.ToString(), message)
        {
            this.ErrorDetail = message;
        }
    }
}
