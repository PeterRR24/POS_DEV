using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Microsoft.Dynamics.Commerce.Runtime;

using RR.Commerce.Contracts.Reporting;

namespace RR.Commerce.Runtime.Reporting
{
    public class PrcLabelProduct : PrcBaseForm
    {
        private readonly PrhFormLabel _header;
        private List<PrrLabelProduct> _dataset = null;
        
        public PrcLabelProduct(RequestContext ctx) : base(ctx)
        {
            _header = new PrhFormLabel(this);
        }

        public override object Header { get { return _header; } }
        public override object DataSet { get { return _dataset; } }

        public void Generate(IEnumerable<IFormItemLabel> labels)
        {
            List<PrrLabelProduct> rlabels = new List<PrrLabelProduct>();

            if (labels != null)
                foreach (IFormItemLabel label in labels)
                    rlabels.Add(new PrrLabelProduct(this, label));

            _dataset = rlabels;
        }

        public void Build(long layoutRecId, string cultureName)
        {
            _header.Build(layoutRecId, cultureName);
        }
    }

    public class PrrLabelProduct : PrrLabelBase
    {
        private readonly PrcLabelProduct RO;
        private readonly IFormItemLabel LABEL;

        private PrrLabelProduct() { }

        internal PrrLabelProduct(PrcLabelProduct ro, IFormItemLabel label)
        {
            if (label == null) throw new ArgumentNullException("label");
            RO = ro;
            LABEL = label;
        }
        
        protected override IFormItemLabel ItemLabel { get { return LABEL; } }

      
        // properties specific for Product labels
        public string BatchId { get { return NotNull(LABEL.BatchId); } }
        public DateTime? BatchExpDate { get { return LABEL.BatchExpDate; } }
        public string BatchExpDateT { get { return BatchExpDate == null ? String.Empty : ((DateTime)BatchExpDate).ToShortDateString(); } }
        public string SerialId { get { return NotNull(LABEL.SerialId); } }
        //public FormItemLabel Item { get { return (FormItemLabel)LABEL; } set { return; } }
    }
}
