namespace RR.Utility
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public static class Connectors
    {
        #region Constants
        public const string Shift4Connector = "Shift4Connector";
        public const string RRVerifoneConnector = "RRVerifoneConnector";
        public const string PceftConnector = "PceftConnector";
        public const string MercuryConnector = "MercuryConnector";
        public const string TyroConnector = "TyroConnector";
        public const string BanctekConnector = "BanctekConnector";
        public const string SynergyConnector = "SynergyConnector";
        public const string AusSecurePayConnector = "AusSecurePayConnector";
        public const string MexicoConnector = "MexicoConnector";
        public const string WorldPayConnector = "WorldPayConnector";
        public const string CayanConnector = "CayanConnector";
        public const string TranServicesConnector = "TranServicesConnector";
        public const string HeartlandConnector = "HeartlandConnector";
        public const string OptomanyConnector = "OptomanyConnector";
        public const string PayGuardianConnector = "PayGuardianConnector";
        public const string IndexConnector = "IndexConnector";
        public const string TriPOSConnector = "TriPOSConnector";
        public const string TriPOSCloudConnector = "TriPOSCloudConnector";
        public const string SterlingConnector = "SterlingConnector";
        public const string AdyenConnector = "AdyenConnector";
        public const string TDMSConnector = "TDMSConnector";
        public const string SoundPaymentConnector = "SoundPaymentConnector";
        public const string ExtendaConnector = "ExtendaConnector";
        public const string ElavonConnector = "ElavonConnector";
		public const string ElavonCanadaConnector = "ElavonCanadaConnector";
        public const string WestPacPaymentConnector = "WestPacPaymentConnector";
        public const string SmartPayCAConnector = "SmartPayCAConnector";
        public const string CardConnectConnector = "CardConnectConnector";
        public const string MercuryPaymentsEuropeConnector = "MercuryPaymentsEuropeConnector";
        public const string WorldpayUKConnector = "WorldpayUKConnector";
        public const string ValitorConnector = "ValitorConnector";
        public const string MonerisConnector = "MonerisConnector";
        public const string ZGivexConnector = "ZGivexConnector";
        public const string PaymentLogisticsConnector = "PaymentLogisticsConnector";
        public const string CloverConnector = "CloverConnector";
        public const string CybersourceConnector = "CybersourceConnector";
        public const string TenderRetailConnector = "TenderRetailConnector";
        public const string CitconConnector = "CitconConnector";
        public const string YuansferConnector = "YuansferConnector";
        public const string WorldNetConnector = "WorldNetConnector";
        public const string NetworkConnector = "NetworkConnector";
        public const string PepperConnector = "PepperConnector";
        public const string VerifoneSCAConnector = "VerifoneSCAConnector";
        public const string BraintreeConnector = "BraintreeConnector";
		public const string SVSConnector = "SVSConnector";
        public const string EmlPaymentsEuropeConnector = "EmlPaymentsEuropeConnector";
		public const string EpayConnector = "EpayConnector";
		public const string SIXPaymentsConnector = "SIXPaymentsConnector";
		public const string SynchronyConnector = "SynchronyConnector";
		public const string GlobalPaymentsUCPConnector = "GlobalPaymentsUCPConnector";
        public const string StripeConnector = "StripeConnector";
		public const string CybersourceBoletoConnector = "CybersourceBoletoConnector";
        public const string NBEConnector = "NBEConnector";
		public const string EPXConnector = "EPXConnector";
		public const string VerifonePSDKConnector = "VerifonePSDKConnector";
        public const string BlueSnapConnector = "BlueSnapConnector";
        public const string QNBConnector = "QNBConnector";
        public const string KlarnaConnector = "KlarnaConnector";
		public const string NMIConnector = "NMIConnector";
		public const string ElavonFuseboxConnector = "ElavonFuseboxConnector";
		public const string JDFConnector = "JDFConnector";
        public const string WorldlineConnector = "WorldlineConnector";	
		public const string AuthorizeNetConnector = "AuthorizeNetConnector";
        public const string FortisConnector = "FortisConnector";
		public const string CybersourceKCPConnector = "CybersourceKCPConnector";
        public const string SensePassConnector = "SensePassConnector";
		public const string AltapayConnector = "AltapayConnector";
        public const string ChaseConnector = "ChaseConnector";
		public const string AuthviaConnector = "AuthviaConnector";
		public const string NexoConnector = "NexoConnector";
		 public const string GlobalPaymentsCanadaConnector = "GlobalPaymentsCanadaConnector";
		 public const string ElavonUKConnector = "ElavonUKConnector";
		public const string WorldlineBeluxConnector = "WorldlineBeluxConnector";
		 public const string EftPosNZConnector = "EftPosNZConnector";
		public const string Factor4Connector = "Factor4Connector";
        public const string LinklyConnector = "LinklyConnector";
        public const string CitconCHOPConnector = "CitconCHOPConnector";
		public const string VerifoneEngageConnector = "VerifoneEngageConnector";
		 public const string PlanetConnector = "PlanetConnector";
		  public const string TDDeskConnector = "TDDeskConnector";
        public const string MonerisGOConnector = "MonerisGOConnector";
		public const string SkipifyConnector = "SkipifyConnector";
        public const string SnapPayConnector = "SnapPayConnector";
		public const string IngenicoUSConnector = "IngenicoUSConnector";
		  public const string AdyenNexoConnector = "AdyenNexoConnector";
        #endregion

        static List<KeyValuePair<int, string>> connectorList = new List<KeyValuePair<int, string>>()
        {
            new KeyValuePair<int, string>(1, "Shift4Connector"),
                    new KeyValuePair<int, string>(2, "RRVerifoneConnector"),
                    new KeyValuePair<int, string>(3, "PceftConnector"),
                    new KeyValuePair<int, string>(4, "MercuryConnector"),
                    new KeyValuePair<int, string>(5, "TyroConnector"),
                    new KeyValuePair<int, string>(6, "BanctekConnector"),
                    new KeyValuePair<int, string>(7, "SynergyConnector"),
                    new KeyValuePair<int, string>(8, "AusSecurePayConnector"),
                    new KeyValuePair<int, string>(9, "MexicoConnector"),
                    new KeyValuePair<int, string>(10, "WorldPayConnector"),
                    new KeyValuePair<int, string>(11, "CayanConnector"),
                    new KeyValuePair<int, string>(12, "TranServicesConnector"),
                    new KeyValuePair<int, string>(13, "HeartlandConnector"),
                    new KeyValuePair<int, string>(14, "OptomanyConnector"),
                    new KeyValuePair<int, string>(15, "PayGuardianConnector"),
                    new KeyValuePair<int, string>(16, "IndexConnector"),
                    new KeyValuePair<int, string>(17, "TriPOSConnector"),
                    new KeyValuePair<int, string>(18, "SterlingConnector"),
                    new KeyValuePair<int, string>(19, "AdyenConnector"),
                    new KeyValuePair<int, string>(20, "TDMSConnector"),
                    new KeyValuePair<int, string>(21, "SoundPaymentConnector"),
                    new KeyValuePair<int, string>(22, "ExtendaConnector"),
                    new KeyValuePair<int, string>(23, "ElavonConnector"),
                    new KeyValuePair<int, string>(24, "WestPacPaymentConnector"),
                    new KeyValuePair<int, string>(27, "SmartPayCAConnector"),
                    new KeyValuePair<int, string>(28,"CardConnectConnector"),
                    new KeyValuePair<int, string>(29, "MercuryPaymentsEuropeConnector"),
					new KeyValuePair<int, string>(30, "ElavonCanadaConnector"),
                    new KeyValuePair<int, string>(31, "WorldpayUKConnector"),
                    new KeyValuePair<int, string>(32, "NetworkConnector"),
                    new KeyValuePair<int, string>(33, "BlueSnapConnector"),
                    new KeyValuePair<int, string>(34, "MonerisConnector"),
                    new KeyValuePair<int, string>(35, "ValitorConnector"),
                    new KeyValuePair<int, string>(36, "ZGivexConnector"),
                    new KeyValuePair<int, string>(42, "PaymentLogisticsConnector"),
                    new KeyValuePair<int, string>(43, "CloverConnector"),
                    new KeyValuePair<int, string>(46, "CybersourceConnector"),
                    new KeyValuePair<int, string>(39, "TenderRetailConnector"),
					new KeyValuePair<int, string>(41, "AdyenNexoConnector"),
                    new KeyValuePair<int, string>(50, "CitconConnector"),
                    new KeyValuePair<int, string>(44, "YuansferConnector"),
                    new KeyValuePair<int, string>(37, "WorldNetConnector"),
                    new KeyValuePair<int, string>(51, "VerifoneSCAConnector"),
                    new KeyValuePair<int, string>(52, "PepperConnector"),
                    new KeyValuePair<int, string>(48, "BraintreeConnector"),
		            new KeyValuePair<int, string>(49, "SIXPaymentsConnector"),
					new KeyValuePair<int, string>(54, "SVSConnector"),
                    new KeyValuePair<int, string>(56, "EmlPaymentsEuropeConnector"),
					new KeyValuePair<int, string>(60, "EpayConnector"),
					new KeyValuePair<int, string>(61, "NMIConnector"),
					new KeyValuePair<int, string>(62, "SynchronyConnector"),
					new KeyValuePair<int, string>(58, "GlobalPaymentsUCPConnector"),
                    new KeyValuePair<int, string>(59, "StripeConnector"),
					new KeyValuePair<int, string>(65,"CybersourceBoletoConnector"),
                    new KeyValuePair<int, string>(66, "NBEConnector"),
					new KeyValuePair<int, string>(64, "EPXConnector"),
					new KeyValuePair<int, string>(63, "VerifonePSDKConnector"),
                    new KeyValuePair<int, string>(67, "QNBConnector"),
                    new KeyValuePair<int, string>(68, "KlarnaConnector"),
					new KeyValuePair<int, string>(69, "ElavonFuseboxConnector"),
					new KeyValuePair<int, string>(70, "JDFConnector"),
                    new KeyValuePair<int, string>(71, "WorldlineConnector"),
					new KeyValuePair<int, string>(72, "SkipifyConnector"),
					new KeyValuePair<int, string>(74, "AuthorizeNetConnector"),
                    new KeyValuePair<int, string>(75, "FortisConnector"),
					new KeyValuePair<int, string>(73, "CybersourceKCPConnector"),
                    new KeyValuePair<int, string>(77, "SensePassConnector"),
					  new KeyValuePair<int, string>(79, "PlanetConnector"),
                    new KeyValuePair<int, string>(80, "ChaseConnector"),
                    new KeyValuePair<int, string>(81, "AltapayConnector"),
					new KeyValuePair<int, string>(76, "AuthviaConnector"),
					new KeyValuePair<int, string>(78, "NexoConnector"),
					new KeyValuePair<int, string>(82, "ElavonUKConnector"),
					new KeyValuePair<int, string>(83, "GlobalPaymentsCanadaConnector"),
					new KeyValuePair<int, string>(84,"WorldlineBeluxConnector"),
					 new KeyValuePair<int, string>(86,"EftPosNZConnector"),
					new KeyValuePair<int, string>(87,"Factor4Connector"),
                     new KeyValuePair<int, string>(88, "LinklyConnector"),
                     new KeyValuePair<int, string>(89, "CitconCHOPConnector"),
					 new KeyValuePair<int, string>(90, "VerifoneEngageConnector"),
					  new KeyValuePair<int, string>(91, "TDDeskConnector"),
						new KeyValuePair<int, string>(94, "SnapPayConnector"),
						new KeyValuePair<int, string>(96, "IngenicoUSConnector"),
			            new KeyValuePair<int, string>(97, "MonerisGOConnector")



    };

        static List<KeyValuePair<string, string>> connectorPrefixList = new List<KeyValuePair<string, string>>()
        {
            new KeyValuePair<string, string>("Shift4Connector", "Shift4"),
                    new KeyValuePair<string, string>("RRVerifoneConnector", "RRVerifone"),
                    new KeyValuePair<string, string>("PceftConnector", "Pceft"),
                    new KeyValuePair<string, string>("MercuryConnector", "Mercury"),
                    new KeyValuePair<string, string>("TyroConnector", "Tyro"),
                    new KeyValuePair<string, string>("BanctekConnector", "Banctek"),
                    new KeyValuePair<string, string>("SynergyConnector", "Synergy"),
                    new KeyValuePair<string, string>("AusSecurePayConnector", "AusSecurePay"),
                    new KeyValuePair<string, string>("MexicoConnector", "Mexico"),
                    new KeyValuePair<string, string>("WorldPayConnector", "WorldPay"),
                    new KeyValuePair<string, string>("CayanConnector", "Cayan"),
                    new KeyValuePair<string, string>("TranServicesConnector", "TranServices"),
                    new KeyValuePair<string, string>("HeartlandConnector", "Heartland"),
                    new KeyValuePair<string, string>("OptomanyConnector", "Optomany"),
                    new KeyValuePair<string, string>("PayGuardianConnector", "PayGuardian"),
                    new KeyValuePair<string, string>("IndexConnector", "Index"),
                    new KeyValuePair<string, string>("TriPOSConnector", "TriPOS"),
                    new KeyValuePair<string, string>("SterlingConnector", "Sterling"),
                    new KeyValuePair<string, string>("AdyenConnector", "Adyen"),
                    new KeyValuePair<string, string>("TDMSConnector", "TDMS"),
                    new KeyValuePair<string, string>("SoundPaymentConnector", "SoundPayment"),
                    new KeyValuePair<string, string>("ExtendaConnector", "Extenda"),
                    new KeyValuePair<string, string>("ElavonConnector", "Elavon"),
					new KeyValuePair<string, string>("ElavonCanadaConnector", "ElavonCanada"),
                    new KeyValuePair<string, string>("WestPacPaymentConnector", "WestPacPayment"),
                    new KeyValuePair<string, string>("SmartPayCAConnector", "SmartPayCA"),
                    new KeyValuePair<string, string>("CardConnectConnector","CardConnect"),
                    new KeyValuePair<string, string>("MercuryPaymentsEuropeConnector", "MercuryPaymentsEurope"),
                    new KeyValuePair<string, string>("WorldpayUKConnector", "WorldpayUK"),
                    new KeyValuePair<string, string>("NetworkConnector", "Network"),
                    new KeyValuePair<string, string>("MonerisConnector", "Moneris"),
                    new KeyValuePair<string, string>("ValitorConnector", "Valitor"),
                    new KeyValuePair<string, string>("ZGivexConnector", "ZGivex"),
                    new KeyValuePair<string, string>("WorldNetConnector", "WNet"),
                    new KeyValuePair<string, string>("AdyenNexoConnector", "AdyenNexo"),
                    new KeyValuePair<string, string>("TenderRetailConnector", "TenderRetail"),
                    new KeyValuePair<string, string>("PaymentLogisticsConnector", "PaymentLogistics"),
                    new KeyValuePair<string, string>("CloverConnector", "Clover"),
                    new KeyValuePair<string, string>("YuansferConnector", "Yuansfer"),
                    new KeyValuePair<string, string>("CybersourceConnector", "Cybersource"),
                    new KeyValuePair<string, string>("PayWorksConnector", "PayWorks"),
                    new KeyValuePair<string, string>("BraintreeConnector", "Braintree"),
                    new KeyValuePair<string, string>("SIXPaymentsConnector", "SIXPayments"),
                    new KeyValuePair<string, string>("CitconConnector", "Citcon"),
                    new KeyValuePair<string, string>("VerifoneSCAConnector", "VerifoneSCA"),
                    new KeyValuePair<string, string>("BlueSnapConnector", "BlueSnap"),
                    new KeyValuePair<string, string>("PepperConnector", "Pepper"),
					new KeyValuePair<string, string>("SVSConnector", "SVS"),
                    new KeyValuePair<string, string>("EmlPaymentsEuropeConnector","EmlPaymentsEurope"),
					new KeyValuePair<string, string>("EpayConnector","Epay"),
					new KeyValuePair<string, string>("Synchrony", "Synchrony"),
					new KeyValuePair<string, string>("GlobalPaymentsUCPConnector","GlobalPaymentsUCP"),
                    new KeyValuePair<string, string>("StripeConnector","Stripe"),
					new KeyValuePair<string, string>("CybersourceBoletoConnector","CybersourceBoleto"),
                    new KeyValuePair<string, string>("NBEConnector","NBE"),
					new KeyValuePair<string, string>("EPXConnector","EPX"),
					new KeyValuePair<string, string>("VerifonePSDKConnector","VerifonePSDK"),
                    new KeyValuePair<string, string>("QNBConnector","QNB"),
                    new KeyValuePair<string, string>("KlarnaConnector", "Klarna"),
					new KeyValuePair<string, string>("NMIConnector","NMI"),
					new KeyValuePair<string, string>("ElavonFuseboxConnector","ElavonFusebox"),
					new KeyValuePair<string, string>("JDFConnector","JDF"),
                    new KeyValuePair<string, string>("WorldlineConnector", "Worldline"),
					new KeyValuePair<string, string>("AuthorizeNetConnector", "AuthorizeNet"),
                    new KeyValuePair<string, string>("FortisConnector", "Fortis"),
					new KeyValuePair<string, string>("CybersourceKCPConnector","CybersourceKCP"),
                    new KeyValuePair<string, string>("SensePassConnector","SensePass"),
					  new KeyValuePair<string, string>("PlanetConnector", "Planet"),
					new KeyValuePair<string, string>("AltapayConnector","Altapay"),
                    new KeyValuePair<string, string>("ChaseConnector","Chase"),
					new KeyValuePair<string, string>("AuthviaConnector","Authvia"),
					new KeyValuePair<string, string>("NexoConnector", "Nexo"),
					new KeyValuePair<string, string>("ElavonUKConnector","ElavonUK"),
					new KeyValuePair<string, string>("GlobalPaymentsCanadaConnector","GlobalPaymentsCanada"),
					new KeyValuePair<string, string>("WorldlineBeluxConnector","WorldlineBelux"),
					 new KeyValuePair<string, string>("EftPosNZConnector","EftPosNZ"),
					new KeyValuePair<string, string>("Factor4Connector","Factor4"),
                     new KeyValuePair<string, string>("LinklyConnector", "Linkly"),
                     new KeyValuePair<string, string>("CitconCHOPConnector", "CitconCHOP"),
					 new KeyValuePair<string, string>("VerifoneEngageConnector", "VerifoneEngage"),
					 	 new KeyValuePair<string, string>("TDDeskConnector", "TDDesk"),
            new KeyValuePair<string, string>("MonerisGOConnector", "MonerisGO"),
			 new KeyValuePair<string, string>("SkipifyConnector", "Skipify"),
              new KeyValuePair<string, string>("SnapPayConnector", "SnapPay"),
			   new KeyValuePair<string, string>("IngenicoUSConnector", "IngenicoUS")


    };

        public static bool CheckConnectorPresent(string connectorName, int processorId = 0)
        {
            bool result = false;
            // Processor Id,ConnectorName

            if (!string.IsNullOrEmpty(connectorName))
            {
                result = connectorList.Exists(c => c.Value == connectorName);
            }
            else if (processorId != 0)
            {
                result = connectorList.Exists(k => k.Key == processorId);
            }

            return result;
        }

        public static int GetProcessorType(string connectorName)
        {
            int processorType = 0;
            if (!string.IsNullOrEmpty(connectorName))
            {
                processorType = connectorList.FirstOrDefault(x => x.Value == connectorName).Key;
            }
            return processorType;
        }

        public static string GetConnectorName(string processorType)
        {
            string connectorName = string.Empty;
            if (!string.IsNullOrEmpty(processorType))
            {
                connectorName = connectorList.FirstOrDefault(x => x.Key == Convert.ToInt32(processorType)).Value;
            }
            return connectorName;
        }

        public static string GetConnectorPrefixName(string connectorName)
        {
            string connectorPrefix = string.Empty;
            if (!string.IsNullOrEmpty(connectorName))
            {
                connectorPrefix = connectorPrefixList.FirstOrDefault(x => x.Key == connectorName).Value;
            }
            return connectorPrefix;
        }
    }
}


