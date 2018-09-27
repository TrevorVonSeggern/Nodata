using System.Linq;
using Newtonsoft.Json;
using NoData.QueryParser;
using System.Linq.Expressions;
using System.Collections.Generic;
using NoData.QueryParser.ParsingTools;
using NoData.GraphImplementations.Schema;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using NoData.Utility;

namespace NoData
{
    // internal static class SharedClassCache
    // {
    //     public static readonly IClassCache Cache = new ClassCache();
    // }

    public class NoDataBuilder<TDto> : INoData<TDto>
        where TDto : class, new()
    {
        protected Parameters Parameters { get; }
        protected GraphSchema Graph { get; }

        public NoDataBuilder(Parameters parameters)
        {
            Parameters = parameters;
            Graph = GraphSchema.Cache<TDto>.Graph;
        }

        public NoDataBuilder(IHttpContextAccessor accessor) : this(ParametersHelper.FromHttpContext(accessor))
        { }

        public NoDataBuilder(
            string expand,
            string filter = null,
            string select = null,
            string orderBy = null,
            int? top = null,
            int? skip = null,
            bool count = false) : this(new Parameters(expand, filter, select, orderBy, top, skip, count))
        { }

        private QueryParser<TDto> ParseQuery()
        {
            var parser = new QueryParser<TDto>(Parameters, Graph, GraphSchema._Cache);

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
            Expression filterExpr = parser.ApplyFilterExpression(ParameterDtoExpression);

            return new NoDataQuery<TDto>(sourceQueryable, Parameters, Graph, GraphSchema._Cache, parser.SelectionTree, parser.OrderByPath, filterExpr, ParameterDtoExpression);
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
                    string currentInclude = path.First();
                    func(currentInclude);
                    foreach (var inc in path.Skip(1))
                    {
                        currentInclude += "." + inc;
                        func(currentInclude);
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