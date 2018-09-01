using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using NoData.Graph;
using NoData.Graph.Base;
using QueueItem = NoData.QueryParser.Graph.Tree;
using TInfo = NoData.QueryParser.Graph.TextInfo;

namespace NoData.QueryParser.ParsingTools
{
    public enum SortDirection
    {
        Ascending, Descending
    }

    class SelectClauseParser<TRootQueryType> : AbstractClaseParser<TRootQueryType, IEnumerable<PathToProperty>>, IAcceptAdditions
    {
        private readonly NoData.Graph.Graph Graph;
        public override IEnumerable<PathToProperty> Result => ResultList;
        private List<PathToProperty> ResultList = new List<PathToProperty>();


        public SelectClauseParser(Func<string, IList<QueueItem>> tokenFunc, string query, NoData.Graph.Graph graph) : base(tokenFunc, query)
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

        public static PathToProperty PathAndPropertyFromExpandItem(QueueItem addition, NoData.Graph.Graph graph, Type RootQueryType)
        {
            // add to path list.
            var edges = new List<Edge>();
            ClassProperty propertyName = null;
            void traverseExpandTree(Vertex from, QueueItem parsedSelection)
            {
                if (parsedSelection?.Root?.Value.Representation != TInfo.ExpandProperty) return;
                if (!parsedSelection.Children.Any())
                {
                    propertyName = from.Value.Properties.Single(x => x.Name == parsedSelection.Root.Value.Text);
                    return;
                }

                // get the edge in the graph where it is connected from the same type as the from vertex, and the property name matches.
                var edge = graph.Edges.FirstOrDefault(e => e.From.Value.Type == from.Value.Type && e.Value.PropertyName == parsedSelection.Root.Value.Value);
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
