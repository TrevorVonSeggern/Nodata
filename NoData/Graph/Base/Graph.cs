using System;
using System.Collections.Generic;
using System.Linq;

namespace NoData.Graph.Base
{
    using Interfaces;

    public class Graph : IGraph, ICloneable
    {
        private readonly IEnumerable<IVertex> vertices;
        private readonly IEnumerable<IEdge> edges;

        public IEnumerable<IVertex> Vertices { get { return vertices; } }
        public IEnumerable<IEdge> Edges { get { return edges; } }

        public Graph()
        {
            vertices = new List<IVertex>();
            edges = new List<IEdge>();
        }

        public IVertex VertexOfValue(object value) => vertices.FirstOrDefault(v => v.Value == value);

        public Graph(IEnumerable<IVertex> vertices, IEnumerable<IEdge> edges, bool verticesUnique = true, bool edgesUnique = true, bool danglingEdges = false)
        {
            if (verticesUnique && vertices.Count() != vertices.Distinct().Count())
                throw new ArgumentException("Vertices are not unique.");
            if (edgesUnique && edges.Count() != edges.Distinct().Count())
                throw new ArgumentException("Edges are not unique.");
            if (danglingEdges && edges.Count() != edges.Where(e => e.IsFullyConnected(this)).Count())
                throw new ArgumentException("There is a dangling edge.");

            this.vertices = new List<IVertex>(vertices);
            this.edges = new List<IEdge>(edges);
        }

        public Graph(IEnumerable<IVertex> vertices, IEnumerable<IEdge> edges)
        {
            this.vertices = new List<IVertex>(vertices);
            this.edges = new List<IEdge>(edges.Where(e => e.IsFullyConnected(vertices)).Distinct());
        }

        public virtual object Clone()
        {
            var vertices = new List<IVertex>(this.vertices.Select(v => v.Clone() as IVertex));
            var edges = new List<IEdge>(
                this.edges.Select(e => 
                    new Edge(
                        vertices.Single(v => v.Value == e.From.Value),
                        vertices.Single(v => v.Value == e.From.Value),
                        e.Value)
                        )
                    );
            return new Graph(vertices, edges);
        }
    }
}
