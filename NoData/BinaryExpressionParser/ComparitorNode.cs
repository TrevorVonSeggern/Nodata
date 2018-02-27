using System;
using System.Linq.Expressions;

namespace NoData.Internal.TreeParser.BinaryTreeParser
{
    /// <summary>
    /// Comparitor Node. This class represents one value compared to another.
    /// </summary>
    /// <example>Name eq 'George'</example>
    public class ComparitorNode : Node
    {
        public ComparitorNode(Node left, Node comparitor, Node right) : base('v')
        {
            Children.Add(left);
            Children.Add(right);
            Token = comparitor?.Token;
            if (Token is null)
                throw new ArgumentNullException(nameof(comparitor));
        }

        public override Expression GetExpression(ParameterExpression dto)
        {
            bool compare(string expected) => Token.Value.ToLowerInvariant() == expected.ToLower();
            var left = Children[0].GetExpression(dto);
            var right = Children[1].GetExpression(dto);
            // eq, ne, gt, ge, lt ,le
            if (compare("eq"))
                return Expression.Equal(left, right);
            else if (compare("ne"))
                return Expression.NotEqual(left, right);
            else if (compare("gt"))
                return Expression.GreaterThan(left, right);
            else if (compare("ge"))
                return Expression.GreaterThanOrEqual(left, right);
            else if (compare("lt"))
                return Expression.LessThan(left, right);
            else if (compare("le"))
                return Expression.LessThanOrEqual(left, right);

            throw new NotImplementedException();
        }
    }
}