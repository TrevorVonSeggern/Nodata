using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace NoData.Internal.TreeParser.BinaryTreeParser
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

        public override Expression GetExpression(ParameterExpression dto)
        {
            throw new NotImplementedException();
        }
    }
}
