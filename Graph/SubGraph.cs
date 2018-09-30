using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CodeTools;
using Graph.Interfaces;

namespace Graph
{

    [Immutable]
    public class SubGraph<TVertex, TEdge, TVertexValue, TEdgeValue>
            : ISubGraph<TVertex, TEdge, TVertexValue, TEdgeValue>
        where TVertex : class, IVertex<TVertexValue>
        where TEdge : IEdge<TEdgeValue, TVertex, TVertexValue>
    {
        public virtual IEnumerable<TVertex> Vertices { get; }
        public virtual IEnumerable<TEdge> Edges { get; }

        public SubGraph()
        {
            Vertices = new List<TVertex>();
            Edges = new List<TEdge>();
        }

        public SubGraph(IEnumerable<TVertex> vertices, IEnumerable<TEdge> edges)
        {
            Vertices = vertices;
            Edges = edges;
        }

        public virtual TVertex VertexOfValue(TVertexValue value) => Vertices.FirstOrDefault(v => v.Value.Equals(value));

        public virtual IEnumerable<TVertex> VerticesConnectedTo(TVertex vertex) => IncomingEdges(vertex).Select(e => e.From);
        public virtual IEnumerable<TVertex> VerticesConnectedFrom(TVertex vertex) => OutgoingEdges(vertex).Select(e => e.To);

        public virtual IEnumerable<TEdge> IncomingEdges(TVertex vertex) => Edges.Where(e => e.To.Equals(vertex));
        public virtual IEnumerable<TEdge> OutgoingEdges(TVertex vertex) => Edges.Where(e => e.From.Equals(vertex));
    }
}
