using System.Linq;
using NoData.QueryParser;
using System.Linq.Expressions;
using System.Collections.Generic;
using NoData.GraphImplementations.Schema;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using System;
using System.Text;

namespace NoData
{
    public class NoDataBuilder<TDto> : INoData<TDto>
        where TDto : class, new()
    {
        internal Parameters Parameters { get; }
        public SettingsForType<TDto> Settings { get; }
        internal GraphSchema Graph { get; }

        public NoDataBuilder(Parameters parameters, SettingsForType<TDto> settings)
        {
            Parameters = parameters;
            Settings = settings;
            Graph = GraphSchema.Cache<TDto>.Graph;
        }

        public NoDataBuilder(
            string expand,
            string filter = null,
            string select = null,
            string orderBy = null,
            int? top = null,
            int? skip = null,
            bool count = false) : this(new Parameters(expand, filter, select, orderBy, top, skip, count), new SettingsForType<TDto>())
        { }

        private QueryParser<TDto> ParseQuery()
        {
            var parser = new QueryParser<TDto>(Parameters, Graph, GraphSchema.CacheInstance);

            return parser;
        }

        public virtual INoDataQuery<TDto> Load(IQueryable<TDto> sourceQueryable)
        {
            var parser = ParseQuery();
            return buildQuery(sourceQueryable, parser);
        }

        private static readonly ParameterExpression ParameterDtoExpression = Expression.Parameter(typeof(TDto), "Dto");
        private INoDataQuery<TDto> buildQuery(IQueryable<TDto> sourceQueryable, QueryParser<TDto> parser)
        {
            var filterExpr = parser.ApplyFilterExpression(ParameterDtoExpression);
            var selectionTree = Utility.SchemaToQueryable.TranslateTree2(parser.SelectionTree, parser.SelectPaths);

            // security: check max expand
            if (Settings.MaxExpandDepth >= 0)
            {
                var paths = selectionTree.EnumerateAllPaths();
                if (paths.Any(x => x.Count() > Settings.MaxExpandDepth))
                    throw new ArgumentException("Cannot expand past the max expand depth of: " + Settings.MaxExpandDepth);
            }

            var selectExpandExpression = Utility.ExpressionBuilder.GetExpandExpression(ParameterDtoExpression, selectionTree, GraphSchema.CacheInstance);
            return new NoDataQuery<TDto>(sourceQueryable, Parameters, GraphSchema.CacheInstance, parser.OrderByPath, selectExpandExpression, filterExpr, ParameterDtoExpression, selectionTree);
        }

        public INoDataQuery<TDto> Projection<TEntity>(IQueryable<TEntity> sourceQueryable, IConfigurationProvider mapperConfiguration)
        {
            var parser = ParseQuery();
            var selectionTree = parser.SelectionTree;

            var pathEnumerable = selectionTree.EnumerateAllPaths();
            var stringPathEnumerable = pathEnumerable.Select(p => p.Select(x => x.Value.Name).ToList()).ToList();
            void ApplyFunc(Action<string> func)
            {
                // apply includes
                foreach (var path in stringPathEnumerable.Where(x => x.Any()))
                {
                    var strBuilder = new StringBuilder(path.First());
                    if (func != null)
                        func(strBuilder.ToString());
                    foreach (var inc in path.Skip(1))
                    {
                        strBuilder.Append("." + inc);
                        if (func != null)
                            func(strBuilder.ToString());
                    }
                }
            }

            var projectList = new List<string>(); // TODO: reserve size of stringPathEnumerable for performance
            ApplyFunc(x => projectList.Add(x));

            // automapper projection
            var projected = sourceQueryable.ProjectTo<TDto>(mapperConfiguration, null, projectList.ToArray());

            return buildQuery(projected, parser);
        }
    }

}
