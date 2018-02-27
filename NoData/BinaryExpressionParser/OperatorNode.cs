using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace NoData.Internal.TreeParser.BinaryTreeParser
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

        public override Expression GetExpression(ParameterExpression dto)
        {
            throw new NotImplementedException();
        }
    }
}
