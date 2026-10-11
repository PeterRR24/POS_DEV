namespace RR.Dynamics.Retail.Payment.NBE.Portable
{
    using System;
    using System.Collections.Generic;

    public class Level3Data
    {
        public Level3Data()
        {
        }

        public string SequenceNumber { get; set; }

        public string CommodityCode { get; set; }

        public string ProductCode { get; set; }

        public string ProductName { get; set; }

        public string ProductSKU { get; set; }

        public string Descriptor { get; set; }

        public string UnitOfMeasure { get; set; }

        public decimal? UnitPrice { get; set; }

        public decimal? Discount { get; set; }

        public decimal? DiscountRate { get; set; }

        public decimal? Quantity { get; set; }

        public decimal? MiscCharge { get; set; }

        public decimal? NetTotal { get; set; }

        public decimal? TaxAmount { get; set; }

        public decimal? TaxRate { get; set; }

        public decimal? TotalAmount { get; set; }

        public string CostCenter { get; set; }

        public decimal? FreightAmount { get; set; }

        public decimal? HandlingAmount { get; set; }

        public string CarrierTrackingNumber { get; set; }

        public string MerchantTaxID { get; set; }

        public string MerchantCatalogNumber { get; set; }

        public string TaxCategoryApplied { get; set; }

        public string PickupAddress { get; set; }

        public string PickupCity { get; set; }

        public string PickupState { get; set; }

        public string PickupCounty { get; set; }

        public string PickupZip { get; set; }

        public string PickupCountry { get; set; }

        public DateTime? PickupDateTime { get; set; }

        public string PickupRecordNumber { get; set; }

        public string CarrierShipmentNumber { get; set; }

        public string UNSPSCCode { get; set; }

        public IEnumerable<TaxDetail> TaxDetails { get; set; }

        public IEnumerable<MiscellaneousCharge> MiscellaneousCharges { get; set; }
    }
}

