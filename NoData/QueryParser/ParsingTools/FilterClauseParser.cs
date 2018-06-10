using System;
using System.Collections.Generic;
using System.Linq;
using NoData.Graph;
using NoData.Graph.Base;
using NoData.QueryParser.Graph;
using QueueItem = NoData.QueryParser.Graph.Tree;
using TInfo = NoData.QueryParser.Graph.TextInfo;

namespace NoData.QueryParser.ParsingTools
{
    public class FilterClauseParser<TRootQueryType> : AbstractClaseParser<TRootQueryType, QueueItem>, IAcceptAdditions
    {
        public FilterClauseParser(Func<string, IList<QueueItem>> tokenFunc, string query) : base(tokenFunc, query) { }

        public void AddToClause(string clause)
        {
            IsFinished = false;

            if (string.IsNullOrWhiteSpace(QueryString))
                QueryString = clause;
            else
                QueryString = $"({QueryString}) and ({clause})";
        }

        public void AddToClause(QueueItem clause)
        {
            if (Result is null)
            {
                Result = clause;
            }
            else
            {
                var and = new TextInfo();
                and.Text = "and";
                and.Value = TextInfo.LogicalComparison;
                and.Representation = TextInfo.LogicalComparison;
                var rootAnd = new NoData.QueryParser.Graph.Vertex(and);
                var childrenWithEdges = new[]{
                    ITuple.Create(new Graph.Edge(rootAnd, clause.Root), clause),
                    ITuple.Create(new Graph.Edge(rootAnd, Result.Root), Result)
                };
                Result = new QueueItem(rootAnd, childrenWithEdges);
            }
        }

        public override void Parse()
        {
            if (!SetupParsing())
                return;

            AddToClause(Grouper.ParseToSingle(TokenFunc(QueryString)));
        }
    }
}
