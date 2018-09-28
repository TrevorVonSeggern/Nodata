using System.Collections.Generic;
using CodeTools;
using Graph;
using Graph.Interfaces;
using NoData.GraphImplementations.Schema;
using Property = NoData.GraphImplementations.Schema.Property;

namespace NoData.GraphImplementations.Queryable
{
    [Immutable]
    public class QueryTree : Graph.Tree<QueryTree, QueryVertex, QueryEdge, QueryClass, Property>
    {
        public QueryTree(QueryVertex root, IEnumerable<ITuple<QueryEdge, QueryTree>> children = null) : base(root, children)
        {
        }

        public QueryTree(IEnumerable<IEnumerable<QueryEdge>> expandPaths) : base(expandPaths, ePaths => new QueryTree(ePaths), v => new QueryTree(v))
        {
        }
    }
}