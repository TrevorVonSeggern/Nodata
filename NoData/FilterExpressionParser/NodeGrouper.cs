using System.Collections.Generic;

namespace NoData.Internal.TreeParser.FilterExpressionParser
{
    using NoData.Internal.TreeParser.Nodes;
    using System.Linq;
    using Tokenizer;

    /// <summary>
    /// Tokenizes Nodes into their corresponding types.
    /// </summary>
    /// <example>a class property node would be represented as v, for value type.</example>
    internal class NodeGrouper
    {
        public List<TokenDefinition> definitions = new List<TokenDefinition>();
        public NodeGrouper()
        {
            // first parse groupings.
            definitions.Add(new TokenDefinition(@"\(v\)", NodeTokenTypes.GroupValueGroup.ToString()));
            definitions.Add(new TokenDefinition(@"!v", NodeTokenTypes.InverseOfValue.ToString()));
            definitions.Add(new TokenDefinition(@"vov", NodeTokenTypes.ValueOperationValue.ToString()));
            definitions.Add(new TokenDefinition(@"vcv", NodeTokenTypes.ValueComparedToValue.ToString()));
            definitions.Add(new TokenDefinition(@"vlv", NodeTokenTypes.ValueAndOrValue.ToString()));
        }

        public Token Tokenize(string source)
        {
            foreach(var definition in definitions)
            {
                var match = definition.Regex.Match(source);
                if (match == null || !match.Success)
                    continue;
                var token = new Token(match.Captures[0].Value);
                token.Type = definition.Type;
                token.Position = new TokenPosition(match.Index, match.Index + match.Length - 1);
                return token;
            }
            return null;
        }

        public static string GetQueueRepresentationalString(List<Node> queue)
            => string.Join("", queue.Select(x => GetNodeRepresentation(x)));

        public static string GetNodeRepresentation(Node n)
        {
            if (n.GetType() == typeof(NodePlaceHolder))
            {
                var t = n?.Token?.Type;
                if (t == null)
                    return " ";
                if (t == TokenTypes.equals.ToString() ||
                    t == TokenTypes.notEquals.ToString() ||
                    t == TokenTypes.greaterThan.ToString() ||
                    t == TokenTypes.greaterThanOrEqual.ToString() ||
                    t == TokenTypes.lessThan.ToString() ||
                    t == TokenTypes.lessThanOrEqual.ToString())
                    return "c";
                if (t == TokenTypes.not.ToString())
                    return "!";
                if (t == TokenTypes.number.ToString() ||
                    t == TokenTypes.quotedString.ToString())
                    return "v";
                if (t == TokenTypes.subtract.ToString() ||
                    t == TokenTypes.add.ToString())
                    return "o";
                if (t == TokenTypes.and.ToString() ||
                    t == TokenTypes.or.ToString())
                    return "l";
                if (t == TokenTypes.parenthesis.ToString())
                    return n.Token.Value;
                if (t == TokenTypes.whitespace.ToString())
                    return " ";
            }
            return n?.Representation.ToString() ?? "#";
        }
    }
}
