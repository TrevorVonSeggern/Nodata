using System.Collections.Generic;
using Newtonsoft.Json;

namespace NoData
{
    public class NoDataSerializedResultList<T> : NoDataSerializedResult<List<T>> { }
    public class NoDataSerializedResult<T>
    {
        [JsonProperty("@odata.count", NullValueHandling = NullValueHandling.Ignore)]
        public int Count { get; set; }

        [JsonProperty("value")]
        public T Value { get; set; }
    }
}