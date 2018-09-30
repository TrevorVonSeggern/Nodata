using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Graph.Interfaces;
using NoData.GraphImplementations.Schema;
namespace NoData.Utility
{
    public static class TreeUtility
    {
        public static GraphSchema Flatten(Tree selectionTree)
        {
            var edges = new List<Edge>();
            var vertices = new List<Vertex>();
            selectionTree.TraverseDepthFirstSearch(edges.Add);
            selectionTree.TraverseDepthFirstSearch(vertices.Add);
            return new GraphSchema(vertices, edges);
        }
    }
}
