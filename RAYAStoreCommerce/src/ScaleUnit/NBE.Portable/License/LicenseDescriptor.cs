using System;
using System.Text;

using RR.Retail.Security;

namespace RR.Dynamics.Retail.Payment.Givex.Portable
{

    internal class LicenseDescriptor
    {
        private static ProductDescriptor PD;

        public static ProductDescriptor Product
        {
            get
            {
                if (PD == null)
                    PD = new ProductDescriptor("50036", "AX7PCGIVEX", "AX7 Givex Payment Connector", "1.00");
                return PD;
            }
        }

        public static string ToText(ProductLicense license)
        {
            StringBuilder sb = new StringBuilder();
            if (license != null)
            {
                if (license.Seats > 0)
                    sb.AppendFormat("{0} Users", license.Seats);
                if (license.ExpireDate.HasValue)
                {
                    if (sb.Length > 0)
                        sb.Append(", ");
                    sb.AppendFormat("Expires on {0}", license.ExpireDate.Value);
                }

            }
            return sb.ToString();
        }

    }
}
