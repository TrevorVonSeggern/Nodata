using System;
using System.Collections.Generic;
using System.Linq;
using NoData.Graph;
using NoData.Graph.Base;
using QueueItem = NoData.QueryParser.Graph.Tree;
using TInfo = NoData.QueryParser.Graph.TextInfo;

namespace NoData.QueryParser.ParsingTools
{
    public class ExpandClauseParser<TRootQueryType> : AbstractClaseParser<TRootQueryType, IList<Path>>
    {
        private readonly NoData.Graph.Graph Graph;
        private readonly IAcceptAdditions _SelectAdd;
        private readonly IAcceptAdditions _FilterAdd;

        public ExpandClauseParser(Func<string, IList<QueueItem>> tokenFunc, string query, NoData.Graph.Graph graph, IAcceptAdditions select, IAcceptAdditions filter) : base(tokenFunc, query)
        {
            Graph = graph;
            _SelectAdd = select;
            _FilterAdd = filter;
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

            if (parsed is null || (parsed != null && parsed.Root.Value.Representation != TInfo.ListOfExpands &&
                parsed.Root.Value.Representation != TInfo.ExpandProperty))
                throw new ArgumentException("invalid query");

            var groupOfExpansions = parsed?.Children;

            foreach (var expansion in groupOfExpansions.Select(x => x.Item2))
                foreach (var path in ExpandPropertyToEdgeList(expansion))
                    Result.Add(path);
        }

        // class to contain functions specific to parsing.
        private class TraverseExpandQueueItem
        {
            private NoData.Graph.Graph graph { get; set; }
            private Action<QueueItem> SelectAddFunc { get; set; }
            private Action<QueueItem> FilterAddFunc { get; set; }

            public TraverseExpandQueueItem(NoData.Graph.Graph graph, Action<QueueItem> selectAdd, Action<QueueItem> filterAdd)
            {
                this.graph = graph;
                SelectAddFunc = selectAdd;
                FilterAddFunc = filterAdd;
            }

            // note: this mehtod is immutable - it does not change the current state of the "source" argument.
            private QueueItem AppendPathToQueueItem(IEnumerable<Edge> prependEdges, QueueItem source)
            {
                if (!prependEdges.Any())
                    return source;
                if (source.Representation == TInfo.ExpandProperty)
                {
                    var root = new Graph.Vertex(new TInfo()
                    {
                        Representation = TInfo.ExpandProperty,
                        Text = prependEdges.First().Value.PropertyName,
                        Value = prependEdges.First().Value.PropertyName
                    });
                    var child = AppendPathToQueueItem(prependEdges.Skip(1), source);
                    return new QueueItem(root, new[] { ITuple.Create(new Graph.Edge(root, child.Root), child) });
                }
                var children = source.Children.Select(x => AppendPathToQueueItem(prependEdges, x.Item2));
                var childrenWithEdges = children.Select(child => ITuple.Create(new Graph.Edge(source.Root, child.Root), child));
                return new QueueItem(source.Root, childrenWithEdges);
            }

            private IEnumerable<Path> CombinePathWithChildren(IEnumerable<Edge> current, IEnumerable<Path> childrenPaths)
            {
                if (!childrenPaths.Any())
                    yield return new Path(current);
                foreach (var path in childrenPaths)
                {
                    var edgeList = new List<Edge>(current);
                    edgeList.AddRange(path.Edges);
                    yield return new Path(edgeList);
                }
            }

            public IEnumerable<Path> Traverse(Vertex from, QueueItem tree, IEnumerable<Edge> currentPath = null)
            {
                if (currentPath == null)
                    currentPath = new List<Edge>();
                if (tree.Representation == TInfo.ExpandProperty)
                    return ExpandProperty(from, tree, currentPath);

                bool IsPassThrough(string rep)
                {
                    return rep == TInfo.ListOfExpands || rep == TInfo.ListOfClause || rep == TInfo.ExpandExpression;
                }

                if (IsPassThrough(tree.Representation))
                    return TraverseChildren(from, tree, currentPath).Distinct();

                if (tree.Representation == TInfo.SelectExpression)
                {
                    SelectAddFunc(AppendPathToQueueItem(currentPath, tree.Children.First().Item2));
                    return new List<Path>();
                }
                if (tree.Representation == TInfo.FilterExpression)
                {
                    FilterAddFunc(AppendPathToQueueItem(currentPath, tree.Children.First().Item2));
                    return new List<Path>();
                }

                throw new NotImplementedException("Unknown tree type: " + tree.Representation);
            }
            IEnumerable<Path> ExpandProperty(Vertex from, QueueItem tree, IEnumerable<Edge> currentPath)
            {
                // get the edge in the graph where it is connected from the same type as the from vertex, and the property name matches.
                var edge = graph.Edges.FirstOrDefault(e => e.From.Value.Type == from.Value.Type && e.Value.PropertyName == tree.Root.Value.Value);
                if (edge == null)
                    yield break;

                if (tree.IsDirectPropertyAccess())
                {
                    var childEdgePath = new List<Edge>(currentPath);
                    childEdgePath.Add(edge);
                    yield return new Path(childEdgePath);
                }

                var cPath = new List<Edge>(currentPath);
                cPath.Add(edge);
                yield return new Path(cPath);

                foreach (var child in tree.Children)
                    foreach (var path in Traverse(edge.To, child.Item2, cPath).Distinct())
                        yield return path;
            }

            IEnumerable<Path> TraverseChildren(Vertex from, QueueItem tree, IEnumerable<Edge> currentPath)
            {
                foreach (var path in tree.Children.SelectMany(x => Traverse(from, x.Item2, currentPath)))
                    yield return path;
            }
        }

        internal IEnumerable<Path> ExpandPropertyToEdgeList(QueueItem expandItem)
        {
            var traverse = new TraverseExpandQueueItem(Graph, _SelectAdd.AddToClause, _FilterAdd.AddToClause);
            var result = traverse.Traverse(Graph.VertexContainingType(typeof(TRootQueryType)), expandItem).ToList();
            return result;
        }
        internal static IEnumerable<Path> ExpandPropertyToEdgeList(QueueItem expandItem, NoData.Graph.Graph graph)
        {
            var traverse = new TraverseExpandQueueItem(graph, ignored => { }, ignored => { });
            var result = traverse.Traverse(graph.VertexContainingType(typeof(TRootQueryType)), expandItem).ToList();
            return result;
        }
    }
}
