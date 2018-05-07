using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NoData.Graph.Utility
{
    public static class TreeUtility
    {
        public static Graph Flatten(Tree selectionTree)
        {
            var edges = new List<Edge>();
            selectionTree.Traverse(edges.Add);
            var vertices = new Dictionary<Type, Vertex>();

            void MergeVertexes(IEnumerable<Vertex> selection)
            {
                foreach (var v in selection)
                {
                    if (!vertices.ContainsKey(v.Value.Type))
                    {
                        vertices.Add(v.Value.Type, v);
                        continue;
                    }
                    vertices[v.Value.Type].Merge(v);
                }
            }

            MergeVertexes(new[] { selectionTree.Root });
            MergeVertexes(edges.Select(e => e.From).Where(v => v != selectionTree.Root));
            MergeVertexes(edges.Select(e => e.To));
            return new Graph(vertices.Values.ToList(), edges);
        }
    }
}
