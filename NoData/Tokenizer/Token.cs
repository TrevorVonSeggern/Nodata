using System;
using System.Collections.Generic;
using System.Text;
using Immutability;

namespace NoData.Internal.TreeParser.Tokenizer
{
    [Immutable]
    public class Token
    {
        public string Value { get; }
        public TokenPosition Position { get; }
        public string Type { get; }

        public Token(string value)
        {
            Value = value;
        }

        public Token(string value, TokenPosition position) : this(value)
        {
            Position = position;
        }

        public Token(TokenTypes type, string value, TokenPosition position) : this(type.ToString(), value, position) { }

        public Token(string type, string value, TokenPosition position) : this(value, position)
        {
            Type = type.ToString();
        }
    }
}
