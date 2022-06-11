using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sidekick.Api.DataAccessLayer.Interfaces;
using Sidekick.Model;
using Sidekick.Model.SetupConfiguration.Size;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sidekick.Api.Controllers
{
    [Authorize]
    [Produces("application/json")]
    [Route("api/Size")]
    public class SizeController : APIBaseController
    {
        private ISizeRepository _sizeRepo { get; }
        public SizeController(ISizeRepository sizeRepo)
        {
            _sizeRepo = sizeRepo;
        }

        [HttpGet("Get")]
        public async Task<ActionResult<APIResponse<IEnumerable<TeamSize>>>> GetSizes()
        {
            return Ok(await _sizeRepo.GetAllSizes());
        }

        [HttpPost("AddOrEdit")]
        public IActionResult AddOrEditTeamSize([FromHeader] string Authorization, [FromBody] TeamSize size)
        {
            if (ModelState.IsValid)
            {
                return Ok(_sizeRepo.AddOrEditTeamSize(Authorization.Split(' ')[1], size));
            }
            return BadRequest(new APIResponse
            {
                Message = "Invalid Model Object",
                ModelError = ModelState.Errors(),
                StatusCode = System.Net.HttpStatusCode.BadRequest
            });

        }

        [HttpPost("Delete/{sizeId}")]
        public IActionResult DeleteSize([FromHeader] string Authorization, [FromBody] Guid sizeId)
        {
            if (ModelState.IsValid)
            {
                return Ok(_sizeRepo.DeleteSize(Authorization.Split(' ')[1], sizeId));
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
