using NoData.GraphImplementations.Schema;
using NoData.Utility;

namespace NoData.Tests.SharedExampleClasses
{
    public class CacheAndGraphFixture<TDto>
    {
        public GraphSchema graph;
        public IClassCache cache;

        public CacheAndGraphFixture()
        {
            cache = new ClassCache();
            graph = GraphSchema.Cache<TDto>.Graph;
        }
    }
}