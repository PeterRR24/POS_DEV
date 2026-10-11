
using Microsoft.Dynamics.Commerce.Runtime;
using Microsoft.Dynamics.Commerce.Runtime.Data;
using Microsoft.Dynamics.Commerce.Runtime.DataModel;
using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
using Microsoft.Dynamics.Commerce.Runtime.Messages;
using Microsoft.Dynamics.Commerce.Runtime.Services.Messages;

using RR.Commerce.Runtime.Utilities;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace RR.Commerce.Runtime.Mobility
{
    public class SystemService : IRequestHandlerAsync
    {
        internal const char Separator = ':';
        internal const string Mobility = "Mobility";
        private const string EncryptKey = "0123456789ABCEIMNOPQRTUVWXYZsdfghjkl";

        public IEnumerable<Type> SupportedRequestTypes
        {
            get
            {
                return new[]
                {
                    typeof(MobilityConnectionRequest),
                    typeof(MobilityAuthenticationRequest),
                    typeof(CreateMobilityHardwareStationTokenServiceRequest),
                    //typeof(ValidateHardwareStationTokenServiceRequest),
                    typeof(MobilityParametersRequest)
                };
            }
        }

        public async Task<Response> Execute(Request request)
        {
            ThrowIf.Null(request, "request");

            Type requestType = request.GetType();
            Response response;

            if (requestType == typeof(MobilityConnectionRequest))
                response = await this.LogOn((MobilityConnectionRequest)request).ConfigureAwait(false);
            else if (requestType == typeof(MobilityAuthenticationRequest))
                response = await this.Authenticate((MobilityAuthenticationRequest)request).ConfigureAwait(false);
            else if (requestType == typeof(CreateMobilityHardwareStationTokenServiceRequest))
                response = this.CreateHardwareStationToken((CreateMobilityHardwareStationTokenServiceRequest)request);
            //else if (requestType == typeof(ValidateHardwareStationTokenServiceRequest))
            //    response = this.ValidateHardwareStationToken((ValidateHardwareStationTokenServiceRequest)request);
            else if(requestType == typeof(MobilityParametersRequest))
                response = await this.GetMobilityParameters((MobilityParametersRequest)request).ConfigureAwait(false);
            else
                throw new NotSupportedException(String.Format("Request '{0}' is not supported.", request.GetType()));

            return response;
        }

        private async Task<SingleEntityDataServiceResponse<string>> LogOn(MobilityConnectionRequest request)
        {
            ThrowIf.Null(request, "request");
            ThrowIf.NullOrWhiteSpace(request.DeviceId, "request.DeviceId");
            ThrowIf.NullOrWhiteSpace(request.StaffId, "request.StaffId");

            Employee employee = await EmployeeLogOnStore(request.RequestContext, request.StaffId, request.Password, request.RequestContext.GetPrincipal().ChannelId).ConfigureAwait(false);
            if (employee == null)
                throw new UserAuthenticationException(SecurityErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidPassword, "Incorrect user name or password.");
            //throw new UserAuthenticationException(SecurityErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidPassword, Microsoft.Dynamics.Commerce.Runtime.Framework.Exceptions.ExceptionSeverity.Warning, null, ResText.Text("ERR_INVALID_USER"))
            //{
            //    LocalizedMessage = ResText.Text("ERR_INVALID_USER"),
            //    LocalizedMessageParameters = new object[] { }
            //};

            ValidateMobilityDeviceDataRequest validateMobilityDeviceRequest = new ValidateMobilityDeviceDataRequest(request.DeviceId);
            bool isValid = (await request.RequestContext.ExecuteAsync<SingleEntityDataServiceResponse<bool>>(validateMobilityDeviceRequest).ConfigureAwait(false)).Entity;
            if (!isValid)
                throw new DeviceAuthenticationException(SecurityErrors.Microsoft_Dynamics_Commerce_Runtime_DeviceTokenValidationFailed, "Device not valid for mobility.");

            MobilityToken mtoken = new MobilityToken
            {
                DeviceId = request.DeviceId,
                UserId = employee.StaffId
            };

            string token = await CreateMobilityToken(request.RequestContext, request.CertificateLocation, mtoken, DateTime.UtcNow.AddDays(1)).ConfigureAwait(false);

            return new SingleEntityDataServiceResponse<string>(token);
        }

        private async Task<SingleEntityDataServiceResponse<MobilityToken>> Authenticate(MobilityAuthenticationRequest request)
        {
            ThrowIf.Null(request, "request");
            ThrowIf.NullOrWhiteSpace(request.Token, "request.Token");
            
            MobilityToken mtoken = await ParseMobilityToken(request.RequestContext, request.CertificateLocation, request.Token, DateTime.UtcNow).ConfigureAwait(false);

            SetMobilityPrincipal(mtoken, request.RequestContext);

            return new SingleEntityDataServiceResponse<MobilityToken>(mtoken);
        }
        
        //private static Uri GetCertificateLocation(RequestContext context)
        //{
        //    var realtimeService = context.Runtime.Configuration.RealtimeService;
        //    var cryptography = realtimeService.Certificate;
        //    Uri certificateLocation = new Uri("store:///" + (cryptography.StoreName ?? "My") + "/" + (cryptography.StoreLocation ?? "LocalMachine") + "?FindByThumbprint=" + cryptography.Thumbprint);
        //    return certificateLocation;
        //}

        private static void SetMobilityPrincipal(MobilityToken mToken, RequestContext context)
        {
            ICommercePrincipal principal = context.GetPrincipal();
            CommerceIdentity identity = principal.Identity as CommerceIdentity;
            identity.DeviceNumber = mToken.DeviceId;
            identity.UserId = mToken.UserId;
            identity.Roles.Add("MobilityRole");

            context.SetPrincipal(principal);
        }

        //private CreateHardwareStationTokenServiceResponse HardwareStationToken(CreateMobilityHardwareStationTokenServiceRequest request)
        //{
        //    ThrowIf.Null(request, "request");
        //    ThrowIf.Null(request.CertificateLocation, "request.CertificateLocation");

        //    return CreateHardwareStationToken(request);
        //}

        private async Task<SingleEntityDataServiceResponse<MobilityParameters>> GetMobilityParameters(MobilityParametersRequest request)
        {
            UtilitiesConfiguration utilitiesConfiguration = (await request.RequestContext.ExecuteAsync<SingleEntityDataServiceResponse<UtilitiesConfiguration>>(new GetUtilitiesConfigurationDataRequest()).ConfigureAwait(false)).Entity;
            MobilityParameters mobilityParams = new MobilityParameters
            {
                LicenseStatus = await Mobilityicense.Validate(request.RequestContext).ConfigureAwait(false),
                RangingWithFaces = utilitiesConfiguration.RangingWithFaces
            };
            return new SingleEntityDataServiceResponse<MobilityParameters>(mobilityParams);
        }

        private CreateHardwareStationTokenServiceResponse CreateHardwareStationToken(CreateMobilityHardwareStationTokenServiceRequest request)
        {
            ThrowIf.Null(request, "request");

            CreateHardwareStationTokenServiceResponse createHardwareStationTokenServiceResponse;
            string newPairingKey = Guid.NewGuid().ToString();
            string deviceNumber = request.RequestContext.GetPrincipal().DeviceNumber;
            string HWSToken = String.Join(
                Separator.ToString(),
                deviceNumber,
                newPairingKey,
                DateTimeOffset.UtcNow.ToFileTime());            
            try
            {
                CreateHardwareStationTokenResult createHardwareStationTokenResult = new CreateHardwareStationTokenResult();
                createHardwareStationTokenResult.PairingKey = newPairingKey;
                //Uri certificateLocation = request.CertificateLocation;                
                byte[] plainTextBytes = Encoding.UTF8.GetBytes(HWSToken);
                createHardwareStationTokenResult.HardwareStationToken = string.Format("{0}-{1}", Convert.ToBase64String(plainTextBytes), Mobility);
                createHardwareStationTokenServiceResponse = new CreateHardwareStationTokenServiceResponse(createHardwareStationTokenResult);
            }
            catch (Exception exception)
            {
                throw new SecurityException(SecurityErrors.Microsoft_Dynamics_Commerce_Runtime_HardwareStationTokenCreationFailed, exception, "Failed to create a hardware station token.");
            }
            return createHardwareStationTokenServiceResponse;
        }

        //private Response ValidateHardwareStationToken(ValidateHardwareStationTokenServiceRequest request)
        //{
        //    ThrowIf.Null(request, "request");
        //    ThrowIf.NullOrWhiteSpace(request.HardwareStationToken, "request.HardwareStationToken");
        //    bool isMobility = request.HardwareStationToken.IndexOf(Mobility) != -1;

        //    Response response;
        //    if (isMobility)
        //        response = ValidateMobilityHardwareStationToken(request);
        //    else
        //        response = new NotHandledResponse();
        //    return response;
        //}

        //private static ValidateHardwareStationTokenServiceResponse ValidateMobilityHardwareStationToken(ValidateHardwareStationTokenServiceRequest request)
        //{
        //    string deviceNumber = string.Empty;
        //    string hardwareStationToken = request.HardwareStationToken;
        //    ValidateHardwareStationTokenResult validateHardwareStationTokenServiceResult = new ValidateHardwareStationTokenResult();
        //    long fileTime;
        //    try
        //    {
        //        hardwareStationToken = hardwareStationToken.Substring(0, hardwareStationToken.Length - (Mobility.Length + 1)); // remove '-mobility', only base64 string allow for DecryptData
        //        string[] strArray = DecryptData(request.RequestContext, hardwareStationToken, request.CertificateLocation).Split(Separator);
        //        deviceNumber = strArray[0];
        //        validateHardwareStationTokenServiceResult.PairingKey = strArray[1];
        //        fileTime = long.Parse(strArray[2], CultureInfo.InvariantCulture);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new SecurityException(SecurityErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidHardwareStationToken, ex, "Hardware station token validation failed.");
        //    }
        //    if (!request.DeviceNumber.Equals(deviceNumber, StringComparison.OrdinalIgnoreCase) || DateTimeOffset.UtcNow.CompareTo(DateTimeOffset.FromFileTime(fileTime).AddMinutes(1.0)) > 0)
        //        throw new SecurityException(SecurityErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidHardwareStationToken);
        //    return new ValidateHardwareStationTokenServiceResponse(validateHardwareStationTokenServiceResult);
        //}

        //private static ValidateHardwareStationTokenServiceResponse ValidateMSHardwareStationToken(ValidateHardwareStationTokenServiceRequest request)
        //{
        //    return request.RequestContext.Runtime.ExecuteAsync<ValidateHardwareStationTokenServiceResponse>(request, request.RequestContext, skipRequestTriggers: false).GetAwaiter().GetResult();
        //}

        private static async Task<string> CreateMobilityToken(RequestContext context, Uri certificateLocation, MobilityToken mtoken, DateTime expiryDate)
        {
            string plainTextToken = String.Join(
                Separator.ToString(),
                mtoken.DeviceId,
                mtoken.UserId,
                expiryDate.ToFileTime());

            if (certificateLocation != null)
                return await EncryptData(context, plainTextToken, certificateLocation).ConfigureAwait(false);
            else
            {
                //byte[] plainTextBytes = Encoding.UTF8.GetBytes(token);
                //return Convert.ToBase64String(plainTextBytes);

                string encryptToken = string.Empty;
                byte[] buffer = Encoding.UTF8.GetBytes(plainTextToken);
#pragma warning disable CA2000 // Dispose objects before losing scope
                SHA512 hash = SHA512.Create();
#pragma warning restore CA2000 // Dispose objects before losing scope
                byte[] aesKey = new byte[24];
                Buffer.BlockCopy(hash.ComputeHash(Encoding.UTF8.GetBytes(EncryptKey)), 0, aesKey, 0, 24);

                using (Aes aes = Aes.Create())
                {
                    aes.Key = aesKey;
                    using (ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                    using (MemoryStream resultStream = new MemoryStream())
                    {
                        using (CryptoStream aesStream = new CryptoStream(resultStream, encryptor, CryptoStreamMode.Write))
                        using (MemoryStream plainStream = new MemoryStream(buffer))
                        {
                            await plainStream.CopyToAsync(aesStream).ConfigureAwait(false);
                        }
                        byte[] result = resultStream.ToArray();
                        byte[] combined = new byte[aes.IV.Length + result.Length];
                        Array.ConstrainedCopy(aes.IV, 0, combined, 0, aes.IV.Length);
                        Array.ConstrainedCopy(result, 0, combined, aes.IV.Length, result.Length);
                        encryptToken = Convert.ToBase64String(combined);
                    }
                }
                return encryptToken;
            }
                
        }

        private static async Task<MobilityToken> ParseMobilityToken(RequestContext context, Uri certificateLocation, string etoken, DateTime testDate)
        {
            DateTime? expiryDate = null;
            string[] fields = null;
            string token;
            bool tokenOk = false;

            try
            {
                if (certificateLocation != null)
                    token = await DecryptData(context, etoken, certificateLocation).ConfigureAwait(false);
                else
                {
                    //byte[] base64EncodedBytes = Convert.FromBase64String(etoken);
                    //token = Encoding.UTF8.GetString(base64EncodedBytes, 0, base64EncodedBytes.Length);

                    byte[] base64EncodedBytes = Convert.FromBase64String(etoken);
                    byte[] buffer = new byte[base64EncodedBytes.Length];
#pragma warning disable CA2000 // Dispose objects before losing scope
                    SHA512 hash = SHA512.Create();
#pragma warning restore CA2000 // Dispose objects before losing scope
                    byte[] aesKey = new byte[24];
                    Buffer.BlockCopy(hash.ComputeHash(Encoding.UTF8.GetBytes(EncryptKey)), 0, aesKey, 0, 24);
                    using (Aes aes = Aes.Create())
                    {
                        aes.Key = aesKey;
                        byte[] iv = new byte[aes.IV.Length];
                        byte[] ciphertext = new byte[buffer.Length - iv.Length];
                        Array.ConstrainedCopy(base64EncodedBytes, 0, iv, 0, iv.Length);
                        Array.ConstrainedCopy(base64EncodedBytes, iv.Length, ciphertext, 0, ciphertext.Length);
                        aes.IV = iv;

                        using (ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
                        using (MemoryStream resultStream = new MemoryStream())
                        {
                            using (var aesStream = new CryptoStream(resultStream, decryptor, CryptoStreamMode.Write))
                            using (var plainStream = new MemoryStream(ciphertext))
                            {
                                await plainStream.CopyToAsync(aesStream).ConfigureAwait(false);
                            }
                            token = Encoding.UTF8.GetString(resultStream.ToArray());
                        }
                    }
                }

                fields = token.Split(Separator);
                if (fields.Length == 3)
                {
                    long fileTime = Convert.ToInt64(fields[2], CultureInfo.InvariantCulture);
                    expiryDate = DateTime.FromFileTime(fileTime);
                    tokenOk = true;
                }
            }
            catch {}

            if (!tokenOk)
                throw new DeviceAuthenticationException(SecurityErrors.Microsoft_Dynamics_Commerce_Runtime_DeviceTokenValidationFailed, "Incorrect mobility token.");

            if (testDate.CompareTo(expiryDate.Value) > 0)
                throw new DeviceAuthenticationException(SecurityErrors.Microsoft_Dynamics_Commerce_Runtime_DeviceTokenExpired, "Mobility token expired.");

            MobilityToken mtoken = new MobilityToken
            {
                DeviceId = fields[0],
                UserId = fields[1]
            };

            return mtoken;
        }

        private static async Task<string> EncryptData(RequestContext context, string data, Uri certificateLocation)
        {
            CertificateEncryptionServiceRequest encryptionServiceRequest = new CertificateEncryptionServiceRequest(data, certificateLocation);
            return (await context.ExecuteAsync<CertificateEncryptionServiceResponse>(encryptionServiceRequest).ConfigureAwait(false)).Data;
        }

        internal static async Task<string> DecryptData(RequestContext context, string data, Uri certificateLocation)
        {
            CertificateDecryptionServiceRequest decryptionServiceRequest = new CertificateDecryptionServiceRequest(data, certificateLocation);
            return (await context.ExecuteAsync<CertificateDecryptionServiceResponse>(decryptionServiceRequest).ConfigureAwait(false)).Data;
        }

        private static async Task<Employee> EmployeeLogOnStore(RequestContext requestContext, string staffId, string password, long channelId)
        {
            GetEmployeePasswordCryptoInfoDataRequest cryptoInfoDataRequest = new GetEmployeePasswordCryptoInfoDataRequest(channelId, staffId);
            EmployeePasswordCryptoInfo cryptoInfo = (await requestContext.ExecuteAsync<SingleEntityDataServiceResponse<EmployeePasswordCryptoInfo>>(cryptoInfoDataRequest).ConfigureAwait(false)).Entity;
            if (cryptoInfo == null)
                throw new UserAuthenticationException(SecurityErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidAuthenticationCredentials, "Log on to the mobility failed due to incorrect Staff ID provided.");

            string passwordSalt = cryptoInfo.PasswordSalt;
            if (String.IsNullOrEmpty(passwordSalt))
                throw new UserAuthenticationException(SecurityErrors.Microsoft_Dynamics_Commerce_Runtime_CredentialsNotConfigured, "Log on to the mobility failed because the employee password is not configured.");

            string algorithm = cryptoInfo.PasswordHashAlgorithm;
            if (String.IsNullOrEmpty(algorithm))
                algorithm = requestContext.GetChannelConfiguration().StaffPasswordHash; // GetTransactionServiceProfile(requestContext).StaffPasswordHash;

            string legacyLocalContext = cryptoInfo.LegacyLocalContext ?? String.Empty;
            string legacyRegistryKeyData2Value = cryptoInfo.LegacyRegistryKeyData2Value ?? String.Empty;

            string passwordHash = String.Empty;
            if (!String.IsNullOrWhiteSpace(password))
                passwordHash = await HashPassword(requestContext, password, algorithm, staffId, passwordSalt, legacyLocalContext, legacyRegistryKeyData2Value).ConfigureAwait(false);

            EmployeeLogOnStoreDataRequest storeDataRequest = new EmployeeLogOnStoreDataRequest(channelId, staffId, passwordHash);
            return (await requestContext.ExecuteAsync<SingleEntityDataServiceResponse<Employee>>(storeDataRequest).ConfigureAwait(false)).Entity;
        }

        private static async Task<string> HashPassword(RequestContext context, string password, string algorithm, string invariant, string salt, string legacyLocalContext, string legacyRegistryKeyData2Value)
        {
            HashDataServiceRequest dataServiceRequest = new HashDataServiceRequest(password, algorithm, invariant, salt, legacyLocalContext, legacyRegistryKeyData2Value);
            return (await context.ExecuteAsync<HashDataServiceResponse>(dataServiceRequest).ConfigureAwait(false)).Data;
        }

        //private static TransactionServiceProfile GetTransactionServiceProfile(RequestContext context)
        //{
        //    GetTransactionServiceProfileDataRequest profileDataRequest = new GetTransactionServiceProfileDataRequest();
        //    return context.Runtime.ExecuteAsync<SingleEntityDataServiceResponse<TransactionServiceProfile>>(profileDataRequest, context).ConfigureAwait(false).GetAwaiter().GetResult().Entity;
        //}

        private string GenerateToken(MobilityToken mToken)
        {
            string deviceToken = string.Join(
                Separator.ToString(),
                mToken.DeviceId,
                mToken.UserId,
                DateTimeOffset.UtcNow.AddDays(1).ToFileTime());
            byte[] plainTextBytes = Encoding.UTF8.GetBytes(deviceToken);
            return Convert.ToBase64String(plainTextBytes);
        }

        private MobilityToken ParseToken(string base64Token)
        {
            string[] fields = null;
            bool tokenOk = false;
            DateTime? expiryDate = null;
            byte[] base64EncodedBytes = Convert.FromBase64String(base64Token);
            string strToken = Encoding.UTF8.GetString(base64EncodedBytes, 0, base64EncodedBytes.Length);
            if (!string.IsNullOrEmpty(strToken))
            {
                fields = strToken.Split(Separator);
                if (fields.Length == 3)
                {
                    long fileTime = Convert.ToInt64(fields[2], CultureInfo.InvariantCulture);
                    expiryDate = DateTime.FromFileTime(fileTime);
                    tokenOk = true;
                }
            }
            if (!tokenOk)
                throw new DeviceAuthenticationException(SecurityErrors.Microsoft_Dynamics_Commerce_Runtime_DeviceTokenValidationFailed, "Incorrect mobility token.");

            if (DateTime.UtcNow.CompareTo(expiryDate.Value) > 0)
                throw new DeviceAuthenticationException(SecurityErrors.Microsoft_Dynamics_Commerce_Runtime_DeviceTokenExpired, "Mobility token expired.");

            MobilityToken mtoken = new MobilityToken
            {
                DeviceId = fields[0],
                UserId = fields[1]
            };

            return mtoken;
        }

    }
}
