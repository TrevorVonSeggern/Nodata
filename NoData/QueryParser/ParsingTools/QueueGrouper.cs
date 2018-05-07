using NoData.Graph.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace NoData.QueryParser.ParsingTools
{
    public class QueueGrouper<T>
    {
        private readonly IList<T> list;
        private readonly Func<T, string> getRepresnetationalValueFunc;
        private readonly Dictionary<Regex, Func<List<T>, ITuple<T, int>>> patternDictionary = new Dictionary<Regex, Func<List<T>, ITuple<T, int>>>();

        public QueueGrouper(IList<T> list, Func<T, string> getRepresnetationalValueFunc)
        {
            this.list = list;
            this.getRepresnetationalValueFunc = getRepresnetationalValueFunc;
        }

        private string RepresentationalValueString => string.Join("", list.Select(t => getRepresnetationalValueFunc(t)));

        /// <summary>
        /// After a pattern is matched, this functions returns the contributing index of item in the list of <see cref="T"/>.
        /// </summary>
        /// <returns></returns>
        private int GetListIndexInRepString(int stringIndex)
        {
            if (stringIndex == 0) return 0;
            var currentRep = "";
            for(var i = 0; i < list.Count(); ++i)
            {
                currentRep += getRepresnetationalValueFunc(list[i]);
                if (stringIndex < currentRep.Length)
                    return i;
            }
            return -1;
        }

        /// <summary>
        /// Adds a grouping pattern to match/group on.
        /// </summary>
        /// <param name="pattern">a regex pattern to match on.</param>
        /// <param name="handleGrouping">Given the enumerable, it should return a tuple with the item replaces, and in int for the number of items to replace.</param>
        public void AddGroupingTerm(string pattern, Func<IList<T>, ITuple<T, int>> handleGrouping)
        {
            patternDictionary.Add(new Regex(pattern, RegexOptions.Compiled | RegexOptions.Multiline), handleGrouping);
        }

        public T Reduce()
        {
            var foundMatch = true;
            while (foundMatch)
            {
                foundMatch = false;
                var representation = RepresentationalValueString;
                foreach(var keypair in patternDictionary)
                {
                    var regex = keypair.Key;
                    var func = keypair.Value;
                    var match = regex.Match(representation);
                    if (!match.Success)
                        continue;
                    var i = GetListIndexInRepString(match.Index);
                    if (i == -1 || i >= list.Count())
                        continue;
                    var groupInfo = func(list.ToList().GetRange(i, list.Count() - i));
                    if (groupInfo is null)
                        return default(T);
                    var toRemove = groupInfo.Item2 - 1;
                    list[i] = groupInfo.Item1;
                    for(var r = 0; r <= toRemove; ++r)
                    {
                        list.RemoveAt(i + 1);
                    }
                    foundMatch = true;
                    break;
                }
            }
            if (list.Count() != 1)
                return default(T);
            return list[0];
        }
    }
}
