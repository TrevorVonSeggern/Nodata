using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Cache
{
    public class DictionaryCache<T> : DictionaryCache<string, T>, ICacheForever<T>, ICacheCanRemove { }
    public class DictionaryCache<TKey, T> : ICacheForever<TKey, T>, ICacheCanRemove<TKey>
    {
        protected ReaderWriterLockSlim cacheLock = new ReaderWriterLockSlim();

        protected readonly IDictionary<TKey, T> cache = new ConcurrentDictionary<TKey, T>(); // maybe could be a dictionary

        // Primitive contains can be overrided
        protected virtual bool PrimitiveContains(TKey key)
        {
            return cache.ContainsKey(key);
        }

        // Primitive add does not aquire any locks
        protected virtual void PrimitiveAdd(TKey key, T item)
        {
            cache[key] = item;
        }

        public virtual void Add(TKey key, T item)
        {
            cacheLock.EnterWriteLock();
            PrimitiveAdd(key, item);
            cacheLock.ExitWriteLock();
        }

        // Primitive get does not aquire any locks
        protected T PrimitiveGet(TKey key)
        {
            return cache[key];
        }

        public virtual T Get(TKey key)
        {
            cacheLock.EnterReadLock();
            if (!PrimitiveContains(key))
                throw new ArgumentOutOfRangeException(nameof(key), "Key does not exist in cache.");
            var result = PrimitiveGet(key);
            cacheLock.ExitReadLock();
            return result;
        }

        // can be override and does not aquire locks.
        protected virtual void PrimitiveRemove(TKey key)
        {
            cache.Remove(key);
        }

        public virtual void Remove(TKey key)
        {
            cacheLock.EnterWriteLock();
            if (PrimitiveContains(key))
                PrimitiveRemove(key);
            cacheLock.ExitWriteLock();
        }

        protected virtual T SharedGetOrAddAsync(TKey key, Func<T> addItemFactory, Action<T> AddItemFunc)
        {
            T item;

            cacheLock.EnterUpgradeableReadLock();
            // Try to get the item in memory
            if (PrimitiveContains(key))
            {
                item = PrimitiveGet(key);
                cacheLock.ExitUpgradeableReadLock();
                return item;
            }

            // Item doesn't exist in memory, must aquire a write lock.
            cacheLock.EnterWriteLock();

            // After aquiring the write lock, must check if the key is then in memory.
            // This is to avoid the scenerio that multiple writes try to aquire writes one after another, and each would call addItemFactory.
            if (PrimitiveContains(key))
            {
                item = PrimitiveGet(key);
                cacheLock.ExitWriteLock();
                cacheLock.ExitUpgradeableReadLock();
                return item;
            }

            item = addItemFactory(); // Go get the item. External code call.

            AddItemFunc(item);

            // Release locks before returning.
            cacheLock.ExitWriteLock();
            cacheLock.ExitUpgradeableReadLock();

            return item;
        }

        public virtual T GetOrAdd(TKey key, Func<T> addItemFactory)
        {
            return SharedGetOrAddAsync(key, addItemFactory, item => PrimitiveAdd(key, item));
        }
    }
}
