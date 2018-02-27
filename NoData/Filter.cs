using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System;
using NoData.Internal.TreeParser.BinaryTreeParser;
using Newtonsoft.Json;

namespace NoData
{
    public class Filter<TDto> where TDto : class, new()
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

        internal Filter<TDto> ApplyTop()
        {
            if(top.HasValue)
                query = query.Take(top.Value);
            return this;
        }

        internal Filter<TDto> ApplySkip()
        {
            if(skip.HasValue)
                query = query.Skip(skip.Value);
            return this;
        }

        internal Filter<TDto> ApplyFilter()
        {
            if(!string.IsNullOrEmpty(filter))
                query = new FilterTree<TDto>(filter).ApplyFilter(query);
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