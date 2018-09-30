using System.Linq;
using AutoMapper;
using CodeTools;

namespace NoData
{
    [Immutable]
    public interface INoData<TDto>
    {
        INoDataQuery<TDto> Load(IQueryable<TDto> sourceQueryable);

        INoDataQuery<TDto> Projection<TEntity>(IQueryable<TEntity> sourceQueryable, IConfigurationProvider mapperConfiguration);
    }
}