using System;
using System.Collections.Generic;
using System.Linq;
using Graph;
using System.Linq.Expressions;
using NoData.Internal.TreeParser.Tokenizer;
using NoData.QueryParser.ParsingTools;
using NoData.QueryParser.ParsingTools.Groupings;
using NoData.GraphImplementations.Schema;
using NoData.Utility;
using QuickCache;

using QueueItem = NoData.GraphImplementations.QueryParser.Tree;
using ParserVertex = NoData.GraphImplementations.QueryParser.Vertex;
using TGrouping = Graph.ITuple<System.Text.RegularExpressions.Regex, System.Func<System.Collections.Generic.IList<NoData.GraphImplementations.QueryParser.Tree>, Graph.ITuple<NoData.GraphImplementations.QueryParser.Tree, int>>>;
using TTermDict = System.Collections.Generic.IReadOnlyDictionary<System.Text.RegularExpressions.Regex, System.Func<System.Collections.Generic.IList<NoData.GraphImplementations.QueryParser.Tree>, Graph.ITuple<NoData.GraphImplementations.QueryParser.Tree, int>>>;
using System.Text.RegularExpressions;

namespace NoData.QueryParser
{
    public class QueryParser<TRootVertex>
    {
        private static ICacheForever<int, List<Token>> tokenizerCache = new DictionaryCache<int, List<Token>>();

        public IClassCache Cache { get; }
        public bool IsParsed { get; private set; }
        private static readonly Type RootQueryType = typeof(TRootVertex);

        private FilterClauseParser<TRootVertex> Filter { get; set; }
        private ExpandClauseParser<TRootVertex> Expand { get; set; }
        private SelectClauseParser<TRootVertex> Select { get; set; }
        private OrderByClauseParser<TRootVertex> OrderBy { get; set; }

        private IEnumerable<string> ClassProperties { get; }
        private GraphSchema _Graph { get; }

        private IList<QueueItem> _GetTokens(string parameter)
        {
            var hash = ClassProperties.HashOfList().AndHash(parameter);
            var tokens = tokenizerCache.GetOrAdd(hash, () =>
            {
                return new Tokenizer(ClassProperties).Tokenize(parameter).ToList();
            });
            return tokens.Select(t => new QueueItem(new ParserVertex(t))).ToList();
        }

        public QueryParser(Parameters parameters, GraphSchema graph, IClassCache cache)
        {
            IsParsed = false;

            ClassProperties = graph.Vertices
                .Select(v => v.Value.Properties)
                .SelectMany(x => x).Select(x => x.Name)
                .Distinct();

            OrderBy = new OrderByClauseParser<TRootVertex>(x => _GetTokens(x), parameters.OrderBy, graph, TermHelper.OrderByTerms);
            Select = new SelectClauseParser<TRootVertex>(x => _GetTokens(x), parameters.Select, graph, TermHelper.SelectTerms);
            Filter = new FilterClauseParser<TRootVertex>(x => _GetTokens(x), parameters.Filter, TermHelper.FilterTerms);
            Expand = new ExpandClauseParser<TRootVertex>(x => _GetTokens(x), parameters.Expand, graph, Select, Filter, TermHelper.ExpandTerms);

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
                    var expandPaths = Expand.Result.Where(p => p.Edges.Any()).ToList();
                    if (!expandPaths.Any())
                    {
                        _selectionTree = new Tree(rootQueryVertex);
                    }
                    else
                        _selectionTree = new Tree(Expand.Result.Where(p => p.Edges.Any()));
                }
                return _selectionTree;
            }
        }

        private IEnumerable<PathToProperty> _selectPaths { get; set; }
        public IEnumerable<PathToProperty> SelectPaths
        {
            get
            {
                _AssertParsed();
                if (_selectPaths is null)
                {
                    _selectPaths = Select.Result.ToList(); ;
                }
                return _selectPaths;
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
            return new FilterTreeExpressionBuilder().FilterExpression(Filter.Result, parameter, Cache);
        }
    }

    public static class TermHelper
    {
        private static TGrouping[] _parseGrouping(Func<ITuple<string, Func<IList<QueueItem>, ITuple<QueueItem, int>>>> termFunc) => _parseGrouping(termFunc());
        private static TGrouping[] _parseGrouping(IEnumerable<ITuple<string, Func<IList<QueueItem>, ITuple<QueueItem, int>>>> terms)
        {
            var result = new List<TGrouping>();
            foreach (var term in terms)
                result.AddRange(_parseGrouping(term));
            return result.ToArray();
        }
        private static TGrouping[] _parseGrouping(ITuple<string, Func<IList<QueueItem>, ITuple<QueueItem, int>>> term)
        {
            var pattern = term.Item1;
            var func = term.Item2;
            var regex = new Regex(pattern, RegexOptions.Compiled | RegexOptions.Singleline);
            return new[] { ITuple.Create(regex, func) };
        }

        public static readonly TTermDict OrderByTerms = new List<TGrouping[]>{
                _parseGrouping(ExpandGroupings.ExpandProperty),
                _parseGrouping(OrderGroupings.SortOrderProperty),
                _parseGrouping(OrderGroupings.ListOfSorting),
            }.SelectMany(x => x).ToDictionary(x => x.Item1, x => x.Item2);

        public static readonly TTermDict SelectTerms = new List<TGrouping[]>{
                _parseGrouping(ExpandGroupings.ExpandProperty),
                _parseGrouping(ExpandGroupings.ListOfExpand),
            }.SelectMany(x => x).ToDictionary(x => x.Item1, x => x.Item2);

        public static readonly TTermDict FilterTerms = new List<TGrouping[]>
            {
                _parseGrouping(ExpandGroupings.ExpandProperty),
                _parseGrouping(FilterGroupings.AddTermsForFilter()),
            }.SelectMany(x => x).ToDictionary(x => x.Item1, x => x.Item2);

        public static readonly TTermDict ExpandTerms = new List<TGrouping[]>
            {
                _parseGrouping(ExpandGroupings.ExpandProperty),
                _parseGrouping(FilterGroupings.AddTermsForFilter()),
                _parseGrouping(ExpandGroupings.ExpandPropertyWithListOfClauses),
                _parseGrouping(ExpandGroupings.ListOfExpand),
                _parseGrouping(ExpandGroupings.ExpandExpression),
                _parseGrouping(ExpandGroupings.FilterExpression),
                _parseGrouping(ExpandGroupings.SelectExpression),
                _parseGrouping(ExpandGroupings.ListOfClauseExpressions()),
            }.SelectMany(x => x).ToDictionary(x => x.Item1, x => x.Item2);
    }
}
