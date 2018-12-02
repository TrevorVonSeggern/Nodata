using System.Collections.Generic;
using Immutability;
using Graph;
using Graph.Interfaces;
using NoData.GraphImplementations.Schema;
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