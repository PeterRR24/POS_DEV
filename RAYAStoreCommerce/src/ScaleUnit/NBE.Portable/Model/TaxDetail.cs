namespace RR.Dynamics.Retail.Payment.NBE.Portable
{
    using System;

    public class TaxDetail
    {
        public TaxDetail()
        {
        }

        public string TaxTypeIdentifier { get; set; }

        public decimal? TaxRate { get; set; }

        public string TaxDescription { get; set; }

        public decimal? TaxAmount { get; set; }
    }
}

