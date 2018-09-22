using System;
using System.Threading.Tasks;

namespace Cache
{
    public interface ICache
    {
        void Add<T>(string key, T item);
        void Add<T>(string key, T item, TimeSpan cacheTime);

        T Get<T>(string key);

        T GetOrAdd<T>(string key, Func<T> addItemFactory);
        T GetOrAdd<T>(string key, Func<T> addItemFactory, TimeSpan cacheTime);

        Task<T> GetOrAddAsync<T>(string key, Func<Task<T>> addItemFactory);
        Task<T> GetOrAddAsync<T>(string key, Func<Task<T>> addItemFactory, TimeSpan cacheTime);

        void Remove<T>(string key);
    }
}
