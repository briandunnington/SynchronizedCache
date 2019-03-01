using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace SynchronizedCacheExample.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AnimalController : ControllerBase
    {
        private readonly IAnimalCache animalCache;

        public AnimalController(IAnimalCache animalCache)
        {
            this.animalCache = animalCache;
        }

        // GET api/animal
        [HttpGet()]
        public VirtualFileResult Get()
        {
            //return Ok(new {
            //    tryThis = "/api/animal/dog"
            //});

            return File("/index.html", "text/html");
        }

        // GET api/animal/dog
        [HttpGet("{type}")]
        public async Task<ActionResult<string>> Get(string type)
        {
            var animal = await animalCache.GetAsync(type);
            return Ok(animal);
        }

        // GET api/animal/dog/flush
        [HttpGet("{type}/flush")]
        public async Task<ActionResult> Delete(string type)
        {
            await animalCache.InvalidateAsync(type);
            return NoContent();
        }
    }
}
