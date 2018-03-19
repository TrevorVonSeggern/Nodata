using System;
using System.Collections.Generic;
using System.Linq;

namespace NoData.Graph
{
    using Interfaces;

    public class EdgeMetaData
    {
        public EdgeMetaData(string name, bool isCollection)
        {
            PropertyName = name;
            IsCollection = isCollection;
        }

        public string PropertyName { get; set; }
        public bool IsCollection { get; set; }
    }

    public class Edge : Base.Edge
    {
        public new Vertex From => base.From as Vertex;
        public new Vertex To => base.To as Vertex;
        public new EdgeMetaData Value => base.Value as EdgeMetaData;

        public Edge(IVertex from, IVertex to) : base(from, to) { }
        public Edge(IVertex from, IVertex to, EdgeMetaData metadata) : base(from, to, metadata) { }
        public Edge(IVertex from, IVertex to, string name, bool isCollection) : base(from, to, new EdgeMetaData(name, isCollection)) { }

        public override string ToString() => $"{From.ToString()}({Value.PropertyName})->{To.ToString()}";
    }
}
