using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Cache;

namespace NoData.Internal.TreeParser.Tokenizer
{
    public class TokenDefinitionCache : DictionaryCache<int, TokenDefinition>
    {
        public TokenDefinition Token(TokenTypes tokenType, string pattern)
        {
            int hash = 17;
            unchecked
            {
                hash = hash * 31 + TokenTypes.quotedString.GetHashCode();
                hash = hash * 31 + pattern.GetHashCode();
            }
            return GetOrAdd(hash, () => new TokenDefinition(pattern, tokenType));
        }
    }
}
