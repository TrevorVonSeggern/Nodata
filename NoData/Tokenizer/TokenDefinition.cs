using System.Text.RegularExpressions;

namespace NoData.Internal.TreeParser.Tokenizer
{
    public class TokenDefinition
    {
        public bool IsIgnored { get; set; }
        public Regex Regex { get; set; }
        public string Type { get; set; }

        public TokenDefinition(string pattern, TokenTypes type)
        {
            Regex = new Regex(pattern, RegexOptions.Compiled);
            Type = type.ToString();
            IsIgnored = false;
        }

        public TokenDefinition(string pattern, string type)
        {
            Regex = new Regex(pattern, RegexOptions.Compiled);
            Type = type;
            IsIgnored = false;
        }
    }
}
