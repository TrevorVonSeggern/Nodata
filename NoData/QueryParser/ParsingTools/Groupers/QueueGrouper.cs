using System.Text;
using System.Text.RegularExpressions;
using Graph;
using NoData.QueryParser.ParsingTools.Groupers;

namespace NoData.QueryParser.ParsingTools
{
    public class QueueGrouper<T> : AbstractGrouper<T>, IGrouper<T> where T : class, IRepresent
    {
        public QueueGrouper() : base() { }
        public QueueGrouper(IReadOnlyDictionary<Regex, Func<IList<T>, ITuple<T, int>>> terms) : base(terms) { }

        private static int GetListIndexInRepString(int stringIndex, IList<T> list)
        {
            if (stringIndex == 0) return 0;
            var sb = new StringBuilder();
            for (var i = 0; i < list.Count; ++i)
            {
                sb.Append(list[i].Representation);
                if (stringIndex < sb.Length)
                    return i;
            }
            return -1;
        }

        public override IList<T> Parse(IEnumerable<T> listToGroup)
        {
            var list = new List<T>(listToGroup);
            string RepresentationalValueString() => string.Join("", list.Select(t => t.Representation));
            var foundMatch = true;
            while (foundMatch)
            {
                foundMatch = false;
                var representation = RepresentationalValueString();
                foreach (var keypair in this.GroupingTerms)
                {
                    var regex = keypair.Key;
                    var func = keypair.Value;
                    var match = regex.Match(representation);
                    if (!match.Success)
                        continue;
                    var i = GetListIndexInRepString(match.Index, list);
                    if (i == -1 || i >= list.Count)
                        continue;
                    var endPosition = GetListIndexInRepString(match.Index + match.Length - 1, list);
                    var groupInfo = func(list.ToList().GetRange(i, endPosition - i + 1));
                    if (groupInfo is null)
                        throw new Exception("Grouping result is null.");
                    var toRemove = groupInfo.Item2 - 1;
                    list[i] = groupInfo.Item1;
                    for (var r = 0; r <= toRemove; ++r)
                    {
                        list.RemoveAt(i + 1);
                    }
                    foundMatch = true;
                    break;
                }
            }
            return list;
        }
    }
}
