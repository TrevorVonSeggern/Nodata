using System;
using System.Collections.Generic;
using System.Linq;

namespace NoData.Graph
{
    public class Edge : Base.Edge<EdgeMetaData, Vertex, ClassInfo>
    {
        public new Vertex From => base.From as Vertex;
        public new Vertex To => base.To as Vertex;
        public new EdgeMetaData Value => base.Value as EdgeMetaData;

        public Edge(Vertex from, Vertex to) : base(from, to) { }
        public Edge(Vertex from, Vertex to, EdgeMetaData metadata) : base(from, to, metadata) { }
        public Edge(Vertex from, Vertex to, string name, bool isCollection) : base(from, to, new EdgeMetaData(name, isCollection)) { }

        public override string ToString() => $"{From.ToString()}({Value.PropertyName})->{To.ToString()}";
    }
}
