using CodeTools;

namespace NoData.GraphImplementations.Queryable
{
    [Immutable]
    public class QueryVertex : Graph.Vertex<QueryClass>
    {
        public QueryVertex(QueryClass value) : base(value)
        {
        }
    }
}