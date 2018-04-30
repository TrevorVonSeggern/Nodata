using NoData.Graph.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace NoData.QueryParser.Graph
{
    public class Tree : Tree<Vertex, Edge, TextInfo, EdgeInfo>
    {
        public new Vertex Root => base.Root as Vertex;
        public new IEnumerable<ITuple<Edge, Tree>> Children => base.Children?.Cast<ITuple<Edge, Tree>>();

        public Tree(Vertex root) : base(root, new List<ITuple<Edge, Tree>>()) { }
        public Tree(Vertex root, IEnumerable<ITuple<Edge, Tree>> children) : base(root, children) { }

        internal static string GetRepresentationValue(Tree arg) => arg.Root.Value.Representation;

        public override string ToString() => Root.ToString() + " ";

        static readonly IReadOnlyDictionary<Type, int> NumberDictionary;
        static Tree()
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
        private int GetNumberIndex(Type type) => NumberDictionary[type];
        private Type GetTypeFromIndex(int i) => NumberDictionary.ToList().FirstOrDefault(x => x.Value == i).Key;
        private bool IsPropertyAccess => Children.Count() == 1 && (Children.First().Item2.Root.Value.Representation == TextInfo.ExpandProperty || Children.First().Item2.Root.Value.Representation == TextInfo.ClassProperty);
        private bool IsDirectPropertyAccess => IsPropertyAccess && (Children.FirstOrDefault().Item2.Root.Value.Representation == TextInfo.ClassProperty || Children.FirstOrDefault().Item2.Children.Count() == 0);

        #region Filtering Expressions

        private Expression ComparisonExpression(Expression dto)
        {
            var children = new List<ITuple<Edge, Tree>>(Children);
            if (children.Count() != 2)
                return null;

            var left = children[0].Item2.FilterExpression(dto);
            var right = children[1].Item2.FilterExpression(dto);

            if (left.Type != right.Type && IsNumberType(left.Type) && IsNumberType(right.Type))
            {
                int lIndex = GetNumberIndex(left.Type);
                int rIndex = GetNumberIndex(right.Type);
                int largerIndex = Math.Max(lIndex, rIndex);
                if (lIndex != largerIndex)
                    left = Expression.Convert(left, GetTypeFromIndex(largerIndex));
                if (rIndex != largerIndex)
                    right = Expression.Convert(right, GetTypeFromIndex(largerIndex));
            }

            Expression doComparison()
            {
                bool compare(string expected) => Root.Value.Text.ToLowerInvariant() == expected.ToLower();
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
                if (child.IsPropertyAccess)
                {
                    Expression conditional(Expression propertyExpression)
                    {
                        var nullExpr = Expression.Constant(null);
                        var trueExpr = Expression.Constant(true);
                        var falseExpr = Expression.Constant(false);
                        var NotNullCondition = Expression.NotEqual(propertyExpression, nullExpr);
                        return Expression.Condition(NotNullCondition, trueExpr, falseExpr);
                    }
                    if (!child.IsDirectPropertyAccess)
                    {
                        var subPropertyAccessCheck = IsChildPropertyAccessNull(child.Children.First().Item2, Expression.PropertyOrField(parentClass, child.Children.First().Item2.Root.Value.Text));
                        if(subPropertyAccessCheck is null)
                           return conditional(parentClass);
                        return Expression.AndAlso(conditional(parentClass), subPropertyAccessCheck);
                    }
                    else
                        return conditional(parentClass);
                    //return Expression.Constant(true);
                }
                return null;
            }

            var leftNullCheck = IsChildPropertyAccessNull(children[0].Item2, dto);
            var rightNullCheck = IsChildPropertyAccessNull(children[1].Item2, dto);

            if (leftNullCheck == null && rightNullCheck == null)
                return doComparison();
            if (leftNullCheck != null && rightNullCheck == null)
                return Expression.AndAlso(leftNullCheck, doComparison());
                //return Expression.AndAlso(Expression.IsTrue(leftNullCheck), Expression.Constant(true));
            if (leftNullCheck == null && rightNullCheck != null)
                return Expression.AndAlso(rightNullCheck, doComparison());
            return Expression.AndAlso(Expression.AndAlso(leftNullCheck, rightNullCheck), doComparison());
        }

        private Expression LogicalExpression(Expression dto)
        {
            var children = new List<ITuple<Edge, Tree>>(Children);
            if (children.Count() != 2) return null;

            var left = children[0].Item2.FilterExpression(dto);
            var right = children[1].Item2.FilterExpression(dto);

            bool compare(string expected) => Root.Value.Text.ToLowerInvariant() == expected.ToLower();

            if (compare("and"))
                return Expression.And(left, right);
            if (compare("or"))
                return Expression.Or(left, right);

            throw new NotImplementedException();
        }

        private Expression BoolExpression(Expression dto)
        {
            throw new NotImplementedException();
        }

        public Expression FilterExpression(Expression dto)
        {
            if (IsDirectPropertyAccess)
                return Expression.PropertyOrField(dto, Children.FirstOrDefault().Item2.Root.Value.Text);
            if (IsPropertyAccess)
                return Children.FirstOrDefault().Item2.FilterExpression(Expression.PropertyOrField(dto, Children.FirstOrDefault().Item2.Root.Value.Value));
            if (Root.Value.Value == TextInfo.ValueComparison)
                return ComparisonExpression(dto);
            if (Root.Value.Value == TextInfo.LogicalComparison)
                return LogicalExpression(dto);
            if (Root.Value.Representation == TextInfo.BooleanValue)
                return BoolExpression(dto);
            if (Root.Value.Representation == TextInfo.NumberValue)
                return Expression.Constant(Root.Value.Parsed, Root.Value.Type);
            if (Root.Value.Representation == TextInfo.TextValue)
                return Expression.Constant(Root.Value.Text.Substring(1, Root.Value.Text.Length - 2));

            throw new NotImplementedException();
        }

        #endregion
    }
}
