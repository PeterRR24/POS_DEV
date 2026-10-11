using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using System.Collections.ObjectModel;

using RR.CommerceRuntime.RR_Fintech.Entities;

using Microsoft.Dynamics.Commerce.Runtime;
using Microsoft.Dynamics.Commerce.Runtime.Messages;


namespace RR.CommerceRuntime.RR_Fintech.Messages
{

    [DataContract]
    public sealed class GetFintechProvidersResponse : Response
    {

        public GetFintechProvidersResponse(PagedResult<Provider> providers)
        {
            this.Providers = providers;
        }

		/// <summary>
		/// Gets the found store hours as a paged result set.
		/// </summary>
		// public PagedResult<StoreDayHours> DayHours { get; private set; }
		[DataMember]
		public PagedResult<Provider> Providers { get; }

    }

}
