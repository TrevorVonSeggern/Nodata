using NoData.Graph.Base;
using System;
using System.Collections.Generic;
using QueueItem = NoData.QueryParser.Graph.Tree;

namespace NoData.QueryParser.ParsingTools
{
    interface IQueryClauseParser<TResult>
    {
        bool IsFinished { get; }
        void Parse();
        Type RootQueryType { get; }
        void AddGroupingTerms(ITuple<string, Func<IList<QueueItem>, ITuple<QueueItem, int>>> handleGrouping);
        void AddGroupingTerms(IEnumerable<ITuple<string, Func<IList<QueueItem>, ITuple<QueueItem, int>>>> handleGrouping);
        TResult Result { get; }
        IList<QueueItem> GetTokens(string parmeter);
    }
}
