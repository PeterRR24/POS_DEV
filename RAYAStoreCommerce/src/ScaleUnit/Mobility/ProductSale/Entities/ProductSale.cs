using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Commerce.Runtime.ComponentModel.DataAnnotations;
using Microsoft.Dynamics.Commerce.Runtime.DataModel;
using SystemDataAnnotations = System.ComponentModel.DataAnnotations;

namespace RR.Commerce.Runtime.Mobility
{
    //[DataContract]
    public class ProductSale : CommerceEntity
    {

        private const string IdColumn = "BARCODEID";

        public ProductSale()
                : base("ProductSale")
        {
        }

        [SystemDataAnnotations.Key]
        [Key]
        [DataMember]
        [Column(IdColumn)]
        public string BarcodeId
        {
            get { return (string)this[IdColumn]; }
            set { this[IdColumn] = value; }
        }

        [DataMember]
        public string ItemId { get; set; }

        [IgnoreDataMember]
        public ItemType ItemType { get; set; }

        [DataMember]
        public int ItemTypeValue
        {
            get { return (int)this.ItemType; }
            set { this.ItemType = (ItemType)value; }
        }

        [DataMember]
        public string VariantId { get; set; }

        [DataMember]
        public string UnitId { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public string InventDimId { get; set; }

        [DataMember]
        public string ColorId { get; set; }

        [DataMember]
        public string SizeId { get; set; }

        [DataMember]
        public string StyleId { get; set; }

        [DataMember]
        public string ConfigId { get; set; }

        [DataMember]
        public string ColorName { get; set; }

        [DataMember]
        public string SizeName { get; set; }

        [DataMember]
        public string StyleName { get; set; }

        [DataMember]
        public string ConfigName { get; set; }

        [DataMember]
        public decimal Price { get; set; }

        [DataMember]
        public decimal DiscPrice { get; set; }

        [DataMember]
        public int AvailableQty { get; set; }

        [DataMember]
        public int Faces { get; set; }
    }
}
