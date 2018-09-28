using System.Collections.Generic;
using Graph;

namespace NoData.Tests.GraphQueryable.SharedTypes
{
    public class Tree : Graph.Tree<Tree, Vertex, Edge, string, string>
    {
        public Tree(IEnumerable<IEnumerable<Edge>> expandPaths) : base(expandPaths, ep => new Tree(ep), v => new Tree(v))
        {
        }

        public Tree(Vertex root, IEnumerable<ITuple<Edge, Tree>> children = null) : base(root, children)
        {
        }
    }
}