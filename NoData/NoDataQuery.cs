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

namespace NoData
{
    public class NoDataQuery<TDto> : QueryParameters where TDto : class, new()
    {
        private static readonly GraphSchema baseGraph = GraphSchema.CreateFromGeneric<TDto>();
        protected GraphSchema graph;
        protected IQueryable<TDto> query = null;
        protected QueryParser<TDto> QueryParser;

        private ParameterExpression ParameterDtoExpression = Expression.Parameter(typeof(TDto), "Dto");
        private Expression FilterExpression = null;
        public FilterSecurityTypes FilterSecurity = FilterSecurityTypes.AllowOnlyVisibleValues;

        public NoDataQuery(IHttpContextAccessor context) : base(context)
        {
            graph = baseGraph.Clone() as GraphSchema;
            QueryParser = new QueryParser<TDto>(this as QueryParameters, graph);
        }

        public NoDataQuery(
            string expand = null,
            string filter = null,
            string select = null,
            string orderBy = null,
            int? top = null,
            int? skip = null,
            bool count = false)
            : base(expand, filter, select, orderBy, top, skip, count)
        {
            graph = baseGraph.Clone() as GraphSchema;
            QueryParser = new QueryParser<TDto>(this as QueryParameters, graph);
        }

        public NoDataQuery<TDto> Load(IEnumerable<TDto> enumerable) => Load(enumerable.AsQueryable());
        public NoDataQuery<TDto> Load(IQueryable<TDto> query)
        {
            if (this.query != null)
                throw new Exception("Loaded a query twice.");
            this.query = query;
            return this;
        }


        private bool parsed = false;
        protected void Parse()
        {
            if (!parsed)
            {
                QueryParser.Parse();
                FilterExpression = QueryParser.ApplyFilterExpression(ParameterDtoExpression);
                parsed = true;
            }
        }

        /// <summary>
        /// Applies the top term.
        /// </summary>
        private NoDataQuery<TDto> ApplyTop()
        {
            if (Top.HasValue)
                query = query.Take(Top.Value);
            return this;
        }

        /// <summary>
        /// Applies the skip term
        /// </summary>
        private NoDataQuery<TDto> ApplySkip()
        {
            if (Skip.HasValue)
                query = query.Skip(Skip.Value);
            return this;
        }

        /// <summary>
        /// Applies the filter query string the queryable.
        /// </summary>
        private NoDataQuery<TDto> ApplyFilter()
        {
            if (FilterExpression != null)
                query = QueryParser.SelectionTree.ApplyFilter(query, ParameterDtoExpression, FilterExpression);
            return this;
        }

        /// <summary>
        /// Applies the expansion property to the queryable.
        /// </summary>
        private NoDataQuery<TDto> ApplyExpand()
        {
            query = QueryParser.SelectionTree.ApplyExpand(query, ParameterDtoExpression);
            return this;
        }

        /// <summary>
        /// Selects a subset of properties
        /// </summary>
        /// <remarks>May require that Apply Expand is called first.</remarks>
        private NoDataQuery<TDto> ApplySelect()
        {
            if (!string.IsNullOrEmpty(Select))
                query = QueryParser.SelectionTree.ApplySelect(query, ParameterDtoExpression);
            return this;
        }

        /// <summary>
        /// Selects a subset of properties
        /// </summary>
        /// <remarks>May require that Apply Expand is called first.</remarks>
        private NoDataQuery<TDto> ApplyOrderBy()
        {
            if (!string.IsNullOrEmpty(OrderBy))
                query = OrderByClauseParser<TDto>.OrderBy(query, QueryParser.OrderByPath, ParameterDtoExpression);
            return this;
        }


        /// <summary>
        /// Parses and then applies the filtering to the queryable, without enumerating it.
        /// </summary>
        public NoDataQuery<TDto> BuildExpression()
        {
            Parse();
            switch (FilterSecurity)
            {
                default:
                case FilterSecurityTypes.AllowOnlyVisibleValues:
                    ApplyExpand().ApplySelect().ApplyFilter().ApplyOrderBy().ApplySkip().ApplyTop();
                    break;
                case FilterSecurityTypes.AllowFilteringOnPropertiesThatAreNotDisplayed:
                    ApplyExpand().ApplyFilter().ApplySelect().ApplyOrderBy().ApplySkip().ApplyTop();
                    break;
                case FilterSecurityTypes.AllowFilteringOnNonExplicitlyExpandedExpansions:
                    ApplyFilter().ApplyExpand().ApplySelect().ApplyOrderBy().ApplySkip().ApplyTop();
                    break;
            }
            return this;
        }

        /// <summary>
        /// Applies the odata options to the queryable without enumerating it.
        /// </summary>
        public IQueryable<TDto> Apply() => BuildExpression().query;
    }

}