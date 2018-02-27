using System;
using System.Collections.Generic;
using System.Text;

namespace NoData.Internal.TreeParser.Tokenizer
{
    public interface ILexer
    {
        void AddDefinition(TokenDefinition definition);
        IEnumerable<Token> Tokenize(string source);
    }
}
