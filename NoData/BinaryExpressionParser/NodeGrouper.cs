using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

namespace NoData.Internal.TreeParser.BinaryTreeParser
{
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

            // if the values weren't touched.
            //definitions.Add(new TokenDefinition(@"v", NodeTokenTypes.Value.ToString()));
            //definitions.Add(new TokenDefinition(@"o", NodeTokenTypes.Operator.ToString()));
            //definitions.Add(new TokenDefinition(@"!", NodeTokenTypes.Inverse.ToString()));
            //definitions.Add(new TokenDefinition(@"c", NodeTokenTypes.Comparator.ToString()));
            //definitions.Add(new TokenDefinition(@"l", NodeTokenTypes.LogicalOperator.ToString()));
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
                token.Position = new TokenPosition(match.Index, 0, 0);
                return token;
            }
            return null;
        }
    }
}
