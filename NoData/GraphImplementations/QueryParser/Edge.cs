﻿using Immutability;

namespace NoData.GraphImplementations.QueryParser
{
    [Immutable]
    public class EdgeInfo
    {
        public string PropertyName { get; }
    }

    [Immutable]
    public class Edge : GraphLibrary.Edge<EdgeInfo, Vertex, TextInfo>
    {
        public Edge(Vertex from, Vertex to) : base(from, to) { }
    }
}
