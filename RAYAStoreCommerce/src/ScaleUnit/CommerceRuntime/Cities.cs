using Microsoft.Dynamics.Commerce.Runtime.DataModel;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using SystemDataAnnotations = System.ComponentModel.DataAnnotations;
using System.Text;
using Microsoft.Dynamics.Commerce.Runtime.ComponentModel.DataAnnotations;


namespace Contoso
{
    namespace Commerce.Runtime.DataModel
    {
        public class Cities : CommerceEntity
        {
            private const string IdColumn = "RECID";

            private const string CityColumn = "DESCRIPTION";

            public Cities()
                 : base("Cities")
            {
            }


            [SystemDataAnnotations.Key]
            [Key]
            [DataMember]
            [Column(IdColumn)]
            public long Id
            {
                get { return (long)this[IdColumn]; }
                set { this[IdColumn] = value; }
            }



            [DataMember]
            [Column(CityColumn)]
            public string City
            {
                get { return (string)this[CityColumn]; }
                set { this[CityColumn] = value; }
            }
        }
    }
}
 
