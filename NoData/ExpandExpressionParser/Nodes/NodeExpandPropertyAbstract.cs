using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace NoData.Internal.TreeParser.ExpandExpressionParser.Nodes
{
    using NoData.Internal.TreeParser.Nodes;
    using NoData.Internal.Utility;

    public abstract class NodeExpandPropertyAbstract : Node
    {
        protected NodeExpandPropertyAbstract(char representation) : base(representation) { }

        abstract protected IEnumerable<MemberBinding> GetNonExpandMemberBindings(Expression dto);
        abstract protected IEnumerable<MemberBinding> GetNavigationPropertyMemberBindings(Expression dto);
        abstract protected IEnumerable<MemberBinding> GetCollectionMemberBindings(Expression dto);
        protected IEnumerable<MemberBinding> GetNonExpandMemberBindings<TDto>(Expression dto)
        {
            // Bind all the non-expandable types.
            foreach (var prop in ClassPropertiesUtility<TDto>.GetNonExpandableProperties)
                yield return (Expression.Bind(prop, Expression.PropertyOrField(dto, prop.Name)));
        }

        internal abstract MemberAssignment GetExpressionAsCollection(Expression dto, PropertyInfo property);

        public override Expression GetExpression(ParameterExpression dto) => GetExpression(dto as Expression);
        public Expression GetExpression(Expression dto)
        {
            var memberBindings = new List<MemberBinding>();
            memberBindings.AddRange(GetNonExpandMemberBindings(dto));
            memberBindings.AddRange(GetNavigationPropertyMemberBindings(dto));
            memberBindings.AddRange(GetCollectionMemberBindings(dto));
            return BindingExpression(dto, memberBindings);
        }

        protected abstract Expression BindingExpression(Expression dto, IEnumerable<MemberBinding> bindings);
    }
}
