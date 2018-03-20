using NoData.Internal.TreeParser.Nodes;
using System;
using System.Linq.Expressions;

namespace NoData.Internal.TreeParser.FilterExpressionParser.Nodes
{
    /// <summary>
    /// Operator node. An operator is a symbol that combines to two values or properties.
    /// </summary>
    /// <example>"combined with " + "an operator"</example>
    public class OperatorNode : Node
    {
        public OperatorNode() : base('v')
        {
        }

        public override Expression GetExpression(Expression dto)
        {
            throw new NotImplementedException();
        }
    }
}
