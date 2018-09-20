using System;
using System.Collections.Generic;
using System.Linq;
using CodeTools;

namespace NoData.GraphImplementations.Schema
{
    public class Edge : Graph.Edge<EdgeMetaData, Vertex, ClassInfo>
    {
        public override Vertex From { get; protected set; }
        public override Vertex To { get; protected set; }
        public override EdgeMetaData Value { get; protected set; }

        public Edge(Vertex from, Vertex to) : base(from, to) { }
        public Edge(Vertex from, Vertex to, EdgeMetaData metadata) : base(from, to, metadata) { }
        public Edge(Vertex from, Vertex to, string name, bool isCollection) : base(from, to, new EdgeMetaData(name, isCollection)) { }

        public override string ToString() => $"{From.ToString()}({Value.PropertyName})->{To.ToString()}";

        public Edge CloneWithNewReferences(Vertex from, Vertex to)
        {
            if (!From.Value.Equals(from.Value) || !To.Value.Equals(to.Value))
                throw new ArgumentException("Cloned vertex references don't have the same value.");
            return new Edge(from, to, (EdgeMetaData)Value.Clone());
        }
    }
}
