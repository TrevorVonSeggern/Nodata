using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SampleEFCoreApi.Models;
using AutoMapper;
using NoData;
using AutoMapper.QueryableExtensions;

using Model = SampleEFCoreApi.Models.DtoChild;
using ModelCreate = SampleEFCoreApi.Models.DtoChildCreate;
using ModelModify = SampleEFCoreApi.Models.DtoChildModify;
using DbModel = SampleEFCoreApi.Database.Child;
using Microsoft.EntityFrameworkCore;
using System.Net.Http;
using System.IO;
using System.Net;

namespace SampleEFCoreApi.Controllers
{
    [Route("api/[controller]")]
    public class ChildController : Controller
    {
        private readonly Facade<DbModel, Model, ModelCreate, ModelModify> _facade;
        private readonly DataContext Context;

        public ChildController(DataContext context, IMapper mapper)
        {
            Context = context;
            _facade = new Facade<DbModel, Model, ModelCreate, ModelModify>(context, mapper, context.Children);
        }

        [HttpGet]
        public IQueryable<Model> Get([FromServices] INoData<Model> nodata)
        {
            return _facade.Get(nodata);
        }

        // [HttpGet("{id}")]
        // public Model Get(int id)
        // {
        //     // return _mapper.Map<Model>(Query.FirstOrDefault(x => x.Id.Equals(id)));
        // }

        [HttpPost]
        public async Task<ActionResult> Post([FromBody]ModelCreate value)
        {
            var result = await _facade.Post(value);
            if (result is null)
                return BadRequest();
            return Created(Request.Path.ToString(), result);
        }

        [HttpPatch("{id}")]
        public async Task<ActionResult> Patch(int id, [FromBody]ModelModify dto)
        {
            var result = await _facade.Patch(id, dto, p => p.Id);
            if (result is null)
                return BadRequest();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
