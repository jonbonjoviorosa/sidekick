using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sidekick.Api.DataAccessLayer.Interfaces;
using Sidekick.Model;
using Sidekick.Model.Gym;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Sidekick.Api.Controllers
{
    [Authorize]
    [Produces("application/json")]
    [Route("api/Gym")]
    public class GymController : ControllerBase
    {
        private readonly IGymRepository _gymRepository;

        public GymController(IGymRepository gymRepository)
        {
            _gymRepository = gymRepository;
        }

        [HttpGet("Get")]
        public async Task<ActionResult<APIResponse<IEnumerable<Gym>>>> GetGyms()
        {
            return Ok(await _gymRepository.GetGyms());
        }

        [HttpPost("AddOrEdit")]
        public IActionResult AddOrEditGym([FromHeader] string Authorization, [FromBody] Gym gym)
        {
            if (ModelState.IsValid)
            {
                return Ok(_gymRepository.AddOrEditGym(Authorization.Split(' ')[1], gym));
            }

            return BadRequest(new APIResponse
            {
                Message = "Invalid Model Object",
                ModelError = ModelState.Errors(),
                StatusCode = HttpStatusCode.BadRequest
            });

        }

        [HttpPost("Delete/{gymId}")]
        public IActionResult DeleteGym([FromHeader] string Authorization, [FromBody] Guid gymId)
        {
            if (ModelState.IsValid)
            {
                return Ok(_gymRepository.DeleteGym(Authorization.Split(' ')[1], gymId));
            }

            return BadRequest(new APIResponse
            {
                Message = "Invalid Model Object",
                ModelError = ModelState.Errors(),
                StatusCode = HttpStatusCode.BadRequest
            });

        }
    }
}
