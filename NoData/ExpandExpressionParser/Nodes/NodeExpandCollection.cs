using System.Collections.Generic;
using System.Linq.Expressions;

namespace NoData.Internal.TreeParser.ExpandExpressionParser.Nodes
{
    using System;
    using System.Linq;
    using NoData.Internal.TreeParser.Nodes;
    using NoData.Internal.TreeParser.Tokenizer;

    public class NodeExpandCollection<TDto> : NodeExpandProperty<TDto> where TDto : class
    {
        private List<NodeExpandProperty<TDto>> expandNodeList { get; set; }

        public NodeExpandCollection(IEnumerable<NodeExpandProperty<TDto>> nodes) : base(NodeTokenUtilities.GetCharacterFromType(NodeTokenTypes.ExpandCollection))
        {
            expandNodeList = new List<NodeExpandProperty<TDto>>(nodes);
            if (expandNodeList.Count == 0)
                throw new ArgumentException(nameof(nodes));

            while(expandNodeList.Count > 1)
            {
                expandNodeList[0] = new NodeExpandProperty<TDto>(expandNodeList[0], expandNodeList[1]);
                expandNodeList.RemoveAt(1);
            }
        }

        public NodeExpandCollection(IEnumerable<NodePlaceHolder> nodes) : base(NodeTokenUtilities.GetCharacterFromType(NodeTokenTypes.ExpandCollection))
        {
            expandNodeList = new List<NodePlaceHolder>(nodes)
                .Where(x => x.Token.Type == TokenTypes.classProperties.ToString())
                .Select(x => new NodeExpandProperty<TDto>(x))
                .ToList();

            if (expandNodeList.Count == 0)
                throw new ArgumentException(nameof(nodes));
        }

        public NodeExpandCollection() : base(NodeTokenUtilities.GetCharacterFromType(NodeTokenTypes.ExpandCollection))
        {
            expandNodeList = new List<NodeExpandProperty<TDto>>();
            expandNodeList.Add(new NodeExpandProperty<TDto>());
        }

        public override Expression GetExpression(ParameterExpression dto)
        {
            var memberBindings = new List<MemberBinding>();
            memberBindings.AddRange(expandNodeList[0].GetNonExpandMemberBindings(dto));

            void SafeAdd(List<MemberBinding> other)
            {
                foreach (var member in other)
                    if (!memberBindings.Any(x => x.Member.Name == member.Member.Name))
                        memberBindings.Add(member);
            }

            foreach(var childExpand in expandNodeList)
            {
                SafeAdd(childExpand.GetNavigationPropertyMemberBindings(dto).ToList());
                SafeAdd(childExpand.GetCollectionMemberBindings(dto).ToList());
            }

            return BindingExpression(dto, memberBindings);
        }

        private Expression BindingExpression(Expression dto, IEnumerable<MemberBinding> bindings)
        {
            //var newDto = Expression.New(typeof(TDto).GetConstructors().Single(), new[] { dto }, members: memberBindings); // TODO: might need for EF
            var newDto = Expression.New(typeof(TDto));
            var selectExpression = Expression.MemberInit(newDto, bindings);

            return Expression.Condition(Expression.Equal(dto, Expression.Constant(null)),
                Expression.Constant(null, selectExpression.Type), selectExpression);
        }
    }
}
