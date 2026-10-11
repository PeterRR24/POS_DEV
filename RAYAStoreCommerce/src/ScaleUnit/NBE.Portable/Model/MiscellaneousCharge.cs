namespace RR.Dynamics.Retail.Payment.NBE.Portable
{
    using System;

    public class MiscellaneousCharge
    {
        public MiscellaneousCharge()
        {
        }

        public string ChargeType { get; set; }

        public decimal? ChargeAmount { get; set; }
    }
}

