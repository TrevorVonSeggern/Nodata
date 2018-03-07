using System.Collections.Generic;
using System.Linq.Expressions;
using System;
using System.Linq;
using System.Reflection;

namespace NoData.Internal.TreeParser.ExpandExpressionParser.Nodes
{
    using NoData.Internal.TreeParser.Nodes;
    using NoData.Internal.TreeParser.Tokenizer;
    using NoData.Internal.Utility;
    using NoData.Utility;

    public class NodeExpandProperty<TDto> : NodeExpandPropertyAbstract where TDto : class
    {
        private static NodeExpandPropertyAbstract CreateFromGeneric(string property, params object[] ctorArguments)
        {
            var propertyType = ClassPropertiesUtility<TDto>.GetPropertiesAndType[property];
            if (ClassPropertiesUtility<TDto>.GetCollections.Any(c => c.PropertyType.Name == propertyType.Name))
                propertyType = propertyType.GenericTypeArguments[0];
            var classGenericType = typeof(NodeExpandProperty<>).MakeGenericType(propertyType);
            return Activator.CreateInstance(classGenericType, ctorArguments) as NodeExpandPropertyAbstract;
        }

        protected NodeExpandProperty(char representation) : base(representation) { }
        public NodeExpandProperty() : this(NodeTokenUtilities.GetCharacterFromType(NodeTokenTypes.ExpandProperty)) { }
        public NodeExpandProperty(Node node) : this()
        {
            if (node.Token?.Type != TokenTypes.classProperties.ToString())
                throw new ArgumentException(nameof(node) + " is not representative of a class property.");
            Token = node.Token;

            if (typeof(NodeExpandPropertyAbstract).IsAssignableFrom(node.GetType()))
            {
                Children = node.Children;
                return;
            }
            // if it's not type of expand property - then we need to convert the children to expand properties also.
            foreach(var c in node.Children)
            {
                var nExpand = CreateFromGeneric(c.Token.Value, c) as NodeExpandPropertyAbstract;
                Children.Add(nExpand);
            }
        }
        public NodeExpandProperty(IEnumerable<Node> itemList) : this()
        {
            foreach (var c in itemList)
            {
                var nExpand = CreateFromGeneric(c.Token.Value, c) as NodeExpandPropertyAbstract;
                Children.Add(nExpand);
            }
        }

        public NodeExpandProperty(Node left, Node right) : this()
        {
            if(left?.Children == null || right?.Children == null)
                return;

            if (left.Token?.Value == right.Token?.Value)
                Token = left.Token;

            // if it's not type of expand property - then we need to convert the children to expand properties also.
            foreach (var c in left.Children)
            {
                var nExpand = CreateFromGeneric(c.Token.Value, c) as NodeExpandPropertyAbstract;
                Children.Add(nExpand);
            }

            // merge right into self.
            for (var i = 0; i < Children.Count; ++i)
            {
                // get a node from the right children that matches the current child. If they match, then merge.
                var rChildNode = right.Children.FirstOrDefault(x => x.Token.Value == Children[i].Token.Value);
                if (rChildNode != null) // match. Merge children.
                {
                    Children[i] = CreateFromGeneric(rChildNode.Token.Value, Children[i], rChildNode);
                    //Children[i] = new NodeExpandProperty<TDto>(Children[i] as NodeExpandPropertyAbstract, rChildNode as NodeExpandPropertyAbstract);
                }
            }
            // add right where not in children.
            foreach(var r in right.Children)
            {
                if (!Children.Any(x => x.Token.Value == r.Token.Value))
                {
                    Children.Add(CreateFromGeneric(r.Token.Value, r));
                }
            }
        }

        protected override IEnumerable<MemberBinding> GetNonExpandMemberBindings(Expression dto) => GetNonExpandMemberBindings<TDto>(dto);
        protected override IEnumerable<MemberBinding> GetNavigationPropertyMemberBindings(Expression dto)
        {
            foreach (var prop in ClassPropertiesUtility<TDto>.GetNavigationProperties.Where(x => Children.Any(c => c.Token.Value == x.Name)))
            {
                var child = Children.FirstOrDefault(x => x.Token.Value == prop.Name) as NodeExpandPropertyAbstract;
                yield return (Expression.Bind(prop, child.GetExpression(Expression.PropertyOrField(dto, prop.Name))));
            }
        }
        protected override IEnumerable<MemberBinding> GetCollectionMemberBindings(Expression dto)
        {
            foreach (var prop in ClassPropertiesUtility<TDto>.GetCollections.Where(x => Children.Any(c => c.Token.Value == x.Name)))
            {
                if (!prop.PropertyType.IsGenericType && prop.PropertyType.GenericTypeArguments.Count() != 1)
                    continue;

                var child = Children.Single(x => x.Token.Value == prop.Name) as NodeExpandPropertyAbstract;
                yield return child.GetExpressionAsCollection(dto, prop);
            }
        }

        internal override MemberAssignment GetExpressionAsCollection(Expression dto, PropertyInfo property)
        {
            var parameter = Expression.Parameter(typeof(TDto), Token.Value);
            var dtoCollectionProperty = Expression.Property(dto, Token.Value);
            var select = ExpressionBuilder.BuildSelectExpression<TDto>(dtoCollectionProperty, parameter, GetExpression(parameter));            var list = Expression.Condition(
                Expression.Equal(dtoCollectionProperty, Expression.Constant(null)),
                Expression.New(typeof(List<TDto>).GetConstructor(new Type[] { }), new Expression[] { }),
                Expression.New(typeof(List<TDto>).GetConstructor(new Type[] { typeof(IEnumerable<TDto>) }), select)
                );
            return (Expression.Bind(property, list));
        }

        protected override Expression BindingExpression(Expression dto, IEnumerable<MemberBinding> bindings)
        {
            // TODO: might need for EF
            //var newDto = Expression.New(typeof(TDto).GetConstructors().Single(), new[] { dto }, members: memberBindings);
            var newDto = Expression.New(typeof(TDto));
            var selectExpression = Expression.MemberInit(newDto, bindings);

            return Expression.Condition(Expression.Equal(dto, Expression.Constant(null)),
                Expression.Constant(null, selectExpression.Type), selectExpression);
        }

        public static NodeExpandProperty<TDto> FromLinearPropertyList(IEnumerable<Node> itemList)
        {
            if (itemList is null)
                throw new ArgumentException(nameof(itemList));

            var node = new NodeExpandProperty<TDto>();

            var list = itemList.Where(x => x.Token.Type == TokenTypes.classProperties.ToString()).ToList();
            if (list.Count() == 0)
                return node;

            // build child dependency chain.
            var childRoot = list[0];
            Node previous = childRoot;
            for (int i = 1; i < list.Count(); ++i)
            {
                previous.Children.Add(list[i]);
                previous = list[i];
            }
            
            node.Children.Add(CreateFromGeneric(childRoot.Token.Value, childRoot) as NodeExpandPropertyAbstract);
            return node;
        }

        public override IEnumerable<string> IgnoredProperties()
        {
            // self
            foreach (var prop in ClassPropertiesUtility<TDto>.GetExpandablePropertyNames.Where(x => !Children.Any(c => c.Token?.Value == x)))
                yield return prop;
            // navigation properties
            foreach (var child in Children.Where(c => ClassPropertiesUtility<TDto>.GetExpandablePropertyNames.Any(x => c.Token?.Value == x)))
                foreach (var ignoredChildrenProperties in (child as NodeExpandPropertyAbstract).IgnoredProperties())
                    yield return $"{child.Token.Value}.{ignoredChildrenProperties}";
        }
    }
}
