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
    public static class LabelLicense
    {
        private static readonly int ProductType = 2;

        private static ProductDescriptor Descriptor()
        {
            return new ProductDescriptor("50040", "AX7PlAabe1s", "RR mAX Labels", "1.00", "Lines");
        }

        public static async Task<bool> Validate(RequestContext context)
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
                RetailLogger.Log.LogInformation(EventOpCode.Info, "Got Serial Key for LABEL : {serialNumber}", serialNumber.AsSystemMetadata());
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
                            RetailLogger.Log.LogInformation(EventOpCode.Info, "Got LABEL productLicense details");
                            license.Apply(productLicense);
                            //dataManager.PutCacheLicense(ProductType, license);
                        }
                    }
                }
            }
            bool status = license.Valid;
            RetailLogger.Log.LogInformation(EventOpCode.Info, "Label license validation status is : {status}", status.AsSystemMetadata());
            return license.Valid;
        }

        public static async Task Check(RequestContext context)
        {
            if (!await Validate(context).ConfigureAwait(false))
                throw new LicenseException("MAX Essentials Label printing is not licensed.");
        }
    }
}
