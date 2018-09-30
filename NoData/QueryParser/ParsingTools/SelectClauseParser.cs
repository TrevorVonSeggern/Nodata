using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using Graph;
using NoData.GraphImplementations.Schema;

using QueueItem = NoData.GraphImplementations.QueryParser.Tree;
using TInfo = NoData.GraphImplementations.QueryParser.TextInfo;

namespace NoData.QueryParser.ParsingTools
{
    public enum SortDirection
    {
        Ascending, Descending
    }

    class SelectClauseParser<TRootQueryType> : AbstractClaseParser<TRootQueryType, IEnumerable<PathToProperty>>, IAcceptAdditions
    {
        private readonly GraphSchema Graph;
        public override IEnumerable<PathToProperty> Result => ResultList;
        private List<PathToProperty> ResultList = new List<PathToProperty>();


        public SelectClauseParser(Func<string, IList<QueueItem>> tokenFunc, string query, GraphSchema graph, IReadOnlyDictionary<Regex, Func<IList<QueueItem>, ITuple<QueueItem, int>>> groupingTerms) : base(tokenFunc, query, groupingTerms)
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

        public static PathToProperty PathAndPropertyFromExpandItem(QueueItem addition, GraphSchema graph, Type RootQueryType)
        {
            // add to path list.
            var edges = new List<Edge>();
            Property propertyName = null;
            void traverseExpandTree(Vertex from, QueueItem parsedSelection)
            {
                if (parsedSelection?.Root?.Value.Representation != TInfo.ExpandProperty) return;
                if (!parsedSelection.Children.Any())
                {
                    propertyName = from.Value.Properties.Single(x => x.Name == parsedSelection.Root.Value.Text);
                    return;
                }

                // get the edge in the graph where it is connected from the same type as the from vertex, and the property name matches.
                var edge = graph.Edges.FirstOrDefault(e => e.From.Value.TypeId == from.Value.TypeId && e.Value.Name == parsedSelection.Root.Value.Value);
                if (edge is null)
                    return;
                edges.Add(edge);
                foreach (var child in parsedSelection.Children)
                    traverseExpandTree(edge.To, child.Item2);
            }
            var rootQueryVertex = graph.VertexContainingType(RootQueryType);
            traverseExpandTree(rootQueryVertex, addition);
            return new PathToProperty(edges, propertyName);
        }

        public void AddToClause(QueueItem clause)
        {
            if (clause is null || (clause.Representation != TInfo.ListOfExpands && clause.Representation != TInfo.ExpandProperty))
                throw new ArgumentException("invalid query");
            if (clause.Representation == TInfo.ListOfExpands)
            {
                foreach (var expand in clause.Children.Select(x => x.Item2))
                    AddToClause(expand);
                return;
            }
            ResultList.Add(PathAndPropertyFromExpandItem(clause, Graph, RootQueryType));
        }

        public override void Parse()
        {
            if (SetupParsing())
                AddToClause(Grouper.ParseToSingle(TokenFunc(QueryString)));
        }
    }
}
