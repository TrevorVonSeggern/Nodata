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

        public virtual IEnumerable<IVertex> ConnectedTo(IGraph g)
        {
            return g.Edges.Where(e => e.From == this).Select(e => e.To).Distinct().ToList();
        }

        public virtual IEnumerable<IVertex> ConnectedFrom(IGraph g)
        {
            return g.Edges.Where(e => e.To == this).Select(e => e.From).Distinct().ToList();
        }

        public object Clone() => MemberwiseClone();
    }
}
