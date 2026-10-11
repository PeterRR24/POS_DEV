using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Dynamics.Commerce.Runtime;
using Microsoft.Dynamics.Commerce.Runtime.Data;
using Microsoft.Dynamics.Commerce.Runtime.DataModel;
using Microsoft.Dynamics.Retail.Diagnostics;
using Microsoft.Dynamics.Retail.Diagnostics.Extensions;
using RR.Retail.Security;

namespace RR.Commerce.Runtime.Utilities
{
    public static class Mobilityicense
    {
        private static int ProductType = 1;

        private static ProductDescriptor Descriptor()
        {
            return new ProductDescriptor("50025", "AX7m0b1l1ty", "RR mAX Mobility", "1.00", "Devices");
        }
        
        public static async Task<int> Validate(RequestContext context)
        {
            ThrowIf.Null<RequestContext>(context, "context");

            //LicenseDataManager dataManager = LicenseDataManager.Instantiate(context);
            //StoreProductLicense license = dataManager.GetCacheLicense(ProductType);
            LicenseDataManager dataManager = new LicenseDataManager(context);
            StoreProductLicense license = null;
            if (license == null)
            {
                license = new StoreProductLicense();

                string serialNumber = await dataManager.GetProductSerialNumber(ProductType).ConfigureAwait(false);
                RetailLogger.Log.LogInformation(EventOpCode.Info, "Got Serial Key for MOBILITY : {serialNumber}", serialNumber.AsSystemMetadata());
                if (!String.IsNullOrEmpty(serialNumber))
                {
                    string tenantId = await dataManager.GetSystemTenantId().ConfigureAwait(false);
                    RetailLogger.Log.LogInformation(EventOpCode.Info, "Got TenantID : {tenantId}", tenantId.AsSystemMetadata());
                    if (!String.IsNullOrEmpty(tenantId))
                    {
                        LicenseManager LM = new LicenseManager(tenantId);
                        ProductLicense productLicense = LM.Validate(Descriptor(), serialNumber);
                        if (productLicense != null)
                        {
                            RetailLogger.Log.LogInformation(EventOpCode.Info, "Got MOBILITY productLicense details");
                            license.Apply(productLicense);
                            //dataManager.PutCacheLicense(ProductType, license);
                        }   
                    }
                }
            }

            int seats = await dataManager.GetMobilityLicensedDeviceCount().ConfigureAwait(false);
            // -1 - invalid license.
            //  0 - license and devices fine.
            //  1 - license is fine, number of devices exceed licensed.
            int licenseStatus = license.Valid ? 0 : -1;
            if (seats > license.Seats)
                licenseStatus = 1;

            return licenseStatus;
        }

        public static async Task Check(RequestContext context)
        {
            if (await Validate(context).ConfigureAwait(false) == -1)
                throw new LicenseException("MAX Essentials Mobility is not licensed.");
        }
    }
}
