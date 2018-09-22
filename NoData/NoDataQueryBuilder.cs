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

namespace NoData
{
    public class NoDataQueryBuilder<TDto> : NoDataQuery<TDto> where TDto : class, new()
    {

        // TODO: These are settings specific to the type TDto, and should also be configurable via global settings.
        public bool EagerLoadEFCoreQuery = true;
        public bool AutomapperExplicitExansion = true;

        public NoDataQueryBuilder(IHttpContextAccessor accessor) : base(accessor)
        {
        }

        public NoDataQueryBuilder(
            string expand,
            string filter = null,
            string select = null,
            string orderBy = null,
            int? top = null,
            int? skip = null,
            bool count = false)
            : base(expand, filter, select, orderBy, top, skip, count)
        {
        }

        public IQueryable<TDto> Projection<TEntity>(IEnumerable<TEntity> enumerable, IConfigurationProvider config)
            where TEntity : class
            => Projection(enumerable.AsQueryable(), config);

        public IQueryable<TDto> Projection<TEntity>(IQueryable<TEntity> query, IConfigurationProvider config)
            where TEntity : class
        {
            Parse(); // Called out explicitly to get the select tree before BuildExpression.

            var pathEnumerable = QueryParser.SelectionTree.EnumerateAllPaths();
            var stringPathEnumerable = pathEnumerable.Select(p => p.Select(x => x.Value.PropertyName).ToList()).ToList();
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

            // ApplyFunc(x => query = query.Include(x));
            var projectList = new List<string>();
            ApplyFunc(x => projectList.Add(x));

            var projected = query.ProjectTo<TDto>(config, null, projectList.ToArray());

            Load(projected);

            BuildExpression();
            return Apply();
        }

        public IQueryable<TDto> ApplyQueryable(IEnumerable<TDto> enumerable) => ApplyQueryable(enumerable.AsQueryable());
        public IQueryable<TDto> ApplyQueryable(IQueryable<TDto> query)
        {
            Load(query);
            BuildExpression();
            return Apply();
        }


        /// <summary>
        /// Returns the json result of the queryable with all query commands parsed and applied.
        /// </summary>
        public string JsonResult(IQueryable<TDto> query)
        {
            Load(query);
            BuildExpression();
            return JsonResult();
        }
        /// <summary>
        /// Returns the json result of the queryable with all query commands parsed and applied.
        /// </summary>
        public string JsonResult()
        {
            var q = Apply();
            var list = q.ToList();
            QueryParser.SelectionTree.AddInstances(list);
            var sGraph = QueryParser.SelectionTree.Flatten();
            return JsonConvert.SerializeObject(
                list,
                Formatting.Indented,
                new JsonSerializerSettings
                {
                    PreserveReferencesHandling = PreserveReferencesHandling.None,
                    ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
                    MaxDepth = 5,

                    ContractResolver = new DynamicContractResolver(sGraph)
                });
        }

    }

}