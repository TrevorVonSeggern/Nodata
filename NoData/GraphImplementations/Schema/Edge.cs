using System;
using System.Collections.Generic;
using System.Linq;
using CodeTools;

namespace NoData.GraphImplementations.Schema
{
    [Immutable]
    public class Edge : Graph.Edge<Property, Vertex, ClassInfo>
    {
        public Edge(Vertex from, Vertex to, Property metadata) : base(from, to, metadata) { }
        // public Edge(Vertex from, Vertex to, string name, bool isCollection) : base(from, to, new EdgeMetaData(name, isCollection)) { }

        public override string ToString() => $"{From.ToString()}({Value.Name})->{To.ToString()}";
    }
}
