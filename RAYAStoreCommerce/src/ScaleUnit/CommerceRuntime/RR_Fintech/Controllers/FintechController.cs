using System;
using System.Collections.Generic;
using System.Text;
using RR.CommerceRuntime.RR_Fintech.Entities;
using RR.CommerceRuntime.RR_Fintech.Messages;
using Microsoft.Dynamics.Commerce.Runtime;
using Microsoft.Dynamics.Commerce.Runtime.DataModel;
using Microsoft.Dynamics.Commerce.Runtime.Messages;
using Microsoft.Dynamics.Commerce.Runtime.Hosting.Contracts;
using System.Threading.Tasks;

namespace RR.CommerceRuntime.RR_Fintech.Controllers
{
	[RoutePrefix("Fintech")]
	[BindEntity(typeof(Provider))]

	public class FintechController : IController
	{
		[HttpPost]
		[Authorization(CommerceRoles.Anonymous, CommerceRoles.Customer, CommerceRoles.Device, CommerceRoles.Employee)]
		public async Task<PagedResult<Provider>> GetFintechProviders(IEndpointContext context)
		{
			QueryResultSettings queryResultSettings = QueryResultSettings.AllRecords;
			GetFintechProvidersRequest request =
				new GetFintechProvidersRequest() { QueryResultSettings = queryResultSettings };

			GetFintechProvidersResponse response =
				await context.ExecuteAsync<GetFintechProvidersResponse>(
					request).ConfigureAwait(false);

			return response.Providers;
		}

		[HttpPost]
		[Authorization(CommerceRoles.Anonymous, CommerceRoles.Customer, CommerceRoles.Device, CommerceRoles.Employee)]
		public async Task<PagedResult<ProviderPlan>> GetFintechProviderPlans(
			int tenderTypeId, IEndpointContext context)
		{
			QueryResultSettings queryResultSettings = QueryResultSettings.AllRecords;
			GetFintechProviderPlansRequest request =
				new GetFintechProviderPlansRequest(
					tenderTypeId) { QueryResultSettings = queryResultSettings };

			GetFintechProviderPlansResponse response =
				await context.ExecuteAsync<GetFintechProviderPlansResponse>(
					request).ConfigureAwait(false);

			return response.Plans;
		}
	}
}
