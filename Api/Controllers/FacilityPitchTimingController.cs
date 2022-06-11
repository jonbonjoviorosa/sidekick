using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sidekick.Api.DataAccessLayer.Interfaces;
using System;
using System.Threading.Tasks;

namespace Sidekick.Api.Controllers
{
    [Authorize]
    [Produces("application/json")]
    [Route("api/FacilityPitchTiming")]
    public class FacilityPitchTimingController : APIBaseController
    {
        private readonly IFacilityPitchTimingRepository _facilityPitchTimingRepo;

        public FacilityPitchTimingController(IFacilityPitchTimingRepository facilityPitchTimingRepo)
        {
            _facilityPitchTimingRepo = facilityPitchTimingRepo;
        }

        [HttpGet("Get/{facilityPitchId}")]
        public async Task<IActionResult> GetFacilityPitchTiming(Guid facilityPitchId)
        {
            return Ok(await _facilityPitchTimingRepo.GetFacilityPitchTimings(facilityPitchId));
        }

        [HttpGet("GetAll/{facilityId}")]
        public async Task<IActionResult> GetAll(Guid facilityId)
        {
            return Ok(await _facilityPitchTimingRepo.GetAll(facilityId));
        }

        [HttpGet("GetAllWithBooking/{facilityId}")]
        public async Task<IActionResult> GetAllWithBooking(Guid facilityId)
        {
            return Ok(await _facilityPitchTimingRepo.GetAllWithBooking(facilityId));
        }

        [HttpGet("GetTiming/{facilityPitchTimingId}")]
        public async Task<IActionResult> GetTiming(Guid facilityPitchTimingId)
        {
            return Ok(await _facilityPitchTimingRepo.GetBookingDetails(facilityPitchTimingId));
        }

    }
}
