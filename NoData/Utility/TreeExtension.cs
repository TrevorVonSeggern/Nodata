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
            selectionTree.TraverseDepthFirstSearch(edges.Add);
            var vertices = new Dictionary<ClassInfo, Vertex>();

            void MergeVertexes(IEnumerable<Vertex> selection)
            {
                foreach (var v in selection)
                {
                    if (!vertices.ContainsKey(v.Value))
                        vertices.Add(v.Value, v);
                    // else
                    //     vertices[v.Value].Merge(v);
                }
            }

            MergeVertexes(new[] { selectionTree.Root });
            MergeVertexes(edges.Select(e => e.From).Where(v => v != selectionTree.Root));
            MergeVertexes(edges.Select(e => e.To));
            return new GraphSchema(vertices.Values.ToList(), edges);
        }
    }
}
