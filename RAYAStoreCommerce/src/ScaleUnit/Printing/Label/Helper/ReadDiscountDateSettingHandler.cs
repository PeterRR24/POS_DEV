
namespace RR.Commerce.Runtime.Printing
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Dynamics.Commerce.Runtime;
    using Microsoft.Dynamics.Commerce.Runtime.DataModel;
    using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
    using Microsoft.Dynamics.Commerce.Runtime.Messages;

    public class ReadDiscountDateSettingHandler : SingleAsyncRequestHandler<ReadRetailDiscountsDataRequest>
    {
        protected override async Task<Response> Process(ReadRetailDiscountsDataRequest request)
        {
            object salesDate = request.RequestContext.GetProperty("rr-SalesDate");
            if (salesDate != null)
            {
                DateTimeOffset? dateTime = salesDate as DateTimeOffset?;
                //dateTime.Value.
                var newRequest = new ReadRetailDiscountsDataRequest(
                    request.ItemUnits, request.PriceGroups,
                    // TODO: Change the datetime as needed.
                    //new DateTime(2022, 10, 01),
                    //new DateTime(2022, 10, 01),
                    dateTime.Value, dateTime.Value,
                    request.CurrencyCode)
                {
                    RequestContext = request.RequestContext,
                };
                var response = await request.RequestContext.Runtime.ExecuteNextAsync<EntityDataServiceResponse<PeriodicDiscount, ValidationPeriod>>(this, newRequest, request.RequestContext, skipRequestTriggers: false).ConfigureAwait(false);

                // TODO: Find the specific discount you'd like to make effective.
				DateTimeOffset fromDTO, toDTO, inputDTO = dateTime.Value;
                DateTime fromDate, toDate, inputDate = inputDTO.UtcDateTime;
                foreach (var periodicDiscount in response.EntityCollection)
                {
                    fromDTO = periodicDiscount.ValidFromDate;
                    toDTO = periodicDiscount.ValidToDate;
                    fromDate = fromDTO.UtcDateTime;
                    toDate = toDTO.UtcDateTime;
                    if (fromDate <= inputDate && inputDate.Date <= toDate)
                    {
                        // Update the validation period to include today.
                        periodicDiscount.ValidFromDate = DateTimeOffset.UtcNow.AddDays(-1);
                        periodicDiscount.ValidToDate = DateTimeOffset.UtcNow.AddDays(1);
                    }
                    else
                    {
                        // Update the validation period to future.
                        periodicDiscount.ValidFromDate = DateTimeOffset.UtcNow.AddDays(10);
                        periodicDiscount.ValidToDate = DateTimeOffset.UtcNow.AddDays(11);
                    }
                }
                return response;
            }
            else
            {
                //return new NotHandledResponse();
                return NotHandledResponse.Instance;
            }
        }
    }
}