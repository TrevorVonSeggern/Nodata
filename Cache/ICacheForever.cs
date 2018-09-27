using System;
using System.Threading.Tasks;

namespace Cache
{
    public interface ICacheForever<T> : ICacheForever<string, T> { }
    public interface ICacheForeverAsync<T> : ICacheForeverAsync<string, T> { }

    public interface ICacheForever<in TKey, T>
    {
        bool HasKey(TKey key);
        
        void Add(TKey key, T item);

        T Get(TKey key);

        T GetOrAdd(TKey key, Func<T> addItemFactory);
    }
    public interface ICacheForeverAsync<in TKey, T>
    {
        Task<T> GetOrAddAsync(TKey key, Func<Task<T>> addItemFactory);
    }
}
