using Graph;
using NoData.Tests.SharedExampleClasses;
using Xunit;

namespace NoData.Tests.BloomFilterTests
{

    public class BloomFilterTests
    {
        [Fact]
        public void Bloom_Nothing_PropertyDoesntExist_Success()
        {
            var filter = new BloomFilter();
            var item = new Dto { Name = "I'm not in the filter" };
            Assert.False(filter.PossiblyExists(item));
        }

        [Fact]
        public void Bloom_AddToBloom_PropertyExist_Success()
        {
            var filter = new BloomFilter();
            var item = new Dto { Name = "I'm in the filter" };
            filter.AddItem(item);
            Assert.True(filter.PossiblyExists(item));
        }

        [Fact]
        public void Bloom_AddToBloom_PropertyDoesntExist_Success()
        {
            var filter = new BloomFilter();
            var item = new Dto { Name = "I'm in the filter" };
            var item2 = new Dto { Name = "I'm not in the filter" };
            filter.AddItem(item);
            Assert.True(filter.PossiblyExists(item));
            Assert.False(filter.PossiblyExists(item2));
        }

        [Fact]
        public void Bloom_SamePropertiesAdded_DuplicateExistsInFilter_Success()
        {
            var filter = new BloomFilter();
            var item = new Dto { Name = "I'm in the filter" };
            var item2 = new Dto { Name = "I'm in the filter" };
            filter.AddItem(item);
            filter.AddItem(item2);
            Assert.True(filter.PossiblyExists(item));
            Assert.True(filter.PossiblyExists(item2));
        }

        [Fact]
        public void Bloom_SameProperties_DuplicateExistsInFilter_Success()
        {
            var filter = new BloomFilter();
            var item = new Dto { Name = "I'm in the filter" };
            var notInTheFilter = new Dto { Name = "I'm in the filter" }; // not in the filter. It's a lie.
            filter.AddItem(item);
            Assert.True(filter.PossiblyExists(item));
            Assert.False(filter.PossiblyExists(notInTheFilter));
        }
    }
}
