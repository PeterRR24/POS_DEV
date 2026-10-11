using System;
using System.Runtime.Serialization;
using SystemDataAnnotations = System.ComponentModel.DataAnnotations;
using Microsoft.Dynamics.Commerce.Runtime.DataModel;
using Microsoft.Dynamics.Commerce.Runtime.ComponentModel.DataAnnotations;

namespace RR.Commerce.Runtime.Utilities
{
    //[DataContract]
    public class StoreLocationItem : CommerceEntity
    {
        private const string IdColumn = "LOCATION";

        /// <summary>
        /// Initializes a new instance of the <see cref="StoreLocationItem"/> class.
        /// </summary>
        /// <param name="entityName">Entity name.</param>
        //public StoreLocationItem(string entityName)
        //    : base(entityName)
        //{
        //}
        /// <summary>
        /// Initializes a new instance of the <see cref="StoreLocationItem"/> class.
        /// </summary>
        public StoreLocationItem() : base("ItemStoreLocation")
        {
        }

        [SystemDataAnnotations.Key]
        [Key]
        [DataMember]
        [Column(IdColumn)]
        public int Location {
            get { return (int)this[IdColumn]; }
            set { this[IdColumn] = value; }
        }

        [DataMember]
        [Column("ITEMID")]
        public string ItemId
        {
            get { return (string)this["ITEMID"]; }
            set { this["ITEMID"] = value; }
        }

        [DataMember]
        [Column("INVENTDIMID")]
        public string InventDimId
        {
            get { return (string)this["INVENTDIMID"]; }
            set { this["INVENTDIMID"] = value; }
        }

        [DataMember]
        [Column("DESCRIPTION")]
        public string Description
        {
            get { return (string)this["DESCRIPTION"]; }
            set { this["DESCRIPTION"] = value; }
        }

        [DataMember]
        [Column("UNITID")]
        public string UnitId
        {
            get { return (string)this["UNITID"]; }
            set { this["UNITID"] = value; }
        }

        [DataMember]
        [Column("VARIANTID")]
        public string VariantId
        {
            get { return (string)this["VARIANTID"]; }
            set { this["VARIANTID"] = value; }
        }

        [DataMember]
        [Column("DIMCOLORID")]
        public string DimColorId
        {
            get { return (string)this["DIMCOLORID"]; }
            set { this["DIMCOLORID"] = value; }
        }

        [DataMember]
        [Column("DIMSIZEID")]
        public string DimSizeId
        {
            get { return (string)this["DIMSIZEID"]; }
            set { this["DIMSIZEID"] = value; }
        }

        [DataMember]
        [Column("DIMSTYLEID")]
        public string DimStyleId
        {
            get { return (string)this["DIMSTYLEID"]; }
            set { this["DIMSTYLEID"] = value; }
        }

        [DataMember]
        [Column("DIMCONFIGID")]
        public string DimConfigId
        {
            get { return (string)this["DIMCONFIGID"]; }
            set { this["DIMCONFIGID"] = value; }
        }

        [DataMember]
        [Column("FACINGCOUNT")]
        public int FacingCount
        {
            get { return (int)this["FACINGCOUNT"]; }
            set { this["FACINGCOUNT"] = value; }
        }

        [DataMember]
        [Column("SECTIONID")]
        public string SectionId
        {
            get { return (string)this["SECTIONID"]; }
            set { this["SECTIONID"] = value; }
        }

        [DataMember]
        [Column("SHELFID")]
        public string ShelfId
        {
            get { return (string)this["SHELFID"]; }
            set { this["SHELFID"] = value; }
        }
    }
}
