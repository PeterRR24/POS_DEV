using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Dynamics.Commerce.Runtime.Data;
using Microsoft.Dynamics.Commerce.Runtime;

namespace RR.Commerce.Runtime
{
    public class DatabaseQuery : DatabaseContext
    {
        public DatabaseQuery(RequestContext requestContext) : base(requestContext)
        {
        }

        public async Task<IDatabaseResult> Execute(IDatabaseQuery query)
        {
            return (await ExecuteQueryWithRetryAsync(query).ConfigureAwait(false)).Result;
        }
    }
}
