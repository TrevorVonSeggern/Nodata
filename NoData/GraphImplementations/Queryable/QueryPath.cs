using Immutability;
using Property = NoData.GraphImplementations.Schema.Property;

namespace NoData.GraphImplementations.Queryable
{
    [Immutable]
    public class QueryPath : Graph.Path<QueryEdge, QueryVertex, Property, QueryClass>
    {
        public QueryPath(IEnumerable<QueryEdge> edges) : base(edges)
        {
        }
    }
}
