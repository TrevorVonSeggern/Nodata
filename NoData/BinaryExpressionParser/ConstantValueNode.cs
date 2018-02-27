using NoData.Internal.TreeParser.Tokenizer;
using System;
using System.Linq.Expressions;

namespace NoData.Internal.TreeParser.BinaryTreeParser
{
    /// <summary>
    /// Property Node. Represents a single property.
    /// </summary>
    /// <example>Name, Id, ForeignKeyId, NumberPropertyOnAClassOrSomething.</example>
    public class ConstantValueNode : Node
    {
        public ConstantValueNode(Node propertyNode) : this(propertyNode.Token) { }
        public ConstantValueNode(Tokenizer.Token token) : base('v')
        {
            Token = token;
        }

        private ConstantExpression FromNumber()
        {
            if(int.TryParse(Token.Value, out var intResult))
                return Expression.Constant(intResult);
            else if(float.TryParse(Token.Value, out var floatResult))
                return Expression.Constant(floatResult);
            else if(double.TryParse(Token.Value, out var doubleResult))
                return Expression.Constant(doubleResult);
            else if(decimal.TryParse(Token.Value, out var decimalResult))
                return Expression.Constant(decimalResult);

            throw new NotImplementedException();
        }

        public override Expression GetExpression(ParameterExpression ignored)
        {
            if (Token.Type == TokenTypes.quotedString.ToString())
                return Expression.Constant(Token.Value.Substring(1, Token.Value.Length - 2));
            else if (Token.Type == TokenTypes.number.ToString())
                return FromNumber();
            throw new NotImplementedException();
        }
    }
}
