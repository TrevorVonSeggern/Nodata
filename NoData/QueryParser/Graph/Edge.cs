using NoData.Graph.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace NoData.QueryParser.Graph
{
    public class EdgeInfo : ICloneable
    {
        public object Clone() => default(object);
        public string PropertyName { get; set; }
    }

    public class Edge : NoData.Graph.Base.Edge<EdgeInfo, Vertex, TextInfo>
    {
        public override Vertex From { get; protected set; }
        public override Vertex To { get; protected set; }

        public Edge(Vertex from, Vertex to) : base(from, to) { }
    }
}
