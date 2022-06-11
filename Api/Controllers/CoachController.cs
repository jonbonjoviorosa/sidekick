using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sidekick.Api.DataAccessLayer.Interfaces;
using Sidekick.Api.Handlers.IBusiness;
using Sidekick.Api.Helpers.IHelpers;
using Sidekick.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sidekick.Api.Controllers
{
    [Authorize]
    [Produces("application/json")]
    [Route("api/Coach")]
    public class CoachController : APIBaseController
    {
        private readonly ICoachHandler coachHandler;
        private readonly IUserHelper userHelper;

        ICoachRepository CoachRepo { get; }

        public CoachController(ICoachRepository _cRepo, 
            ICoachHandler coachHandler,
            IUserHelper userHelper)
        {
            CoachRepo = _cRepo;
            this.coachHandler = coachHandler;
            this.userHelper = userHelper;
        }

        [HttpGet("GetCoaches")]
        public async Task<ActionResult<APIResponse<IEnumerable<CoachViewModel>>>> GetCoaches()
        {
            return Ok(await coachHandler.GetCoaches());
        }

        [HttpGet("GetPlayers/{id}")]
        public IActionResult GetPlayers(int id)
        {
            APIResponse apiResp = CoachRepo.GetPlayers(id);
            if (apiResp.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Ok(apiResp);
            }
            else
            {
                return BadRequest(apiResp);
            }
        }

        [HttpGet("SetPlayerToCoach/{id}")]
        public IActionResult SetPlayerToCoach([FromHeader] string Authorization, int id)
        {
            APIResponse apiResp = CoachRepo.SetPlayerToCoach(Authorization.Split(' ')[1], id);
            if (apiResp.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Ok(apiResp);
            }
            else
            {
                return BadRequest(apiResp);
            }
        }

        [HttpGet("GetCoach")]
        public async Task<ActionResult<APIResponse<CoachInfoViewModel>>> GetCoache()
        {
            var userId = userHelper.GetCurrentUserGuidLogin();
            return Ok(await coachHandler.GetCoache(userId));

        }

        [HttpPost("BecomeACoach")]
        public async Task<ActionResult<APIResponse>> BecomeACoach([FromBody] CoachUpdateProfile updateProf)
        {
            APIResponse apiResp = await coachHandler.BecomeACoach(updateProf);
            if (apiResp.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Ok(apiResp);
            }
            else
            {
                return BadRequest(apiResp);
            }
        }

        [HttpGet("CoachSchedule")]
        public async Task<ActionResult<APIResponse<CoachScheduleViewModel>>> GetCoachSchedule()
        {
            var userId = userHelper.GetCurrentUserGuidLogin();
            return Ok(await coachHandler.GetCoachSchedule(userId));
        }

        [HttpGet("CoachSchedule/{userId}")]
        public async Task<ActionResult<APIResponse<CoachScheduleViewModel>>> GetCoachSchedule(Guid userId)
        {
            return Ok(await coachHandler.GetCoachSchedule(userId));
        }

        [HttpPost("UpdateCoachSchedule")]
        public async Task<ActionResult<APIResponse>> UpdateCoachSchedule([FromBody]CoachScheduleViewModel schedule)
        {
            return Ok(await coachHandler.UpdateCoachSchedule(schedule));
        }

        [HttpGet("GetTrainingBookings")]
        public async Task<ActionResult<APIResponse>> GetTrainingBookings()
        {
            return Ok(await coachHandler.GetTrainingBookings());
        }

        [HttpGet("GetCoachProfile")]
        public async Task<IActionResult> GetCoachProfile()
        {
            var userId = userHelper.GetCurrentUserGuidLogin();
            return Ok(await coachHandler.GetCoacheProfile(userId));
        }

        [HttpGet("GetCoachProfileView")]
        public async Task<IActionResult> GetCoachProfileView(Guid CoachId)
        {
            APIResponse apiResp = await CoachRepo.GetCoachProfileView(CoachId);
            if (apiResp.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Ok(apiResp);
            }
            else
            {
                return BadRequest(apiResp);
            }
        }

        [HttpPost("EditCoachProfile")]
        public IActionResult EditCoachProfile([FromBody] CoachEditProfileFormModel _coachEdit)
        {
            APIResponse apiResp = coachHandler.EditCoachProfile(_coachEdit);
            if (apiResp.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Ok(apiResp);
            }
            else
            {
                return BadRequest(apiResp);
            }
        }

        [HttpGet("GetMyCoachingGroupList")]
        public async Task<IActionResult> GetMyCoachingGroupList(string title)
        {
            var userId = userHelper.GetCurrentUserGuidLogin();
            return Ok(await coachHandler.GetMyCoachingGroupList(userId, title));
        }

        [HttpGet("GetMyIndividualGroupList")]
        public async Task<IActionResult> GetMyIndividualGroupList(string name)
        {
            var userId = userHelper.GetCurrentUserGuidLogin();
            return Ok(await coachHandler.GetMyIndividualGroupList(userId, name));
        }

        [HttpGet("GetCoachAbsentSlotList")]
        public async Task<IActionResult> GetCoachAbsentSlot()
        {
            var userId = userHelper.GetCurrentUserGuidLogin();
            return Ok(await coachHandler.GetCoachAbsentSlot(userId));
        }

        [HttpGet("GetCoachAbsentSlotDetails")]
        public async Task<ActionResult<APIResponse>> GetCoachAbsentSlotDetails(int id)
        {
            return Ok(await coachHandler.GetCoachAbsentSlotDetails(id));
        }

        [HttpPost("AddCoachAbsentSlot")]
        public async Task<ActionResult<APIResponse>> AddCoachAbsentSlot([FromBody] CoachCoachAbsentSlotViewModel schedule)
        {

            return Ok(await coachHandler.UpdateCoachAbsentSlot(schedule, 0));
        }

        [HttpPost("UpdateCoachAbsentSlot")]
        public async Task<ActionResult<APIResponse>> UpdateCoachAbsentSlot([FromBody] CoachCoachAbsentSlotViewModel schedule, int id)
        {
            return Ok(await coachHandler.UpdateCoachAbsentSlot(schedule,id));
        }

        [HttpDelete("DeleteCoachAbsentSlot")]
        public async Task<ActionResult<APIResponse>> DeleteCoachAbsentSlot(int id)
        {
            return Ok(await coachHandler.DeleteCoachAbsentSlot(id));
        }

        [HttpGet("GetCoachInsightActivities")]
        public async Task<ActionResult<APIResponse>> GetCoachInsightActivities(int searchType)
        {
            var userId = userHelper.GetCurrentUserGuidLogin();
            return Ok(await coachHandler.GetCoachInsightActivities(searchType, userId));
        }

        [HttpGet("GetCoachCustomer")]
        public async Task<IActionResult> GetCoachCustomer(string name)
        {
            var userId = userHelper.GetCurrentUserGuidLogin();
            return Ok(await coachHandler.GetCoachCustomer(userId, name));
        }

        [HttpGet("GetCoachConfirmation")]
        public async Task<IActionResult> GetCoachConfirmation(ReviewType type, int id)
        {
            return Ok(await CoachRepo.GetCoachConfirmation(type, id));
        }

        [HttpPost("CancelCoachConfirmation")]
        public async Task<ActionResult<APIResponse>> CancelCoachConfirmation(ReviewType type, int id)
        {
            APIResponse apiResp = await CoachRepo.GetCoachConfirmation(type, id);
            if (apiResp.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Ok(apiResp);
            }
            else
            {
                return BadRequest(apiResp);
            }
        }

        [HttpGet("GetCoachHome")]
        public async Task<IActionResult> GetCoachHome(string? dateFrom, string? dateTo)
        {
            APIResponse apiResp = await CoachRepo.GetCoachHome(dateFrom, dateTo);
            if (apiResp.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Ok(apiResp);
            }
            else
            {
                return BadRequest(apiResp);
            }
        }


        [HttpGet("GetCoachHomeDetail")]
        public async Task<IActionResult> GetCoachHomeDetail(ReviewType type, Guid BookingId)
        {
            APIResponse apiResp = await CoachRepo.GetCoachHomeDetail(type,BookingId);
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
