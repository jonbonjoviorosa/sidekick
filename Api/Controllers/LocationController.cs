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
    [Route("api/Location")]
    public class LocationController : APIBaseController
    {
        private ILocationRepository _locationRepo { get; }
        public LocationController(ILocationRepository locationRepo)
        {
            _locationRepo = locationRepo;
        }

        [HttpGet("Get")]
        public async Task<ActionResult<APIResponse<IEnumerable<Location>>>> GetSetupConfigurations()
        {
            return Ok(await _locationRepo.GetLocations());
        }

        [HttpPost("AddOrEdit")]
        public IActionResult AddOrEditLocation([FromHeader] string Authorization, [FromBody] Location location)
        {
            if (ModelState.IsValid)
            {
                return Ok(_locationRepo.AddOrEditLocation(Authorization.Split(' ')[1], location));
            }

            return BadRequest(new APIResponse
            {
                Message = "Invalid Model Object",
                ModelError = ModelState.Errors(),
                StatusCode = System.Net.HttpStatusCode.BadRequest
            });

        }

        [HttpPost("Delete/{locationId}")]
        public IActionResult DeleteSurface([FromHeader] string Authorization, [FromBody] Guid locationId)
        {
            if (ModelState.IsValid)
            {
                return Ok(_locationRepo.DeleteLocation(Authorization.Split(' ')[1], locationId));
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

