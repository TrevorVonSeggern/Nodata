using System;
using System.Threading.Tasks;

namespace Cache
{
    public interface ICacheWithTimeout<T> : ICacheWithTimeout<string, T> { }
    public interface ICacheWithTimeout<in TKey, T> : ICacheForever<TKey, T>, ICacheForeverAsync<TKey, T>, ICacheCanRemove<TKey>
    {
        void Add(TKey key, T item, TimeSpan cacheTime);
        T GetOrAdd(TKey key, Func<T> addItemFactory, TimeSpan cacheTime);
        Task<T> GetOrAddAsync(TKey key, Func<Task<T>> addItemFactory, TimeSpan cacheTime);
    }
}
