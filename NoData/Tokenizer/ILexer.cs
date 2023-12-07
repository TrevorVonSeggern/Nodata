namespace NoData.Internal.TreeParser.Tokenizer
{
    public interface ILexer
    {
        void AddDefinition(TokenDefinition definition);
        void AddDefinitions(IReadOnlyList<TokenDefinition> definition);
        IEnumerable<Token> Tokenize(string source);
    }
}
