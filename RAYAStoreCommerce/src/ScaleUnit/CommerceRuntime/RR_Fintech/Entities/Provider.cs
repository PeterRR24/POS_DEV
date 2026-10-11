using Microsoft.Dynamics.Commerce.Runtime.DataModel;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using SystemDataAnnotations = System.ComponentModel.DataAnnotations;
using System.Text;
using Microsoft.Dynamics.Commerce.Runtime.ComponentModel.DataAnnotations;


namespace RR
{
    namespace CommerceRuntime.RR_Fintech.Entities
	{
        public class Provider : CommerceEntity
        {
            private const string TenderTypeIdColumn = "TENDERTYPEID";

            private const string NameColumn = "NAME";

            public Provider()
                 : base("Provider")
            {
            }


            [SystemDataAnnotations.Key]
            [Key]
            [DataMember]
            [Column(TenderTypeIdColumn)]
            public long TenderTypeId
			{
                get { return (long)this[TenderTypeIdColumn]; }
                set { this[TenderTypeIdColumn] = value; }
            }



            [DataMember]
            [Column(NameColumn)]
            public string Name
			{
                get { return (string)this[NameColumn]; }
                set { this[NameColumn] = value; }
            }
        }
    }
}
 
