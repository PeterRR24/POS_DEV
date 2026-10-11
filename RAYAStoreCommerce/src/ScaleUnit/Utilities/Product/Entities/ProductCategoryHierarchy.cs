using System;
using System.Runtime.Serialization;
using System.ComponentModel.DataAnnotations; // Replace the existing namespace for Key attribute
using Microsoft.Dynamics.Commerce.Runtime.DataModel;
using Microsoft.Dynamics.Commerce.Runtime.ComponentModel.DataAnnotations;

namespace RR.Commerce.Runtime.Utilities
{
    public class ProductCategoryHierarchy : CommerceEntity
    {
        public ProductCategoryHierarchy() : base("ProductCategoryHierarchy")
        {
        }

        [System.ComponentModel.DataAnnotations.Key] // This is now from System.ComponentModel.DataAnnotations
        [DataMember]
        [Column("LEVEL")]
        public int Id { get { return (int)this["LEVEL"]; } }

        [DataMember]
        [Column("NAME")]
        public string Name { get { return (string)this["NAME"]; } }

        [DataMember]
        [Column("CODE")]
        public string Code { get { return (string)this["CODE"]; } }
    }
}
