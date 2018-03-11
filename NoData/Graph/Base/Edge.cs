using System;
using System.Collections.Generic;
using System.Linq;

namespace NoData.Graph.Base
{
    using Interfaces;

    public class Edge : IEdge
    {
        private readonly IVertex from;
        private readonly IVertex to;
        private readonly bool hasMany = false;
        private readonly string name;

        public IVertex From { get { return from; } }
        public IVertex To { get { return to; } }
        public bool HasMany { get { return hasMany; } }
        public string Name { get { return name; } }

        public Edge(IVertex from, IVertex to)
        {
            this.from = from ?? throw new ArgumentNullException(nameof(from));
            this.to = to ?? throw new ArgumentNullException(nameof(to));
        }

        public Edge(IVertex from, IVertex to, string name, bool isCollection) : this(from, to)
        {
            this.name = name ?? throw new ArgumentNullException(nameof(name));
            hasMany = isCollection;
        }

        public bool IsFullyConnected(IGraph g) => IsFullyConnected(g.Vertices);
        public bool IsFullyConnected(IEnumerable<IVertex> vertices)
        {
            if (from != null && to != null)
                if (from == to) // only perform one check when possible
                    return vertices.Any(v => v == from);
                else
                    return vertices.Any(v => v == from) && vertices.Any(v => v == to);
            return false;
        }

        public override bool Equals(object obj)
        {
            if(obj is Edge)
            {
                var other = obj as Edge;
                return From == other.From && to == other.to && Name == other.Name /*&& HasMany == other.HasMany*/;
            }
            return false;
        }

        public object Clone() => MemberwiseClone();
    }
}
