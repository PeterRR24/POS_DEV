using System;
using System.Runtime.Serialization;
using SystemDataAnnotations = System.ComponentModel.DataAnnotations;

using Microsoft.Dynamics.Commerce.Runtime.DataModel;
using Microsoft.Dynamics.Commerce.Runtime.ComponentModel.DataAnnotations;

namespace RR.Commerce.Runtime.Utilities
{
    public class StoreSection : CommerceEntity
    {
        private const string IdColumn = "RECID";

        public StoreSection() : base("StoreSection")
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
        [Column("SECTIONID")]
        public string SectionId
        {
            get { return (string)this["SECTIONID"]; }
            set { this["SECTIONID"] = value; }
        }

        [DataMember]
        [Column("NAME")]
        public string SectionName
        {
            get { return (string)(this["NAME"] ?? String.Empty); }
            set { this["NAME"] = value; }
        }
    }
}
