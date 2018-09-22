using System;
using System.Collections.Generic;
using System.Linq;
using Graph;
using NoData.GraphImplementations;

using QueueItem = NoData.GraphImplementations.QueryParser.Tree;
using TInfo = NoData.GraphImplementations.QueryParser.TextInfo;
using ParserVertex = NoData.GraphImplementations.QueryParser.Vertex;
using ParserEdge = NoData.GraphImplementations.QueryParser.Edge;

// I didn't want to import the whole schema namespace here. So at the cost of verbosity, I'm including the specific types I want to import.
using SchemaPath = NoData.GraphImplementations.Schema.Path;
using SchemaEdge = NoData.GraphImplementations.Schema.Edge;
using SchemaVertex = NoData.GraphImplementations.Schema.Vertex;
using GraphSchema = NoData.GraphImplementations.Schema.GraphSchema;


namespace NoData.QueryParser.ParsingTools
{
    public class ExpandClauseParser<TRootQueryType> : AbstractClaseParser<TRootQueryType, IList<SchemaPath>>
    {
        private readonly GraphSchema Graph;
        private readonly IAcceptAdditions _SelectAdd;
        private readonly IAcceptAdditions _FilterAdd;

        public ExpandClauseParser(Func<string, IList<QueueItem>> tokenFunc, string query, GraphSchema graph, IAcceptAdditions select, IAcceptAdditions filter) : base(tokenFunc, query)
        {
            Graph = graph;
            _SelectAdd = select;
            _FilterAdd = filter;
            Result = new List<SchemaPath>();
        }

        public void AddToResult(IEnumerable<SchemaEdge> edgePath)
        {
            Result.Add(new SchemaPath(edgePath));
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
            private GraphSchema graph { get; set; }
            private Action<QueueItem> SelectAddFunc { get; set; }
            private Action<QueueItem> FilterAddFunc { get; set; }

            public TraverseExpandQueueItem(GraphSchema graph, Action<QueueItem> selectAdd, Action<QueueItem> filterAdd)
            {
                this.graph = graph;
                SelectAddFunc = selectAdd;
                FilterAddFunc = filterAdd;
            }

            // note: this mehtod is immutable - it does not change the current state of the "source" argument.
            private QueueItem AppendPathToQueueItem(IEnumerable<SchemaEdge> prependEdges, QueueItem source)
            {
                if (!prependEdges.Any())
                    return source;
                if (source.Representation == TInfo.ExpandProperty)
                {
                    var root = new ParserVertex(new TInfo()
                    {
                        Representation = TInfo.ExpandProperty,
                        Text = prependEdges.First().Value.PropertyName,
                        Value = prependEdges.First().Value.PropertyName
                    });
                    var child = AppendPathToQueueItem(prependEdges.Skip(1), source);
                    return new QueueItem(root, new[] { ITuple.Create(new ParserEdge(root, child.Root), child) });
                }
                var children = source.Children.Select(x => AppendPathToQueueItem(prependEdges, x.Item2));
                var childrenWithEdges = children.Select(child => ITuple.Create(new ParserEdge(source.Root, child.Root), child));
                return new QueueItem(source.Root, childrenWithEdges);
            }

            private IEnumerable<SchemaPath> CombinePathWithChildren(IEnumerable<SchemaEdge> current, IEnumerable<SchemaPath> childrenPaths)
            {
                if (!childrenPaths.Any())
                    yield return new SchemaPath(current);
                foreach (var path in childrenPaths)
                {
                    var edgeList = new List<SchemaEdge>(current);
                    edgeList.AddRange(path.Edges);
                    yield return new SchemaPath(edgeList);
                }
            }

            public IEnumerable<SchemaPath> Traverse(SchemaVertex from, QueueItem tree, IEnumerable<SchemaEdge> currentPath = null)
            {
                if (currentPath == null)
                    currentPath = new List<SchemaEdge>();
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
                    return new List<SchemaPath>();
                }
                if (tree.Representation == TInfo.FilterExpression)
                {
                    FilterAddFunc(AppendPathToQueueItem(currentPath, tree.Children.First().Item2));
                    return new List<SchemaPath>();
                }

                throw new NotImplementedException("Unknown tree type: " + tree.Representation);
            }
            IEnumerable<SchemaPath> ExpandProperty(SchemaVertex from, QueueItem tree, IEnumerable<SchemaEdge> currentPath)
            {
                // get the edge in the graph where it is connected from the same type as the from vertex, and the property name matches.
                var edge = graph.Edges.FirstOrDefault(e => e.From.Value.Type == from.Value.Type && e.Value.PropertyName == tree.Root.Value.Value);
                if (edge == null)
                    yield break;

                if (tree.IsDirectPropertyAccess())
                {
                    var childEdgePath = new List<SchemaEdge>(currentPath);
                    childEdgePath.Add(edge);
                    yield return new SchemaPath(childEdgePath);
                }

                var cPath = new List<SchemaEdge>(currentPath);
                cPath.Add(edge);
                yield return new SchemaPath(cPath);

                foreach (var child in tree.Children)
                    foreach (var path in Traverse(edge.To, child.Item2, cPath).Distinct())
                        yield return path;
            }

            IEnumerable<SchemaPath> TraverseChildren(SchemaVertex from, QueueItem tree, IEnumerable<SchemaEdge> currentPath)
            {
                foreach (var path in tree.Children.SelectMany(x => Traverse(from, x.Item2, currentPath)))
                    yield return path;
            }
        }

        internal IEnumerable<SchemaPath> ExpandPropertyToEdgeList(QueueItem expandItem)
        {
            var traverse = new TraverseExpandQueueItem(Graph, _SelectAdd.AddToClause, _FilterAdd.AddToClause);
            var result = traverse.Traverse(Graph.VertexContainingType(typeof(TRootQueryType)), expandItem).ToList();
            return result;
        }
        internal static IEnumerable<SchemaPath> ExpandPropertyToEdgeList(QueueItem expandItem, GraphSchema graph)
        {
            var traverse = new TraverseExpandQueueItem(graph, ignored => { }, ignored => { });
            var result = traverse.Traverse(graph.VertexContainingType(typeof(TRootQueryType)), expandItem).ToList();
            return result;
        }
    }
}
