using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Cache;

namespace NoData.Internal.TreeParser.Tokenizer
{
    public class Tokenizer
    {
        private static TokenDefinitionCache _DefinitionCache = new TokenDefinitionCache();
        private static string filterString = Regex.Escape("$filter=");
        private static string expandString = Regex.Escape("$expand=");
        private static string selectString = Regex.Escape("$select=");

        private readonly ILexer Lexer;

        public Tokenizer(IEnumerable<string> classProperties) : this(new Lexer(), classProperties) { }

        public Tokenizer(ILexer lexer, IEnumerable<string> classProperties)
        {
            var definitions = new List<TokenDefinition>(30);
            definitions.AddRange(
                new[]{
                    _DefinitionCache.Token(TokenTypes.quotedString, @"([\""'])(?:\\\1|.)*?\1"),
                    _DefinitionCache.Token(TokenTypes.parenthesis, @"\("),
                    _DefinitionCache.Token(TokenTypes.parenthesis, @"\)"),
                    _DefinitionCache.Token(TokenTypes.not, "!"),
                    _DefinitionCache.Token(TokenTypes.add, @"\+"),
                    _DefinitionCache.Token(TokenTypes.subtract, @"\-"),
                    _DefinitionCache.Token(TokenTypes.number, @"[-+]?[0-9]*\.?[0-9]+"),
                    _DefinitionCache.Token(TokenTypes.equals, "eq"),
                    _DefinitionCache.Token(TokenTypes.notEquals, "ne"),
                    _DefinitionCache.Token(TokenTypes.not, "not"),
                    _DefinitionCache.Token(TokenTypes.and, "and"),
                    _DefinitionCache.Token(TokenTypes.or, "or"),
                    _DefinitionCache.Token(TokenTypes.greaterThan, "gt"),
                    _DefinitionCache.Token(TokenTypes.lessThan, "lt"),
                    _DefinitionCache.Token(TokenTypes.greaterThanOrEqual, "ge"),
                    _DefinitionCache.Token(TokenTypes.lessThanOrEqual, "le"),
                    _DefinitionCache.Token(TokenTypes.truth, "[Tt]rue"),
                    _DefinitionCache.Token(TokenTypes.falsey, "[Ff]alse"),
                    _DefinitionCache.Token(TokenTypes.forwardSlash, "/"),
                    _DefinitionCache.Token(TokenTypes.semiColin, ";"),
                    _DefinitionCache.Token(TokenTypes.comma, ","),
                    _DefinitionCache.Token(TokenTypes.ascending, "asc"),
                    _DefinitionCache.Token(TokenTypes.descending, "desc"),
                    _DefinitionCache.Token(TokenTypes.filterClause, filterString),
                    _DefinitionCache.Token(TokenTypes.expandClause, expandString),
                    _DefinitionCache.Token(TokenTypes.selectClause, selectString),
                    _DefinitionCache.Token(TokenTypes.selectClause, selectString),
                    _DefinitionCache.Token(TokenTypes.whitespace, @"\s+")
                }
            );

            foreach (var prop in classProperties)
            {
                // if (!Regex.IsMatch(prop, "^[a-zA-Z_]+$"))
                //     throw new ArgumentException($"class property {prop}, contains an invaild character.");

                definitions.Add(_DefinitionCache.Token(TokenTypes.classProperties, $"(?<![^\\W]){prop}(?![^\\W])"));
            }

            lexer.AddDefinitions(definitions);
            Lexer = lexer;
        }

        public IEnumerable<Token> Tokenize(string source)
        {
            var result = new List<Token>();
            foreach (var token in Lexer.Tokenize(source))
            {
                if (token.Type == TokenTypes.whitespace.ToString())
                    continue;
                result.Add(token);
            }
            return result;
        }
    }
}
