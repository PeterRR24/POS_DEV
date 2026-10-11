using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Runtime.Serialization;

using Microsoft.Dynamics.Commerce.Runtime;
using Microsoft.Dynamics.Commerce.Runtime.Messages;
using Microsoft.Dynamics.Commerce.Runtime.DataModel;

namespace RR.Commerce.Runtime.Mobility
{
    [DataContract]
    public sealed class MobilityConnectionRequest : Request
    {
        public MobilityConnectionRequest(string deviceId, string staffId, string password, Uri certificateLocation)
        {
            this.DeviceId = deviceId;
            this.StaffId = staffId;
            this.Password = password;
            this.CertificateLocation = certificateLocation;
        }

        [DataMember]
        public string DeviceId { get; }
        [DataMember]
        public string StaffId { get; }
        [DataMember]
        public string Password { get; }
        [DataMember]
        public Uri CertificateLocation { get; }
    }

    [DataContract]
    public sealed class MobilityAuthenticationRequest : Request
    {
        public MobilityAuthenticationRequest(string token, Uri certificateLocation)
        {
            this.Token = token;
            this.CertificateLocation = certificateLocation;
        }

        [DataMember]
        public string Token { get; }
        [DataMember]
        public Uri CertificateLocation { get; }
    }

    [DataContract]
    public sealed class CreateMobilityHardwareStationTokenServiceRequest : Request
    {
        public CreateMobilityHardwareStationTokenServiceRequest(Uri certificateLocation)
        {
            this.CertificateLocation = certificateLocation;
        }

        [DataMember]
        public Uri CertificateLocation { get; }
    }

    [DataContract]
    public sealed class ValidateMobilityDeviceDataRequest : Request
    {
        public ValidateMobilityDeviceDataRequest(string deviceNumber)
        {
            this.DeviceNumber = deviceNumber;
        }

        [DataMember]
        public string DeviceNumber { get; }
        
    }

    [DataContract]
    public sealed class MobilityParametersRequest : Request
    {
        public MobilityParametersRequest()
        {
        }
    }
}
