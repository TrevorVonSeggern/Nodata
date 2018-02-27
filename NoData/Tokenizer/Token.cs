using System;
using System.Collections.Generic;
using System.Text;

namespace NoData.Internal.TreeParser.Tokenizer
{
    public class Token
    {
        public string Value { get; set; }
        public TokenPosition Position { get; set; }
        public string Type { get; set; }

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
