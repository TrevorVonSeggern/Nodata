using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Cache
{
    public class DictionaryCache<T> : ICache<T>
    {
        private static readonly Object cacheLock = new Object();
        private static readonly TimeSpan infinity = TimeSpan.FromMilliseconds(-1);

        private readonly IDictionary<string, Tuple<T, CancellationTokenSource>> cache = new ConcurrentDictionary<string, Tuple<T, CancellationTokenSource>>();

        private T FetchItem(string key) => cache[key].Item1;
        private bool ContainsKey(string key) => cache.ContainsKey(key) && !cache[key].Item2.IsCancellationRequested;
        private void InsertItem(string key, T item, TimeSpan timeout, bool replaceItem)
        {
            if (replaceItem)
            {
                if (ContainsKey(key))
                    Remove(key);
            }
            else if (ContainsKey(key))
                return;

            // Set item to expire.
            var tokenSource = new CancellationTokenSource();
            var token = tokenSource.Token;

            // Don't bother with expire task/timeout if it's infinity.
            if (!timeout.Equals(infinity))
                Task.Delay(timeout, token).ContinueWith(_ => Expire(key), token);

            cache.Add(key, Tuple.Create(item, tokenSource));
        }

        public void Remove(string key) => Expire(key);

        private void Expire(string key)
        {
            lock (cacheLock)
            {
                if (ContainsKey(key))
                {
                    cache[key].Item2.Cancel();
                    cache.Remove(key);
                }
            }
        }

        public void Add(string key, T item) => Add(key, item, infinity);
        public void Add(string key, T item, TimeSpan cacheTime)
        {
            lock (cacheLock)
            {
                InsertItem(key, item, cacheTime, true);
            }
        }

        public T Get(string key)
        {
            lock (cacheLock)
            {
                if (!ContainsKey(key))
                    throw new ArgumentOutOfRangeException(nameof(key), "Key does not exist in cache.");
                return FetchItem(key);
            }
        }
        

        public T GetOrAdd(string key, Func<T> addItemFactory) => GetOrAdd(key, addItemFactory, infinity);
        public T GetOrAdd(string key, Func<T> addItemFactory, TimeSpan cacheTime)
        {
            lock (cacheLock)
            {
                if (ContainsKey(key))
                    return FetchItem(key);
            }
            var item = addItemFactory();

            lock (cacheLock)
            {
                InsertItem(key, item, cacheTime, false);
                return item;
            }
        }

        public Task<T> GetOrAddAsync(string key, Func<Task<T>> addItemFactory) => GetOrAddAsync(key, addItemFactory, infinity);
        public async Task<T> GetOrAddAsync(string key, Func<Task<T>> addItemFactory, TimeSpan cacheTime)
        {
            lock (cacheLock)
            {
                if (ContainsKey(key))
                    return FetchItem(key);
            }

            var item = await addItemFactory();

            lock (cacheLock)
            {
                InsertItem(key, item, cacheTime, false);
                return item;
            }

        }
    }
}
