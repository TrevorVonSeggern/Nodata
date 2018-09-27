using System.Collections.Generic;
using CodeTools;
using Graph;
using Graph.Interfaces;
using NoData.GraphImplementations.Schema;
using Property = NoData.GraphImplementations.Schema.Property;

namespace NoData.GraphImplementations.Queryable
{
    [Immutable]
    public class QueryTree : Graph.Tree<QueryVertex, QueryEdge, QueryClass, Property>
    {
        public QueryTree(QueryVertex root, IEnumerable<ITuple<QueryEdge, ITree<QueryVertex, QueryEdge, QueryClass, Property>>> children) : base(root, children)
        {
        }
    }
}