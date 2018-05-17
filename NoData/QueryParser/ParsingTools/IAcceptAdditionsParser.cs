using NoData.Graph.Base;
using System;
using System.Collections.Generic;
using QueueItem = NoData.QueryParser.Graph.Tree;

namespace NoData.QueryParser.ParsingTools
{
    public interface IAcceptAdditions
    {
        void AddToClause(string clause);
        void AddToClause(QueueItem clause);
    }
}
