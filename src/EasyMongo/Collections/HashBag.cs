using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EasyMongo.Collections
{
    public class HashBag<TKey, TValue>
    {
        private Dictionary<TKey, HashSet<TValue>> m_container = new Dictionary<TKey, HashSet<TValue>>();

        public void Add(TKey key, TValue value)
        {
            this.GetSet(key).Add(value);
        }

        private HashSet<TValue> GetSet(TKey key)
        {
            HashSet<TValue> items;
            if (!this.m_container.TryGetValue(key, out items))
            {
                items = new HashSet<TValue>();
                this.m_container.Add(key, items);
            }

            return items;
        }

        public void AddAll(TKey key, IEnumerable<TValue> values)
        {
            var set = this.GetSet(key);
            foreach (var v in values) set.Add(v);
        }

        public bool RemoveAll(TKey key)
        {
            return this.m_container.Remove(key);
        }

        public IEnumerable<TKey> Keys
        {
            get
            {
                return this.m_container.Keys;
            }
        }

        public IEnumerable<TValue> this[TKey key]
        {
            get
            {
                return this.m_container[key];
            }
        }
    }
}
