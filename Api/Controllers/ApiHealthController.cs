using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace Sidekick.Api.Controllers
{
    [Route("api/ApiHealth")]
    [ApiController]
    public class ApiHealthController : ControllerBase
    {
        // GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            return new string[] { "Status:", "API Running.." };
        }

        [HttpGet("GetCurrentCultureDate")]
        public string GetCurrentCultureDate()
        {
            return DateTime.Now.ToString();
        }
    }
}
