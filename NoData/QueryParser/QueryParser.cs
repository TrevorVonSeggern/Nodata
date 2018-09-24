﻿using System;
using System.Collections.Generic;
using System.Linq;
using Graph;
using System.Linq.Expressions;
using NoData.Internal.TreeParser.Tokenizer;
using NoData.QueryParser.ParsingTools;
using NoData.QueryParser.ParsingTools.Groupings;
using NoData.GraphImplementations.Schema;

using QueueItem = NoData.GraphImplementations.QueryParser.Tree;
using ParserVertex = NoData.GraphImplementations.QueryParser.Vertex;
using NoData.Utility;

namespace NoData.QueryParser
{
    public class QueryParser<TRootVertex>
    {
        public IClassCache Cache { get; }
        public bool IsParsed { get; private set; }
        private static readonly Type RootQueryType = typeof(TRootVertex);

        private FilterClauseParser<TRootVertex> Filter { get; set; }
        private ExpandClauseParser<TRootVertex> Expand { get; set; }
        private SelectClauseParser<TRootVertex> Select { get; set; }
        private OrderByClauseParser<TRootVertex> OrderBy { get; set; }

        private readonly IEnumerable<string> ClassProperties;
        private readonly GraphSchema _Graph;

        private IList<QueueItem> _GetTokens(string parmeter) => new Tokenizer(ClassProperties).Tokenize(parmeter).Select(t => new QueueItem(new ParserVertex(t))).ToList();

        public QueryParser(Parameters parameters, GraphSchema graph, IClassCache cache)
        {
            IsParsed = false;

            ClassProperties = graph.Vertices
                .Select(v => v.Value.Properties)
                .SelectMany(x => x).Select(x => x.Name)
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
            Cache = cache;

            Parse();
        }

        private void Parse()
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

        private Tree _selectionTree { get; set; }
        public Tree SelectionTree
        {
            get
            {
                _AssertParsed();
                if (_selectionTree is null)
                {
                    var rootQueryVertex = _Graph.VertexContainingType(RootQueryType);
                    _selectionTree = Tree.CreateFromPathsTree(rootQueryVertex, Expand.Result.Where(p => p.Edges.Any()), Select.Result);
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
            return new FilterTreeExpressionBuilder(_Graph).FilterExpression(Filter.Result, parameter, Cache);
        }
    }
}
