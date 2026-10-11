using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.Dynamics.Commerce.Runtime;
using Microsoft.Dynamics.Commerce.Runtime.Data;
using Microsoft.Dynamics.Commerce.Runtime.DataModel;
using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
using Microsoft.Dynamics.Commerce.Runtime.Messages;
using Microsoft.Dynamics.Commerce.Runtime.RealtimeServices.Messages;
using Microsoft.Dynamics.Commerce.Runtime.Services.Messages;
using RR.CommerceRuntime.RR_Fintech.Messages;
using RR.CommerceRuntime.RR_Fintech.Entities;
namespace RR
{

    namespace CommerceRuntime.RR_Fintech.RequestHandlers

	{



        public class ProviderRequestService : IRequestHandlerAsync
        {
            public IEnumerable<Type> SupportedRequestTypes
            {
                get
                {
                    return new[]
                    {
                        typeof(GetFintechProvidersRequest),
						typeof(GetFintechProviderPlansRequest)
					};
                }
            }

            public async Task<Response> Execute(Request request)
            {
                if (request == null)
                {
                    throw new ArgumentNullException("request");
                }

                Type requestedType = request.GetType();
                if (requestedType == typeof(GetFintechProvidersRequest))
                {
                    return await this.GetAllProvidersAsync((GetFintechProvidersRequest)request).ConfigureAwait(false);
                }

				if (requestedType == typeof(GetFintechProviderPlansRequest))
				{
					return await this.GetProviderPlansAsync((GetFintechProviderPlansRequest)request).ConfigureAwait(false);
				}

				throw new NotSupportedException(string.Format("Request '{0}' is not supported.", request.GetType()));
            }
			private async Task<Response> GetAllProvidersAsync(GetFintechProvidersRequest request)
			{
				ThrowIf.Null(request, nameof(request));

				using (DatabaseContext databaseContext =
					new DatabaseContext(request.RequestContext))
				{
					var query = new SqlPagedQuery(request.QueryResultSettings)
					{
						DatabaseSchema = "ext",

						Select = new ColumnSet(
							"TENDERTYPEID",
							"NAME"
						),

						From = "RR_FINTECHPROVIDERVIEW",

						OrderBy = "NAME"
					};

					var providers =
						await databaseContext
							.ReadEntityAsync<Provider>(query)
							.ConfigureAwait(false);

					return new Messages.GetFintechProvidersResponse(providers);
				}
			}

			private async Task<Response> GetProviderPlansAsync(GetFintechProviderPlansRequest request)
			{
				ThrowIf.Null(request, "request");

				using (DatabaseContext databaseContext = new DatabaseContext(request.RequestContext))
				{
					var query = new SqlPagedQuery(request.QueryResultSettings)
					{
						DatabaseSchema = "ax",
						Select = new ColumnSet("TENDERTYPEID", "MONTHS", "INTERESTPERCENT", "MINIMUMFINANCE", "MAXIMUMFINANCE", "MINIMUMCASH"),
						From = "RR_FINTECHPROVIDERPLAN",
						Where = "TENDERTYPEID = @tenderTypeId AND ISACTIVE = 1",
						OrderBy = "MONTHS"
					};

					query.Parameters["@tenderTypeId"] = request.TenderTypeId;

					return new Messages.GetFintechProviderPlansResponse(await databaseContext.ReadEntityAsync<ProviderPlan>(query).ConfigureAwait(false));
				}
			}

		}
    }
}
