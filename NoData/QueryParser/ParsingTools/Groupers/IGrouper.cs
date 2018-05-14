using System;
using System.Collections.Generic;
using NoData.Graph.Base;

namespace NoData.QueryParser.ParsingTools.Groupers
{
    public interface IGrouper<T> where T : IRepresent
    {
        void AddGroupingTerms(ITuple<string, Func<IList<T>, ITuple<T, int>>> handleGrouping);
        void AddGroupingTerms(IEnumerable<ITuple<string, Func<IList<T>, ITuple<T, int>>>> handleGroupings);
        void AddGroupingTerms(string pattern, Func<IList<T>, ITuple<T, int>> groupingAction);

        IList<T> Parse(IEnumerable<T> listToGroup);
        T ParseToSingle(IEnumerable<T> listToGroup);
    }
}