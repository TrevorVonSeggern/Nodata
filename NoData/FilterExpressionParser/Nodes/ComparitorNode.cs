using NoData.Internal.TreeParser.Nodes;
using System;
using System.Linq.Expressions;

namespace NoData.Internal.TreeParser.FilterExpressionParser.Nodes
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

        private Expression IsChildPropertyAccessNull(Node child, Expression argument)
        {
            if(child.GetType() == typeof(PropertyNode) && child.Children.Count == 1)
            {
                var propertyExpression = Expression.PropertyOrField(argument, child.Token.Value);
                var conditional = Expression.Condition(Expression.NotEqual(propertyExpression, Expression.Constant(null)), Expression.Constant(true), Expression.Constant(false));
                var childNullCheck = IsChildPropertyAccessNull(child.Children[0], propertyExpression);
                if (childNullCheck == null)
                    return conditional;
                return Expression.And(conditional, childNullCheck);
            }
            return null;
        }

        public override Expression GetExpression(Expression dto)
        {
            var left = Children[0].GetExpression(dto);
            var right = Children[1].GetExpression(dto);

            Expression doComparison()
            {
                bool compare(string expected) => Token.Value.ToLowerInvariant() == expected.ToLower();
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

            var leftNullCheck = IsChildPropertyAccessNull(Children[0], dto);
            var rightNullCheck = IsChildPropertyAccessNull(Children[1], dto);

            if (leftNullCheck == null && rightNullCheck == null)
                return doComparison();
            if (leftNullCheck != null && rightNullCheck == null)
                return Expression.AndAlso(Expression.IsTrue(leftNullCheck), doComparison());
            if (leftNullCheck == null && rightNullCheck != null)
                return Expression.AndAlso(Expression.IsTrue(rightNullCheck), doComparison());
            return Expression.AndAlso(Expression.AndAlso(Expression.IsTrue(leftNullCheck), Expression.IsTrue(rightNullCheck)), doComparison());
        }
    }
}