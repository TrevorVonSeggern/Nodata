using System.Text.RegularExpressions;
using Graph;

namespace NoData.QueryParser.ParsingTools.Groupers
{
    public abstract class AbstractGrouper<T> : IGrouper<T> where T : class, IRepresent
    {
        protected IReadOnlyDictionary<Regex, Func<IList<T>, ITuple<T, int>>> GroupingTerms { get; } = new Dictionary<Regex, Func<IList<T>, ITuple<T, int>>>();

        protected AbstractGrouper() { }
        protected AbstractGrouper(IReadOnlyDictionary<Regex, Func<IList<T>, ITuple<T, int>>> terms)
        {
            GroupingTerms = terms;
        }

        public abstract IList<T> Parse(IEnumerable<T> listToGroup);
        public T? ParseToSingle(IEnumerable<T> listToGroup)
        {
            var list = Parse(listToGroup);
            if (list.Count != 1)
                return default(T);
            return list[0];
        }
    }
}
