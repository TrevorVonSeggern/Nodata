using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using QuickCache;
using NoData.Utility;

namespace NoData.Internal.TreeParser.Tokenizer
{
    public class TokenDefinitionCache : DictionaryCache<int, TokenDefinition>
    {
        public TokenDefinition Token(TokenType tokenType, string pattern)
        {
            var hash = TokenType.quotedString.GetHashCode().AndHash(pattern);
            return GetOrAdd(hash, () => new TokenDefinition(pattern, tokenType));
        }
    }
}
