using System;
using System.Runtime.Serialization;
using SystemDataAnnotations = System.ComponentModel.DataAnnotations;

using Microsoft.Dynamics.Commerce.Runtime.DataModel;
using Microsoft.Dynamics.Commerce.Runtime.ComponentModel.DataAnnotations;

namespace RR.Commerce.Runtime.Printing
{
    public class LabelCode : CommerceEntity
    {
        private const string IdColumn = "RECID";

        public LabelCode() : base("LabelCode")
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
        [Column("CODEID")]
        public string Code
        {
            get { return (string)this["CODEID"]; }
            set { this["CODEID"] = value; }
        }

        [DataMember]
        [Column("DESCRIPTION")]
        public string Description
        {
            get { return (string)this["DESCRIPTION"]; }
            set { this["DESCRIPTION"] = value; }
        }
    }
}
