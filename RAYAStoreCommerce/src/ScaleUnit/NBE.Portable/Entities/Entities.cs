namespace RR.Dynamics.Retail.Payment.NBE.Portable
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    [DataContract]
    internal class PaymentTransactionReferenceData
    {
        public PaymentTransactionReferenceData() { }

        [DataMember]
        public decimal Amount { get; set; }
        [DataMember]
        public string Command { get; set; }
        [DataMember]
        public string IdFromConnector { get; set; }
        [DataMember]
        public DateTime InitiatedDate { get; set; }
        [DataMember]
        public string UniqueTransactionId { get; set; }
    }

    [DataContract]
    internal class RRCart
    {
        [DataMember]
        public IList<RRCartLine> CartLines { get; set; }
        [DataMember]
        public decimal TaxAmount { get; set; }
        [DataMember]
        public decimal TotalAmount { get; set; }
    }

    [DataContract]
    internal class RRCartLine
    {
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public bool IsVoided { get; set; }
        [DataMember]
        public string ItemId { get; set; }
        [DataMember]
        public decimal Quantity { get; set; }
        [DataMember]
        public decimal Price { get; set; }
        [DataMember]
        public decimal LineDiscount { get; set; }
    }
}
