using NoData.Internal.TreeParser.Tokenizer;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;

namespace NoData.GraphImplementations.QueryParser
{
    public class Vertex : Graph.Vertex<TextInfo>
    {
        public new TextInfo Value => base.Value as TextInfo;
        public override string ToString() => Value.ToString();

        public Vertex(TextInfo info) : base(info) { }
        public Vertex(Token token) : base(new TextInfo(token)) { }
    }
}
