﻿using NoData.Graph.Base;
using NoData.QueryParser.ParsingTools.Groupers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace NoData.QueryParser.Graph
{
    public class Tree : Tree<Vertex, Edge, TextInfo, EdgeInfo>, IRepresent
    {
        public override Vertex Root { get; protected set; }
        public new IEnumerable<ITuple<Edge, Tree>> Children => base.Children?.Cast<ITuple<Edge, Tree>>();
        public string Representation => Root.Value.Representation;

        public Tree(Vertex root) : base(root, new List<ITuple<Edge, Tree>>()) { }
        public Tree(Vertex root, IEnumerable<ITuple<Edge, Tree>> children) : base(root, children) { }


        public bool IsPropertyAccess => Root.Value.Representation == TextInfo.ExpandProperty || Root.Value.Representation == TextInfo.ClassProperty;
        public bool IsDirectPropertyAccess => IsPropertyAccess && !Children.Any();
        public bool IsFakeExpandProperty => IsPropertyAccess && Root.Value.Type == typeof(TextInfo);

        public override string ToString() => Root.ToString() + " ";


        #region Filtering Expressions

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
        private int IndexFromType(Type type) => NumberDictionary[type];
        private Type TypeFromIndex(int i) => NumberDictionary.ToList().FirstOrDefault(x => x.Value == i).Key;

        private Expression ComparisonExpression(Expression dto)
        {
            var children = new List<ITuple<Edge, Tree>>(Children);
            if (children.Count != 2)
                return null;

            var left = children[0].Item2.FilterExpression(dto);
            var right = children[1].Item2.FilterExpression(dto);

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
                        if (propertyExpression.Type.IsValueType)
                            return null;
                        return Expression.NotEqual(propertyExpression, nullExpr);
                    }
                    if (!child.IsDirectPropertyAccess)
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

        private Expression LogicalExpression(Expression dto)
        {
            var children = new List<ITuple<Edge, Tree>>(Children);
            if (children.Count != 2) return null;

            var left = children[0].Item2.FilterExpression(dto);
            var right = children[1].Item2.FilterExpression(dto);

            bool compare(string expected) => Root.Value.Text.ToLowerInvariant() == expected.ToLower();

            if (compare("and"))
                return Expression.And(left, right);
            if (compare("or"))
                return Expression.Or(left, right);

            throw new NotImplementedException();
        }

        private Expression BoolExpression()
        {
            if (Root.Value.Value.Equals("true", StringComparison.OrdinalIgnoreCase))
                return Expression.Constant(true);
            if (Root.Value.Value.Equals("false", StringComparison.OrdinalIgnoreCase))
                return Expression.Constant(false);
            throw new NotImplementedException();
        }

        public Expression FilterExpression(Expression dto)
        {
            if (IsDirectPropertyAccess)
                return Expression.PropertyOrField(dto, Root.Value.Text);
            if (IsPropertyAccess)
                return Children.FirstOrDefault().Item2.FilterExpression(Expression.PropertyOrField(dto, Root.Value.Value));
            if (Root.Value.Value == TextInfo.ValueComparison)
                return ComparisonExpression(dto);
            if (Root.Value.Value == TextInfo.LogicalComparison)
                return LogicalExpression(dto);
            if (Root.Value.Representation == TextInfo.BooleanValue)
                return BoolExpression();
            if (Root.Value.Representation == TextInfo.NumberValue)
                return Expression.Constant(Root.Value.Parsed, Root.Value.Type);
            if (Root.Value.Representation == TextInfo.TextValue)
                return Expression.Constant(Root.Value.Text.Substring(1, Root.Value.Text.Length - 2));

            throw new NotImplementedException();
        }

        #endregion
    }
}
