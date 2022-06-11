
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sidekick.Api.Configurations.Resources;
using Sidekick.Api.DataAccessLayer.Interfaces;
using Sidekick.Api.Helpers;
using Sidekick.Model;
using Sidekick.Model.Player;
using System;
using System.Threading.Tasks;

namespace Sidekick.Api.Controllers
{
    [Authorize]
    [Produces("application/json")]
    [Route("api/FacilityPlayer")]
    public class FacilityPlayerController : APIBaseController
    {
        IFacilityPlayerRepository FacilityPlayerRepo { get; }
        private IMainHttpClient MainHttpClient { get; }
        private APIConfigurationManager MConf { get; }

        public FacilityPlayerController(IFacilityPlayerRepository _fpRepo, IMainHttpClient _mhttpc, APIConfigurationManager _conf)
        {
            FacilityPlayerRepo = _fpRepo;
            MainHttpClient = _mhttpc;
            MConf = _conf;
        }


        [HttpGet("Get/{_guid}")]
        public IActionResult GetFacilityPlayers(Guid _guid)
        {
            APIResponse apiResp = FacilityPlayerRepo.GetFacilityPlayers(_guid);
            if (apiResp.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Ok(apiResp);
            }
            else
            {
                return BadRequest(apiResp);
            }
        }

        [HttpPost("Delete/{_guid}/{_userNo}")]
        public async Task<APIResponse> DeleteFacilityPlayer(Guid _guid, string _userNo)
        {
            return await FacilityPlayerRepo.DeleteFacilityPlayer(_guid, _userNo);
        }

        [HttpGet("All")]
        public async Task<IActionResult> GetAllFacilityPlayers()
        {
            var apiResp = await FacilityPlayerRepo.GetAllFacilityPlayers();
            if (apiResp.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Ok(apiResp);
            }


            return BadRequest(apiResp);
        }

        [HttpGet("AllDate/{dateFrom}/{dateTo}")]
        public async Task<IActionResult> GetAllFacilityPlayersDates(string dateFrom, string dateTo)
        {
            var apiResp = await FacilityPlayerRepo.GetAllFacilityPlayersDates(dateFrom, dateTo);
            if (apiResp.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Ok(apiResp);
            }


            return BadRequest(apiResp);
        }

        [HttpPost("AddOrEditPlayer")]
        public async Task<IActionResult> AddOrEditPlayer([FromHeader] string Authorization, [FromBody] PlayerViewModel player)
        {
            var response = await FacilityPlayerRepo.AddOrEditPlayer(Authorization.Split(' ')[1],  player);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }

        [HttpGet("GetPlayer/{userId}")]
        public async Task<IActionResult> GetPlayer(Guid userId)
        {
            var response = await FacilityPlayerRepo.GetPlayer(userId);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }

        [HttpPost("ChangeStatus")]
        public async Task<ActionResult<APIResponse>> ChangeStatus([FromHeader] string Authorization, [FromBody] ChangeStatus user)
        {
            var response = await FacilityPlayerRepo.ChangeStatus(Authorization.Split(' ')[1], user);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }
    }
}