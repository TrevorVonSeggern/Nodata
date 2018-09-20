using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Cache
{
    public class DictionaryCache : ICache
    {
        private readonly IDictionary<Type, object> cacheList = new ConcurrentDictionary<Type, object>();
        private static readonly Object cacheListLock = new Object();

        private ICache<T> GetCache<T>()
        {
            var t = typeof(T);
            CacheAddIfNotExists<T>();
            return (ICache<T>)cacheList[t];
        }

        private void CacheAddIfNotExists<T>()
        {
            var t = typeof(T);
            lock (cacheListLock)
            {
                if (!cacheList.ContainsKey(t))
                    cacheList.Add(t, new DictionaryCache<T>());
            }
        }

        public void Add<T>(string key, T item) => GetCache<T>().Add(key, item);

        public void Add<T>(string key, T item, TimeSpan cacheTime) => GetCache<T>().Add(key, item, cacheTime);

        public T Get<T>(string key) => GetCache<T>().Get(key);

        public T GetOrAdd<T>(string key, Func<T> addItemFactory) => GetCache<T>().GetOrAdd(key, addItemFactory);

        public T GetOrAdd<T>(string key, Func<T> addItemFactory, TimeSpan cacheTime) => GetCache<T>().GetOrAdd(key, addItemFactory, cacheTime);

        public Task<T> GetOrAddAsync<T>(string key, Func<Task<T>> addItemFactory) => GetCache<T>().GetOrAddAsync(key, addItemFactory);

        public Task<T> GetOrAddAsync<T>(string key, Func<Task<T>> addItemFactory, TimeSpan cacheTime) => GetCache<T>().GetOrAddAsync(key, addItemFactory, cacheTime);

        public void Remove<T>(string key) => GetCache<T>().Remove(key);
    }
}
