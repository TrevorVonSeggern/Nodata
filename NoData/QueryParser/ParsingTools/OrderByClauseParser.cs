using System;
using System.Collections.Generic;
using System.Linq;
using NoData.Graph;
using NoData.Graph.Base;
using QueueItem = NoData.QueryParser.Graph.Tree;
using TInfo = NoData.QueryParser.Graph.TextInfo;

namespace NoData.QueryParser.ParsingTools
{
    class OrderByClauseParser<TRootQueryType> : AbstractClaseParser<TRootQueryType, Path>, IAcceptAdditions
    {
        private readonly NoData.Graph.Graph Graph;

        public OrderByClauseParser(Func<string, IList<QueueItem>> tokenFunc, string query, NoData.Graph.Graph graph) : base(tokenFunc, query)
        {
            Graph = graph;
        }

        public void AddToClause(string clause)
        {
            IsFinished = false;
            if (string.IsNullOrWhiteSpace(QueryString))
                QueryString = clause;
            else if (string.IsNullOrWhiteSpace(clause))
                return;
            else
                QueryString += "," + clause;
        }

        public void AddToClause(QueueItem addition)
        {
            if (addition is null || addition.Representation != TInfo.ExpandProperty)
                throw new ArgumentException("invalid query");

            Result = ExpandClauseParser<TRootQueryType>.ExpandPropertyToEdgeList(addition, Graph).FirstOrDefault();
        }

        public override void Parse()
        {
            if (SetupParsing())
                AddToClause(Grouper.ParseToSingle(TokenFunc(QueryString)));
        }
    }
}
