using NoData.Internal.TreeParser.Tokenizer;
using System;
using System.Collections.Generic;
using System.Linq;
using QueueItem = NoData.QueryParser.Graph.Tree;
using QueueGrouper = NoData.QueryParser.ParsingTools.QueueGrouper<NoData.QueryParser.Graph.Tree>;
using NoData.Graph.Base;
using System.Linq.Expressions;

namespace NoData.QueryParser
{
    internal class QueryParser<TRootVertex>
    {
        public bool IsParsed { get; private set; }
        private static readonly Type RootQueryType = typeof(TRootVertex);

        private string filter { get; set; }
        private string select { get; set; }
        private string expand { get; set; }
        private string Filter { get { return filter; } set { if (IsParsed) throw new Exception("Cannot change after parsed"); filter = value; } }
        private string Select { get { return select; } set { if (IsParsed) throw new Exception("Cannot change after parsed"); select = value; } }
        private string Expand { get { return expand; } set { if (IsParsed) throw new Exception("Cannot change after parsed"); expand = value; } }

        private List<NoData.Graph.Path> ExpandPaths { get; set; }
        private List<ITuple<NoData.Graph.Path, string>> SelectionPaths { get; set; }
        private QueueItem FilterTree { get; set; }

        private readonly IEnumerable<string> ClassProperties;
        private readonly NoData.Graph.Graph _Graph;

        public QueryParser(QueryParameters parameters, NoData.Graph.Graph graph)
        {
            IsParsed = false;
            Filter = parameters.Filter;
            Select = parameters.Select;
            Expand = parameters.Expand;

            ExpandPaths = new List<NoData.Graph.Path>();
            SelectionPaths = new List<ITuple<NoData.Graph.Path, string>>();
            ClassProperties = graph.Vertices
                .Select(v => Utility.ClassInfoCache.GetOrAdd(v.Value.Type))
                .SelectMany(cp => cp.PropertyNames)
                .Distinct();

            _Graph = graph;
        }

        private IEnumerable<QueueItem> GetTokens(string parmeter)
            => new Tokenizer(ClassProperties).Tokenize(parmeter).Select(t => new QueueItem(new Graph.Vertex(t)));

        public void Parse()
        {
            do
            {
                var expand = Expand;
                var filter = Filter;
                var select = Select;
                Expand = null;
                Filter = null;
                Select = null;

                if (expand != null)
                    ParseExpand(expand);
                if (filter != null)
                    ParseFilter(filter);
                if (select != null)
                    ParseSelect(select);
            } while (Expand != null || Filter != null || Select != null);
            IsParsed = true;
        }

        private void AssertParsed()
        {
            if (!IsParsed)
                throw new Exception("Need to parse before the this is available.");
        }

        private NoData.Graph.Tree _selectionTree { get; set; }
        public NoData.Graph.Tree SelectionTree
        {
            get
            {
                AssertParsed();
                if (_selectionTree is null)
                {
                    var rootQueryVertex = _Graph.VertexContainingType(RootQueryType);
                    _selectionTree = NoData.Graph.Tree.CreateFromPathsTree(rootQueryVertex, ExpandPaths.Where(p => p.Edges.Count() > 0), SelectionPaths);
                }
                return _selectionTree;
            }
        }

        public Expression ApplyFilterExpression(ParameterExpression parameter)
        {
            AssertParsed();
            if (FilterTree is null)
                return null;
            return FilterTree.FilterExpression(parameter);
        }

        public Expression ApplySelectExpression(ParameterExpression parameter)
        {
            AssertParsed();
            var tree = SelectionTree;
            return null;
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
        private void AddCollectionOfExpandProperty(QueueGrouper grouper)
        {
            // expand property grouper
            grouper.AddGroupingTerm($"{Graph.TextInfo.ExpandProperty}(\\/{Graph.TextInfo.ExpandProperty})*", list =>
            {
                var children = new Queue<QueueItem>();
                children.Enqueue(list[0]);

                var toRemove = 0;
                while (toRemove + 2 < list.Count)
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
                foreach (var child in children)
                    childrenItems.Add(ITuple.Create(edges.First(e => e.To == child.Root), child));

                QueueItem item = new QueueItem(root, childrenItems);

                return new Tuple<QueueItem, int>(item, toRemove);
            });
        }

        private void AddTermsForExpand(QueueGrouper grouper)
        {
            AddExpandProperty(grouper);
            AddCollectionOfExpandProperty(grouper);
        }

        private void AddTermsForFilter(QueueGrouper grouper)
        {
            AddExpandProperty(grouper);

            // Take an expand property, and make it a value type.
            grouper.AddGroupingTerm(Graph.TextInfo.ExpandProperty, list =>
            {
                var item = list[0];
                var propertyNameList = new List<string>();

                item.Traverse((Graph.Vertex e) =>
                {
                    propertyNameList.Add(e.Value.Value);
                });

                var propInfo = NoData.Graph.Utility.GraphUtility.GetPropertyFromPathString(string.Join("/", propertyNameList), RootQueryType, _Graph);
                if (propInfo is null) return null;
                var rep = Graph.TextInfo.RawTextRepresentation;
                var type = propInfo.PropertyType;
                if (type == typeof(Int16) ||
                    type == typeof(Int32) ||
                    type == typeof(long) ||
                    type == typeof(double) ||
                    type == typeof(float) ||
                    type == typeof(decimal))
                    rep = Graph.TextInfo.NumberValue;
                else if (type == typeof(DateTime) || type == typeof(DateTimeOffset))
                    rep = Graph.TextInfo.DateValue;
                else if (type == typeof(string))
                    rep = Graph.TextInfo.TextValue;

                var root = new Graph.Vertex(new Graph.TextInfo { Value = propInfo.PropertyType.Name, Text = propInfo.Name, Representation = rep, Type = type });
                var child = item;
                var edge = new Graph.Edge(root, child.Root);
                return new Tuple<QueueItem, int>(new QueueItem(root, new[] { ITuple.Create(edge, child) }), 0);
            });

            grouper.AddGroupingTerm(Graph.TextInfo.Inverse + Graph.TextInfo.BooleanValue, list =>
            {
                var root = new Graph.Vertex(new Graph.TextInfo { Value = "!", Representation = Graph.TextInfo.BooleanValue });
                var child = list[1];
                var edge = new Graph.Edge(root, child.Root);
                return new Tuple<QueueItem, int>(new QueueItem(root, new[] { ITuple.Create(edge, new QueueItem(child.Root)) }), 1);
            });

            Tuple<QueueItem, int> valueItemValue(IList<QueueItem> list)
            {
                var root = new Graph.Vertex(new Graph.TextInfo { Value = list[1].Root.Value.Representation, Text = list[1].Root.Value.Text, Representation = Graph.TextInfo.BooleanValue });
                var left = list[0];
                var right = list[2];
                var edgeLeft = new Graph.Edge(root, left.Root);
                var edgeRight = new Graph.Edge(root, right.Root);
                return new Tuple<QueueItem, int>(new QueueItem(root, new[] {
                    ITuple.Create(edgeLeft, left),
                    ITuple.Create(edgeRight, right),
                }), 2);
            }

            string valueComparisonPattern(string a, string b) => a + Graph.TextInfo.ValueComparison + b;
            grouper.AddGroupingTerm(valueComparisonPattern(Graph.TextInfo.BooleanValue, Graph.TextInfo.BooleanValue), valueItemValue);
            grouper.AddGroupingTerm(valueComparisonPattern(Graph.TextInfo.TextValue, Graph.TextInfo.TextValue), valueItemValue);
            grouper.AddGroupingTerm(valueComparisonPattern(Graph.TextInfo.NumberValue, Graph.TextInfo.NumberValue), valueItemValue);
            grouper.AddGroupingTerm(valueComparisonPattern(Graph.TextInfo.DateValue, Graph.TextInfo.DateValue), valueItemValue);

            Tuple<QueueItem, int> undoGrouping(IList<QueueItem> list)
            {
                return new Tuple<QueueItem, int>(list[1], 2);
            }
            string anyValueTypeRegex = $"({Graph.TextInfo.BooleanValue}|{Graph.TextInfo.NumberValue}|{Graph.TextInfo.TextValue}|{Graph.TextInfo.DateValue})";
            grouper.AddGroupingTerm(Graph.TextInfo.OpenParenthesis + anyValueTypeRegex + Graph.TextInfo.CloseParenthesis, undoGrouping);

            string logicComparisonPattern(string a, string b) => $"{a}{Graph.TextInfo.LogicalComparison}{b}";
            grouper.AddGroupingTerm(logicComparisonPattern(Graph.TextInfo.BooleanValue, Graph.TextInfo.BooleanValue), valueItemValue);
        }

        private void AddTermsForSelect(QueueGrouper grouper)
        {
            AddExpandProperty(grouper);
            AddCollectionOfExpandProperty(grouper);
        }

        private void ParseExpand(string querystring)
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
                var rootQueryVertex = _Graph.VertexContainingType(RootQueryType);
                traverseExpandTree(rootQueryVertex, expansion);
                ExpandPaths.Add(new NoData.Graph.Path(edges));
            }
        }

        private void ParseFilter(string querystring)
        {
            var tokens = new List<QueueItem>(GetTokens(querystring));
            var queueGrouper = new QueueGrouper(tokens, QueueItem.GetRepresentationValue);
            AddTermsForFilter(queueGrouper);

            var filter = queueGrouper.Reduce();
            if (tokens.Count != 1)
                return;

            FilterTree = filter;
        }

        private void ParseSelect(string querystring)
        {
            var tokens = new List<QueueItem>(GetTokens(querystring));
            var queueGrouper = new QueueGrouper(tokens, QueueItem.GetRepresentationValue);
            AddTermsForSelect(queueGrouper);

            var select = queueGrouper.Reduce();
            if (tokens.Count != 1)
                throw new Exception("Unexped output from parsing.");

            var parsed = queueGrouper.Reduce();
            if (parsed.Root.Value.Representation != Graph.TextInfo.ListOfExpands &&
                parsed.Root.Value.Representation != Graph.TextInfo.ExpandProperty)
                throw new ArgumentException("invalid query");

            var groupOfSelects = parsed?.Children;

            if (groupOfSelects is null)
                throw new ArgumentException("invalid query");

            foreach (var propertySelection in groupOfSelects.Select(x => x.Item2))
            {
                // add to paths.
                var edges = new List<NoData.Graph.Edge>();
                string propertyName = null;
                void traverseExpandTree(NoData.Graph.Vertex from, QueueItem parsedSelection)
                {
                    if (parsedSelection?.Root?.Value.Representation != Graph.TextInfo.ExpandProperty) return;
                    if(!parsedSelection.Children.Any())
                    {
                        propertyName = parsedSelection.Root.Value.Text;
                        return;
                    }

                    // get the edge in the graph where it is connected from the same type as the from vertex, and the property name matches.
                    var edge = _Graph.Edges.FirstOrDefault(e => e.From.Value.Type == from.Value.Type && e.Value.PropertyName == parsedSelection.Root.Value.Value);
                    if (edge is null)
                        return;
                    edges.Add(edge);
                    foreach (var child in parsedSelection.Children)
                        traverseExpandTree(edge.To, child.Item2);
                }
                var rootQueryVertex = _Graph.VertexContainingType(RootQueryType);
                traverseExpandTree(rootQueryVertex, propertySelection);
                SelectionPaths.Add(ITuple.Create(new NoData.Graph.Path(edges), propertyName));
            }

        }
    }
}
