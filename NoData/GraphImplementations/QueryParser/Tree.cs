using Immutability;
using NoData.QueryParser.ParsingTools.Groupers;
using System.Collections.Generic;
using GraphLibrary;

namespace NoData.GraphImplementations.QueryParser
{
    [Immutable]
    public class Tree : GraphLibrary.Tree<Tree, Vertex, Edge, TextInfo, EdgeInfo>, IRepresent
    {
        public string Representation => Root.Value.Representation;

        public Tree(Vertex root) : base(root, new List<ITuple<Edge, Tree>>()) { }
        public Tree(Vertex root, IEnumerable<ITuple<Edge, Tree>> children) : base(root, children) { }


        public override string ToString() => Root.ToString() + " ";
    }
}
