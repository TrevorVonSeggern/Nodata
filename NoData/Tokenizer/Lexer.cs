using System.Text.RegularExpressions;

namespace NoData.Internal.TreeParser.Tokenizer
{
    public class Lexer : ILexer
    {
        private static Regex endOfLineRegex = new Regex(@"(\r\n|\r|\n)", RegexOptions.Compiled | RegexOptions.Singleline);

        public Queue<Token> Tokens { get; } = new();
        public List<TokenDefinition> Definitions { get; } = new List<TokenDefinition>(); // 29 just for normal parsing.

        public Lexer()
        {
        }

        public void AddDefinition(TokenDefinition definition)
        {
            Definitions.Add(definition);
        }

        public void AddDefinitions(IReadOnlyList<TokenDefinition> definition)
        {
            Definitions.AddRange(definition);
        }

        public IEnumerable<Token> Tokenize(string source)
        {
            var currentIndex = 0;
            var currentLine = 1;
            var currentColumn = 0;

            while (currentIndex < source.Length)
            {
                TokenDefinition? matchedDefinition = null;
                var matchLength = 0;

                foreach (var rule in Definitions)
                {
                    var match = rule.Regex.Match(source, currentIndex);

                    if (match.Success && (match.Index - currentIndex) == 0)
                    {
                        matchedDefinition = rule;
                        matchLength = match.Length;
                        break;
                    }
                }

                if (matchedDefinition == null)
                {
                    throw new Exception(
                        string.Format("Unrecognized symbol '{0}' at index {1} (line {2}, column {3}).", source[currentIndex], currentIndex, currentLine, currentColumn));
                }
                else
                {
                    var value = source.Substring(currentIndex, matchLength);

                    if (!matchedDefinition.IsIgnored)
                        yield return new Token(matchedDefinition.Type, value, new TokenPosition(currentIndex, currentIndex + value.Length - 1));

                    var endOfLineMatch = endOfLineRegex.Match(value);
                    if (endOfLineMatch.Success)
                    {
                        currentLine += 1;
                        currentColumn = value.Length - (endOfLineMatch.Index + endOfLineMatch.Length);
                    }
                    else
                    {
                        currentColumn += matchLength;
                    }

                    currentIndex += matchLength;
                }
            }
        }
    }
}
