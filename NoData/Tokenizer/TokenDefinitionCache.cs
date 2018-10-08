using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Cache;
using NoData.Utility;

namespace NoData.Internal.TreeParser.Tokenizer
{
    public class TokenDefinitionCache : DictionaryCache<int, TokenDefinition>
    {
        public TokenDefinition Token(TokenTypes tokenType, string pattern)
        {
            var hash = TokenTypes.quotedString.GetHashCode().AndHash(pattern);
            return GetOrAdd(hash, () => new TokenDefinition(pattern, tokenType));
        }
    }
}
