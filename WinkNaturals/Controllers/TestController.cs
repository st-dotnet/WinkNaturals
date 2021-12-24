
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Threading.Tasks;
using WinkNatural.Web.Services.Interfaces;

namespace WinkNaturals.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TestController : ControllerBase
    {

        private readonly IDistributedCache _distributedCache;
        private readonly IEnrollmentService _enrollmentService;

        public TestController(IDistributedCache distributedCache, IEnrollmentService enrollmentService)
        {
            _distributedCache = distributedCache;
            _enrollmentService = enrollmentService;
        }

        [Route("add-cache-no-time-options")]
        [HttpGet]
        public async Task<IActionResult> AddCacheNoTimeOptions()
        {
            string key = "test1";
            string value = "naveen";
            await _distributedCache.SetStringAsync(key, value);
            return Ok("success");
        }
        [Route("add-cache")]
        [HttpGet]
        public async Task<IActionResult> AddCache()
        {
            string key = "test2";
            string value = "Naveen Bommindi";
            DistributedCacheEntryOptions options = new DistributedCacheEntryOptions
            {
                SlidingExpiration = TimeSpan.FromMinutes(1),
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
            };
            await _distributedCache.SetStringAsync(key, value, options);
            return Ok("success");
        }
        [Route("get-cache")]
        [HttpGet]
        public async Task<IActionResult> GetCache()
        {
            string name = await _distributedCache.GetStringAsync("test2");
            return Ok(name);
        }
        [Route("delete-cache")]
        [HttpGet]
        public async Task<IActionResult> DeleteCache(string key)
        {
            await _distributedCache.RemoveAsync(key);
            return Ok();
        }
      
    }
}
