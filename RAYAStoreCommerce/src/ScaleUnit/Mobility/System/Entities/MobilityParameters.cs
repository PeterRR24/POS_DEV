using System;
using System.Runtime.Serialization;

namespace RR.Commerce.Runtime.Mobility
{
    [DataContract]
    public class MobilityParameters
    {
        [DataMember]
        public int LicenseStatus { get; set; }
        [DataMember]
        public int RangingWithFaces { get; set; }

    }
}
