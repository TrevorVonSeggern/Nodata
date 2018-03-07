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

        internal IQueryable<TDto> query;
        private ExpandTree<TDto> expandTree = new ExpandTree<TDto>();


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
                expandTree = new ExpandTree<TDto>();
                expandTree.ParseExpand(expand);
                query = expandTree.ApplyExpand(query);
            }
            return this;
        }

        public NoDataQuery<TDto> BuildQueryable(IQueryable<TDto> query)
        {
            this.query = query;
            this.query = ApplyExpand().ApplyFilter().ApplySkip().ApplyTop().query;
            return this;
        }

        public string JsonResult(IQueryable<TDto> query)
        {
            ApplyQueryable(query);
            return JsonConvert.SerializeObject(
                this.query,
                Formatting.Indented, 
                new JsonSerializerSettings { ContractResolver = new DynamicContractResolver(expandTree.IgnoredProperties().ToArray()) });
        }

        public IQueryable<TDto> ApplyQueryable(IQueryable<TDto> query) => BuildQueryable(query).query;
    }

    public class DynamicContractResolver : DefaultContractResolver
    {
        private readonly string[] props;

        public DynamicContractResolver(params string[] prop)
        {
            props = prop;
        }
        // TODO: Needs to serialize multiple types. Not just the root.
        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            IList<JsonProperty> retval = base.CreateProperties(type, memberSerialization);
            
            retval = retval.Where(p => !props.Contains(p.PropertyName)).ToList();

            return retval;
        }
    }

}