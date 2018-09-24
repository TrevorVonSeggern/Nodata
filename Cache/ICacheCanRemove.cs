using System;
using System.Threading.Tasks;

namespace Cache
{
    public interface ICacheCanRemove : ICacheCanRemove<string> { }

    public interface ICacheCanRemove<in TKey>
    {
        void Remove(TKey key);
    }
}
