using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RR.Dynamics.Retail.Payment.NBE.Contract
{
    public enum EftOperationType
    {
        None = 0,
        Sale = 2,
        Refund = 4,
        Void = 5,
        // Gift Card Transactions (common in all providers)
        Activate = 10,  //Activate or reload balance
        BalanceEnquiry = 12,
        Recharge = 18,
        Cancel=19,
        DuplicateProtection=20,
        TestCommunication=21
    }
    public enum EftSecurityMode
    {
        None = 0,   // credit card
        Token = 1,  // security token 
        P2PE = 2,    // P2PE
        Refund = 3  //refund using AuthOrderID & AuthTransactionId of sale 
    }

    public enum EftCardEntryMode
    {
        System = 0,         // card not present
        Manual = 1,         // card present
        MagneticStripe = 2, // card present
    }
    
    public enum EftCardType
    {
        CreditCard = 0,
        DebitCard = 1,
        Interac = 2,
        Unknown = 3,
        GiftCard = 4
    }
    
    public enum EftRequestSource
    {
        Eft = 0,
        GiftCard = 1,
        EBTCash = 2,
        EBTfoodStamp = 3,
        Check = 4,
        Loyalty = 5,
        FSA = 6,
        LineDisplay = 7
    }

    // non mutable EFT request
    public class EftRequest
    {
        public EftRequest() { }
        public string TransactionId { get; set; }
        
        public decimal Amount { get; set; }

        public EftOperationType Operation { get; set; }
        public EftRequestSource Source { get; set; }
        public string AuthCode { get; set; }
        public string AuthTransId { get; set; }
        public string StaffId { get; set; }
        public EftCardType CardType { get; set; }

    }
}
