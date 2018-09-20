using CodeTools;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NoData.QueryParser
{
    internal static class Helper
    {
        public static int? GetInt(this IQueryCollection query, string key)
        {
            var value = query.GetStr(key);
            if (value == null)
                return null;
            if (int.TryParse(value, out var result))
                return result;
            return null;
        }
        public static string GetStr(this IQueryCollection query, string key)
        {
            return query.FirstOrDefault(x => x.Key == key).Value.FirstOrDefault();
        }
        public static bool GetBool(this IQueryCollection query, string key)
        {
            var value = query.GetStr(key);
            if (value == null)
                return false;
            if (bool.TryParse(value, out var result))
                return result;
            return false;
        }
    }

    // [Immutable] Should be immutable, but inherited classes are not. Unit test throws error becaues of this setup.
    public class QueryParameters
    {

        public QueryParameters(IHttpContextAccessor accessor) : this(accessor.HttpContext.Request.Query) { }
        public QueryParameters(IQueryCollection queryCollection) : this(
            queryCollection.GetStr("$expand"),
            queryCollection.GetStr("$filter"),
            queryCollection.GetStr("$select"),
            queryCollection.GetStr("$orderBy"),
            queryCollection.GetInt("$top"),
            queryCollection.GetInt("$skip"),
            queryCollection.GetBool("$count")
        )
        { }

        public QueryParameters(string expand, string filter, string select, string orderBy, int? top, int? skip, bool count = false)
        {
            Count = count;
            Top = top;
            Skip = skip;
            Filter = filter;
            Select = select;
            Expand = expand;
            OrderBy = orderBy;
        }

        public bool Count { get; }
        public int? Top { get; }
        public int? Skip { get; }
        public string Filter { get; }
        public string Select { get; }
        public string Expand { get; }
        public string OrderBy { get; }
    }
}
