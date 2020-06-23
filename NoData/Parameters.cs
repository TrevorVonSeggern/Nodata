using Immutability;
using Microsoft.AspNetCore.Http;
using System.Linq;

namespace NoData
{
    [Immutable]
    public class Parameters
    {
        public Parameters(string expand = null, string filter = null, string select = null, string orderBy = null, int? top = null, int? skip = null, bool count = false)
        {
            Count = count;
            Top = top;
            Skip = skip;
            Filter = filter;
            Select = select;
            Expand = expand;
            OrderBy = orderBy;
        }
        public Parameters(Parameters other)
        {
            Count = other.Count;
            Top = other.Top;
            Skip = other.Skip;
            Filter = other.Filter;
            Select = other.Select;
            Expand = other.Expand;
            OrderBy = other.OrderBy;
        }

        public bool Count { get; }
        public int? Top { get; }
        public int? Skip { get; }
        public string Filter { get; }
        public string Select { get; }
        public string Expand { get; }
        public string OrderBy { get; }
    }
    internal static class ParametersHelper
    {
        public static Parameters FromHttpContext(IHttpContextAccessor accessor)
        {
            var queryCollection = accessor.HttpContext.Request.Query;
            return new Parameters(
                queryCollection.GetStr("$expand"),
                queryCollection.GetStr("$filter"),
                queryCollection.GetStr("$select"),
                queryCollection.GetStr("$orderBy"),
                queryCollection.GetInt("$top"),
                queryCollection.GetInt("$skip"),
                queryCollection.GetBool("$count")
            );
        }

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

}
