using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace nodata.Controllers
{
    using Models;
    using NoData;
    using System;

    [Route("api/[controller]")]
    public class ValuesController : Controller
    {
        public static IEnumerable<DtoGrandChild> GrandChildCollection => new List<DtoGrandChild>
        {
            new DtoGrandChild{ id = 100, Name = "George German grand child 1", region_code = "de"  },
            new DtoGrandChild{ id = 200, Name = "George German grand child 2", region_code = "de"  },
            new DtoGrandChild{ id = 300, Name = "George US grand child 1", region_code = "en"  },
            new DtoGrandChild{ id = 400, Name = "George US grand child 2", region_code = "en"  },
            new DtoGrandChild{ id = 500, Name = "George Mexican grand child 1", region_code = "es"  },
            new DtoGrandChild{ id = 600, Name = "George Mexican grand child 2", region_code = "es"  },
        };

        public static IEnumerable<DtoChild> ChildCollection
        {
            get
            {
                var result = new List<DtoChild>
                {
                    new DtoChild{ id = 10, Name = "John child 1", region_code = "en" },
                    new DtoChild{ id = 30, Name = "George child 1", region_code = "de", children = GrandChildCollection.Where(x => x.id == 100 || x.id == 200).ToList()},
                    new DtoChild{
                        id = 40,
                        Name = "George child 2",
                        region_code = "es",
                        children = GrandChildCollection.Where(x => x.id == 300 || x.id == 400).ToList(),
                        favorite = GrandChildCollection.Single(x => x.id == 300)
                    },
                    new DtoChild{ id = 50, Name = "George child 3", region_code = "en", children = GrandChildCollection.Where(x => x.id == 500 || x.id == 600).ToList()  },
                    new DtoChild{ id = 60, Name = "Joe child 1", region_code = "en" },
                };
                result[4].partner = result[0];
                result[0].partner = result[4];
                return result;
            }
        }

        public static IEnumerable<Dto> ParentCollection
        {
            get
            {
                var result = new List<Dto>
                {
                    new Dto{ id = 1, Name = "John", region_code = "en", children = ChildCollection.Where(x => x.id == 10).ToList() },
                    new Dto{ id = 2, Name = "Jane", region_code = "en", favorite = ChildCollection.Single(x => x.id == 10) },
                    new Dto{ id = 3, Name = "George", region_code = "de", children = ChildCollection.Where(x => x.id == 30).ToList() },
                    new Dto{ id = 4, Name = "George", region_code = "en", children = ChildCollection.Where(x => x.id == 40).ToList(), favorite = ChildCollection.Single(x => x.id == 40) },
                    new Dto{ id = 5, Name = "George", region_code = "es", children = ChildCollection.Where(x => x.id == 50).ToList() },
                    new Dto{ id = 6, Name = "Joe", region_code = "en", children = ChildCollection.Where(x => x.id == 60).ToList() },
                };
                result[1].partner = result[0];
                result[0].partner = result[1];
                return result;
            }
        }

        // GET api/values
        [HttpGet]
        public string Get([FromQuery] NoDataQuery<Dto> f)
        {
            string response = "";
            try
            {
                response =  f.JsonResult(ParentCollection.AsQueryable());
            }
            catch (Exception e)
            {
                return response;
            }
            return response;
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
