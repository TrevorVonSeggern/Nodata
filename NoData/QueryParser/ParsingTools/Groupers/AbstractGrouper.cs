using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NoData.Graph.Base;

namespace NoData.QueryParser.ParsingTools.Groupers
{
    public abstract class AbstractGrouper<T> : IGrouper<T> where T : IRepresent
    {
        protected IDictionary<Regex, Func<IList<T>, ITuple<T, int>>> GroupingTerms { get; set; } = new Dictionary<Regex, Func<IList<T>, ITuple<T, int>>>();

        public AbstractGrouper() { }
        public AbstractGrouper(IDictionary<Regex, Func<IList<T>, ITuple<T, int>>> terms)
        {
            GroupingTerms = terms;
        }
        public AbstractGrouper(IDictionary<string, Func<IList<T>, ITuple<T, int>>> terms)
        {
            GroupingTerms = terms.Select(x => ITuple.Create(new Regex(x.Key, RegexOptions.Compiled), x.Value)).ToDictionary(x => x.Item1, x => x.Item2);
        }


        public void AddGroupingTerms(string pattern, Func<IList<T>, ITuple<T, int>> groupingAction) => AddGroupingTerms(ITuple.Create(pattern, groupingAction));
        public void AddGroupingTerms(IEnumerable<ITuple<string, Func<IList<T>, ITuple<T, int>>>> groupingTerms)
        {
            foreach (var term in groupingTerms)
                AddGroupingTerms(term);
        }
        public void AddGroupingTerms(ITuple<string, Func<IList<T>, ITuple<T, int>>> term)
        {
            GroupingTerms.Add(new Regex(term.Item1, RegexOptions.Compiled), term.Item2);
        }

        public abstract IList<T> Parse(IEnumerable<T> listToGroup);
        public T ParseToSingle(IEnumerable<T> listToGroup)
        {
            var list = Parse(listToGroup);
            if (list.Count != 1)
                return default(T);
            return list[0];
        }
    }
}