using System.Text.RegularExpressions;
using Graph;
using NoData.GraphImplementations.QueryParser;
using NoData.QueryParser.ParsingTools.Groupers;

namespace NoData.QueryParser.ParsingTools
{
    public class OrderdGrouper<T> : AbstractGrouper<T>, IGrouper<T> where T : class, IRepresent
    {
        public string OpenGroupingValue { get; set; } = TextRepresentation.OpenParenthesis;
        public string CloseGroupingValue { get; set; } = TextRepresentation.CloseParenthesis;

        public OrderdGrouper(IReadOnlyDictionary<Regex, Func<IList<T>, ITuple<T, int>>> terms) : base(terms) { }

        public override IList<T> Parse(IEnumerable<T> listToGroup)
        {
            var list = listToGroup.ToList();
            var levelList = Enumerable.Range(0, list.Count).ToList(); // create an initialized list of same size
            var level = 0;
            var maxLevel = 0;
            for (int i = 0; i < list.Count; ++i)
            {
                if (list[i].Representation == OpenGroupingValue)
                    levelList[i] = level++; // increase the level after the open grouping value
                else if (list[i].Representation == CloseGroupingValue)
                    levelList[i] = --level; // decrease the level for close grouping. Set the close grouping to that level.
                else
                    levelList[i] = level;
                if (level > maxLevel)
                    maxLevel = level;
            }

            while (true)
            {
                // setup the range to parse
                var startRange = 0;
                var endRange = 0;
                for (int i = 0; i < levelList.Count; ++i)
                {
                    startRange = i;
                    if (maxLevel == levelList[i])
                    {
                        for (int e = i; e <= levelList.Count; ++e)
                        {
                            endRange = e;
                            if (e == levelList.Count || maxLevel != levelList[e])
                                break;
                        }
                        break;
                    }
                }

                // parse
                var toParse = list.GetRange(startRange, endRange - startRange);
                var parsed = new QueueGrouper<T>(GroupingTerms).Parse(toParse);

                // update changes within the range & decrement the level of the range.
                for (var i = 0; i < parsed.Count; ++i)
                {
                    list[startRange + i] = parsed[i];
                    levelList[startRange + i]--;
                }

                // remove extra.
                var toRemoveCount = toParse.Count - parsed.Count;
                var toRemoveStartIndex = startRange + parsed.Count;
                list.RemoveRange(toRemoveStartIndex, toRemoveCount);
                levelList.RemoveRange(toRemoveStartIndex, toRemoveCount);
                if (maxLevel == 0)
                    break;
                maxLevel = levelList.Max();
            }
            return list;
        }
    }
}
