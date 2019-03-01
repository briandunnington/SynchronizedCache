using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace SynchronizedCacheExample.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class carController : ControllerBase
    {
        private readonly ICarCache carCache;

        public carController(ICarCache carCache)
        {
            this.carCache = carCache;
        }

        // GET api/car/make/model
        [HttpGet("{make}/{model}")]
        public async Task<ActionResult<string>> Get(string make, string model)
        {
            var carType = new CarType() { Make = make, Model = model };
            var car = await carCache.GetAsync(carType);
            return Ok(car);
        }

        // GET api/car/make/model/flush
        [HttpGet("{make}/{model}/flush")]
        public async Task<ActionResult> Delete(string make, string model)
        {
            var carType = new CarType() { Make = make, Model = model };
            await carCache.InvalidateAsync(carType);
            return NoContent();
        }
    }
}
