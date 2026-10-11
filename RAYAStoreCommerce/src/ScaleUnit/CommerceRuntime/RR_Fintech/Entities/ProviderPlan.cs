using Microsoft.Dynamics.Commerce.Runtime.DataModel;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using SystemDataAnnotations = System.ComponentModel.DataAnnotations;
using System.Text;
using Microsoft.Dynamics.Commerce.Runtime.ComponentModel.DataAnnotations;

namespace RR.CommerceRuntime.RR_Fintech.Entities
{
	public class ProviderPlan : CommerceEntity
	{
		private const string ProviderIdColumn = "PROVIDERID";
		private const string MonthsColumn = "MONTHS";
		private const string InterestPercentColumn = "INTERESTPERCENT";
		private const string MinimumFinanceColumn = "MINIMUMFINANCE";
		private const string MaximumFinanceColumn = "MAXIMUMFINANCE";
		private const string MinimumCashColumn = "MINIMUMCASH";

		public ProviderPlan()
			: base("ProviderPlan")
		{
		}

		[DataMember]
		[Column(ProviderIdColumn)]
		public string ProviderId
		{
			get { return (string)this[ProviderIdColumn]; }
			set { this[ProviderIdColumn] = value; }
		}

		[DataMember]
		[Column(MonthsColumn)]
		public int Months
		{
			get { return (int)this[MonthsColumn]; }
			set { this[MonthsColumn] = value; }
		}
		[DataMember]
		[Column(InterestPercentColumn)]
		public decimal InterestPercent
		{
			get { return (decimal)this[InterestPercentColumn]; }
			set { this[InterestPercentColumn] = value; }
		}
		[DataMember]
		[Column(MinimumFinanceColumn)]
		public decimal MinimumFinance
		{
			get { return (decimal)this[MinimumFinanceColumn]; }
			set { this[MinimumFinanceColumn] = value; }
		}
		[DataMember]
		[Column(MaximumFinanceColumn)]
		public decimal MaximumFinance
		{
			get { return (decimal)this[MaximumFinanceColumn]; }
			set { this[MaximumFinanceColumn] = value; }
		}
		[DataMember]
		[Column(MinimumCashColumn)]
		public decimal MinimumCash
		{
			get { return (decimal)this[MinimumCashColumn]; }
			set { this[MinimumCashColumn] = value; }
		}
	}
}
