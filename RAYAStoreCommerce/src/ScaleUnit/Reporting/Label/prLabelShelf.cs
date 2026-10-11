using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Dynamics.Commerce.Runtime;

using RR.Commerce.Contracts.Reporting;

namespace RR.Commerce.Runtime.Reporting
{
    public class PrcLabelShelf : PrcBaseForm
    {
        private readonly PrhFormLabel _header;
        private List<PrrLabelShelf> _dataset = null;

        public PrcLabelShelf(RequestContext ctx) : base(ctx)
        {
            _header = new PrhFormLabel(this);
        }

        public override object Header { get { return _header; } }
        public override object DataSet { get { return _dataset; } }

        public void Generate(IEnumerable<IFormItemLabel> labels)
        {
            List<PrrLabelShelf> rlabels = new List<PrrLabelShelf>();

            if (labels != null)
                foreach (IFormItemLabel label in labels)
                    rlabels.Add(new PrrLabelShelf(this, label));

            _dataset = rlabels;
        }

        public void Build(long layoutRecId, string cultureName)
        {
            _header.Build(layoutRecId, cultureName);
        }
    }

    public class PrrLabelShelf : PrrLabelBase
    {
        private readonly PrcLabelShelf RO;
        private readonly IFormItemLabel LABEL;

        internal PrrLabelShelf(PrcLabelShelf ro, IFormItemLabel label)
        {
            if (label == null) throw new ArgumentNullException("label");
            RO = ro;
            LABEL = label;
        }

        protected override IFormItemLabel ItemLabel { get { return LABEL; } }

        // properties specific for Shelf labels
        public string SectionId { get { return NotNull(LABEL.SeasonId); } }
        public string SectionName { get { return NotNull(LABEL.SectionName); } }
        public string ShelfId { get { return NotNull(LABEL.ShelfId); } }
        public string ShelfName { get { return NotNull(LABEL.ShelfName); } }
        //public FormItemLabel Item { get { return (FormItemLabel)LABEL; } set { return; } }
    }
}
