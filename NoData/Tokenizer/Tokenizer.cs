using System.Text.RegularExpressions;

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
                    _DefinitionCache.Token(TokenType.quotedString, @"([\""'])(?:\\\1|.)*?\1"),
                    _DefinitionCache.Token(TokenType.parenthesis, @"\("),
                    _DefinitionCache.Token(TokenType.parenthesis, @"\)"),
                    _DefinitionCache.Token(TokenType.whitespace, @"\s+"),
                    _DefinitionCache.Token(TokenType.not, "!"),
                    _DefinitionCache.Token(TokenType.add, @"\+"),
                    _DefinitionCache.Token(TokenType.subtract, @"\-"),
                    _DefinitionCache.Token(TokenType.number, @"[-+]?[0-9]*\.?[0-9]+"),

                    _DefinitionCache.Token(TokenType.truth, "[Tt]rue"),
                    _DefinitionCache.Token(TokenType.falsey, "[Ff]alse"),
                    _DefinitionCache.Token(TokenType.forwardSlash, "/"),
                    _DefinitionCache.Token(TokenType.semiColin, ";"),
                    _DefinitionCache.Token(TokenType.comma, ","),
                    _DefinitionCache.Token(TokenType.ascending, "asc"),
                    _DefinitionCache.Token(TokenType.descending, "desc"),
                    _DefinitionCache.Token(TokenType.filterClause, filterString),
                    _DefinitionCache.Token(TokenType.expandClause, expandString),
                    _DefinitionCache.Token(TokenType.selectClause, selectString),

                    // string functions.
                    _DefinitionCache.Token(TokenType.strLength, "length"),
                    _DefinitionCache.Token(TokenType.strSubstring, "substring"),
                    _DefinitionCache.Token(TokenType.strStartsWith, "startswith"),
                    _DefinitionCache.Token(TokenType.strEndsWith, "endswith"),
                    _DefinitionCache.Token(TokenType.strConcat, "concat"),
                    _DefinitionCache.Token(TokenType.strContains, "contains"),
                    _DefinitionCache.Token(TokenType.strIndexOf, "indexof"),
                    _DefinitionCache.Token(TokenType.strToLower, "tolower"),
                    _DefinitionCache.Token(TokenType.strToUpper, "toupper"),
                    _DefinitionCache.Token(TokenType.strTrim, "trim"),
                    _DefinitionCache.Token(TokenType.strReplace, "replace"),

                    // Logical functions
                    _DefinitionCache.Token(TokenType.equals, "eq"),
                    _DefinitionCache.Token(TokenType.notEquals, "ne"),
                    _DefinitionCache.Token(TokenType.not, "not"),
                    _DefinitionCache.Token(TokenType.and, "and"),
                    _DefinitionCache.Token(TokenType.or, "or"),
                    _DefinitionCache.Token(TokenType.greaterThan, "gt"),
                    _DefinitionCache.Token(TokenType.lessThan, "lt"),
                    _DefinitionCache.Token(TokenType.greaterThanOrEqual, "ge"),
                    _DefinitionCache.Token(TokenType.lessThanOrEqual, "le"),
                }
            );

            foreach (var prop in classProperties)
            {
                // if (!Regex.IsMatch(prop, "^[a-zA-Z_]+$"))
                //     throw new ArgumentException($"class property {prop}, contains an invaild character.");

                definitions.Add(_DefinitionCache.Token(TokenType.classProperties, $"(?<![^\\W]){prop}(?![^\\W])"));
            }

            lexer.AddDefinitions(definitions);
            Lexer = lexer;
        }

        public IEnumerable<Token> Tokenize(string source)
        {
            var result = new List<Token>();
            foreach (var token in Lexer.Tokenize(source))
            {
                if (token.Type == TokenType.whitespace.ToString())
                    continue;
                result.Add(token);
            }
            return result;
        }
    }
}
