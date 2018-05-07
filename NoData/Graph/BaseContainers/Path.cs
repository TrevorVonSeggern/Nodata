using NoData.Graph.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NoData.Graph.Base
{
    public class Path<TEdge, TVertex, TEdgeValue, TVertexValue> : IPath<TEdge, TVertex, TEdgeValue, TVertexValue>
        where TEdge : IEdge<TEdgeValue, TVertex, TVertexValue>
        where TVertex : IVertex<TVertexValue>
        where TVertexValue : IMergable<TVertexValue>
    {
        public virtual IEnumerable<TEdge> Edges { get; protected set; }

        public Path(IEnumerable<TEdge> edges)
        {
            Edges = edges;
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
            Edges = toAdd;
        }

        public virtual void Traverse(Action<TEdge> action)
        {
            foreach (var edge in Edges)
                action(edge);
        }
    }
}
