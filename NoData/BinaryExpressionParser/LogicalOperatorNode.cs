using System;
using System.Linq.Expressions;

namespace NoData.Internal.TreeParser.BinaryTreeParser
{
    /// <summary>
    /// Logical Operator Node. This does a binary comparison on two values that will evaluate to a boolean.
    /// </summary>
    /// <example>true or false. true and true. false and true. true or false.</example>
    public class LogicalOperatorNode : Node
    {
        public LogicalOperatorNode(Node left, Node binaryOperator, Node right) : base('v')
        {
            Children.Add(left);
            Children.Add(right);
            Token = binaryOperator?.Token;
            if (Token is null)
                throw new ArgumentNullException(nameof(binaryOperator));
        }

        public override Expression GetExpression(ParameterExpression dto)
        {
            bool compare(string expected) => Token.Value.ToLowerInvariant() == expected.ToLower();
            var left = Children[0].GetExpression(dto);
            var right = Children[1].GetExpression(dto);

            if (compare("and"))
                return Expression.And(left, right);
            else if (compare("or"))
                return Expression.Or(left, right);

            throw new NotImplementedException();
        }
    }
}
