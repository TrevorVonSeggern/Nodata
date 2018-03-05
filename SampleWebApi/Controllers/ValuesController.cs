using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace nodata.Controllers
{
    using Models;
    using NoData;

    [Route("api/[controller]")]
    public class ValuesController : Controller
    {
        // GET api/values
        [HttpGet]
        public IQueryable<MyDto> Get([FromQuery] NoDataQuery<MyDto> f)
        {
            var result = new List<MyDto>{
                new MyDto{id = 1, Name = "one"},
                new MyDto{id = 2, Name = "two"},
                new MyDto{id = 3, Name = "three"},
                new MyDto{id = 4, Name = "four"},
                new MyDto{id = 5, Name = "five"},
            };

            return f.ApplyTo(result.AsQueryable());
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
