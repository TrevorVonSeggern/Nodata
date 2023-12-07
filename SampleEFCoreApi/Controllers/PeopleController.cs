using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using NoData;
using Model = SampleEFCoreApi.Models.DtoPerson;
using ModelCreate = SampleEFCoreApi.Models.DtoPersonCreate;
using ModelModify = SampleEFCoreApi.Models.DtoPersonModify;
using DbModel = SampleEFCoreApi.Database.Person;

namespace SampleEFCoreApi.Controllers;

[Route("api/[controller]")]
public class PeopleController : Controller
{
    private readonly Facade<DbModel, Model, ModelCreate, ModelModify> _facade;
    private readonly DataContext Context;

    public PeopleController(DataContext context, IMapper mapper)
    {
        Context = context;
        _facade = new Facade<DbModel, Model, ModelCreate, ModelModify>(context, mapper, context.People);
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
