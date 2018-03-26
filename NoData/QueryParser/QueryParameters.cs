using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;

namespace NoData.QueryParser
{
    public class QueryParameters
    {
        public QueryParameters() { }
        public QueryParameters(string expand, string filter, string select, int? top, int? skip, bool count = false)
        {
            Count = count;
            Top = top;
            Skip = skip;
            Filter = filter;
            Select = select;
            Expand = expand;
        }

        [FromQuery(Name = "$count")]
        public bool Count { get; private set; }
        [FromQuery(Name = "$top")]
        public int? Top { get; private set; }
        [FromQuery(Name = "$skip")]
        public int? Skip { get; private set; }
        [FromQuery(Name = "$filter")]
        public string Filter { get; private set; }
        [FromQuery(Name = "$select")]
        public string Select { get; private set; }
        [FromQuery(Name = "$expand")]
        public string Expand { get; private set; }
    }
}
