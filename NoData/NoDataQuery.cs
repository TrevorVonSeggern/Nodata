using System.Collections.Generic;
using System.Linq;
using System;
using Newtonsoft.Json;
using NoData.Internal.TreeParser.FilterExpressionParser;
using NoData.Internal.TreeParser.ExpandExpressionParser;
using Newtonsoft.Json.Serialization;

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

        [JsonIgnore]
        private static readonly Graph.Graph baseGraph = Graph.Graph.CreateFromGeneric<TDto>();
        [JsonIgnore]
        private Graph.Graph graph;
        [JsonIgnore]
        private Graph.Tree selectionTree;

        public NoDataQuery()
        {
            graph = baseGraph.Clone() as Graph.Graph;
        }

        internal IQueryable<TDto> query;
        private ExpandParser<TDto> expandParser = new ExpandParser<TDto>();

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
            if (!string.IsNullOrEmpty(expand))
            {
                expandParser = new ExpandParser<TDto>();
                selectionTree = expandParser.ParseExpand(expand, graph);
                graph = selectionTree.Flatten() as Graph.Graph;
                query = expandParser.ApplyExpand(query);
            }
            else
                selectionTree = new Graph.Tree(graph.VertexContainingType(typeof(TDto)));
            return this;
        }

        /// <summary>
        /// Selects a subset of properties
        /// </summary>
        /// <returns></returns>
        /// <remarks>Requires that Apply Expand is called first.</remarks>
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
            var list = this.query.ToList();
            selectionTree.AddInstances(list);
            var sGraph = selectionTree.Flatten() as Graph.Graph;
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

    public class DynamicContractResolver : DefaultContractResolver
    {
        public readonly Graph.Graph Graph;

        public DynamicContractResolver(Graph.Graph graph)
        {
            Graph = graph;
        }

        // TODO: Needs to serialize multiple types. Not just the root.
        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            IList<JsonProperty> retval = base.CreateProperties(type, memberSerialization);
            
            retval = retval.ToList(); //.Where(p => !props.Contains(p.PropertyName))

            var result = new List<JsonProperty>();
            foreach(var property in retval)
            {
                //if (vertex.Value.PropertyNames.Contains(property.PropertyName))
                //    continue;
                property.ShouldSerialize = instance =>
                {
                    if (property.Ignored)
                        return false;
                    
                    return Graph.ShouldSerializeProperty(instance, property.PropertyName, property.PropertyType);
                };
                result.Add(property);
            }

            return result;
        }
    }

}