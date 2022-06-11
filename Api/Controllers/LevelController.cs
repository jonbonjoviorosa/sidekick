using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sidekick.Api.DataAccessLayer.Interfaces;
using Sidekick.Model;
using Sidekick.Model.SetupConfiguration.Level;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sidekick.Api.Controllers
{
    [Authorize]
    [Produces("application/json")]
    [Route("api/Level")]
    public class LevelController : APIBaseController
    {
        private ILevelRepository _levelRepo { get; }
        public LevelController(ILevelRepository levelRepo)
        {
            _levelRepo = levelRepo;
        }

        [HttpGet("Get")]
        public async Task<ActionResult<APIResponse<IEnumerable<Level>>>> GetSetupConfigurations()
        {
            return Ok(await _levelRepo.GetLevels());
        }

        [HttpPost("AddOrEdit")]
        public IActionResult AddOrEditLocation([FromHeader] string Authorization, [FromBody] Level level)
        {
            if (ModelState.IsValid)
            {
                return Ok(_levelRepo.AddOrEditLevel(Authorization.Split(' ')[1], level));
            }

            return BadRequest(new APIResponse
            {
                Message = "Invalid Model Object",
                ModelError = ModelState.Errors(),
                StatusCode = System.Net.HttpStatusCode.BadRequest
            });

        }

        [HttpPost("Delete/{levelId}")]
        public IActionResult DeleteLevel([FromHeader] string Authorization, [FromBody] Guid levelId)
        {
            if (ModelState.IsValid)
            {
                return Ok(_levelRepo.DeleteLevel(Authorization.Split(' ')[1], levelId));
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
