using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using CodeTools;
using NoData.GraphImplementations.QueryParser;
using NoData.Utility;
using GraphSchema = NoData.GraphImplementations.Schema.GraphSchema;

namespace NoData.QueryParser.ParsingTools
{
    [Immutable]
    public static class TreeExpressionExtensions
    {
        public static bool IsPropertyAccess(this Tree tree) =>
                            tree.Root.Value.Representation == TextRepresentation.ExpandProperty
                            || tree.Root.Value.Representation == TextRepresentation.ClassProperty;
        public static bool IsDirectPropertyAccess(this Tree tree) => tree.IsPropertyAccess() && !tree.Children.Any();
        public static bool IsCollectionPropertyAccess(this Tree tree, GraphSchema graph) => tree.IsPropertyAccess();
        public static bool IsFakeExpandProperty(this Tree tree) => tree.IsPropertyAccess() && tree.Root.Value.Type == typeof(TextInfo);
    }

    [Immutable]
    public class FilterTreeExpressionBuilder
    {
        public FilterTreeExpressionBuilder(GraphSchema graph)
        {

        }

        static readonly IReadOnlyDictionary<Type, int> NumberDictionary;
        static FilterTreeExpressionBuilder()
        {
            int index = 1;
            var nDict = new Dictionary<Type, int>
            {
                { typeof(short), index++ },
                { typeof(int), index++ },
                { typeof(long), index++ },
                { typeof(float), index++ },
                { typeof(double), index++ },
                { typeof(decimal), index++ },
            };
            NumberDictionary = nDict;
        }

        private bool IsNumberType(Type type) => NumberDictionary.ContainsKey(type);
        private int IndexFromType(Type type) => NumberDictionary[type];
        private Type TypeFromIndex(int i) => NumberDictionary.ToList().FirstOrDefault(x => x.Value == i).Key;

        private Expression ComparisonExpression(Tree tree, Expression dto, IClassCache cache)
        {
            var children = new List<Graph.ITuple<Edge, Tree>>(tree.Children);
            if (children.Count != 3)
                return null;

            var left = FilterExpression(children[0].Item2, dto, cache);
            var right = FilterExpression(children[2].Item2, dto, cache);

            if (left.Type != right.Type && IsNumberType(left.Type) && IsNumberType(right.Type))
            {
                int lIndex = IndexFromType(left.Type);
                int rIndex = IndexFromType(right.Type);
                int largerIndex = Math.Max(lIndex, rIndex);
                if (lIndex != largerIndex)
                    left = Expression.Convert(left, TypeFromIndex(largerIndex));
                if (rIndex != largerIndex)
                    right = Expression.Convert(right, TypeFromIndex(largerIndex));
            }

            Expression doComparison()
            {
                var compareText = tree.Children.ToList()[1].Item2.Root.Value.Text.ToLower();
                bool compare(string expected) => compareText == expected.ToLower();
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
            Expression IsChildPropertyAccessNull(Tree child, Expression parentClass)
            {
                if (child.IsPropertyAccess())
                {
                    Expression conditional(Expression propertyExpression)
                    {
                        var nullExpr = Expression.Constant(null);
                        if (propertyExpression.Type.IsValueType)
                            return null;
                        return Expression.NotEqual(propertyExpression, nullExpr);
                    }
                    if (!child.IsDirectPropertyAccess())
                    {
                        var subPropertyAccessCheck = IsChildPropertyAccessNull(child.Children.First().Item2, Expression.PropertyOrField(parentClass, child.Root.Value.Text));
                        if (subPropertyAccessCheck is null)
                            return conditional(parentClass);
                        return Expression.AndAlso(conditional(parentClass), subPropertyAccessCheck);
                    }
                    else
                        return conditional(parentClass);
                }
                return null;
            }

            var leftNullCheck = IsChildPropertyAccessNull(children[0].Item2, dto);
            var rightNullCheck = IsChildPropertyAccessNull(children[1].Item2, dto);

            if (leftNullCheck == null && rightNullCheck == null)
                return doComparison();
            if (leftNullCheck != null && rightNullCheck == null)
                return Expression.AndAlso(leftNullCheck, doComparison());
            if (leftNullCheck == null && rightNullCheck != null)
                return Expression.AndAlso(rightNullCheck, doComparison());
            return Expression.AndAlso(Expression.AndAlso(leftNullCheck, rightNullCheck), doComparison());
        }

        private Expression LogicalExpression(Tree tree, Expression dto, IClassCache cache)
        {
            var children = new List<Graph.ITuple<Edge, Tree>>(tree.Children);
            if (children.Count != 2) return null;

            var left = FilterExpression(children[0].Item2, dto, cache);
            var right = FilterExpression(children[2].Item2, dto, cache);

            var compareText = tree.Children.ToList()[1].Item2.Root.Value.Text.ToLower();
            bool compare(string expected) => compareText == expected.ToLower();

            if (compare("and"))
                return Expression.And(left, right);
            if (compare("or"))
                return Expression.Or(left, right);

            throw new NotImplementedException();
        }

        private Expression BoolExpression(Tree tree)
        {
            if (tree.Root.Value.Text.Equals("true", StringComparison.OrdinalIgnoreCase))
                return Expression.Constant(true);
            if (tree.Root.Value.Text.Equals("false", StringComparison.OrdinalIgnoreCase))
                return Expression.Constant(false);
            throw new NotImplementedException();
        }

        public Expression FilterExpression(Tree tree, Expression dto, IClassCache cache)
        {
            if (tree.IsDirectPropertyAccess())
                return Expression.PropertyOrField(dto, tree.Root.Value.Text);
            if (tree.IsPropertyAccess())
            {
                if (cache.GetOrAdd(dto.Type).CollectionNames.Contains(tree.Root.Value.Text))
                    return Expression.Empty();
                return FilterExpression(tree.Children.FirstOrDefault().Item2, Expression.PropertyOrField(dto, tree.Root.Value.Text), cache);
            }
            if (tree.Root.Value.Text == TextRepresentation.ValueComparison)
                return ComparisonExpression(tree, dto, cache);
            if (tree.Root.Value.Text == TextRepresentation.LogicalComparison)
                return LogicalExpression(tree, dto, cache);
            if (tree.Representation == TextRepresentation.BooleanValue)
                return BoolExpression(tree);
            if (tree.Representation == TextRepresentation.NumberValue)
                return Expression.Constant(tree.Root.Value.Parsed, tree.Root.Value.Type);
            if (tree.Representation == TextRepresentation.TextValue)
                return Expression.Constant(tree.Root.Value.Text.Substring(1, tree.Root.Value.Text.Length - 2));

            throw new NotImplementedException();
        }
    }
}