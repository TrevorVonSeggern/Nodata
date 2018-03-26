using NoData.Graph.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace NoData.QueryParser.Graph
{
    public class EdgeInfo : ICloneable
    {
        public object Clone() => default(object);
    }

    public class Edge : NoData.Graph.Base.Edge<EdgeInfo, Vertex, TextInfo>
    {
        public new Vertex From => base.From as Vertex;
        public new Vertex To => base.To as Vertex;

        public Edge(Vertex from, Vertex to) : base(from, to) { }
    }
}
