using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Sidekick.Api.Handlers.IBusiness;
using Sidekick.Model;
using Sidekick.Model.Nationality;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sidekick.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NationalityController : ControllerBase
    {
        private readonly INationalityRepository nationalityRepository;

        public NationalityController(INationalityRepository nationalityRepository)
        {
            this.nationalityRepository = nationalityRepository;
        }

        [HttpGet("Get")]
        public async Task<ActionResult<APIResponse<List<Nationality>>>> GetNationalities()
        {
            return Ok(await nationalityRepository.GetNationalities());
        }
    }
}
