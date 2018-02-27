using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace NoData.Internal.TreeParser.BinaryTreeParser
{
    /// <summary>
    /// Placeholder for Node classes..
    /// </summary>
    public class NodePlaceHolder : Node
    {
        public override Expression GetExpression(ParameterExpression dto)
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
