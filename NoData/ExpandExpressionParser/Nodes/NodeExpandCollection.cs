using System.Collections.Generic;
using System.Linq.Expressions;

namespace NoData.Internal.TreeParser.ExpandExpressionParser.Nodes
{
    using System;

    public class NodeExpandCollection<TDto> : NodeExpandProperty<TDto> where TDto : class
    {
        public NodeExpandCollection(IEnumerable<NodeExpandProperty<TDto>> nodes) : base(NodeTokenUtilities.GetCharacterFromType(NodeTokenTypes.ExpandCollection))
        {
            var expandNodeList = new List<NodeExpandProperty<TDto>>(nodes);
            if (expandNodeList.Count == 0)
                throw new ArgumentException(nameof(nodes));

            while(expandNodeList.Count > 1)
            {
                expandNodeList[0] = new NodeExpandProperty<TDto>(expandNodeList[0], expandNodeList[1]);
                expandNodeList.RemoveAt(1);
            }
            Children = expandNodeList[0].Children;
        }
    }
}
