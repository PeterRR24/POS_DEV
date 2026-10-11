using System;
using System.Text;

using Microsoft.Dynamics.Commerce.Runtime;
using Microsoft.Dynamics.Commerce.Runtime.Data;
using Microsoft.Dynamics.Commerce.Runtime.DataModel;

using RR.Retail.Security;

namespace RR.Commerce.Runtime.Utilities
{
    /* - Obsolete
    public static class UtilitiesLicense
    {
        private static string Code = "mAXUtil";

        private static ProductDescriptor Descriptor()
        {
            return new ProductDescriptor("50000", "AX7mAXUtil", "RR mAX Utilities", "1.00");
        }

        public static bool Validate(RequestContext context)
        {
            ThrowIf.Null<RequestContext>(context, "context");

            long channelId = context.GetPrincipal().ChannelId;
            if (channelId == 0)
                return false;

            LicenseDataManager dataManager = LicenseDataManager.Instantiate(context);

            StoreProductLicense license = dataManager.GetCacheLicense(channelId, Code);
            if (license == null)
            {
                license = new StoreProductLicense();

                OrgUnit ou = context.GetOrgUnit();
                if (ou != null && !String.IsNullOrEmpty(ou.OrgUnitNumber))
                {
                    string serialNumber = dataManager.GetProductSerialNumber(channelId, Code);
                    if (!String.IsNullOrEmpty(serialNumber))
                    {
                        string tenantId = dataManager.GetSystemTenantId();
                        if (!String.IsNullOrEmpty(tenantId))
                        {
                            LicenseManager LM = new LicenseManager(tenantId, ou.OrgUnitNumber);
                            ProductLicense productLicense = LM.Validate(Descriptor(), serialNumber);
                            if (productLicense != null)
                                license.Apply(productLicense);
                        }
                    }

                }

                dataManager.PutCacheLicense(channelId, Code, license);
            }

            return license.Valid;
        }

        public static void Check(RequestContext context)
        {
            if (!Validate(context))
                throw new LicenseException("RR mAX Utilities not licensed.");
        }
    }
    */
}
