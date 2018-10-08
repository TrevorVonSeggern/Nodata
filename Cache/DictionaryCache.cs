using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Cache
{
    public class DictionaryCacheWithTimeouts<T> : DictionaryCacheWithTimeouts<string, T>, ICache<T> { }
    public class DictionaryCacheWithTimeouts<TKey, T> : DictionaryCacheAsync<TKey, T>, ICache<TKey, T>
    {
        protected readonly IDictionary<TKey, CancellationTokenSource> tokenCache = new ConcurrentDictionary<TKey, CancellationTokenSource>();
        private static readonly TimeSpan infinity = TimeSpan.FromMilliseconds(-1);

        protected override bool PrimitiveContains(TKey key)
        {
            return base.PrimitiveContains(key) && !tokenCache[key].IsCancellationRequested;
        }
        protected override void PrimitiveRemove(TKey key)
        {
            if (PrimitiveContains(key))
            {
                tokenCache[key].Cancel();
                tokenCache.Remove(key);
                base.PrimitiveRemove(key);
            }
        }
        protected virtual void PrimitiveAdd(TKey key, T item, TimeSpan timeout)
        {
            PrimitiveRemove(key);
            var tokenSource = new CancellationTokenSource();

            cache[key] = item;
            tokenCache[key] = tokenSource;

            // Don't bother with expire task/timeout if it's infinity.
            if (timeout != infinity)
            {
                // start timeout task to remove item
                var token = tokenSource.Token;
                Task.Delay(timeout, token).ContinueWith(_ => Remove(key), token);
            }
        }
        protected override void PrimitiveAdd(TKey key, T item) => PrimitiveAdd(key, item, infinity);

        public override void Add(TKey key, T item) => Add(key, item, infinity);
        public virtual void Add(TKey key, T item, TimeSpan timeout)
        {
            cacheLock.EnterWriteLock();

            if (PrimitiveContains(key))
                PrimitiveRemove(key);
            PrimitiveAdd(key, item, timeout);

            cacheLock.ExitWriteLock();
        }

        public override Task<T> GetOrAddAsync(TKey key, Func<Task<T>> addItemFactory) => GetOrAddAsync(key, addItemFactory, infinity);
        public override T GetOrAdd(TKey key, Func<T> addItemFactory) => GetOrAdd(key, addItemFactory, infinity);

        protected override T SharedGetOrAddAsync(TKey key, Func<T> addItemFactory, Action<T> AddItemFunc)
        {
            return SharedGetOrAddAsync(key, addItemFactory, AddItemFunc, infinity);
        }

        protected virtual T SharedGetOrAddAsync(TKey key, Func<T> addItemFactory, Action<T> AddItemFunc, TimeSpan timeout)
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
            try
            {
                item = addItemFactory(); // Go get the item. External code call.
            }
            catch { throw; }
            finally
            {
                // Release locks before returning.
                cacheLock.ExitWriteLock();
                cacheLock.ExitUpgradeableReadLock();
            }
            AddItemFunc(item);
            return item;
        }

        public virtual T GetOrAdd(TKey key, Func<T> addItemFactory, TimeSpan timeout)
        {
            return SharedGetOrAddAsync(key, addItemFactory, item => PrimitiveAdd(key, item, timeout), timeout);
        }

        public virtual Task<T> GetOrAddAsync(TKey key, Func<Task<T>> addItemFactory, TimeSpan timeout)
        {
            return Task.FromResult(SharedGetOrAddAsync(key, () => addItemFactory().GetAwaiter().GetResult(), item => PrimitiveAdd(key, item, timeout), timeout));
        }
    }
}
