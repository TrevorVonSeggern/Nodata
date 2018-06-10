using System.Linq;
using Newtonsoft.Json;
using NoData.QueryParser;
using System.Linq.Expressions;
using System.Collections.Generic;
using NoData.QueryParser.ParsingTools;

namespace NoData
{
    public class NoDataQuery<TDto> : QueryParameters where TDto : class, new()
    {
        [JsonIgnore]
        private static readonly Graph.Graph baseGraph = Graph.Graph.CreateFromGeneric<TDto>();
        [JsonIgnore]
        private Graph.Graph graph;
        [JsonIgnore]
        internal IQueryable<TDto> query;
        [JsonIgnore]
        QueryParser<TDto> QueryParser;

        [JsonIgnore]
        private ParameterExpression ParameterDtoExpression = Expression.Parameter(typeof(TDto), "Dto");
        [JsonIgnore]
        private Expression FilterExpression = null;

        public NoDataQuery()
        {
            graph = baseGraph.Clone() as Graph.Graph;
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
            graph = baseGraph.Clone() as Graph.Graph;
            QueryParser = new QueryParser<TDto>(this as QueryParameters, graph);
        }

        public FilterSecurityTypes FilterSecurity = FilterSecurityTypes.AllowOnlyVisibleValues;

        private bool parsed = false;
        private void ValidateParsed()
        {
            if (!parsed)
            {
                QueryParser.Parse();
                FilterExpression = QueryParser.ApplyFilterExpression(ParameterDtoExpression);
            }
            parsed = true;
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
        /// Parses and then applies the filtering to the queryable without enumerating it.
        /// </summary>
        public NoDataQuery<TDto> BuildQueryable(IQueryable<TDto> query)
        {
            this.query = query;
            ValidateParsed();
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
        /// Returns the json result of the queryable with all query commands parsed and applied.
        /// </summary>
        public string JsonResult(IQueryable<TDto> query)
        {
            ApplyQueryable(query);
            var list = this.query.ToList();
            QueryParser.SelectionTree.AddInstances(list);
            var sGraph = Graph.Utility.TreeUtility.Flatten(QueryParser.SelectionTree);
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

        /// <summary>
        /// Applies the filtering to the queryable without enumerating it.
        /// </summary>
        public IQueryable<TDto> ApplyQueryable(IQueryable<TDto> query) => BuildQueryable(query).query;

        /// <summary>
        /// Applies the filtering to the queryable without enumerating it.
        /// </summary>
        public IQueryable<TDto> ApplyQueryable(IEnumerable<TDto> query) => BuildQueryable(query.AsQueryable()).query;
    }

}