using QueueItem = NoData.GraphImplementations.QueryParser.Tree;

namespace NoData.QueryParser.ParsingTools
{
    interface IQueryClauseParser<TResult>
    {
        bool IsFinished { get; }
        void Parse();
        Type RootQueryType { get; }
        TResult? Result { get; }
        IList<QueueItem> GetTokens(string parmeter);
    }
}
