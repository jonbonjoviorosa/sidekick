
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
    [Route("api/FacilityPitch")]
    public class FacilityPitchController : APIBaseController
    {
        IFacilityPitchRepository FacilityPitchRepo { get; }
        private IMainHttpClient MainHttpClient { get; }
        private APIConfigurationManager MConf { get; }

        public FacilityPitchController(IFacilityPitchRepository _fpRepo, IMainHttpClient _mhttpc, APIConfigurationManager _conf)
        {
            FacilityPitchRepo = _fpRepo;
            MainHttpClient = _mhttpc;
            MConf = _conf;
        }

        [HttpPost("Add")]
        public async Task<IActionResult> AddFacilityPitch([FromHeader] string Authorization, [FromBody] FacilityPitchDto _pitch)
        {
            if (ModelState.IsValid)
            {
                return Ok(await FacilityPitchRepo.AddFacilityPitch(Authorization.Split(' ')[1], _pitch));
            }

            return BadRequest(new APIResponse
            {
                Message = "Invalid Model Object",
                ModelError = ModelState.Errors(),
                StatusCode = System.Net.HttpStatusCode.BadRequest
            });

        }

        [HttpPost("CreateSlot")]
        public async Task<IActionResult> CreateSlot([FromHeader] string Authorization, [FromBody] FacilityPitchVM pitch)
        {
            var response = await FacilityPitchRepo.CreateSlot(Authorization.Split(' ')[1], pitch);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }

        [HttpGet("GetAllFacilityPitchTiming")]
        public async Task<IActionResult> GetAllFacilityPitchTiming()
        {
            var response = await FacilityPitchRepo.GetAllFacilityPitchTiming();
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }

        [HttpGet("GetAllFacilityPitchBooking")]
        public async Task<IActionResult> GetAllFacilityPitchBooking()
        {
            var response = await FacilityPitchRepo.GetAllFacilityPitchBooking();
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }

        [HttpGet("Get/{_guid}")]
        public async Task<IActionResult> GetFacilityPitch(Guid _guid)
        {
            APIResponse apiResp = await FacilityPitchRepo.GetFacilityPitch(_guid);
            if (apiResp.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Ok(apiResp);
            }

            return BadRequest(apiResp);
        }

        [HttpGet("Get/BookingDetails/{_guid}")]
        public async Task<IActionResult> GetBookingDetails(Guid _guid)
        {
            APIResponse apiResp = await FacilityPitchRepo.GetFacilityBookingPitch(_guid);



            if (apiResp.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Ok(apiResp);
            }

            return BadRequest(apiResp);
        }


        [HttpGet("Get/{_guid}/{_facilityPitchId}/{sportId}")]
        public async Task<IActionResult> GetFacilityPitchByFacilityPitchId(Guid _guid, Guid _facilityPitchId, Guid sportId)
        {
            APIResponse apiResp = await FacilityPitchRepo.GetFacilityPitchByFacilityPitchId(_guid, _facilityPitchId, sportId);
            if (apiResp.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Ok(apiResp);
            }

            return BadRequest(apiResp);

        }


        [HttpGet("Get/{_guid}/{_facilityPitchId}/{sportId}/{facilityPitchTimingId}")]
        public async Task<IActionResult> GetFacilityPitchByFacilityPitchIdWithTiming(Guid _guid, Guid _facilityPitchId, Guid sportId,Guid facilityPitchTimingId)
        {
            APIResponse apiResp = await FacilityPitchRepo.GetFacilityPitchByFacilityPitchIdWithTiming(_guid, _facilityPitchId, sportId, facilityPitchTimingId,Guid.Empty);
            if (apiResp.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Ok(apiResp);
            }

            return BadRequest(apiResp);

        }

        [HttpGet("Get/{_guid}/{_facilityPitchId}/{sportId}/{facilityPitchTimingId}/{bookingId}")]
        public async Task<IActionResult> GetFacilityPitchByFacilityPitchIdWithTimingByBooking(Guid _guid, Guid _facilityPitchId, Guid sportId, Guid facilityPitchTimingId, Guid bookingId)
        {
            APIResponse apiResp = await FacilityPitchRepo.GetFacilityPitchByFacilityPitchIdWithTiming(_guid, _facilityPitchId, sportId, facilityPitchTimingId, bookingId);
            if (apiResp.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Ok(apiResp);
            }

            return BadRequest(apiResp);

        }

        [HttpGet("PitchSports/{_facilityId}")]
        public async Task<IActionResult> GetFacilityPitchSports(Guid _facilityId)
        {
            var apiResp = await FacilityPitchRepo.GetFacilityPitchSports(_facilityId);
            if (apiResp.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Ok(apiResp);
            }
            else
            {
                return BadRequest(apiResp);
            }
        }

        [HttpPost("AddOrUpdateBlockSlot")]
        public async Task<IActionResult> AddBlockSlot([FromHeader] string Authorization, [FromBody] UnavailableSlot blockSlot)
        {
            var response = await FacilityPitchRepo.AddBlockSlot(Authorization.Split(' ')[1], blockSlot);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }

        [HttpGet("GetBlockedSlots/{facilityId}")]
        public async Task<IActionResult> GetBlockedSlots(Guid facilityId)
        {
            var response = await FacilityPitchRepo.GetBlockedSlots(facilityId);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }

        [HttpGet("GetUnavailableSlot/{unavailableSlotId}")]
        public async Task<IActionResult> GetUnavailableSlot(Guid unavailableSlotId)
        {
            var response = await FacilityPitchRepo.GetUnavailableSlot(unavailableSlotId);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }

        [HttpGet("GetSlot/{facilityPitchTimingId}")]
        public async Task<IActionResult> GetSlot(Guid facilityPitchTimingId)
        {
            var response = await FacilityPitchRepo.GetSlot(facilityPitchTimingId);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }

        [HttpPost("UpdateSlot")]
        public async Task<IActionResult> UpdateSlot([FromHeader] string Authorization, [FromBody] EditSlotViewModel viewModel)
        {
            var response = await FacilityPitchRepo.UpdateSlot(Authorization.Split(' ')[1], viewModel);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }

        [HttpPost("DeleteSlot")]
        public async Task<IActionResult> DeleteSlot([FromBody] ChangeStatus status)
        {
            var response = await FacilityPitchRepo.DeleteSlot(status);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }

        [HttpPost("DeleteUnavailableSlot")]
        public async Task<IActionResult> DeleteUnavailableSlot([FromBody] ChangeStatus status)
        {
            var response = await FacilityPitchRepo.DeleteUnavailableSlot(status);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }

        [HttpGet("GetFacilityPitchesByFacilityId/{facilityId}/{facilityPitchId}")]
        public async Task<IActionResult> GetFacilityPitchesByFacilityId(Guid facilityId, Guid facilityPitchId)
        {
            var response = await FacilityPitchRepo.GetFacilityPitchesByFacilityId(facilityId, facilityPitchId);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }
    }
}