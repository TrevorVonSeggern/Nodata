using NoData.Graph.Base;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NoData.QueryParser.Graph
{
    public class Tree : Tree<Vertex, Edge, TextInfo, EdgeInfo>
    {
        public new Vertex Root => base.Root as Vertex;
        public new IEnumerable<Tuple<Edge, Tree>> Children => base.Children?.Cast<Tuple<Edge, Tree>>();

        public Tree(Vertex root) : base(root, new List<ITuple<Edge, Tree>>()) { }
        public Tree(Vertex root, IEnumerable<ITuple<Edge, Tree>> children) : base(root, children) { }

        internal static string GetRepresentationValue(Tree arg) => arg.Root.Value.Representation;

        public override string ToString() => Root.ToString() + " ";
    }
}
