using System;

using Microsoft.Dynamics.Commerce.Runtime;

using RR.Retail.Security;

namespace RR.Commerce.Runtime.Utilities
{
    public class StoreProductLicense
    {
        private int _seats = 0;
        private byte _options = 0;
        private byte _version = 0;
        private bool _valid = false;

        public int Seats { get { return _seats; } }
        public byte Options { get { return _options; } }
        public byte Version { get { return _version; } }
        public bool Valid { get { return _valid; } }

        public void Apply(ProductLicense license)
        {
            ThrowIf.Null<ProductLicense>(license, "license");

            _valid = license.Valid;
            _seats = license.Seats;
            _options = license.Options;
            _version = license.Version;
        }
    }
}
