using System;
using System.Runtime.Serialization;
using SystemDataAnnotations = System.ComponentModel.DataAnnotations;
using Microsoft.Dynamics.Commerce.Runtime.DataModel;
using Microsoft.Dynamics.Commerce.Runtime.ComponentModel.DataAnnotations;

namespace RR.Commerce.Runtime.Utilities
{
    public class UtilitiesConfiguration : CommerceEntity
    {
        private const string IdColumn = "RECID";

        public UtilitiesConfiguration() : base("UtilitiesConfiguration")
        {
            this.Initialize();
        }

        [OnDeserializing]
        public new void OnDeserializing(StreamingContext context)
        {
            this.Initialize();
        }

        private void Initialize()
        {
        }
        [SystemDataAnnotations.Key]
        [Key]
        [DataMember]
        [Column(IdColumn)]
        public long Id
        {
            get { return (long)(this[IdColumn] ?? 0L); }
            set { this[IdColumn] = value; }
        }

        [DataMember]
        [Column("LABELHIERARCHYRECID")]
        public long LabelHierarchyRecId
        {
            get { return (long)(this["LABELHIERARCHYRECID"] ?? 0L); }
            set { this["LABELHIERARCHYRECID"] = value; }
        }

        [DataMember]
        [Column("DEFAULTLABELPROFILE")]
        public long DefaultLabelProfileRecId
        {
            get { return (long)(this["DEFAULTLABELPROFILE"] ?? 0L); }
            set { this["DEFAULTLABELPROFILE"] = value; }
        }

        [DataMember]
        [Column("FIRESALEBARCODEPREFIX")]
        public string FireSaleBarcodePrefix
        {
            get { return (string)(this["FIRESALEBARCODEPREFIX"] ?? String.Empty); }
            set { this["FIRESALEBARCODEPREFIX"] = value; }
        }

        [DataMember]
        [Column("LABELCURRENCYCODE")]
        public string LabelCurrencyCode
        {
            get { return (string)(this["LABELCURRENCYCODE"] ?? String.Empty); }
            set { this["LABELCURRENCYCODE"] = value; }
        }

        [DataMember]
        [Column("RANGINGWITHFACES")]
        public int RangingWithFaces
        {
            get { return Convert.ToInt32(this["RANGINGWITHFACES"]); }
            set { this["RANGINGWITHFACES"] = value; }
        }

        [DataMember]
        [Column("DISABLELABELWIZARD")]
        public int DisableLabelWizard
        {
            get { return Convert.ToInt32(this["DISABLELABELWIZARD"]); }
            set { this["DISABLELABELWIZARD"] = value; }
        }

        [DataMember]
        [Column("BLOCKSALES")]
        public int BlockSales
        {
            get { return Convert.ToInt32(this["BLOCKSALES"]); }
            set { this["BLOCKSALES"] = value; }
        }

        [DataMember]
        [Column("LABELPRINTDEVICEITEMS")]
        public int LabelPrintDeviceItems
        {
            get { return Convert.ToInt32(this["LABELPRINTDEVICEITEMS"]); }
            set { this["LABELPRINTDEVICEITEMS"] = value; }
        }
    }
}
