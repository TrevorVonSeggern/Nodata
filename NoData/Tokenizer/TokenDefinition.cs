using System.Text.RegularExpressions;
using CodeTools;

namespace NoData.Internal.TreeParser.Tokenizer
{
    public class TokenDefinition
    {
        public bool IsIgnored { get; }
        public Regex Regex { get; }
        public string Type { get; }

        public TokenDefinition(string pattern, TokenTypes type)
        {
            Regex = new Regex(pattern, RegexOptions.Compiled | RegexOptions.Singleline);
            Type = type.ToString();
            IsIgnored = false;
        }

        public TokenDefinition(string pattern, string type)
        {
            Regex = new Regex(pattern, RegexOptions.Compiled | RegexOptions.Singleline);
            Type = type;
            IsIgnored = false;
        }
    }
}
