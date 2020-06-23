using Immutability;
using NoData.Internal.TreeParser.Tokenizer;

namespace NoData.GraphImplementations.QueryParser
{
    [Immutable]
    public class Vertex : GraphLibrary.Vertex<TextInfo>
    {
        public new TextInfo Value => base.Value as TextInfo;
        public override string ToString() => Value.ToString();

        public Vertex(TextInfo info) : base(info) { }
        public Vertex(Token token) : base(new TextInfo(token)) { }
    }
}
