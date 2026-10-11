using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace RR.Commerce.Runtime.Mobility
{
    public class RangingList
    {
        private string _sectionId;
        private string _shelfId;
        private List<RangingItem> _list;
        private int _lastId;

        public RangingList(string sectionId, string shelfId)
        {
            _sectionId = sectionId;
            _shelfId = shelfId;
            _list = new List<RangingItem>();
            _lastId = 0;
        }

        public string SectionId { get { return _sectionId; } }
        public string ShelfId { get { return _shelfId; } }

        public List<RangingItem> List { get { return _list; } }
        public int Count { get { return _list.Count; } }

        public RangingStat GetStat()
        {
            RangingStat stat = new RangingStat();
            stat.SectionId = _sectionId;
            stat.ShelfId = _shelfId;
            stat.Count = _list.Count;
            RangingItem item = GetLast();
            if (item != null)
            {
                stat.ItemKey = item.Id;
                stat.ItemId = item.ItemId;
                stat.ItemName = item.ItemName;
            }
            return stat;
        }

        public RangingItem Get(int Id)
        {
            return _list.FirstOrDefault(i => i.Id == Id);
        }

        public RangingItem Get(string itemId)
        {
            return _list.FirstOrDefault(i => i.ItemId == itemId);
        }

        public RangingItem GetLast()
        {
            return _list.Count == 0 ? null : _list[_list.Count - 1];
        }

        public void Add(RangingItem item)
        {
            RangingItem item2 = Get(item.ItemId);
            if (item2 != null)
                _list.Remove(item2);
            item.Id = ++_lastId;
            _list.Add(item);
        }

        public void Delete(int key)
        {
            RangingItem item2 = Get(key);
            if (item2 != null)
                _list.Remove(item2);
        }

        public void DeleteLast()
        {
            if (_list.Count > 0)
                _list.RemoveAt(_list.Count - 1);
        }
    }
}
