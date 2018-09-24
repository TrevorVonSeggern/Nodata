using System;
using System.Threading.Tasks;

namespace Cache
{
    public interface ICache<T> : ICache<string, T> { }
    public interface ICache<in TKey, T> : ICacheForever<TKey, T>, ICacheForeverAsync<TKey, T>, ICacheWithTimeout<TKey, T>, ICacheCanRemove<TKey>
    {
    }
}
