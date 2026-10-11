using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;

using Microsoft.Dynamics.Commerce.Runtime;
//using Microsoft.Dynamics.Commerce.Runtime.Client;
using Microsoft.Dynamics.Commerce.Runtime.DataModel;
using Microsoft.Dynamics.Commerce.Runtime.Messages;
using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;

namespace RR.Commerce.Runtime.Mobility
{
    public sealed class SystemManager
    {
        private readonly ICommerceRuntime runtime;

        private SystemManager(ICommerceRuntime runtime)
        {
            this.runtime = runtime;
        }

        public static SystemManager Create(ICommerceRuntime runtime)
        {
            if (runtime == null)
                throw new ArgumentNullException("runtime");
            return new SystemManager(runtime);
        }

        internal TResponse Execute<TResponse>(Request request) where TResponse : Response
        {
#pragma warning disable CS0618 // Type or member is obsolete
#pragma warning disable AvoidBlockingCallsAnalyzer // Avoid executing requests synchronously.
            return this.runtime.Execute<TResponse>(request, null);
#pragma warning restore AvoidBlockingCallsAnalyzer // Avoid executing requests synchronously.
#pragma warning restore CS0618 // Type or member is obsolete
        }

        //

        public string LogOn(string deviceId, string staffId, string password, Uri certificateLocation)
        {
            ThrowIf.NullOrWhiteSpace(deviceId, "deviceId");
            ThrowIf.NullOrWhiteSpace(staffId, "staffId");
            return Execute<SingleEntityDataServiceResponse<string>>(new MobilityConnectionRequest(deviceId, staffId, password, certificateLocation)).Entity;
        }

        public MobilityToken Authenticate(string token, Uri certificateLocation)
        {
            ThrowIf.NullOrWhiteSpace(token, "token");
            return Execute<SingleEntityDataServiceResponse<MobilityToken>>(new MobilityAuthenticationRequest(token, certificateLocation)).Entity;
        }

        public MobilityParameters GetParameters()
        {
            return Execute<SingleEntityDataServiceResponse<MobilityParameters>>(new MobilityParametersRequest()).Entity;
        }
    }
}
