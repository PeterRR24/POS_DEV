
using Microsoft.Dynamics.Commerce.Runtime;
using Microsoft.Dynamics.Commerce.Runtime.DataModel;
using Microsoft.Dynamics.Commerce.Runtime.Messages;
using Microsoft.Dynamics.Commerce.Runtime.Services.Messages;

using System;
using System.Globalization;
using System.Threading.Tasks;

namespace RR.Commerce.Runtime.Mobility
{
    public class ValidateHWSTokenService : SingleAsyncRequestHandler<ValidateHardwareStationTokenServiceRequest>
    {
        protected override async Task<Response> Process(ValidateHardwareStationTokenServiceRequest request)
        {
            ThrowIf.Null(request, "request");
            ThrowIf.NullOrWhiteSpace(request.HardwareStationToken, "request.HardwareStationToken");

            bool isMobility = request.HardwareStationToken.IndexOf(SystemService.Mobility) != -1;
            if (isMobility)
                return await Task.FromResult(ValidateMobilityHardwareStationToken(request)).ConfigureAwait(false);
            else
                return NotHandledResponse.Instance;    //return new NotHandledResponse();

        }

        private static ValidateHardwareStationTokenServiceResponse ValidateMobilityHardwareStationToken(ValidateHardwareStationTokenServiceRequest request)
        {
            string deviceNumber = string.Empty;
            string hardwareStationToken = request.HardwareStationToken;
            ValidateHardwareStationTokenResult validateHardwareStationTokenServiceResult = new ValidateHardwareStationTokenResult();
            long fileTime;
            try
            {
                hardwareStationToken = hardwareStationToken.Substring(0, hardwareStationToken.Length - (SystemService.Mobility.Length + 1)); // remove '-mobility', only base64 string allow for DecryptData
                //string[] strArray = SystemService.DecryptData(request.RequestContext, hardwareStationToken, request.CertificateLocation).Split(SystemService.Separator);
                byte[] base64EncodedBytes = Convert.FromBase64String(hardwareStationToken);
                string strHWSToken = System.Text.Encoding.UTF8.GetString(base64EncodedBytes, 0, base64EncodedBytes.Length);
                string[] strArray = strHWSToken.Split(SystemService.Separator);
                deviceNumber = strArray[0];
                validateHardwareStationTokenServiceResult.PairingKey = strArray[1];
                fileTime = long.Parse(strArray[2], CultureInfo.InvariantCulture);
            }
            catch (Exception ex)
            {
                throw new SecurityException(SecurityErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidHardwareStationToken, ex, "Hardware station token validation failed.");
            }
            if (!request.DeviceNumber.Equals(deviceNumber, StringComparison.OrdinalIgnoreCase) || DateTimeOffset.UtcNow.CompareTo(DateTimeOffset.FromFileTime(fileTime).AddMinutes(1.0)) > 0)
                throw new SecurityException(SecurityErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidHardwareStationToken);
            return new ValidateHardwareStationTokenServiceResponse(validateHardwareStationTokenServiceResult);
        }

    }
}
