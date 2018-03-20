using NoData.Internal.TreeParser.Nodes;
using System;
using System.Linq.Expressions;

namespace NoData.Internal.TreeParser.FilterExpressionParser.Nodes
{
    /// <summary>
    /// Inverse Operator Node. Applies the inverse of a boolean property or value.
    /// </summary>
    /// <example>!a. !true.</example>
    public class InverseOperatorNode : Node
    {
        public InverseOperatorNode(Node inverse, Node value) : base('v')
        {
            Children.Add(value);
        }

        public override Expression GetExpression(Expression dto)
        {
            throw new NotImplementedException();
        }
    }
}
