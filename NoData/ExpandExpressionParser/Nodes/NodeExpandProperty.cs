using System.Collections.Generic;
using System.Linq.Expressions;

namespace NoData.Internal.TreeParser.ExpandExpressionParser.Nodes
{
    using NoData.Internal.TreeParser.Nodes;
    using NoData.Internal.TreeParser.Tokenizer;
    using NoData.Internal.Utility;
    using NoData.Utility;
    using System;
    using System.Linq;
    using System.Reflection;

    public class NodeExpandProperty<TDto> : Node where TDto : class
    {
        protected NodeExpandProperty(char representation) : base(representation) { }
        public NodeExpandProperty() : base(NodeTokenUtilities.GetCharacterFromType(NodeTokenTypes.ExpandProperty)) { }
        public NodeExpandProperty(Node node) : this()
        {
            if (node.Token?.Type != TokenTypes.classProperties.ToString())
                throw new ArgumentException(nameof(node) + " is not representative of a class property.");
            Children.Add(node);
        }

        public NodeExpandProperty(Node left, Node right) : this()
        {
            if(left?.Children == null || right?.Children == null)
                return;

            Children = new List<Node>(left.Children);
            Token = new Token(TokenTypes.classProperties.ToString(), new TokenPosition(0, 0));

            // merge right into self.
            for (var i = 0; i < Children.Count; ++i)
            {
                // get a node from the right children that matches the current child. If they match, then merge.
                var rChildNode = right.Children.FirstOrDefault(x => x.Token.Value == Children[i].Token.Value);
                if (rChildNode != null) // match. Merge children.
                    Children[i] = new NodeExpandProperty<TDto>(Children[i], rChildNode);
            }
            // add right where not in children.
            foreach(var r in right.Children)
            {
                if (!Children.Any(x => x.Token.Value == r.Token.Value))
                    Children.Add(r);
            }
        }

        public NodeExpandProperty(IEnumerable<Node> itemList) : this()
        {
            if (itemList is null)
                throw new ArgumentException(nameof(itemList));

            var list = itemList.Where(x => x.Token.Type == TokenTypes.classProperties.ToString()).ToList();

            if (list.Count() == 0)
                return;

            // build child dependency chain.
            var childRoot = list[0];
            Node previous = childRoot;
            for(int i = 1; i < list.Count(); ++i)
            {
                previous.Children.Add(list[i]);
                previous = list[i];
            }
            Children.Add(childRoot);
        }

        internal IEnumerable<MemberBinding> GetNonExpandMemberBindings(Expression dto)
        {
            // Bind all the non-expandable types.
            foreach (var prop in ClassPropertiesUtility<TDto>.GetNonExpandableProperties)
                yield return (Expression.Bind(prop, Expression.PropertyOrField(dto, prop.Name)));
        }

        internal IEnumerable<MemberBinding> GetNavigationPropertyMemberBindings(Expression dto)
        {
            foreach (var prop in ClassPropertiesUtility<TDto>.GetNavigationProperties.Where(x => Children.Any(c => c.Token.Value == x.Name)))
            {
                // Need to get the expression of a select without all the expandable child properties.
                var selectPropertyType = typeof(NodeExpandProperty<>).MakeGenericType(prop.PropertyType);
                object subExpandObject;

                var child = Children.FirstOrDefault(x => x.Token.Value == prop.Name);
                if (child != null)
                    subExpandObject = Activator.CreateInstance(selectPropertyType, child.Children);
                else
                    subExpandObject = Activator.CreateInstance(selectPropertyType);

                var methodInfo = selectPropertyType.GetMethod(nameof(GetExpression), new[] { typeof(Expression) });

                var subSelectExpression = (Expression)methodInfo.Invoke(subExpandObject, new[] { Expression.PropertyOrField(dto as Expression, prop.Name) });

                yield return (Expression.Bind(prop, subSelectExpression));
            }
        }

        internal IEnumerable<MemberBinding> GetCollectionMemberBindings(ParameterExpression dto)
        {
            foreach (var prop in ClassPropertiesUtility<TDto>.GetCollections.Where(x => Children.Any(c => c.Token.Value == x.Name)))
            {
                if (!prop.PropertyType.IsGenericType && prop.PropertyType.GenericTypeArguments.Count() != 1)
                    continue;

                var genericType = prop.PropertyType.GenericTypeArguments[0];
                // Need to get the expression of a select without all the expandable child properties.
                var expr = (MemberBinding)GenericHelper.CreateAndCallMethodOnStaticClass(
                    typeof(NodeExpandPropertyHelper),
                    new[] { genericType },
                    nameof(NodeExpandPropertyHelper.GetCollectionBinding),
                    new[] { typeof(Expression), typeof(PropertyInfo), typeof(IEnumerable<Node>) },
                    new object[] { dto, prop, Children }
                    );

                yield return expr;
            }
        }


        public override Expression GetExpression(ParameterExpression dto)
        {
            var memberBindings = new List<MemberBinding>();
            memberBindings.AddRange(GetNonExpandMemberBindings(dto));
            memberBindings.AddRange(GetNavigationPropertyMemberBindings(dto as Expression));
            memberBindings.AddRange(GetCollectionMemberBindings(dto));
            return BindingExpression(dto, memberBindings);
        }

        public Expression GetExpression(Expression dto)
        {
            var memberBindings = new List<MemberBinding>();
            memberBindings.AddRange(GetNonExpandMemberBindings(dto));
            memberBindings.AddRange(GetNavigationPropertyMemberBindings(dto));
            return BindingExpression(dto, memberBindings);
        }

        private Expression BindingExpression(Expression dto, IEnumerable<MemberBinding> bindings)
        {
            // TODO: might need for EF
            //var newDto = Expression.New(typeof(TDto).GetConstructors().Single(), new[] { dto }, members: memberBindings);
            var newDto = Expression.New(typeof(TDto));
            var selectExpression = Expression.MemberInit(newDto, bindings);

            return Expression.Condition(Expression.Equal(dto, Expression.Constant(null)),
                Expression.Constant(null, selectExpression.Type), selectExpression);
        }
    }

    static class NodeExpandPropertyHelper
    {
        public static MemberBinding GetCollectionBinding<TMember>(Expression dto, PropertyInfo prop, IEnumerable<Node> Children) where TMember : class
        {
            var parameter = Expression.Parameter(typeof(TMember), prop.Name);
            var dtoCollectionProperty = Expression.Property(dto, prop.Name);
            var nullExpression = Expression.Constant(null);

            NodeExpandProperty<TMember> node;
            if (Children.Any())
                node = new NodeExpandProperty<TMember>(Children);
            else
                node = new NodeExpandProperty<TMember>(new NodePlaceHolder(' ', new Token(TokenTypes.classProperties, "", new TokenPosition(0, 0))));

            var body = node.GetExpression(parameter);

            var expr = ExpressionBuilder.BuildSelectExpression<TMember>(dtoCollectionProperty, parameter, body);

            var list = Expression.Condition(
                Expression.Equal(dtoCollectionProperty, nullExpression),
                Expression.New(typeof(List<TMember>).GetConstructor(new Type[] { }), new Expression[] { }),
                Expression.New(typeof(List<TMember>).GetConstructor(new Type[] { typeof(IEnumerable<TMember>) }), expr)
                );
            return (Expression.Bind(prop, list));
        }

        private static Expression AccessMember(Expression obj, string propertyName)
        {
            string[] parts = propertyName.Split(new char[] { '.' }, 2);
            Expression member = Expression.PropertyOrField(obj, parts[0]);

            if (parts.Length > 1)
                member = AccessMember(member, parts[1]);

            return Expression.Condition(Expression.Equal(obj, Expression.Constant(null)),
                Expression.Constant(null, member.Type), member);
        }
    }
}
