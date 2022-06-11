using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sidekick.Api.DataAccessLayer.Interfaces;
using Sidekick.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sidekick.Api.Controllers
{
    [Authorize]
    [Produces("application/json")]
    [Route("api/Commission")]
    public class CommissionController : APIBaseController
    {
        private ICommissionRepository _comissionRepository { get; }
        public CommissionController(ICommissionRepository comissionRepository)
        {
            _comissionRepository = comissionRepository;
        }

        [HttpGet("GetComissionPlays")]
        public async Task<ActionResult<APIResponse<IEnumerable<CommissionPlay>>>> GetComissionPlays()
        {
            return Ok(await _comissionRepository.ComissionPlays());
        }

        [HttpGet("GetComissionTrains")]
        public async Task<ActionResult<APIResponse<CommissionTrain>>> GetComissionTrains()
        {
            return Ok(await _comissionRepository.ComissionTrains());
        }

        [HttpPost("AddOrEditCommissionPlay")]
        public async Task<IActionResult> AddOrEditCommissionPlay([FromHeader] string Authorization, [FromBody] List<CommissionPlaySportViewModel> play)
        {
            if (ModelState.IsValid)
            {
                return Ok(await _comissionRepository.AddOrEditComissionPlay(Authorization.Split(' ')[1], play));
            }

            return BadRequest(new APIResponse
            {
                Message = "Invalid Model Object",
                ModelError = ModelState.Errors(),
                StatusCode = System.Net.HttpStatusCode.BadRequest
            });

        }

        [HttpPost("AddOrEditCommissionTrain")]
        public IActionResult AddOrEditComissionTrain([FromHeader] string Authorization, [FromBody] CommissionTrain train)
        {
            if (ModelState.IsValid)
            {
                return Ok(_comissionRepository.AddOrEditComissionTrain(Authorization.Split(' ')[1], train));
            }

            return BadRequest(new APIResponse
            {
                Message = "Invalid Model Object",
                ModelError = ModelState.Errors(),
                StatusCode = System.Net.HttpStatusCode.BadRequest
            });

        }

        [HttpGet("GetComissionReport")]
        public async Task<ActionResult<APIResponse<IEnumerable<CommisionReport>>>> GetComissionReport()
        {
            return Ok(await _comissionRepository.GetComissionReport());
        }
    }
}
