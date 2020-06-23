using System;
using System.Collections.Generic;
using System.Linq;
using Immutability;

namespace NoData.GraphImplementations.Schema
{
    [Immutable]
    public class SubGraphSchema : GraphLibrary.Graph<Vertex, Edge, ClassInfo, Property>
    {
        public SubGraphSchema(IEnumerable<Vertex> vertices, IEnumerable<Edge> edges) : base(vertices, edges) { }

        public Vertex VertexContainingType(Type type) => Vertices.First(v => v.Value.TypeId.Equals(type));
    }
}
