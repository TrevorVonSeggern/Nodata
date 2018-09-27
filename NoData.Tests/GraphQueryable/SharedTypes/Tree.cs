using System.Collections.Generic;

namespace NoData.Tests.GraphQueryable.SharedTypes
{
    public class Tree : Graph.Tree<Vertex, Edge, string, string>
    {
        public Tree(IEnumerable<IEnumerable<Edge>> expandPaths) : base(expandPaths)
        {
        }
    }
}