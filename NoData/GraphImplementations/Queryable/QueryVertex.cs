using Immutability;

namespace NoData.GraphImplementations.Queryable
{
    [Immutable]
    public class QueryVertex : GraphLibrary.Vertex<QueryClass>
    {
        public QueryVertex(QueryClass value) : base(value)
        {
        }
    }
}
