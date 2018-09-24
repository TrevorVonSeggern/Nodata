using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SampleEFCoreApi.Models;
using AutoMapper;
using NoData;
using AutoMapper.QueryableExtensions;

using Model = SampleEFCoreApi.Models.DtoPerson;
using DbModel = SampleEFCoreApi.Database.Person;
using Microsoft.EntityFrameworkCore;

namespace SampleEFCoreApi.Controllers
{
    [Route("api/[controller]")]
    public class PeopleController : Controller
    {
        private readonly DataContext Context;
        private readonly IMapper _mapper;
        private IQueryable<DbModel> Query => Context.People.AsNoTracking();

        public PeopleController(DataContext context, IMapper mapper)
        {
            Context = context;
            _mapper = mapper;
        }

        [HttpGet]
        public IQueryable<Model> Get([FromServices] INoData<Model> nodata)
        {
            return nodata.Projection(Query, _mapper.ConfigurationProvider).BuildQueryable();
        }

        [HttpGet("{id}")]
        public Model Get(int id)
        {
            return _mapper.Map<Model>(Query.FirstOrDefault(x => x.Id.Equals(id)));
        }

        [HttpPost]
        public void Post([FromBody]Model value)
        {
            if (Query.Any(x => x.Id.Equals(value.Id)))
                return;
            Context.People.Add(_mapper.Map<DbModel>(value));
            Context.SaveChanges();
        }

        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
