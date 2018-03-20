using System;
using System.Linq.Expressions;

namespace NoData.Internal.TreeParser.Nodes
{
    /// <summary>
    /// Placeholder for Node classes..
    /// </summary>
    public class NodePlaceHolder : Node
    {
        public override Expression GetExpression(Expression dto)
        {
            throw new NotImplementedException();
        }

        public NodePlaceHolder(char v, Tokenizer.Token token): base (v, token)
        {
        }

        public NodePlaceHolder(char v) : base(v)
        {
        }
    }
}
