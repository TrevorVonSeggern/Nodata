using System;
using System.Collections.Generic;
using System.Linq;
using NoData.Graph;
using NoData.Graph.Base;
using QueueItem = NoData.QueryParser.Graph.Tree;
using TInfo = NoData.QueryParser.Graph.TextInfo;

namespace NoData.QueryParser.ParsingTools
{
    class SelectClauseParser<TRootQueryType> : AbstractClaseParser<TRootQueryType, IEnumerable<ITuple<Path, string>>>, IAcceptAdditions
    {
        private readonly NoData.Graph.Graph Graph;
        public override IEnumerable<ITuple<Path, string>> Result => ResultList;
        private List<ITuple<Path, string>> ResultList = new List<ITuple<Path, string>>();

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

        public static ITuple<Path, string> PathAndPropertyFromExpandItem(QueueItem addition, NoData.Graph.Graph graph, Type RootQueryType)
        {
            // add to path list.
            var edges = new List<Edge>();
            string propertyName = null;
            void traverseExpandTree(Vertex from, QueueItem parsedSelection)
            {
                if (parsedSelection?.Root?.Value.Representation != TInfo.ExpandProperty) return;
                if (!parsedSelection.Children.Any())
                {
                    propertyName = parsedSelection.Root.Value.Text;
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
            return ITuple.Create(new Path(edges), propertyName);
        }
        public static Path PathFromExpandItem(QueueItem addition, NoData.Graph.Graph graph, Type RootQueryType)
        {
            // add to path list.
            var edges = new List<Edge>();
            void traverseExpandTree(Vertex from, QueueItem parsedSelection)
            {
                if (parsedSelection?.Root?.Value.Representation != TInfo.ExpandProperty) return;
                if (!parsedSelection.Children.Any())
                    return;

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
            return new Path(edges);
        }

        public void AddToClause(QueueItem addition)
        {
            if (addition is null || (addition.Representation != TInfo.ListOfExpands && addition.Representation != TInfo.ExpandProperty))
                throw new ArgumentException("invalid query");
            if (addition.Representation == TInfo.ListOfExpands)
            {
                foreach (var expand in addition.Children.Select(x => x.Item2))
                    AddToClause(expand);
                return;
            }
            ResultList.Add(PathAndPropertyFromExpandItem(addition, Graph, RootQueryType));
        }

        public override void Parse()
        {
            if (SetupParsing())
                AddToClause(Grouper.ParseToSingle(TokenFunc(QueryString)));
        }
    }
}
