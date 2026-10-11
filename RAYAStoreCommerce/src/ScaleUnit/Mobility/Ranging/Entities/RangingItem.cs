using System.Runtime.Serialization;
using SystemDataAnnotations = System.ComponentModel.DataAnnotations;

using Microsoft.Dynamics.Commerce.Runtime.DataModel;
using Microsoft.Dynamics.Commerce.Runtime.ComponentModel.DataAnnotations;
using System;

namespace RR.Commerce.Runtime.Mobility
{
    [DataContract]
    public class RangingItem1
    {
        [DataMember]
        public int Id { get; set; }
        [DataMember]
        public string ItemId { get; set; }
        [DataMember]
        public string ItemName { get; set; }
        [DataMember]
        public int FacingCount { get; set; }
    }

    public class RangingItem : CommerceEntity
    {
        private const string IdColumn = "ID";

        public RangingItem() : base("ItemRanging")
        {
        }
        [SystemDataAnnotations.Key]

        [Key]
        [DataMember]
        [Column(IdColumn)]
        public int Id
        {
            get { return Convert.ToInt32(this[IdColumn]); }
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
        [Column("ITEMNAME")]
        public string ItemName
        {
            get { return (string)this["ITEMNAME"]; }
            set { this["ITEMNAME"] = value; }
        }
        
        [DataMember]
        [Column("FACES")]
        public int Faces
        {
            get { return Convert.ToInt32(this["FACES"]); }
            set { this["FACES"] = value; }
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

        [DataMember]
        [Column("SERIALNUMBER")]
        public string SerialNumber
        {
            get { return (string)this["SERIALNUMBER"]; }
            set { this["SERIALNUMBER"] = value; }
        }
    }
}
