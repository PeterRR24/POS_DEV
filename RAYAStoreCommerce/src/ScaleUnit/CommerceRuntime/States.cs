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
        public class States : CommerceEntity
        {
            private const string IdColumn = "RECID";

            private const string StateColumn = "NAME";

            public States()
                 : base("States")
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
            [Column(StateColumn)]
            public string State
            {
                get { return (string)this[StateColumn]; }
                set { this[StateColumn] = value; }
            }
        }
    }
}
 
