using System;
using System.Collections.Generic;
using System.Linq;
using Graph;
using NoData.GraphImplementations;
using NoData.GraphImplementations.QueryParser;

using QueueItem = NoData.GraphImplementations.QueryParser.Tree;
using ParserVertex = NoData.GraphImplementations.QueryParser.Vertex;
using ParserEdge = NoData.GraphImplementations.QueryParser.Edge;

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
                var rootAnd = new ParserVertex(and);
                var childrenWithEdges = new[]{
                    ITuple.Create(new ParserEdge(rootAnd, clause.Root), clause),
                    ITuple.Create(new ParserEdge(rootAnd, Result.Root), Result)
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
