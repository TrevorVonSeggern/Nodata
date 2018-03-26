using NoData.Graph.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NoData.Graph.Base
{
    public class Path<TEdge, TVertex, TEdgeValue, TVertexValue> : IPath<TEdge, TVertex, TEdgeValue, TVertexValue>
        where TEdge : IEdge<TEdgeValue, TVertex, TVertexValue>
        where TVertex : IVertex<TVertexValue>
        where TVertexValue : IMergable<TVertexValue>
    {
        private readonly IEnumerable<TEdge> edges;
        public IEnumerable<TEdge> Edges => edges;

        public Path(IEnumerable<TEdge> edges)
        {
            this.edges = edges;
            var toAdd = new List<TEdge>(edges.Count());
            TEdge previousEdge = default(TEdge);
            foreach(var edge in edges)
            {
                if (ReferenceEquals(previousEdge, null))
                    toAdd.Add(edge);
                else
                {
                    if (!edge.From.Equals(previousEdge.To))
                        throw new ArgumentException("IEdge list is not continuous.");
                    else
                        toAdd.Add(edge);
                }
                previousEdge = edge;
            }
            this.edges = toAdd;
        }

        public void Traverse(Action<TEdge> action)
        {
            foreach (var edge in edges)
                action(edge);
        }
    }
}
