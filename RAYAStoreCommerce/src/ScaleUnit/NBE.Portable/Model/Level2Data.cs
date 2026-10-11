namespace RR.Dynamics.Retail.Payment.NBE.Portable
{
    using System;
    using System.Collections.Generic;

    public class Level2Data
    {
        public Level2Data()
        {
        }

        public DateTime? OrderDateTime { get; set; }

        public string OrderNumber { get; set; }

        public DateTime? InvoiceDateTime { get; set; }

        public string InvoiceNumber { get; set; }

        public string OrderDescription { get; set; }

        public string SummaryCommodityCode { get; set; }

        public string MerchantContact { get; set; }

        public string MerchantTaxId { get; set; }

        public string MerchantType { get; set; }

        public string PurchaserId { get; set; }

        public string PurchaserTaxId { get; set; }

        public string ShipToCity { get; set; }

        public string ShipToCounty { get; set; }

        public string ShipToState_ProvinceCode { get; set; }

        public string ShipToPostalCode { get; set; }

        public string ShipToCountryCode { get; set; }

        public string ShipFromCity { get; set; }

        public string ShipFromCounty { get; set; }

        public string ShipFromState_ProvinceCode { get; set; }

        public string ShipFromPostalCode { get; set; }

        public string ShipFromCountryCode { get; set; }

        public decimal? DiscountAmount { get; set; }

        public decimal? MiscCharge { get; set; }

        public decimal? DutyAmount { get; set; }

        public decimal? FreightAmount { get; set; }

        public bool? IsTaxable { get; set; }

        public decimal? TotalTaxAmount { get; set; }

        public decimal? TotalTaxRate { get; set; }

        public string MerchantName { get; set; }

        public string MerchantStreet { get; set; }

        public string MerchantCity { get; set; }

        public string MerchantState { get; set; }

        public string MerchantCounty { get; set; }

        public string MerchantCountryCode { get; set; }

        public string MerchantZip { get; set; }

        public decimal? TaxRate { get; set; }

        public decimal? TaxAmount { get; set; }

        public string TaxDescription { get; set; }

        public string TaxTypeIdentifier { get; set; }

        public string RequesterName { get; set; }

        public decimal? TotalAmount { get; set; }

        public string PurchaseCardType { get; set; }

        public string AmexLegacyDescription1 { get; set; }

        public string AmexLegacyDescription2 { get; set; }

        public string AmexLegacyDescription3 { get; set; }

        public string AmexLegacyDescription4 { get; set; }

        public IEnumerable<TaxDetail> TaxDetails { get; set; }

        public IEnumerable<MiscellaneousCharge> MiscellaneousCharges { get; set; }
    }
}

