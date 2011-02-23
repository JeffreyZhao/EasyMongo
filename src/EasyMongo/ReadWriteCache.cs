using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace EasyMongo
{
    internal abstract class ReadWriteCache<TKey, TValue>
    {
        protected ReadWriteCache()
            : this(null)
        { }

        protected ReadWriteCache(IEqualityComparer<TKey> comparer)
        {
            this.m_storage = new Dictionary<TKey, TValue>(comparer);
        }

        private readonly Dictionary<TKey, TValue> m_storage;
        private readonly ReaderWriterLockSlim m_rwLock = new ReaderWriterLockSlim();

        protected abstract TValue Create(TKey key);

        public TValue Get(TKey key)
        {
            TValue value;

            this.m_rwLock.EnterReadLock();
            try
            {
                if (this.m_storage.TryGetValue(key, out value))
                {
                    return value;
                }
            }
            finally
            {
                this.m_rwLock.ExitReadLock();
            }

            this.m_rwLock.EnterWriteLock();
            try
            {
                if (this.m_storage.TryGetValue(key, out value))
                {
                    return value;
                }

                value = this.Create(key);
                this.m_storage.Add(key, value);
            }
            finally
            {
                this.m_rwLock.ExitWriteLock();
            }

            return value;
        }
    }
}
