using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RR.Commerce.Runtime.Reporting
{
    public class ReportTranslation
    {
        private SortedList<string, string> _list;

        public ReportTranslation()
        {
            _list = new SortedList<string, string>();
        }

        public bool IsEmpty { get { return _list.Count == 0; } }

        public void Add(string id, string text)
        {
            if (!String.IsNullOrEmpty(id)) _list[id] = text;
        }

        public bool Contains(string id)
        {
            return _list.ContainsKey(id);
        }

        public string Get(string id)
        {
            string text;
            _list.TryGetValue(id, out text);
            return text == null ? String.Empty : text;
        }
    }
}
