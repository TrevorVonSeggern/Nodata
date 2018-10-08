using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
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

        private static string[] _StrFunctions = new string[]{
            TextRepresentation.StrConcat,
            TextRepresentation.StrContains,
            TextRepresentation.StrEndsWith,
            TextRepresentation.StrIndexOf,
            TextRepresentation.StrLength,
            TextRepresentation.StrReplace,
            TextRepresentation.StrStartsWith,
            TextRepresentation.StrSubString,
            TextRepresentation.StrToLower,
            TextRepresentation.StrToUpper,
            TextRepresentation.StrTrim,
        };

        public static string GetStringFunction(this Tree tree)
        {
            var text = tree.Root.Value.Text;
            if (_StrFunctions.Contains(text))
                return text;
            return null;
        }

        public static bool IsStringFunction(this Tree tree) => tree.GetStringFunction() != null;
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
            if (children.Count != 3) return null;

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

        private Expression StringFunction(Tree tree, Expression dto, IClassCache cache)
        {
            var strFunction = tree.GetStringFunction();
            if (strFunction == TextRepresentation.StrLength)
            {
                // eval child expr
                var childExpression = FilterExpression(tree.Children.First().Item2, dto, cache);
                // fail if type isn't a string
                if (childExpression.Type != typeof(string))
                    throw new ArgumentException("Tried to get length of non string property.");
                return Expression.Property(childExpression, nameof(string.Length));
            }
            if (strFunction == TextRepresentation.StrToLower ||
                strFunction == TextRepresentation.StrToUpper ||
                strFunction == TextRepresentation.StrTrim)
            {
                // eval child expr
                var child1Expression = FilterExpression(tree.Children.First().Item2, dto, cache);
                // fail if type isn't a string
                if (child1Expression.Type != typeof(string))
                    throw new ArgumentException("Can't check function endswith with argument of a non-string type.");

                string methodName = "";

                if (strFunction == TextRepresentation.StrToUpper)
                    methodName = nameof(string.ToUpper);
                else if (strFunction == TextRepresentation.StrToLower)
                    methodName = nameof(string.ToLower);
                else if (strFunction == TextRepresentation.StrTrim)
                    methodName = nameof(string.Trim);

                MethodInfo method = typeof(string).GetMethod(methodName, new Type[] { });

                return Expression.Call(child1Expression, method);
            }
            if (strFunction == TextRepresentation.StrEndsWith ||
                strFunction == TextRepresentation.StrStartsWith ||
                strFunction == TextRepresentation.StrIndexOf ||
                strFunction == TextRepresentation.StrContains)
            {
                // eval child expr
                var child1Expression = FilterExpression(tree.Children.First().Item2, dto, cache);
                var child2Expression = FilterExpression(tree.Children.Last().Item2, dto, cache);
                // fail if types aren't a string
                if (child1Expression.Type != typeof(string) || child2Expression.Type != typeof(string))
                    throw new ArgumentException("Can't check function endswith with argument of a non-string type.");

                string methodName = "";

                if (strFunction == TextRepresentation.StrEndsWith)
                    methodName = nameof(string.EndsWith);
                else if (strFunction == TextRepresentation.StrStartsWith)
                    methodName = nameof(string.StartsWith);
                else if (strFunction == TextRepresentation.StrIndexOf)
                    methodName = nameof(string.IndexOf);
                else if (strFunction == TextRepresentation.StrContains)
                    methodName = nameof(string.Contains);

                MethodInfo method = typeof(string).GetMethod(methodName, new[] { typeof(string) });

                return Expression.Call(child1Expression, method, child2Expression);
            }
            if (strFunction == TextRepresentation.StrReplace)
            {
                var childList = tree.Children.ToList();
                // eval child expr
                var child1Expression = FilterExpression(childList[0].Item2, dto, cache);
                var child2Expression = FilterExpression(childList[1].Item2, dto, cache);
                var child3Expression = FilterExpression(childList[2].Item2, dto, cache);
                // fail if types aren't a string
                if (child1Expression.Type != typeof(string) || child2Expression.Type != typeof(string) || child3Expression.Type != typeof(string))
                    throw new ArgumentException("Can't check function endswith with argument of a non-string type.");

                string methodName = nameof(string.Replace);

                MethodInfo method = typeof(string).GetMethod(methodName, new[] { typeof(string), typeof(string) });

                return Expression.Call(child1Expression, method, child2Expression, child3Expression);
            }
            if (strFunction == TextRepresentation.StrConcat)
            {
                var childList = tree.Children.ToList();
                // eval child expr
                var child1Expression = FilterExpression(childList[0].Item2, dto, cache);
                var child2Expression = FilterExpression(childList[1].Item2, dto, cache);
                // fail if types aren't a string
                if (child1Expression.Type != typeof(string) || child2Expression.Type != typeof(string))
                    throw new ArgumentException("Can't check function endswith with argument of a non-string type.");

                string methodName = null;

                if (strFunction == TextRepresentation.StrConcat)
                    methodName = nameof(string.Concat);

                MethodInfo method = typeof(string).GetMethod(methodName, new[] { typeof(string), typeof(string) });

                return Expression.Call(method, child1Expression, child2Expression);
            }
            if (strFunction == TextRepresentation.StrSubString)
            {
                var childList = tree.Children.ToList();
                // eval child expr
                var child1Expression = FilterExpression(childList[0].Item2, dto, cache);

                var integerArgs = new List<Expression>(childList.Count - 1);
                // if (childList.Count == 2)
                //     integerArgs.Add(Expression.Constant(0, typeof(int)));
                for (var i = 1; i < childList.Count; ++i)
                    integerArgs.Add(Expression.Convert(FilterExpression(childList[i].Item2, dto, cache), typeof(int)));

                // fail if types aren't a string
                if (child1Expression.Type != typeof(string))
                    throw new ArgumentException("Can't check function endswith with argument of a non-string type.");

                if (!integerArgs.All(x => x.Type == typeof(int) || x.Type == typeof(long)))
                    throw new ArgumentException("Argument not an integer for substring function.");

                MethodInfo method = typeof(string).GetMethod(nameof(string.Substring), integerArgs.Select(x => x.Type).ToArray());

                return Expression.Call(child1Expression, method, integerArgs);
            }

            throw new NotImplementedException("String function is not implemented: " + strFunction);
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
            if (tree.IsStringFunction())
                return StringFunction(tree, dto, cache);
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