using System;
using System.Runtime.Serialization;
using System.Text;

namespace RR.Commerce.Runtime.Mobility
{
    [DataContract]
    public class ItemLabelRequest
    {
        [DataMember]
        public string BarcodeId { get; set; }
        [DataMember]
        public string ItemId { get; set; }
        [DataMember]
        public string InventDimId { get; set; }
        [DataMember]
        public string UnitId { get; set; }
        [DataMember]
        public string InventBatchId { get; set; }
        [DataMember]
        public string InventSerialId { get; set; }
        [DataMember]
        public int TrackingInfo { get; set; }
        [DataMember]
        public int Quantity { get; set; }
    }

    [DataContract]
    public class LabelPrintRequest : ItemLabelRequest
    {
    }

    [DataContract]
    public class LabelPrintItem : ItemLabelRequest
    {
        [DataMember]
        public long Id { get; set; }
        [DataMember]
        public string ItemName { get; set; }
        [DataMember]
        public string VariantId { get; set; }
        [DataMember]
        public string InventSizeId { get; set; }
        [DataMember]
        public string InventColorId { get; set; }
        [DataMember]
        public string InventStyleId { get; set; }
        [DataMember]
        public string ConfigId { get; set; }
        [DataMember]
        public string InventDimText { get; set; }

        public void CreateInventDim()
        {
            StringBuilder sb = new StringBuilder();
            AddInventDimText(sb, ConfigId);
            AddInventDimText(sb, InventStyleId);
            AddInventDimText(sb, InventColorId);
            AddInventDimText(sb, InventSizeId);

            StringBuilder sb2 = new StringBuilder();
            AddInventDimText(sb2, InventBatchId);
            AddInventDimText(sb2, InventSerialId);
            if (sb2.Length > 0)
            {
                if (sb.Length > 0)
                    sb.Append(" ");
                sb.Append(sb2.ToString());
            }

            InventDimText = sb.ToString();
        }

        private void AddInventDimText(StringBuilder sb, string text)
        {
            if (!String.IsNullOrEmpty(text))
            {
                if (sb.Length > 0)
                    sb.Append("/");
                sb.Append(text);
            }
        }
    }
}
