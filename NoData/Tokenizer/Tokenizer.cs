using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace NoData.Internal.TreeParser.Tokenizer
{
    public class Tokenizer
    {
        private readonly ILexer Lexer;

        public Tokenizer(IEnumerable<string> classProperties) : this(new Lexer(), classProperties) { }

        public Tokenizer(ILexer lexer, IEnumerable<string> classProperties)
        {
            lexer.AddDefinition(new TokenDefinition(@"([\""'])(?:\\\1|.)*?\1", TokenTypes.quotedString));

            lexer.AddDefinition(new TokenDefinition(@"\(", TokenTypes.parenthesis));
            lexer.AddDefinition(new TokenDefinition(@"\)", TokenTypes.parenthesis));

            lexer.AddDefinition(new TokenDefinition(@"!", TokenTypes.not));
            lexer.AddDefinition(new TokenDefinition(@"\+", TokenTypes.add));
            lexer.AddDefinition(new TokenDefinition(@"\-", TokenTypes.subtract));

            lexer.AddDefinition(new TokenDefinition(@"[-+]?[0-9]*\.?[0-9]+", TokenTypes.number));

            lexer.AddDefinition(new TokenDefinition(@"eq", TokenTypes.equals));
            lexer.AddDefinition(new TokenDefinition(@"ne", TokenTypes.notEquals));
            lexer.AddDefinition(new TokenDefinition(@"not", TokenTypes.not));
            lexer.AddDefinition(new TokenDefinition(@"and", TokenTypes.and));
            lexer.AddDefinition(new TokenDefinition(@"or", TokenTypes.or));

            lexer.AddDefinition(new TokenDefinition(@"gt", TokenTypes.greaterThan));
            lexer.AddDefinition(new TokenDefinition(@"lt", TokenTypes.lessThan));
            lexer.AddDefinition(new TokenDefinition(@"ge", TokenTypes.greaterThanOrEqual));
            lexer.AddDefinition(new TokenDefinition(@"le", TokenTypes.lessThanOrEqual));

            lexer.AddDefinition(new TokenDefinition(@"[Tt]rue", TokenTypes.truth));
            lexer.AddDefinition(new TokenDefinition(@"[Ff]alse", TokenTypes.falsey));

            lexer.AddDefinition(new TokenDefinition(@"/", TokenTypes.forwardSlash));
            lexer.AddDefinition(new TokenDefinition(@";", TokenTypes.semiColin));
            lexer.AddDefinition(new TokenDefinition(@",", TokenTypes.comma));

            lexer.AddDefinition(new TokenDefinition(Regex.Escape("$filter="), TokenTypes.filterClause));
            lexer.AddDefinition(new TokenDefinition(Regex.Escape("$expand="), TokenTypes.expandClause));
            lexer.AddDefinition(new TokenDefinition(Regex.Escape("$select="), TokenTypes.selectClause));

            foreach (var prop in classProperties)
            {
                if (!Regex.IsMatch(prop, "^[a-zA-Z_]+$"))
                    throw new ArgumentException($"class property {prop}, contains an invaild character.");

                lexer.AddDefinition(new TokenDefinition($"(?<![^\\W]){prop}(?![^\\W])", TokenTypes.classProperties));
            }
            lexer.AddDefinition(new TokenDefinition(@"\s+", TokenTypes.whitespace));
            Lexer = lexer;
        }

        public IEnumerable<Token> Tokenize(string source)
        {
            var result = new List<Token>();
            foreach(var token in Lexer.Tokenize(source))
            {
                if (token.Type == TokenTypes.whitespace.ToString())
                    continue;
                result.Add(token);
            }
            return result;
        }
    }
}
