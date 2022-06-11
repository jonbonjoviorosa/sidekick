using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sidekick.Api.DataAccessLayer.Interfaces;
using Sidekick.Model;
using Sidekick.Model.SetupConfiguration.Goals;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sidekick.Api.Controllers
{
    [Authorize]
    [Produces("application/json")]
    [Route("api/Goal")]
    public class GoalController : APIBaseController
    {
        private IGoalRepository _goalRepository { get; }
        public GoalController(IGoalRepository goalRepository)
        {
            _goalRepository = goalRepository;
        }

        [HttpGet("Get")]
        public async Task<ActionResult<APIResponse<IEnumerable<Goal>>>> GetSetupConfigurations()
        {
            return Ok(await _goalRepository.GetGoals());
        }

        [HttpPost("AddOrEdit")]
        public IActionResult AddOrEditGoal([FromHeader] string Authorization, [FromBody] Goal goal)
        {
            if (ModelState.IsValid)
            {
                return Ok(_goalRepository.AddOrEditGoal(Authorization.Split(' ')[1], goal));
            }

            return BadRequest(new APIResponse
            {
                Message = "Invalid Model Object",
                ModelError = ModelState.Errors(),
                StatusCode = System.Net.HttpStatusCode.BadRequest
            });

        }

        [HttpPost("Delete/{goalId}")]
        public IActionResult DeleteSurface([FromHeader] string Authorization, [FromBody] Guid goalId)
        {
            if (ModelState.IsValid)
            {
                return Ok(_goalRepository.DeleteGoal(Authorization.Split(' ')[1], goalId));
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
