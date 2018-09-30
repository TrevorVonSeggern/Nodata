using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Cache
{
    public class DictionaryCacheAsync<TKey, T> : DictionaryCache<TKey, T>, ICacheForeverAsync<TKey, T>
    {
        public virtual Task<T> GetOrAddAsync(TKey key, Func<Task<T>> addItemFactory)
        {
            return Task.FromResult(SharedGetOrAddAsync(key, () => addItemFactory().GetAwaiter().GetResult(), item => PrimitiveAdd(key, item)));
        }
    }
}
