using Immutability;
using Property = NoData.GraphImplementations.Schema.Property;

namespace NoData.GraphImplementations.Queryable
{
    [Immutable]
    public class QueryEdge : GraphLibrary.Edge<Property, QueryVertex, QueryClass>
    {
        public QueryEdge(QueryVertex from, QueryVertex to, Property value = null) : base(from, to, value)
        {
        }
    }
}
