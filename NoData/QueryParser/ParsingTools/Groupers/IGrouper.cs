using System;
using System.Collections.Generic;
using Graph;

namespace NoData.QueryParser.ParsingTools.Groupers
{
    public interface IGrouper<T> where T : IRepresent
    {
        IList<T> Parse(IEnumerable<T> listToGroup);
        T ParseToSingle(IEnumerable<T> listToGroup);
    }
}