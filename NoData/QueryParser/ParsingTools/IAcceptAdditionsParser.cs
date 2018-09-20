using System;
using System.Collections.Generic;
using Graph;
using QueueItem = NoData.GraphImplementations.QueryParser.Tree;

namespace NoData.QueryParser.ParsingTools
{
    public interface IAcceptAdditions
    {
        void AddToClause(string clause);
        void AddToClause(QueueItem clause);
    }
}
