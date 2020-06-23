using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using GraphLibrary;

namespace NoData.QueryParser.ParsingTools.Groupers
{
    public abstract class AbstractGrouper<T> : IGrouper<T> where T : IRepresent
    {
        protected IReadOnlyDictionary<Regex, Func<IList<T>, ITuple<T, int>>> GroupingTerms { get; }

        protected AbstractGrouper() { }
        protected AbstractGrouper(IReadOnlyDictionary<Regex, Func<IList<T>, ITuple<T, int>>> terms)
        {
            GroupingTerms = terms;
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
