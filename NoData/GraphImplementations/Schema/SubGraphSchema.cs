using System;
using System.Collections.Generic;
using System.Linq;
using Immutability;
using NoData.Utility;

namespace NoData.GraphImplementations.Schema
{
    [Immutable]
    public class SubGraphSchema : Graph.Graph<Vertex, Edge, ClassInfo, Property>
    {
        public SubGraphSchema(IEnumerable<Vertex> vertices, IEnumerable<Edge> edges) : base(vertices, edges) { }

        public Vertex VertexContainingType(Type type) => Vertices.First(v => v.Value.TypeId.Equals(type));
    }
}
