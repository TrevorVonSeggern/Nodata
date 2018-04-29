using System.Collections.Generic;
using System.Linq;
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NoData.QueryParser;
using System.Linq.Expressions;

namespace NoData
{
    public class NoDataQuery<TDto> : QueryParameters where TDto : class, new()
    {
        [JsonIgnore]
        private static readonly Graph.Graph baseGraph = Graph.Graph.CreateFromGeneric<TDto>();
        [JsonIgnore]
        private Graph.Graph graph;
        [JsonIgnore]
        private Graph.Tree selectionTree;
        [JsonIgnore]
        internal IQueryable<TDto> query;
        [JsonIgnore]
        QueryParser.QueryParser QueryParser;
        
        [JsonIgnore]
        private ParameterExpression ParameterDtoExpression = Expression.Parameter(typeof(TDto), "Dto");
        [JsonIgnore]
        private Expression FilterExpression = Expression.Empty();



        public NoDataQuery(
            string expand = null, 
            string filter = null, 
            string select = null, 
            int? top = null, 
            int? skip = null, 
            bool count = false)
            : base(expand, filter, select, top, skip, count)
        {
            graph = baseGraph.Clone() as Graph.Graph;
            QueryParser = new QueryParser.QueryParser(this as QueryParameters, graph);
        }

        public FilterSecurityTypes FilterSecurity = FilterSecurityTypes.AllowOnlyVisibleValues;

        private bool parsed = false;
        private void ValidateParsed()
        {
            if (!parsed)
            {
                QueryParser.Parse(typeof(TDto));
                var type = typeof(TDto);
                selectionTree = QueryParser.SelectionTree(type);
                QueryParser.ApplyFilterExpression(type, ParameterDtoExpression);
            }
            parsed = true;
        }

        private NoDataQuery<TDto> ApplyTop()
        {
            if (Top.HasValue)
                query = query.Take(Top.Value);
            return this;
        }

        private NoDataQuery<TDto> ApplySkip()
        {
            if (Skip.HasValue)
                query = query.Skip(Skip.Value);
            return this;
        }

        private NoDataQuery<TDto> ApplyFilter()
        {
            ValidateParsed();
            if(!string.IsNullOrEmpty(Filter))
            {
                query = selectionTree.ApplyFilter(query, ParameterDtoExpression);

                //var tree = new FilterTree<TDto>();
                //tree.ParseTree(Filter);
                //query = tree.ApplyFilter(query);
            }
            return this;
        }

        private NoDataQuery<TDto> ApplyExpand()
        {
            ValidateParsed();
            if (string.IsNullOrEmpty(Expand) || selectionTree is null)
                selectionTree = new Graph.Tree(graph.VertexContainingType(typeof(TDto)));
            query = selectionTree.ApplyExpand(query);
            return this;
        }

        /// <summary>
        /// Selects a subset of properties
        /// </summary>
        /// <returns></returns>
        /// <remarks>Requires that Apply Expand is called first.</remarks>
        private NoDataQuery<TDto> ApplySelect()
        {
            if(!string.IsNullOrEmpty(Select))
            {
                // TODO: Select
            }
            return this;
        }

        public NoDataQuery<TDto> BuildQueryable(IQueryable<TDto> query)
        {
            this.query = query;
            switch(FilterSecurity)
            {
                default:
                case FilterSecurityTypes.AllowOnlyVisibleValues:
                    ApplyExpand().ApplySelect().ApplyFilter().ApplySkip().ApplyTop();
                    break;
                case FilterSecurityTypes.AllowFilteringOnPropertiesThatAreNotDisplayed:
                    ApplyExpand().ApplyFilter().ApplySelect().ApplySkip().ApplyTop();
                    break;
                case FilterSecurityTypes.AllowFilteringOnNonExplicitlyExpandedExpansions:
                    ApplyFilter().ApplyExpand().ApplySelect().ApplySkip().ApplyTop();
                    break;
            }
            return this;
        }

        public string JsonResult(IQueryable<TDto> query)
        {
            ApplyQueryable(query);
            var list = this.query.ToList();
            selectionTree.AddInstances(list);
            var sGraph = Graph.Utility.TreeUtility.Flatten(selectionTree);
            return JsonConvert.SerializeObject(
                list,
                Formatting.Indented, 
                new JsonSerializerSettings {
                    PreserveReferencesHandling = PreserveReferencesHandling.None,
                    ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
                    MaxDepth = 5,
                    
                    ContractResolver = new DynamicContractResolver(sGraph)
                });
        }

        public IQueryable<TDto> ApplyQueryable(IQueryable<TDto> query) => BuildQueryable(query).query;
    }

}