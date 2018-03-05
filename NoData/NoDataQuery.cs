using System.Collections.Generic;
using System.Linq;
using System;
using Newtonsoft.Json;
using NoData.Internal.TreeParser.FilterExpressionParser;

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

        internal IQueryable<TDto> query;

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

        private List<string> GetMatchingProperties()
        {
            var selected = select.Split(',').ToList();

            var result = selected.Intersect(typeof(TDto).GetProperties().Select(x => x.Name)).ToList();
            foreach(var line in result)
                Console.WriteLine(line);
            return result.ToList();
        }

        public IQueryable<TDto> ApplyTo(IQueryable<TDto> query)
        {
            this.query = query;

            //GetMatchingProperties();
            return ApplyFilter().ApplySkip().ApplyTop().query;
        }
    }
}