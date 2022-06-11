using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Sidekick.Admin.DataLayer;
using Sidekick.Admin.Helpers;
using Sidekick.Model;
using System.Collections.Generic;
using System.Linq;

namespace Sidekick.Admin.Controllers
{
    public class CalendarController : BaseController
    {
        private IMainHttpClient MainHTTPClient { get; }
        public FacilityUserContext FacilityUCtxt { get; set; }
        private ConfigMaster MConf { get; }
        private IHttpContextAccessor _httpCtxtAcc;
        private ISession _session => _httpCtxtAcc.HttpContext.Session;
        public CalendarController(IMainHttpClient _mhttpc, IHttpContextAccessor httpContextAccessor, ConfigMaster _conf)
        {
            MainHTTPClient = _mhttpc;
            _httpCtxtAcc = httpContextAccessor;
            MConf = _conf;
            FacilityUCtxt = _session.GetObjectFromJson<FacilityUserContext>("facilityUserContext");
        }
        public IActionResult Index()
        {
            if (IsFCUserLoggedIn(FacilityUCtxt))
            {
                ViewBag.TimeSlotsAM = PopulateTimeSlots("00:00", "23:30");
                ViewBag.TimeSlotsPM = PopulateTimeSlots("00:00", "23:30");
                var facilityId = FacilityUCtxt.FacilityUserInfo.FacilityId;
                var response = JsonConvert.DeserializeObject<APIResponse>(MainHTTPClient.GetHttpClientRequest("Play/GetFacilityPitchBooking/" + facilityId));
                var pitchResponse = JsonConvert.DeserializeObject<APIResponse>(MainHTTPClient.GetHttpClientRequest("FacilityPitch/Get/" + facilityId));
                var blockedSlotsResponse = JsonConvert.DeserializeObject<APIResponse>(MainHTTPClient.GetHttpClientRequest("FacilityPitch/GetBlockedSlots/" + facilityId));
                //var facilityPitchTimingResponse = JsonConvert.DeserializeObject<APIResponse>(MainHTTPClient.GetHttpClientRequest("FacilityPitchTiming/GetAll/" + facilityId));
                var facilityPitchTimingResponse = JsonConvert.DeserializeObject<APIResponse>(MainHTTPClient.GetHttpClientRequest("FacilityPitchTiming/GetAllWithBooking/" + facilityId));
                if (!IsTokenInvalidUsingResponse(response, "Unathorized access."))
                {
                    var settings = new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Ignore,
                        MissingMemberHandling = MissingMemberHandling.Ignore
                    };
                    var userPitchBookings = JsonConvert.DeserializeObject<IEnumerable<UserPitchBooking>>(response.Payload.ToString(), settings);
                    var pitches = pitchResponse.Payload != null ? JsonConvert.DeserializeObject<IEnumerable<FacilityPitchList>>(pitchResponse.Payload.ToString(), settings) : new List<FacilityPitchList>();
                    var blockedSlots = blockedSlotsResponse.Payload != null ? JsonConvert.DeserializeObject<IEnumerable<UnavailableSlotViewModel>>(blockedSlotsResponse.Payload.ToString(), settings) : new List<UnavailableSlotViewModel>();
                    var facilityPitchTimings = facilityPitchTimingResponse.Payload != null ? JsonConvert.DeserializeObject<IEnumerable<TimingCalendarViewModel>>(facilityPitchTimingResponse.Payload.ToString(), settings) : new List<TimingCalendarViewModel>();
                    ViewBag.Pitches = pitches.Any() ? pitches.GroupBy(p => p.FacilityPitchId.Value).Select(y => y.First()).ToList() : new List<FacilityPitchList>();
                    ViewBag.Bookings = userPitchBookings;
                    ViewBag.UnavailableSlots = blockedSlots;
                    ViewBag.Timings = facilityPitchTimings;
                    ViewBag.Current = "Calendar";
                    return View();
                }
            }

            ViewBag.Current = "Calendar";
            return View();
        }

        [HttpPost]
        public IActionResult SaveUnavailableSlot([FromBody] UnavailableSlot blockSlot)
        {
            if (ModelState.IsValid)
            {
                var facilityId = FacilityUCtxt.FacilityUserInfo.FacilityId;
                blockSlot.FacilityId = facilityId;
                var response = JsonConvert.DeserializeObject<APIResponse>(MainHTTPClient.PostHttpClientRequest("FacilityPitch/AddOrUpdateBlockSlot", blockSlot));
                if (!IsTokenInvalidUsingResponse(response, "Unathorized access."))
                {
                    ViewBag.ShowModal = "true";
                    return Json(new APIResponse { StatusCode = System.Net.HttpStatusCode.OK, Message = response.Message, Payload = response.Payload });
                }
                ViewBag.ShowModal = "false";
            }
            return Json(new APIResponse { StatusCode = System.Net.HttpStatusCode.BadRequest, Message = "Invalid Object!", ModelError = ModelState.Errors() });
        }

        [HttpGet]
        public IActionResult GetSlot()
        {
            var id = Request.Query["id"];
            var slotResponse = JsonConvert.DeserializeObject<APIResponse>(MainHTTPClient.GetHttpClientRequest($"FacilityPitch/GetUnavailableSlot/{id}"));
            if (!IsTokenInvalidUsingResponse(slotResponse, "Unathorized access."))
            {
                var settings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    MissingMemberHandling = MissingMemberHandling.Ignore
                };
               
                return Json(new APIResponse { StatusCode = slotResponse.StatusCode, Message = slotResponse.Message, Payload = slotResponse.Payload });
            }
            return Ok();
        }

        [HttpPost]
        public IActionResult DeleteSlot([FromBody] ChangeStatus changeStatus)
        {
            if (IsFCUserLoggedIn(FacilityUCtxt))
            {
                var response = JsonConvert.DeserializeObject<APIResponse>(MainHTTPClient.PostHttpClientRequest("FacilityPitch/DeleteUnavailableSlot/", changeStatus));
                if (IsTokenInvalidUsingResponse(response, "Unathorized access.")) { return RedirectToAction("Logout", "Home"); }

                return Json(new APIResponse { StatusCode = System.Net.HttpStatusCode.OK, Message = response.Message, Payload = response.Payload });
            }

            return Json(new APIResponse { StatusCode = System.Net.HttpStatusCode.BadRequest, Message = "Invalid Object!", ModelError = ModelState.Errors() });
        }
    }
}
