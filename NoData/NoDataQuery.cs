using System.Collections.Generic;
using System.Linq;
using System;
using Newtonsoft.Json;
using NoData.Internal.TreeParser.FilterExpressionParser;
using NoData.Internal.TreeParser.ExpandExpressionParser;
using Newtonsoft.Json.Serialization;
using NoData.Serialization;

namespace NoData
{
    public class NoDataQuery<TDto> where TDto : class, new()
    {
        [JsonProperty("$count")]
        public bool count { get; set; }
        [JsonProperty("$top")]
        public int? top { get; set; }
        [JsonProperty("$skip")]
        public int? skip { get; set; }
        [JsonProperty("$filter")]
        public string filter { get; set; }
        [JsonProperty("$select")]
        public string select { get; set; }
        [JsonProperty("$expand")]
        public string expand { get; set; }

        private Graph.Graph graph = Graph.Graph.CreateFromGeneric<TDto>();

        internal IQueryable<TDto> query;
        private ExpandTree<TDto> expandTree = new ExpandTree<TDto>();

        public enum FilterSecurityTypes
        {
            AllowOnlyVisibleValues,
            AllowFilteringOnPropertiesThatAreNotDisplayed,
            AllowFilteringOnNonExplicitlyExpandedExpansions,
        }

        public FilterSecurityTypes FilterSecurity = FilterSecurityTypes.AllowOnlyVisibleValues;

        internal NoDataQuery<TDto> ApplyTop()
        {
            if(top.HasValue)
                query = query.Take(top.Value);
            return this;
        }

        internal NoDataQuery<TDto> ApplySkip()
        {
            if(skip.HasValue)
                query = query.Skip(skip.Value);
            return this;
        }

        internal NoDataQuery<TDto> ApplyFilter()
        {
            if(!string.IsNullOrEmpty(filter))
            {
                var tree = new FilterTree<TDto>();
                tree.ParseTree(filter);
                query = tree.ApplyFilter(query);
            }
            return this;
        }

        internal NoDataQuery<TDto> ApplyExpand()
        {
            if(!string.IsNullOrEmpty(expand))
            {
                // TODO filter the graph.
                expandTree = new ExpandTree<TDto>();
                expandTree.ParseExpand(expand);
                query = expandTree.ApplyExpand(query);
            }
            return this;
        }

        internal NoDataQuery<TDto> ApplySelect()
        {
            if(!string.IsNullOrEmpty(select))
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
            var sGraph = new SerializeGraphTraversal().GetSerializationSettings(this.query, graph);
            return JsonConvert.SerializeObject(
                this.query,
                Formatting.Indented, 
                new JsonSerializerSettings {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = new DynamicContractResolver(sGraph)
                });
        }

        public IQueryable<TDto> ApplyQueryable(IQueryable<TDto> query) => BuildQueryable(query).query;
    }

    public class DynamicContractResolver : DefaultContractResolver
    {
        private readonly SerializeGraph Graph;

        public DynamicContractResolver(SerializeGraph graph)
        {
            Graph = graph;
        }
        // TODO: Needs to serialize multiple types. Not just the root.
        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            IList<JsonProperty> retval = base.CreateProperties(type, memberSerialization);
            
            retval = retval.ToList(); //.Where(p => !props.Contains(p.PropertyName))

            var vertex = Graph.Vertices.Single(x => x.Value.Type == type);
            var result = new List<JsonProperty>();
            foreach(var property in retval)
            {
                //if (vertex.Value.PropertyNames.Contains(property.PropertyName))
                //    continue;
                property.ShouldSerialize = instance =>
                {
                    if (property.Ignored)
                        return false;
                    
                    return Graph.ShouldSerializeProperty(property.DeclaringType, property.PropertyName, property.PropertyType);
                };
                result.Add(property);
            }

            return result;
        }
    }

}