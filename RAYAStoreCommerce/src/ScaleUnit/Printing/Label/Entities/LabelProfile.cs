using System;
using System.Runtime.Serialization;
using SystemDataAnnotations = System.ComponentModel.DataAnnotations;

using Microsoft.Dynamics.Commerce.Runtime.DataModel;
using Microsoft.Dynamics.Commerce.Runtime.ComponentModel.DataAnnotations;

namespace RR.Commerce.Runtime.Printing
{
    public class LabelProfile : CommerceEntity
    {
        private const string IdColumn = "RECID";

        public LabelProfile() : base("LabelProfile")
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
        [Column("PROFILEID")]
        public string ProfileId
        {
            get { return (string)this["PROFILEID"]; }
            set { this["PROFILEID"] = value; }
        }

        [DataMember]
        [Column("NAME")]
        public string Name
        {
            get { return (string)this["NAME"]; }
            set { this["NAME"] = value; }
        }

        [DataMember]
        [Column("DEFAULTPRINTERRECID")]
        public long PrinterId
        {
            get { return (long)this["DEFAULTPRINTERRECID"]; }
            set { this["DEFAULTPRINTERRECID"] = value; }
        }
    }
}
