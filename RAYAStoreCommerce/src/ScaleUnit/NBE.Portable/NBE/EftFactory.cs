
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using RR.Dynamics.Retail.Payment.NBE;
using RR.Dynamics.Retail.Payment.NBE.Contract;

namespace RR.Dynamics.Retail.Payment.NBE
{
    public static class EftFactory
    {
        public static IEftProcessor Create(EftService service)

        {
            if (service == null) throw new ArgumentNullException("service");
            switch (service.ServiceType)
            {
                case EftProcessorType.NBE:
                    return new NBEEft(service);


                default:
                    return null;
            }
        }

        public static EftProcessorDescriptor CreateDescriptor(EftProcessorType serviceType)
        {
            switch (serviceType)
            {
                case EftProcessorType.NBE:
                    return new NBEDescriptor();
                default:
                    return null;
            }
        }

    }
}
