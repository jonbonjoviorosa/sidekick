
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using Sidekick.Api.DataAccessLayer.Interfaces;
using Sidekick.Api.Helpers;
using Sidekick.Model;
using System.Threading.Tasks;

namespace Sidekick.Api.Controllers
{
    [Authorize]
    [Produces("application/json")]
    [Route("api/Facility")]
    public class FacilityController : APIBaseController
    {
        IFacilityRepository FacilityRepo { get; }
        private IMainHttpClient MainHttpClient { get; }
        private APIConfigurationManager MConf { get; }

        public FacilityController(IFacilityRepository _fRepo, IMainHttpClient _mhttpc, APIConfigurationManager _conf)
        {
            FacilityRepo = _fRepo;
            MainHttpClient = _mhttpc;
            MConf = _conf;
        }

        [HttpPost("Add")]
        public async Task<IActionResult> AddFacility([FromHeader] string Authorization, [FromBody] FacilityProfile _facility)
        {
            if (ModelState.IsValid)
            {
                return Ok(await FacilityRepo.AddFacility(Authorization.Split(' ')[1], _facility, MainHttpClient, MConf));
            }
            else
            {
                return BadRequest(new APIResponse
                {
                    Message = "Invalid Model Object",
                    ModelError = ModelState.Errors(),
                    StatusCode = System.Net.HttpStatusCode.BadRequest
                });
            }
        }

        [HttpGet("AllFacilities")]
        public IActionResult GetAllFacilities()
        {
            APIResponse apiResp = FacilityRepo.GetAllFacilities();
            if (apiResp.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Ok(apiResp);
            }
            else
            {
                return BadRequest(apiResp);
            }
        }

        [HttpGet("Profile/{_facilityId}")]
        public IActionResult GetFacilityProfile(Guid _facilityId)
        {
            APIResponse apiResp = FacilityRepo.GetFacilityProfile(_facilityId);
            if (apiResp.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Ok(apiResp);
            }
            else
            {
                return BadRequest(apiResp);
            }
        }

        [HttpPost("EditProfile")]
        public IActionResult EditFacilityProfile([FromBody] FacilityProfile _facility)
        {
            if (ModelState.IsValid)
            {
                return Ok(FacilityRepo.EditFacilityProfile(_facility));
            }
            else
            {
                return BadRequest(new APIResponse
                {
                    Message = "Invalid Model Object",
                    ModelError = ModelState.Errors(),
                    StatusCode = System.Net.HttpStatusCode.BadRequest
                });
            }
        }

        [HttpPost("ChangeStatus")]
        public async Task<IActionResult> FacilityChangeStatus([FromHeader] string Authorization, [FromBody] ChangeStatus _facility)
        {
            if (ModelState.IsValid)
            {
                return Ok(await FacilityRepo.FacilityChangeStatus(Authorization.Split(' ')[1], _facility));
            }
            else
            {
                return BadRequest(new APIResponse
                {
                    Message = "Invalid Model Object",
                    ModelError = ModelState.Errors(),
                    StatusCode = System.Net.HttpStatusCode.BadRequest
                });
            }
        }

        [HttpPost("Update")]
        public IActionResult UpdateFacilityProfile([FromBody] FacilityProfile facilityProfile)
        {
            if (ModelState.IsValid)
            {
                return Ok(FacilityRepo.UpdateFacilityProfile(facilityProfile));
            }
            else
            {
                return BadRequest(new APIResponse
                {
                    Message = "Invalid Model Object",
                    ModelError = ModelState.Errors(),
                    StatusCode = System.Net.HttpStatusCode.BadRequest
                });
            }
        }

        [HttpGet("GetAreas")]
        public IActionResult GetAreas()
        {
            var response = FacilityRepo.GetAreas();
            if(response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }

        [HttpGet("FacilityUserTypes")]
        public IActionResult GetFacilityUserTypes()
        {
            APIResponse apiResp = FacilityRepo.GetFacilityUserTypes();
            if (apiResp.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Ok(apiResp);
            }
            else
            {
                return BadRequest(apiResp);
            }
        }

        [HttpPost("FacilityUserTypes")]
        public IActionResult AddEditFacilityUserTypes([FromBody]FacilityUserType facilityUserType)
        {
            APIResponse apiResp = FacilityRepo.AddEditFacilityUserType(facilityUserType);
            if (apiResp.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Ok(apiResp);
            }
            else
            {
                return BadRequest(apiResp);
            }
        }

        [HttpPost("FacilityUserTypes/Delete")]
        public IActionResult DeleteFacilityUserTypes([FromBody] FacilityUserType facilityUserType)
        {
            APIResponse apiResp = FacilityRepo.DeleteFacilityUserType(facilityUserType);
            if (apiResp.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Ok(apiResp);
            }
            else
            {
                return BadRequest(apiResp);
            }
        }
    }
}