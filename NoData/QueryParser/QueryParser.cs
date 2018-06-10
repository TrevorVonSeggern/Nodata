using NoData.Internal.TreeParser.Tokenizer;
using System;
using System.Collections.Generic;
using System.Linq;
using QueueItem = NoData.QueryParser.Graph.Tree;
using NoData.Graph.Base;
using System.Linq.Expressions;
using NoData.QueryParser.ParsingTools;
using NoData.QueryParser.ParsingTools.Groupings;
using NoData.Graph;

namespace NoData.QueryParser
{
    internal class QueryParser<TRootVertex>
    {
        public bool IsParsed { get; private set; }
        private static readonly Type RootQueryType = typeof(TRootVertex);

        private FilterClauseParser<TRootVertex> Filter { get; set; }
        private ExpandClauseParser<TRootVertex> Expand { get; set; }
        private SelectClauseParser<TRootVertex> Select { get; set; }
        private OrderByClauseParser<TRootVertex> OrderBy { get; set; }

        private readonly IEnumerable<string> ClassProperties;
        private readonly NoData.Graph.Graph _Graph;

        private IList<QueueItem> _GetTokens(string parmeter) => new Tokenizer(ClassProperties).Tokenize(parmeter).Select(t => new QueueItem(new Graph.Vertex(t))).ToList();

        public QueryParser(QueryParameters parameters, NoData.Graph.Graph graph)
        {
            IsParsed = false;

            ClassProperties = graph.Vertices
                .Select(v => Utility.ClassInfoCache.GetOrAdd(v.Value.Type))
                .SelectMany(cp => cp.PropertyNames)
                .Distinct();

            OrderBy = new OrderByClauseParser<TRootVertex>(x => _GetTokens(x), parameters.OrderBy, graph);
            OrderBy.AddGroupingTerms(ExpandGroupings.ExpandProperty);
            OrderBy.AddGroupingTerms(OrderGroupings.SortOrderProperty);
            OrderBy.AddGroupingTerms(OrderGroupings.ListOfSorting);

            Select = new SelectClauseParser<TRootVertex>(x => _GetTokens(x), parameters.Select, graph);
            Select.AddGroupingTerms(ExpandGroupings.ExpandProperty);
            Select.AddGroupingTerms(ExpandGroupings.ListOfExpand);

            Filter = new FilterClauseParser<TRootVertex>(x => _GetTokens(x), parameters.Filter);
            Filter.AddGroupingTerms(ExpandGroupings.ExpandProperty);
            Filter.AddGroupingTerms(FilterGroupings.AddTermsForFilter());

            Expand = new ExpandClauseParser<TRootVertex>(x => _GetTokens(x), parameters.Expand, graph, Select, Filter);
            Expand.AddGroupingTerms(ExpandGroupings.ExpandProperty);
            Expand.AddGroupingTerms(FilterGroupings.AddTermsForFilter());
            Expand.AddGroupingTerms(ExpandGroupings.ExpandPropertyWithListOfClauses);
            Expand.AddGroupingTerms(ExpandGroupings.ListOfExpand);
            Expand.AddGroupingTerms(ExpandGroupings.ExpandExpression);
            Expand.AddGroupingTerms(ExpandGroupings.FilterExpression);
            Expand.AddGroupingTerms(ExpandGroupings.SelectExpression);
            Expand.AddGroupingTerms(ExpandGroupings.ListOfClauseExpressions());

            _Graph = graph;
        }

        public void Parse()
        {
            do
            {
                if (!OrderBy.IsFinished)
                    OrderBy.Parse();
                if (!Expand.IsFinished)
                    Expand.Parse();
                if (!Filter.IsFinished)
                    Filter.Parse();
                if (!Select.IsFinished)
                    Select.Parse();
            } while (!Expand.IsFinished || !Filter.IsFinished || !Select.IsFinished);
            IsParsed = true;
        }

        private void _AssertParsed()
        {
            if (!IsParsed)
                throw new Exception("Need to parse before the this is available.");
        }

        private NoData.Graph.Tree _selectionTree { get; set; }
        public NoData.Graph.Tree SelectionTree
        {
            get
            {
                _AssertParsed();
                if (_selectionTree is null)
                {
                    var rootQueryVertex = _Graph.VertexContainingType(RootQueryType);
                    _selectionTree = NoData.Graph.Tree.CreateFromPathsTree(rootQueryVertex, Expand.Result.Where(p => p.Edges.Any()), Select.Result);
                }
                return _selectionTree;
            }
        }

        private List<ITuple<PathToProperty, SortDirection>> _orderByPath { get; set; }
        public IEnumerable<ITuple<PathToProperty, SortDirection>> OrderByPath
        {
            get
            {
                _AssertParsed();
                if (_orderByPath is null)
                    _orderByPath = OrderBy.Result.ToList();
                return _orderByPath;
            }
        }

        public Expression ApplyFilterExpression(ParameterExpression parameter)
        {
            _AssertParsed();
            if (Filter.Result is null)
                return null;
            return new FilterTreeExpressionBuilder(_Graph).FilterExpression(Filter.Result, parameter);
        }
    }
}
