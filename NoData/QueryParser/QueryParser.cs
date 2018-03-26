using NoData.Internal.TreeParser.Tokenizer;
using System;
using System.Collections.Generic;
using System.Linq;
using QueueItem = NoData.QueryParser.Graph.Tree;
using QueueGrouper = NoData.QueryParser.ParsingTools.QueueGrouper<NoData.QueryParser.Graph.Tree>;
using NoData.Graph.Base;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

namespace NoData.QueryParser
{
    class QueryParser
    {
        private string Filter { get; set; }
        private string Select { get; set; }
        private string Expand { get; set; }

        private List<NoData.Graph.Path> ExpandPaths { get; set; }
        private QueueItem FilterTree { get; set; }

        private readonly IEnumerable<string> ClassProperties;
        private readonly NoData.Graph.Graph _Graph;

        public QueryParser(QueryParameters parameters, NoData.Graph.Graph graph)
        {
            //graph = _graph;
            Filter = parameters.Filter;
            Select = parameters.Select;
            Expand = parameters.Expand;

            ExpandPaths = new List<NoData.Graph.Path>();
            ClassProperties = graph.Vertices
                .Select(v => Utility.ClassInfoCache.GetOrAdd(v.Value.Type))
                .SelectMany(cp => cp.PropertyNames)
                .Distinct();

            _Graph = graph;
        }

        private IEnumerable<QueueItem> GetTokens(string parmeter)
            => new Tokenizer(ClassProperties).Tokenize(parmeter).Select(t => new QueueItem(new Graph.Vertex(t)));

        public void Parse(Type root)
        {
            do
            {
                var expand = Expand;
                var filter = Filter;
                var select = Select;
                Expand = null;
                Filter = null;
                Select = null;

                if(expand != null)
                    ParseExpand(expand, root);
                if(filter != null)
                    ParseFilter(filter, root);
                if(select != null)
                    ParseSelect(select, root);
            } while (Expand != null || Filter != null || Select != null);
        }

        public NoData.Graph.Tree SelectionTree(Type rootQueryType)
        {
            var rootQueryVertex = _Graph.VertexContainingType(rootQueryType);
            return NoData.Graph.Tree.CreateFromPathsTree(rootQueryVertex, ExpandPaths.Where(p => p.Edges.Count() > 0));
        }

        public Expression ApplyFilterExpression(Type rootQueryType)
        {
            if (FilterTree is null)
                return null;
            var expression = FilterTree.Root.FilterExpression(Expression.Parameter(rootQueryType));
            return _Graph.VertexContainingType(rootQueryType).Value.FilterExpression = expression;
        }

        private void AddCommonTerms(QueueGrouper grouper)
        {
            
        }

        private void AddExpandProperty(QueueGrouper grouper)
        {
            grouper.AddGroupingTerm($"{Graph.TextInfo.ClassProperty}({Graph.TextInfo.ForwardSlash}{Graph.TextInfo.ClassProperty})*", list =>
            {
                var itemList = new Stack<Graph.Vertex>();
                itemList.Push(list[0].Root);

                var toRemove = 0;
                while (toRemove + 2 < list.Count)
                {
                    var representation = list[toRemove + 1].Root.Value.Representation;
                    if (list[toRemove + 1].Root.Value.Representation == Graph.TextInfo.ForwardSlash &&
                        list[toRemove + 2].Root.Value.Representation == Graph.TextInfo.ClassProperty)
                    {
                        itemList.Push(list[toRemove + 2].Root);
                        toRemove += 2;
                    }
                    else break;
                }

                // build from linear list, the tree
                QueueItem item = null;
                while (itemList.Count() != 0)
                {
                    var temp = itemList.Pop();
                    temp.Value.Representation = Graph.TextInfo.ExpandProperty;
                    if (item == null)
                        item = new QueueItem(temp);
                    else
                    {
                        var edge = new Graph.Edge(temp, item.Root);
                        item = new QueueItem(temp, new[] { ITuple.Create(edge, item) });
                    }
                }

                return new Tuple<QueueItem, int>(item, toRemove);
            });

        }

        private void AddTermsForExpand(QueueGrouper grouper)
        {
            AddExpandProperty(grouper);

            // expand property grouper
            grouper.AddGroupingTerm($"{Graph.TextInfo.ExpandProperty}(\\/{Graph.TextInfo.ExpandProperty})*", list =>
            {
                var children = new Queue<QueueItem>();
                children.Enqueue(list[0]);

                var toRemove = 0;
                while(toRemove + 2 < list.Count)
                {
                    var representation = list[toRemove + 1].Root.Value.Representation;
                    if (list[toRemove + 1].Root.Value.Representation == Graph.TextInfo.Comma &&
                        list[toRemove + 2].Root.Value.Representation == Graph.TextInfo.ExpandProperty)
                    {
                        children.Enqueue(list[toRemove + 2]);
                        toRemove += 2;
                    }
                    else break;
                }

                var root = new Graph.Vertex(new Graph.TextInfo { Representation = Graph.TextInfo.ListOfExpands });
                var edges = children.Select(t => new Graph.Edge(root, t.Root));
                var childrenItems = new List<ITuple<Graph.Edge, QueueItem>>();
                foreach(var child in children)
                    childrenItems.Add(ITuple.Create(edges.First(e => e.To == child.Root), child));

                QueueItem item = new QueueItem(root, childrenItems);

                return new Tuple<QueueItem, int>(item, toRemove);
            });
        }

        private void AddTermsForFilter(QueueGrouper grouper, Type rootQueryType)
        {
            AddExpandProperty(grouper);

            // Take an expand property, and make it a value type.
            grouper.AddGroupingTerm(Graph.TextInfo.ExpandProperty, list =>
            {
                var item = list[0];
                var properyNameList = new List<string>();
                item.Traverse((Graph.Vertex e) =>
                {
                    properyNameList.Add(e.Value.Value);
                });
                var path = string.Join("/", properyNameList);

                var propInfo = NoData.Graph.Utility.GraphUtility.GetPropertyFromPathString(path, rootQueryType, _Graph);
                if(propInfo is null) return null;
                var rep = Graph.TextInfo.RawTextRepresentation;
                var type = propInfo.PropertyType;
                if (
                    type == typeof(Int16) ||
                    type == typeof(Int32) ||
                    type == typeof(long) ||
                    type == typeof(double) ||
                    type == typeof(float) ||
                    type == typeof(decimal)
                )
                    rep = Graph.TextInfo.NumberValue;
                else if (
                    type == typeof(DateTime) ||
                    type == typeof(DateTimeOffset)
                )
                    rep = Graph.TextInfo.DateValue;
                else if (
                    type == typeof(string)
                )
                    rep = Graph.TextInfo.TextValue;

                var root = new Graph.Vertex(new Graph.TextInfo { Value = propInfo.PropertyType.Name, Representation = rep });
                var child = item;
                var edge = new Graph.Edge(root, child.Root);
                return new Tuple<QueueItem, int>(new QueueItem(root, new[] { ITuple.Create(edge, new QueueItem(child.Root)) }), 0);
            });

            grouper.AddGroupingTerm(Graph.TextInfo.Inverse + Graph.TextInfo.BooleanValue, list =>
            {
                var root = new Graph.Vertex(new Graph.TextInfo { Value = "!", Representation = Graph.TextInfo.BooleanValue });
                var child = list[1];
                var edge = new Graph.Edge(root, child.Root);
                return new Tuple<QueueItem, int>(new QueueItem(root, new[] { ITuple.Create(edge, new QueueItem(child.Root)) }), 1);
            });

            Tuple<QueueItem, int> valueItemValue(IList<QueueItem> list) {
                var root = new Graph.Vertex(new Graph.TextInfo { Value = list[1].Root.Value.Representation, Representation = Graph.TextInfo.BooleanValue });
                var left = list[0];
                var right = list[0];
                var edgeLeft = new Graph.Edge(root, left.Root);
                var edgeRight = new Graph.Edge(root, right.Root);
                return new Tuple<QueueItem, int>(new QueueItem(root, new[] {
                    ITuple.Create(edgeLeft, left),
                    ITuple.Create(edgeRight, right),
                }), 2);
            }

            string valueComparisonPattern(string a, string b) => Regex.Escape(a + Graph.TextInfo.ValueComparison + b);
            grouper.AddGroupingTerm(valueComparisonPattern(Graph.TextInfo.BooleanValue, Graph.TextInfo.BooleanValue), valueItemValue);
            grouper.AddGroupingTerm(valueComparisonPattern(Graph.TextInfo.TextValue, Graph.TextInfo.TextValue), valueItemValue);
            grouper.AddGroupingTerm(valueComparisonPattern(Graph.TextInfo.NumberValue, Graph.TextInfo.NumberValue), valueItemValue);
            grouper.AddGroupingTerm(valueComparisonPattern(Graph.TextInfo.DateValue, Graph.TextInfo.DateValue), valueItemValue);

            string logicComparisonPattern(string a, string b) => $"{a}{Graph.TextInfo.LogicalComparison}{b}";
            grouper.AddGroupingTerm(logicComparisonPattern(Graph.TextInfo.DateValue, Graph.TextInfo.DateValue), valueItemValue);
            //    else if (token.Type == NodeTokenTypes.GroupValueGroup.ToString())
            //    {
            //        foundMatch = true;
            //        queue.RemoveAt(three);
            //        queue.RemoveAt(one);
            //    }
            //}
        }

        private void ParseExpand(string querystring, Type rootQueryType)
        {
            var tokens = new List<QueueItem>(GetTokens(querystring));
            if (tokens.Count == 0)
                return;
            var queueGrouper = new QueueGrouper(tokens, QueueItem.GetRepresentationValue);
            AddTermsForExpand(queueGrouper);
            var parsed = queueGrouper.Reduce();
            if(parsed.Root.Value.Representation != Graph.TextInfo.ListOfExpands && 
                parsed.Root.Value.Representation != Graph.TextInfo.ExpandProperty)
                throw new ArgumentException("invalid query");

            var groupOfExpansions = parsed?.Children;

            if(groupOfExpansions is null)
                throw new ArgumentException("invalid query");

            foreach(var expansion in groupOfExpansions.Select(x => x.Item2))
            {
                // add to paths.
                var edges = new List<NoData.Graph.Edge>();
                void traverseExpandTree(NoData.Graph.Vertex from, QueueItem tree)
                {
                    if (tree?.Root?.Value.Representation != Graph.TextInfo.ExpandProperty) return;
                    // get the edge in the graph where it is connected from the same type as the from vertex, and the property name matches.
                    var edge = _Graph.Edges.FirstOrDefault(e => e.From.Value.Type == from.Value.Type && e.Value.PropertyName == tree.Root.Value.Value);
                    edges.Add(edge);
                    foreach (var child in tree.Children)
                        traverseExpandTree(edge.To, child.Item2);
                }
                var rootQueryVertex = _Graph.VertexContainingType(rootQueryType);
                traverseExpandTree(rootQueryVertex, expansion);
                ExpandPaths.Add(new NoData.Graph.Path(edges));
            }
        }

        private void ParseFilter(string querystring, Type rootQueryType)
        {
            var tokens = new List<QueueItem>(GetTokens(querystring));
            var queueGrouper = new QueueGrouper(tokens, QueueItem.GetRepresentationValue);
            AddTermsForFilter(queueGrouper, rootQueryType);

            var filter = queueGrouper.Reduce();
            if (tokens.Count != 0)
                return;

            FilterTree = filter;;
        }

        private void ParseSelect(string querystring, Type rootQueryType)
        {
        }
    }
}
