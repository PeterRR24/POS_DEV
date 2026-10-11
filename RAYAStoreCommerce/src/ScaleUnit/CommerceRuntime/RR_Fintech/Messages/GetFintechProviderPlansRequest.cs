using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Commerce.Runtime.Messages;

namespace RR.CommerceRuntime.RR_Fintech.Messages
{


    /// <summary>
    [DataContract]
    public sealed class GetFintechProviderPlansRequest : Request
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GetStoreHoursDataRequest"/> class.
        public GetFintechProviderPlansRequest(int tenderTypeId)
        {
			this.TenderTypeId = tenderTypeId;
		}

		[DataMember]
		public int TenderTypeId { get; }
	}
}
