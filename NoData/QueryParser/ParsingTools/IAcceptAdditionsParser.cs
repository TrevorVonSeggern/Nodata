using NoData.Graph.Base;
using System;
using System.Collections.Generic;
using QueueItem = NoData.QueryParser.Graph.Tree;

namespace NoData.QueryParser.ParsingTools
{
    interface IAcceptAdditions
    {
        void AddToClause(string clause);
    }
}
