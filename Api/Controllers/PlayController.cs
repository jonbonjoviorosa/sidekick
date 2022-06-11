using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sidekick.Api.DataAccessLayer.Interfaces;
using Sidekick.Api.Handlers.IBusiness;
using Sidekick.Api.Helpers;
using Sidekick.Api.ViewModel;
using Sidekick.Model;
using Sidekick.Model.Play;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sidekick.Api.Controllers
{
    [Authorize]
    [Produces("application/json")]
    [Route("api/Play")]
    public class PlayController : APIBaseController
    {
        private readonly IPlayHandler playHandler;

        IPlayRepository PlayRepo { get; }
        private IMainHttpClient MainHttpClient { get; }
        private APIConfigurationManager MConf { get; }

        public PlayController(IPlayRepository _uRepo,
            IPlayHandler playHandler,
            IMainHttpClient _mhttpc,
            APIConfigurationManager _conf)
        {
            PlayRepo = _uRepo;
            this.playHandler = playHandler;
            MainHttpClient = _mhttpc;
            MConf = _conf;
        }

        [HttpPost("FilterFacilities/{_sportId}")]
        public async Task<ActionResult<IEnumerable<PlayFacilitiesModel>>> FilterPlayFacilities([FromBody] IEnumerable<PlayFilterViewModel> filters, Guid _sportId, [FromQuery] string facilityName)
        {
            return Ok(await playHandler.FilterPlayFacilities(filters, _sportId, facilityName));
        }

        [HttpGet("Facilities/{_facilityId}/Sports/{_sportId}/BookDate/{bookDate}")]
        public async Task<ActionResult<IEnumerable<PlayFacilitiesModel>>> PlayFacilitiesGet(Guid _facilityId, DateTime bookDate, Guid _sportId)
        {
            return Ok(await playHandler.GetPlayFacilities(_facilityId, bookDate, _sportId));
        }

        [HttpGet("SearchFacilityByName/{_facilityName}")]
        public ActionResult<IEnumerable<PlayFacilitiesModel>> SearchFacilityByName(string _facilityName)
        {
            return Ok(playHandler.SearchFacilityByName(_facilityName));
        }

        [HttpPost("FreeGame")]
        public async Task<APIResponse> OrganizeFreeGame([FromBody] FreeGame freeGame)
        {
            return await playHandler.OrganizeFreeGame(freeGame);
        }

        [HttpPost("PlayRequest")]
        public async Task<APIResponse> PlayRequest([FromBody] PlayRequest playRequest)
        {
            return await playHandler.PlayRequest(playRequest, MainHttpClient, MConf);
        }

        [HttpGet("PlayRequest/Booking/{bookingId}")]
        public async Task<APIResponse<IEnumerable<PlayRequestViewModel>>> PlayRequestByBooking(Guid bookingId)
        {
            return await playHandler.GetPlayRequest(bookingId);
        }

        [HttpGet("InviteRequest/Booking/{bookingId}")]
        public async Task<APIResponse<InviteRequestResponseModel>> InviteRequestByBooking(Guid bookingId,[FromQuery] string search)
        {
            return await playHandler.GetInviteRequest(bookingId,search);
        }

        [HttpPost("InviteRequest/Booking/{bookingId}")]
        public async Task<APIResponse> SentInviteRequestByBooking([FromBody] List<string> userSids, Guid bookingId)
        {
            return await playHandler.SentInviteRequest(userSids, bookingId);
        }


        [HttpPost("FacilityPitchBooking")]
        public async Task<APIResponse> PitchBooking([FromBody] UserPitchBookingModel pitchBooking)
        {
            return await playHandler.PitchBooking(pitchBooking, MainHttpClient, MConf);
        }

        [HttpGet("GetFacilityPitchBooking/{facilityId}")]
        public async Task<APIResponse> GetPitchBookings(Guid facilityId)
        {
            return await playHandler.GetPitchBookings(facilityId);
        }

        [HttpGet("AllFilters")]
        public async Task<APIResponse<Filters>> GetAllFilters()
        {
            return await playHandler.GetAllFilters();
        }

        [HttpGet("GetAllFacilityPitchBookings")]
        public async Task<APIResponse> GetFacilityPitchBookings()
        {
            return await playHandler.GetAllPitchBookings();
        }

        [HttpGet("GetFacilityPitchBookingsDate/{dateFrom}/{dateTo}")]
        public async Task<APIResponse> GetFacilityPitchBookingsDate(string dateFrom, string dateTo)
        {
            return await playHandler.GetFacilityPitchBookingsDate(dateFrom, dateTo);
        }

        [HttpGet("GetPitchBooking/{bookingId}")]
        public async Task<APIResponse> GetPitchBooking(Guid bookingId)
        {
            return await playHandler.GetPitchBooking(bookingId);
        }


        [HttpPost("GetFacilityPitchBookingBySportId/{sportId}")]
        public async Task<APIResponse> GetFacilityPitchBookingBySportId([FromBody] IEnumerable<PlayFilterViewModel> filters, Guid sportId, [FromQuery] string facilityName)
        {
            return await playHandler.GetPitchBookingBySportId(filters, sportId, facilityName);
        }

        [HttpGet("Payment/{bookingId}")]
        public async Task<ActionResult<APIResponse<TelRResponseViewModel>>> Payment(Guid bookingId)
        {
            var apiResp = await playHandler.Payment(bookingId);

            if (apiResp.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Ok(apiResp);
            }
            else
            {
                return BadRequest(apiResp);
            }
        }

        [AllowAnonymous]
        [HttpGet("GetAllPitchBookingPriorToStartDate")]
        public async Task<ActionResult<APIResponse<IEnumerable<UserPitchBooking>>>> GetAllPitchBookingPriorToStartDate()
        {
            return Ok(await playHandler.GetAllPitchBookingPriorToStartDate());
        }

        [AllowAnonymous]
        [HttpGet("GetAllPitchBookingPrior48hrsToStartDate")]
        public async Task<ActionResult<APIResponse<IEnumerable<UserPitchBooking>>>> GetAllPitchBookingPrior48hrsToStartDate()
        {
            return Ok(await playHandler.GetAllPitchBookingPrior48hrsToStartDate());
        }

        // THIS ONE SHOULD CALL FROM THE SERVICE
        // AND ONCE THE PAYMENT FROM MOBILE HAS BEEN SUCCESSFUL
        [AllowAnonymous]
        [HttpGet("Payment/Capture/{forDeposit}/{bookingId}")]
        public async Task<ActionResult<APIResponse>> CapturePayment(bool forDeposit, Guid bookingId)
        {
            var apiResp = await playHandler.CapturePayment(forDeposit, bookingId);
            if (apiResp.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Ok(apiResp);
            }
            else
            {
                return BadRequest(apiResp);
            }
        }


        // THIS ONE SHOULD CALL FROM THE SERVICE
        // AND ONCE THE PAYMENT FROM MOBILE HAS BEEN SUCCESSFUL
        [AllowAnonymous]
        [HttpGet("PaymentAuthProcess/{bookingId}")]
        public async Task<ActionResult<APIResponse>> PaymentAuthProcess(Guid bookingId)
        {
            var apiResp = await playHandler.PaymentAuthProcess(bookingId);
            if (apiResp.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Ok(apiResp);
            }
            else
            {
                return BadRequest(apiResp);
            }
        }


        // THIS ONE SHOULD CALL FROM THE SERVICE
        // AND ONCE THE PAYMENT FROM MOBILE HAS BEEN SUCCESSFUL
        [AllowAnonymous]
        [HttpGet("PaymentCaptureProcess/{bookingId}")]
        public async Task<ActionResult<APIResponse>> PaymentCaptureProcess(Guid bookingId)
        {
            var apiResp = await playHandler.PaymentCaptureProcess(bookingId);
            if (apiResp.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Ok(apiResp);
            }
            else
            {
                return BadRequest(apiResp);
            }
        }



        [AllowAnonymous]
        [HttpGet("Payment/Refund/{bookingId}")]
        public async Task<ActionResult<APIResponse>> RefundPayment(Guid bookingId)
        {
            var apiResp = await playHandler.RefundPayment(bookingId);
            if (apiResp.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Ok(apiResp);
            }
            else
            {
                return BadRequest(apiResp);
            }
        }

        //[HttpGet("Payment/Remaining/{facilityId}")]
        //public async Task<ActionResult<APIResponse<TelRResponseViewModel>>> RemainingPayment(Guid facilityId)
        //{
        //    var apiResp = await playHandler.RemainingPayment(facilityId);
        //    if (apiResp.StatusCode == System.Net.HttpStatusCode.OK)
        //    {
        //        return Ok(apiResp);
        //    }
        //    else
        //    {
        //        return BadRequest(apiResp);
        //    }
        //}

        [HttpGet("mybooking")]
        public async Task<APIResponse> GetPlayBookingsForProfile()
        {
            return await playHandler.MyPlayBooking();
        }

        [HttpPost("SendContactMessageToPlayer")]
        public async Task<APIResponse> GetPlayBookingsForProfile([FromBody] FacilitySendContactMessageToPlayerRequestModel messageToPlayerRequestModel)
        {
            return await playHandler.FacilitySendContactMessageToPlayer(messageToPlayerRequestModel);
        }
    }
}
