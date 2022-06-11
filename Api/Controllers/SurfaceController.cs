using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sidekick.Api.DataAccessLayer.Interfaces;
using Sidekick.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sidekick.Api.Controllers
{
    [Authorize]
    [Produces("application/json")]
    [Route("api/Surface")]
    public class SurfaceController : APIBaseController
    {
        private readonly ISurfaceRepository _surfaceRepo;

        public SurfaceController(ISurfaceRepository surfaceRepo)
        {
            _surfaceRepo = surfaceRepo;
        }

        [HttpGet("Get")]
        public async Task<ActionResult<APIResponse<List<Surface>>>> GetSetupConfigurations()
        {
            return Ok(await _surfaceRepo.GetAllSurface());
        }

        [HttpPost("AddOrEdit")]
        public IActionResult AddOrEditSurface([FromHeader] string Authorization, [FromBody] Surface surface)
        {
            if (ModelState.IsValid)
            {
                return Ok(_surfaceRepo.AddOrEditSurface(Authorization.Split(' ')[1], surface));
            }

            return BadRequest(new APIResponse
            {
                Message = "Invalid Model Object",
                ModelError = ModelState.Errors(),
                StatusCode = System.Net.HttpStatusCode.BadRequest
            });
        }

        [HttpPost("Delete/{surfaceId}")]
        public IActionResult DeleteSurface([FromHeader] string Authorization, [FromBody] Guid surfaceId)
        {
            if (ModelState.IsValid)
            {
                return Ok(_surfaceRepo.DeleteSurface(Authorization.Split(' ')[1], surfaceId));
            }

            return BadRequest(new APIResponse
            {
                Message = "Invalid Model Object",
                ModelError = ModelState.Errors(),
                StatusCode = System.Net.HttpStatusCode.BadRequest
            });

        }
    }
}
