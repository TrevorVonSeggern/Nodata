using System.Collections.Generic;

namespace NoData.Tests.GraphQueryable.SharedTypes
{
    public class Tree : Graph.Tree<Vertex, Edge, string, string>
    {
        public Tree(Vertex root, IEnumerable<IEnumerable<Edge>> expandPaths) : base(root, expandPaths)
        {
        }
    }
}