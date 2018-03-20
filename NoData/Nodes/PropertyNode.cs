using System;
using System.Linq.Expressions;

namespace NoData.Internal.TreeParser.Nodes
{
    /// <summary>
    /// Property Node. Represents a single property.
    /// </summary>
    /// <example>Name, Id, ForeignKeyId, NumberPropertyOnAClassOrSomething.</example>
    public class PropertyNode : Node
    {
        public PropertyNode(Node propertyNode) : this(propertyNode.Token) { }
        public PropertyNode(Tokenizer.Token token) : base('v')
        {
            Token = token;
        }

        public override Expression GetExpression(Expression dto)
        {
            var propertyExpression = Expression.PropertyOrField(dto, Token.Value);
            if (Children != null && Children.Count == 1)
                return Children[0].GetExpression(propertyExpression);
            return propertyExpression;
        }
    }
}
