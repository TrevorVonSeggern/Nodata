using Immutability;
using Graph;
using NoData.QueryParser.ParsingTools.Groupers;

namespace NoData.GraphImplementations.QueryParser
{
    [Immutable]
    public class Tree : Tree<Tree, Vertex, Edge, TextInfo, EdgeInfo>, IRepresent
    {
        public string Representation => Root.Value.Representation;

        public Tree(Vertex root) : base(root, new List<ITuple<Edge, Tree>>()) { }
        public Tree(Vertex root, IEnumerable<ITuple<Edge, Tree>> children) : base(root, children) { }


        public override string ToString() => Root.ToString() + " ";
    }
}
