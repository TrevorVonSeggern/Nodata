using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Immutability;
using Graph.Interfaces;

namespace Graph
{

    [Immutable]
    public class Graph<TVertex, TEdge, TVertexValue, TEdgeValue>
            : IGraph<TVertex, TEdge, TVertexValue, TEdgeValue>
        where TVertex : class, IVertex<TVertexValue>
        where TEdge : IEdge<TEdgeValue, TVertex, TVertexValue>
    {
        public IEnumerable<TVertex> Vertices { get; }
        public IEnumerable<TEdge> Edges { get; }

        public Graph()
        {
            Vertices = new List<TVertex>();
            Edges = new List<TEdge>();
        }

        public Graph(IEnumerable<TVertex> vertices, IEnumerable<TEdge> edges, bool verticesUnique = true, bool edgesUnique = true)
        {
            if (verticesUnique && vertices.Count() != vertices.Distinct().Count())
                throw new ArgumentException("Vertices are not unique.");
            if (edgesUnique && edges.Count() != edges.Distinct().Count())
                throw new ArgumentException("Edges are not unique.");

            Vertices = new ReadOnlyCollection<TVertex>(vertices.ToArray());
            Edges = new ReadOnlyCollection<TEdge>(edges.ToArray());
        }

        public virtual TVertex VertexOfValue(TVertexValue value) => Vertices.FirstOrDefault(v => v.Value.Equals(value));

        public virtual IEnumerable<TVertex> VerticesConnectedTo(TVertex vertex) => IncomingEdges(vertex).Select(e => e.From);
        public virtual IEnumerable<TVertex> VerticesConnectedFrom(TVertex vertex) => OutgoingEdges(vertex).Select(e => e.To);

        public virtual IEnumerable<TEdge> IncomingEdges(TVertex vertex) => Edges.Where(e => e.To.Equals(vertex));
        public virtual IEnumerable<TEdge> OutgoingEdges(TVertex vertex) => Edges.Where(e => e.From.Equals(vertex));

        // public virtual object Clone()
        // {
        //     var vertices = new List<TVertex>(Vertices.Select(v => (TVertex)v.Clone()));
        //     var edges = new List<TEdge>(
        //         Edges.Select(e => (TEdge)e.CloneWithNewReferences(
        //             vertices.Single(v => v.Value.Equals(e.From.Value)),
        //             vertices.Single(v => v.Value.Equals(e.From.Value))
        //     )));
        //     return new Graph<TVertex, TEdge, TVertexValue, TEdgeValue>(vertices, edges);
        // }
    }
}
