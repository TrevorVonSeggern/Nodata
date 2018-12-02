using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Immutability;
using Microsoft.AspNetCore.Http;

namespace NoData
{

    [Immutable]
    /// <summary>
    /// This interface is designed to have just enough information to apply the expression tree to the query, and return the result query in the correct format.
    /// </summary>
    public interface INoDataQuery<out TDto>
    {
        IQueryable<TDto> BuildQueryable();

        string AsJson();
        Task StreamResponse(HttpResponse response);
        Task StreamResponse(Stream responseBody);
        Stream Stream();
    }
}