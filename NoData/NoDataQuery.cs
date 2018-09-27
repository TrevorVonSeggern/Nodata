using System.Linq;
using Newtonsoft.Json;
using NoData.QueryParser;
using System.Linq.Expressions;
using System.Collections.Generic;
using NoData.QueryParser.ParsingTools;
using NoData.GraphImplementations.Schema;
using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using NoData.Utility;
using Graph;
using CodeTools;

namespace NoData
{
    [Immutable]
    public class NoDataQuery<TDto> : INoDataQuery<TDto>
        where TDto : class, new()
    {
        public FilterSecurityTypes FilterSecurity { get; } = FilterSecurityTypes.AllowOnlyVisibleValues;

        // Properties that represent the object model. Ie setup operations
        public Parameters Parameters { get; }
        protected IQueryable<TDto> Source { get; }


        // Properties that are a result of parsing.
        protected IEnumerable<ITuple<PathToProperty, SortDirection>> OrderByPath { get; }
        private ParameterExpression DtoExpression { get; }
        private Expression FilterExpression { get; }



        private IClassCache Cache { get; }

        public NoDataQuery(
            IQueryable<TDto> source,
            Parameters parameters,
            IClassCache cache,
            IEnumerable<ITuple<PathToProperty, SortDirection>> orderBy,
            Expression selectExpandExpression,
            Expression filterExpression,
            ParameterExpression dtoParameterExpression)
        {
            Source = source;
            Parameters = parameters;
            // /////////////


            // Note to self: Need to build a way to parse the QueryTree into an expression.


            // /////////////////

            DtoExpression = dtoParameterExpression;
            FilterExpression = filterExpression;
            OrderByPath = orderBy;

            Cache = cache;
        }


        /// <summary>
        /// Applies the top term.
        /// </summary>
        private IQueryable<TDto> ApplyTop(IQueryable<TDto> query)
        {
            if (Parameters.Top.HasValue)
                query = query.Take(Parameters.Top.Value);
            return query;
        }

        /// <summary>
        /// Applies the skip term
        /// </summary>
        private IQueryable<TDto> ApplySkip(IQueryable<TDto> query)
        {
            if (Parameters.Skip.HasValue)
                query = query.Skip(Parameters.Skip.Value);
            return query;
        }

        /// <summary>
        /// Applies the filter query string the queryable.
        /// </summary>
        private IQueryable<TDto> ApplyFilter(IQueryable<TDto> query)
        {
            if (FilterExpression != null || !string.IsNullOrWhiteSpace(Parameters.Filter))
                query = Utility.ExpressionBuilder.ApplyFilter(SelectionTree, query, DtoExpression, FilterExpression);
            return query;
        }

        /// <summary>
        /// Applies the expansion property to the queryable.
        /// </summary>
        private IQueryable<TDto> ApplyExpand(IQueryable<TDto> query)
        {
            query = Utility.ExpressionBuilder.ApplyExpand(SelectionTree, query, DtoExpression, Cache);
            return query;
        }

        /// <summary>
        /// Selects a subset of properties
        /// </summary>
        /// <remarks>May require that Apply Expand is called first.</remarks>
        private IQueryable<TDto> ApplySelect(IQueryable<TDto> query)
        {
            // if (!string.IsNullOrEmpty(Parameters.Select))
            //     query = Utility.ExpressionBuilder.ApplySelect(SelectionTree, query, DtoExpression, Cache);
            return query;
        }

        /// <summary>
        /// Selects a subset of properties
        /// </summary>
        /// <remarks>May require that Apply Expand is called first.</remarks>
        private IQueryable<TDto> ApplyOrderBy(IQueryable<TDto> query)
        {
            if (!string.IsNullOrEmpty(Parameters.OrderBy))
                query = OrderByClauseParser<TDto>.OrderBy(query, OrderByPath, DtoExpression);
            return query;
        }

        /// <summary>
        /// Parses and then applies the filtering to the queryable, without enumerating it.
        /// </summary>
        private IQueryable<TDto> Apply()
        {
            var query = this.Source;
            switch (FilterSecurity)
            {
                default:
                case FilterSecurityTypes.AllowOnlyVisibleValues:
                    query = ApplyExpand(query);
                    query = ApplySelect(query);
                    query = ApplyFilter(query);
                    break;
                case FilterSecurityTypes.AllowFilteringOnPropertiesThatAreNotDisplayed:
                    query = ApplyExpand(query);
                    query = ApplyFilter(query);
                    query = ApplySelect(query);
                    break;
                case FilterSecurityTypes.AllowFilteringOnNonExplicitlyExpandedItems:
                    query = ApplyFilter(query);
                    query = ApplyExpand(query);
                    query = ApplySelect(query);
                    break;
            }
            query = ApplyOrderBy(query);
            query = ApplySkip(query);
            return ApplyTop(query);
        }

        public IQueryable<TDto> BuildQueryable() => Apply();

        /// <summary>
        /// Returns the json result of the queryable with all query commands parsed and applied.
        /// </summary>
        public string AsJson()
        {
            var q = Apply();
            var list = q.ToList();
            return "bad input";
            // Query.QueryParser.SelectionTree.AddInstances(list);
            // var sGraph = Query.QueryParser.SelectionTree.Flatten();
            // return JsonConvert.SerializeObject(
            //     list,
            //     Formatting.Indented,
            //     new JsonSerializerSettings
            //     {
            //         PreserveReferencesHandling = PreserveReferencesHandling.None,
            //         ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
            //         MaxDepth = 5,

            //         ContractResolver = new DynamicContractResolver(sGraph)
            //     });
        }

    }

}