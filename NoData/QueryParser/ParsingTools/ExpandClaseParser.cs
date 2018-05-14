using System;
using System.Collections.Generic;
using System.Linq;
using NoData.Graph;
using NoData.Graph.Base;
using QueueItem = NoData.QueryParser.Graph.Tree;
using TInfo = NoData.QueryParser.Graph.TextInfo;

namespace NoData.QueryParser.ParsingTools
{
    public class ExpandClaseParser<TRootQueryType> : AbstractClaseParser<TRootQueryType, IList<Path>>
    {
        private readonly NoData.Graph.Graph Graph;

        public ExpandClaseParser(Func<string, IList<QueueItem>> tokenFunc, string query, NoData.Graph.Graph graph) : base(tokenFunc, query)
        {
            Graph = graph;
            Result = new List<Path>();
        }

        public void AddToResult(IEnumerable<Edge> edgePath)
        {
            Result.Add(new Path(edgePath));
        }

        public override void Parse()
        {
            if (!SetupParsing())
                return;

            var queueGrouper = Grouper;

            var parsed = queueGrouper.ParseToSingle(TokenFunc(QueryString));
            if (parsed != null && parsed.Root.Value.Representation != TInfo.ListOfExpands &&
                parsed.Root.Value.Representation != TInfo.ExpandProperty)
                throw new ArgumentException("invalid query");

            var groupOfExpansions = parsed?.Children;

            if (groupOfExpansions is null)
                throw new ArgumentException("invalid query");

            foreach (var expansion in groupOfExpansions.Select(x => x.Item2))
            {
                foreach (var path in _ExpandPropertyToEdgeList(expansion, Graph))
                    Result.Add(path);
            }
        }

        private static IEnumerable<Path> TraverseFakeProperty(Vertex from, QueueItem property, NoData.Graph.Graph graph)
        {
            if (!property.IsFakeExpandProperty && property.Root.Value.Representation != TInfo.ExpandExpression)
                yield break;

            foreach (var treeItem in property.Children.Where(x => x.Item2.Root.Value.Representation == TInfo.ExpandExpression).SelectMany(x => x.Item2.Children))
            {
                if (treeItem.Item2.Root.Value.Representation == TInfo.ExpandExpression)
                    foreach (var path in TraverseFakeProperty(from, treeItem.Item2, graph))
                        yield return path;
                else
                    foreach (var path in _ExpandPropertyToEdgeList(treeItem.Item2, graph))
                        yield return path.PrependEdge(graph.Edges.FirstOrDefault(e => e.From.Value.Type == from.Value.Type && e.To.Value.Type == path.Edges.FirstOrDefault().From.Value.Type));
            }
        }

        private static IEnumerable<Path> _ExpandRec(Vertex from, QueueItem tree, NoData.Graph.Graph graph)
        {
            if (tree.IsFakeExpandProperty)
            {
                foreach (var path in TraverseFakeProperty(from, tree, graph))
                    yield return path;
            }
            else
            {
                // get the edge in the graph where it is connected from the same type as the from vertex, and the property name matches.
                var edge = graph.Edges.FirstOrDefault(e => e.From.Value.Type == from.Value.Type && e.Value.PropertyName == tree.Root.Value.Value);
                if (edge == null)
                    throw new IndexOutOfRangeException("Cound not find edge: " + $"from {from.Value.Type} with name: {tree.Root.Value.Value}");

                if (tree.IsDirectPropertyAccess)
                    yield return new Path(new[] { edge });
                foreach (var child in tree.Children)
                    foreach (var path in _ExpandRec(edge.To, child.Item2, graph).Select(x => x.PrependEdge(edge)))
                        yield return path;
            }

        }

        internal static IEnumerable<Path> _ExpandPropertyToEdgeList(QueueItem expandItem, NoData.Graph.Graph graph)
        {
            var rootQueryVertex = graph.VertexContainingType(typeof(TRootQueryType));
            return _ExpandRec(rootQueryVertex, expandItem, graph);
        }
    }
}
