using System;
using System.Collections.Generic;
using System.Linq;
using NoData.Graph;
using NoData.Graph.Base;
using QueueItem = NoData.QueryParser.Graph.Tree;
using TInfo = NoData.QueryParser.Graph.TextInfo;

namespace NoData.QueryParser.ParsingTools
{
    public class FilterClaseParser<TRootQueryType> : AbstractClaseParser<TRootQueryType, QueueItem>, IAcceptAdditions
    {
        public FilterClaseParser(Func<string, IList<QueueItem>> tokenFunc, string query) : base(tokenFunc, query) { }

        public void AddToClause(string clause)
        {
            IsFinished = false;

            if (string.IsNullOrWhiteSpace(QueryString))
                QueryString = clause;
            else
                QueryString = $"({QueryString}) and ({clause})";
        }

        public override void Parse()
        {
            if (!SetupParsing())
                return;

            var filter = Grouper.Reduce();

            Result = filter;
        }
    }
}
