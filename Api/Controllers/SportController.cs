
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sidekick.Api.DataAccessLayer.Interfaces;
using Sidekick.Api.Helpers;
using Sidekick.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sidekick.Api.Controllers
{
    [Authorize]
    [Produces("application/json")]
    [Route("api/Sport")]
    public class SportController : APIBaseController
    {
        ISportRepository SportRepo { get; }
        private IMainHttpClient MainHttpClient { get; }
        private APIConfigurationManager MConf { get; }

        public SportController(ISportRepository _spRepo, IMainHttpClient _mhttpc, APIConfigurationManager _conf)
        {
            SportRepo = _spRepo;
            MainHttpClient = _mhttpc;
            MConf = _conf;
        }

        [HttpGet("Get")]
        public async Task<ActionResult<APIResponse<List<Sport>>>> GetSports()
        {
            return Ok(await SportRepo.GetSports());
        }

        [HttpPost("Add")]
        public IActionResult AddSport([FromHeader] string Authorization, [FromBody] SportDto _sport)
        {
            if (ModelState.IsValid)
            {
                return Ok(SportRepo.AddSport(Authorization.Split(' ')[1], _sport));
            }

            return BadRequest(new APIResponse
            {
                Message = "Invalid Model Object",
                ModelError = ModelState.Errors(),
                StatusCode = System.Net.HttpStatusCode.BadRequest
            });
        }

        [HttpPost("Edit")]
        public IActionResult EditSport([FromHeader] string Authorization, [FromBody] SportDto _sport)
        {
            if (ModelState.IsValid)
            {
                return Ok(SportRepo.EditSport(Authorization.Split(' ')[1], _sport));
            }

            return BadRequest(new APIResponse
            {
                Message = "Invalid Model Object",
                ModelError = ModelState.Errors(),
                StatusCode = System.Net.HttpStatusCode.BadRequest
            });
        }

        [HttpGet("Get/{_sportId}")]
        public IActionResult GetSport(int _sportId)
        {
            APIResponse apiResp = SportRepo.GetSport(_sportId);
            if (apiResp.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Ok(apiResp);
            }

            return BadRequest(apiResp);

        }

        [HttpPost("Delete/{_sportId}")]
        public IActionResult DeleteSport([FromHeader] string Authorization, [FromBody] Guid _sportId)
        {
            if (ModelState.IsValid)
            {
                return Ok(SportRepo.DeleteSport(Authorization.Split(' ')[1], _sportId));
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