using CodeTools;
using Graph.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace NoData.GraphImplementations.QueryParser
{
    [Immutable]
    public class EdgeInfo
    {
        public string PropertyName { get; }
    }

    [Immutable]
    public class Edge : Graph.Edge<EdgeInfo, Vertex, TextInfo>
    {
        public Edge(Vertex from, Vertex to) : base(from, to) { }
    }
}
