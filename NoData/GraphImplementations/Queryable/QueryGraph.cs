using System.Collections.Generic;
using Immutability;
using Property = NoData.GraphImplementations.Schema.Property;

namespace NoData.GraphImplementations.Queryable
{
    [Immutable]
    public class QueryGraph : Graph.Graph<QueryVertex, QueryEdge, QueryClass, Property>
    {
        public QueryGraph(IEnumerable<QueryVertex> vertices, IEnumerable<QueryEdge> edges) : base(vertices, edges)
        {
        }
    }
}