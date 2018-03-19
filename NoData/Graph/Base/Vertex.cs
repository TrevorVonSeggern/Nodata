using System.Collections.Generic;
using System.Linq;

namespace NoData.Graph.Base
{
    using Interfaces;
    using System;

    /// <summary>
    /// A class representation in a graph context.
    /// </summary>
    public class Vertex : IVertex
    {
        public Vertex(object value)
        {
            this.value = value;
        }

        private readonly object value;
        public object Value { get { return value; } }

        public virtual IEnumerable<IVertex> ConnectedTo(IGraph g) => OutgoingEdges(g).Select(e => e.To).Distinct().ToList();
        public virtual IEnumerable<IVertex> ConnectedFrom(IGraph g) => IncomingEdges(g).Select(e => e.From).Distinct().ToList();

        public IEnumerable<IEdge> OutgoingEdges(IGraph g) => g.Edges.Where(e => e.From == this);
        public IEnumerable<IEdge> IncomingEdges(IGraph g) => g.Edges.Where(e => e.To == this);

        public object Clone() => MemberwiseClone();
        public override string ToString() => value.ToString();

    }
}
