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
    class ExpandClaseParser<TRootQueryType> : AbstractClaseParser<TRootQueryType, IList<Path>>
    {
        private readonly NoData.Graph.Graph Graph;

        public ExpandClaseParser(Func<string, IList<QueueItem>> tokenFunc, string query, NoData.Graph.Graph graph) : base(tokenFunc, query)
        {
            Graph = graph;
            Result = new List<Path>();
        }

        public override void Parse()
        {
            if(!SetupParsing())
                return;

            var queueGrouper = Grouper;

            var parsed = queueGrouper.Reduce();
            if (parsed.Root.Value.Representation != TInfo.ListOfExpands &&
                parsed.Root.Value.Representation != TInfo.ExpandProperty)
                throw new ArgumentException("invalid query");

            var groupOfExpansions = parsed?.Children;

            if (groupOfExpansions is null)
                throw new ArgumentException("invalid query");

            foreach (var expansion in groupOfExpansions)
                Result.Add(new Path(_ExpandPropertyToEdgeList(expansion, Graph)));
        }

        internal static IEnumerable<NoData.Graph.Edge> _ExpandPropertyToEdgeList(ITuple<Graph.Edge, QueueItem> expandItem, NoData.Graph.Graph graph)
        {
            var edges = new List<NoData.Graph.Edge>();
            void traverseExpandTree(NoData.Graph.Vertex from, QueueItem tree)
            {
                if (tree?.Root?.Value.Representation != TInfo.ExpandProperty) return;
                // get the edge in the graph where it is connected from the same type as the from vertex, and the property name matches.
                var edge = graph.Edges.FirstOrDefault(e => e.From.Value.Type == from.Value.Type && e.Value.PropertyName == tree.Root.Value.Value);
                edges.Add(edge);
                foreach (var child in tree.Children)
                    traverseExpandTree(edge.To, child.Item2);
            }
            var rootQueryVertex = graph.VertexContainingType(typeof(TRootQueryType));
            traverseExpandTree(rootQueryVertex, expandItem.Item2);
            return edges;
        }
    }
}
