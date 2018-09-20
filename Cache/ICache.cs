using System;
using System.Threading.Tasks;

namespace Cache
{
    public interface ICache<T>
    {
        void Add(string key, T item);
        void Add(string key, T item, TimeSpan cacheTime);

        T Get(string key);

        T GetOrAdd(string key, Func<T> addItemFactory);
        T GetOrAdd(string key, Func<T> addItemFactory, TimeSpan cacheTime);

        Task<T> GetOrAddAsync(string key, Func<Task<T>> addItemFactory);
        Task<T> GetOrAddAsync(string key, Func<Task<T>> addItemFactory, TimeSpan cacheTime);

        void Remove(string key);
    }
}
