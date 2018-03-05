using System.Collections.Generic;

namespace NoData.Internal.TreeParser.ExpandExpressionParser
{
    using NoData.Internal.TreeParser.Nodes;
    using System;
    using System.Linq;
    using Tokenizer;

    /// <summary>
    /// Tokenizes Nodes into their corresponding types.
    /// </summary>
    /// <example>a class property node would be represented as v, for value type.</example>
    internal class NodeGrouper
    {
        static readonly char propertyChar = NodeTokenUtilities.GetCharacterFromType(NodeTokenTypes.Property);
        static readonly char expandPropertyChar = NodeTokenUtilities.GetCharacterFromType(NodeTokenTypes.ExpandProperty);
        static readonly char expandCollectionChar = NodeTokenUtilities.GetCharacterFromType(NodeTokenTypes.ExpandCollection);

        public List<TokenDefinition> definitions = new List<TokenDefinition>();
        public NodeGrouper()
        {
            // first parse groupings.
            definitions.Add(new TokenDefinition($"{propertyChar}(\\/{propertyChar})*", NodeTokenTypes.ExpandProperty.ToString()));
            definitions.Add(new TokenDefinition($"{expandPropertyChar}(,{expandPropertyChar})+", NodeTokenTypes.ExpandCollection.ToString()));
        }

        public Token Tokenize(string source)
        {
            foreach(var definition in definitions)
            {
                var match = definition.Regex.Match(source);
                if (match == null || !match.Success)
                    continue;
                var token = new Token(definition.Type, match.Captures[0].Value, new TokenPosition(match.Index, match.Index + match.Length - 1));
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
                if (!Enum.TryParse(n?.Token?.Type, out TokenTypes t))
                    return " ";
                if (t == TokenTypes.classProperties)
                    return propertyChar.ToString();
                if (t == TokenTypes.parenthesis)
                    return n.Token.Value;
                if (t == TokenTypes.comma ||
                    t == TokenTypes.forwardSlash)
                    return n.Token.Value;
                if (t == TokenTypes.whitespace)
                    return " ";
            }
            return n?.Representation.ToString() ?? "#";
        }
    }
}
