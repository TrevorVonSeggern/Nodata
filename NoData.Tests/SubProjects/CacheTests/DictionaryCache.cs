using Cache;
using System;
using System.Threading.Tasks;
using Xunit;

namespace NoData.Tests.CacheTests
{
    public class DictionaryCacheTests
    {
        class ValueClass
        {
            public string Value;
        }
        class A : ValueClass { }
        class B : ValueClass { }

        private ICache cache { get; set; }

        public DictionaryCacheTests()
        {
            cache = new DictionaryCache();
        }

        [Fact]
        public void Cache_CanAddThenGet()
        {
            var a = new A { Value = "a" };
            var key = "cache me plz";
            cache.Add(key, a);
            var fromCache = cache.Get<A>(key);
            Assert.Same(a, fromCache);
        }

        [Fact]
        public void Cache_GetOrAdd()
        {
            var a = new A { Value = "a" };
            var key = "cache me plz";
            var fromCache = cache.GetOrAdd<A>(key, () => a);
            Assert.Same(a, fromCache);
        }

        [Fact]
        public void Cache_GetOrAdd_KeepsOriginal()
        {
            var a = new A { Value = "a" };
            var key = "cache me plz";
            var fromCache = cache.GetOrAdd<A>(key, () => a);
            var fromCache2 = cache.GetOrAdd<A>(key, () => a);
            Assert.Same(a, fromCache);
            Assert.Same(a, fromCache2);
        }

        [Fact]
        public void Cache_AddItem_ReplacesExistingItem()
        {
            var a = new A { Value = "a" };
            var a1 = new A { Value = "a1" };
            var key = "key";
            cache.Add(key, a);
            cache.Add(key, a1);
            var fromCache = cache.Get<A>(key);
            Assert.Same(a1, fromCache);
        }

        [Fact]
        public void Cache_GetNothing_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                cache.Get<A>(Guid.NewGuid().ToString());
            });
        }

        [Fact]
        public void Cache_KeyInterference_KeysAreIsolated()
        {
            var a1 = new A { Value = "a1" };
            var a2 = new A { Value = "a2" };
            var key1 = "1";
            var key2 = "2";
            cache.Add(key1, a1);
            cache.Add(key2, a2);
            var fromCache1 = cache.Get<A>(key1);
            var fromCache2 = cache.Get<A>(key2);
            Assert.Same(a1, fromCache1);
            Assert.Same(a2, fromCache2);
            Assert.NotSame(fromCache1, fromCache2);
        }

        [Fact]
        public void Cache_TypeInterference_TypesAreIsolated()
        {
            var a = new A { Value = "a1" };
            var b = new B { Value = "a2" };
            var key = "key";
            cache.Add(key, a);
            cache.Add(key, b);
            var fromCache1 = cache.Get<A>(key);
            var fromCache2 = cache.Get<B>(key);
            Assert.Same(a, fromCache1);
            Assert.Same(b, fromCache2);
            Assert.NotSame(fromCache1, fromCache2);
        }

        [Fact]
        public void Cache_Remove_Item()
        {
            var a = new A { Value = "a1" };
            var key = "key";
            cache.Add(key, a);
            var fromCache1 = cache.Get<A>(key);
            cache.Remove<A>(key);
            Assert.Same(a, fromCache1);
        }

        [Fact]
        public void Cache_Remove_RemoveNothing_DoesNotThrows()
        {
            cache.Remove<A>(Guid.NewGuid().ToString());
        }

        [Fact]
        public void Cache_Remove_Item_AfterTimeout()
        {
            var a = new A { Value = "a1" };
            var key = "key";
            cache.Add(key, a, TimeSpan.FromMilliseconds(100));
            var fromCache = cache.Get<A>(key);
            Task.Delay(TimeSpan.FromMilliseconds(150)).Wait();

            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                cache.Get<A>(key);
            });
        }

        [Fact]
        public void Cache_Add_Twice()
        {
            var key = Guid.NewGuid().ToString();
            cache.Add<A>(key, new A());
            cache.Add<A>(key, new A());
        }

        [Fact]
        public void Cache_GetOrAdd_Twice()
        {
            var key = Guid.NewGuid().ToString();
            cache.GetOrAdd<A>(key, () => new A());
            cache.GetOrAdd<A>(key, () => new A());
        }

        [Fact]
        public async Task Cache_GetOrAddAsync_Twice()
        {
            var key = Guid.NewGuid().ToString();
            await cache.GetOrAddAsync<A>(key, () => Task.FromResult(new A()));
            await cache.GetOrAddAsync<A>(key, () => Task.FromResult(new A()));
        }
    }
}