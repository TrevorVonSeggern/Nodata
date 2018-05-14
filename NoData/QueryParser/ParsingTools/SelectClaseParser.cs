using System;
using System.Collections.Generic;
using System.Linq;
using NoData.Graph;
using NoData.Graph.Base;
using QueueItem = NoData.QueryParser.Graph.Tree;
using TInfo = NoData.QueryParser.Graph.TextInfo;

namespace NoData.QueryParser.ParsingTools
{
    class SelectClaseParser<TRootQueryType> : AbstractClaseParser<TRootQueryType, IList<ITuple<Path, string>>>, IAcceptAdditions
    {
        private readonly NoData.Graph.Graph Graph;

        public SelectClaseParser(Func<string, IList<QueueItem>> tokenFunc, string query, NoData.Graph.Graph graph) : base(tokenFunc, query)
        {
            Graph = graph;
            Result = new List<ITuple<Path, string>>();
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

        public override void Parse()
        {
            if (!SetupParsing())
                return;

            var select = Grouper.ParseToSingle(TokenFunc(QueryString));

            if (select.Root.Value.Representation != TInfo.ListOfExpands &&
                select.Root.Value.Representation != TInfo.ExpandProperty)
                throw new ArgumentException("invalid query");

            var groupOfSelects = select?.Children;

            if (groupOfSelects is null)
                throw new ArgumentException("invalid query");

            foreach (var propertySelection in groupOfSelects.Select(x => x.Item2))
            {
                // add to paths.
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
                    var edge = Graph.Edges.FirstOrDefault(e => e.From.Value.Type == from.Value.Type && e.Value.PropertyName == parsedSelection.Root.Value.Value);
                    if (edge is null)
                        return;
                    edges.Add(edge);
                    foreach (var child in parsedSelection.Children)
                        traverseExpandTree(edge.To, child.Item2);
                }
                var rootQueryVertex = Graph.VertexContainingType(RootQueryType);
                traverseExpandTree(rootQueryVertex, propertySelection);
                Result.Add(ITuple.Create(new Path(edges), propertyName));
            }
        }
    }
}
