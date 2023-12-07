using AutoMapper;
using NoData;
using Microsoft.EntityFrameworkCore;

namespace SampleEFCoreApi.Controllers
{
    public class Facade<DbModel, Model, ModelCreate, ModelModify>
        where DbModel : class
    {
        private readonly DataContext Context;
        private readonly IMapper _mapper;
        private IQueryable<DbModel> Query => Set.AsNoTracking();
        private DbSet<DbModel> Set { get; }

        public Facade(DataContext context, IMapper mapper, DbSet<DbModel> set)
        {
            Set = set;
            Context = context;
            _mapper = mapper;
        }

        public IQueryable<Model> Get(INoData<Model> nodata)
        {
            return nodata.Projection(Query, _mapper.ConfigurationProvider).BuildQueryable();
        }

        public async Task<Model> Post(ModelCreate value)
        {
            var entity = _mapper.Map<DbModel>(value);
            var e = Set.Add(entity);
            await Context.SaveChangesAsync();
            return _mapper.Map<Model>(e.Entity);
        }

        public async Task<Model> Patch(int id, ModelModify dto, Func<DbModel, int> idFunc)
        {
            var entity = await Set.FirstOrDefaultAsync(x => idFunc(x).Equals(id));
            if (EqualityComparer<DbModel>.Default.Equals(entity, default(DbModel)))
                return default(Model);
            entity = _mapper.Map<ModelModify, DbModel>(dto, entity);
            // Context.Entry(entity).State = EntityState.Modified;
            Set.Attach(entity);
            await Context.SaveChangesAsync();
            return _mapper.Map<DbModel, Model>(entity);
        }

        public void Delete(int id)
        {
        }
    }
}
