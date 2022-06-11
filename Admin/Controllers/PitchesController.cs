using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Sidekick.Admin.DataLayer;
using Sidekick.Admin.Helpers;
using Sidekick.Model;
using Sidekick.Model.Play;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sidekick.Admin.Controllers
{
    public class PitchesController : BaseController
    {
        private IMainHttpClient MainHTTPClient { get; }
        public FacilityUserContext FacilityUCtxt { get; set; }
        private ConfigMaster MConf { get; }
        private IHttpContextAccessor _httpCtxtAcc;
        private ISession _session => _httpCtxtAcc.HttpContext.Session;

        public PitchesController(IMainHttpClient _mhttpc, IHttpContextAccessor httpContextAccessor, ConfigMaster _conf)
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
                var facilityId = FacilityUCtxt.FacilityUserInfo.FacilityId;
                var pitchResponse = JsonConvert.DeserializeObject<APIResponse>(MainHTTPClient.GetHttpClientRequest("FacilityPitch/Get/" + facilityId));
                if (!IsTokenInvalidUsingResponse(pitchResponse, "Unathorized access."))
                {
                    var pitches = pitchResponse.Payload != null ? JsonConvert.DeserializeObject<IEnumerable<FacilityPitchList>>(pitchResponse.Payload.ToString()) : new List<FacilityPitchList>();

                    ViewBag.Current = "Pitches";
                    return View(pitches);
                }
            }

            ViewBag.Current = "Pitches";
            return View();
        }

        [HttpGet]
        public IActionResult FacilityPitchDetail(string facilityPitchId)
        {
            if (!string.IsNullOrWhiteSpace(facilityPitchId.ToString()))
            {
                var Ids = facilityPitchId.Split(";");
                var pitchDetailResponse = new APIResponse();
                var facilityId = FacilityUCtxt.FacilityUserInfo.FacilityId;
                if (Ids.Count() > 3)
                {
                    pitchDetailResponse = JsonConvert.DeserializeObject<APIResponse>(MainHTTPClient.GetHttpClientRequest($"FacilityPitch/Get/{facilityId}/{Ids[0]}/{Ids[1]}/{Ids[2]}/{Ids[3]}"));
                }
                else
                {
                    pitchDetailResponse = JsonConvert.DeserializeObject<APIResponse>(MainHTTPClient.GetHttpClientRequest($"FacilityPitch/Get/{facilityId}/{Ids[0]}/{Ids[1]}/{Ids[2]}"));
                }
                if (!IsTokenInvalidUsingResponse(pitchDetailResponse, "Unathorized access."))
                {
                    var pitchDetail = JsonConvert.DeserializeObject<FacilityPitchVM>(pitchDetailResponse.Payload.ToString());
                    var sportResponse = JsonConvert.DeserializeObject<APIResponse>(MainHTTPClient.GetHttpClientRequest($"Sport/Get/{pitchDetail.SportId}"));
                    var sport = JsonConvert.DeserializeObject<IEnumerable<Sport>>(sportResponse.Payload.ToString());
                    if (pitchDetail.BookingPitchTimings != null && pitchDetail.BookingPitchTimings.Count > 0)
                    {
                        ViewBag.FacilityPitchTimingId = pitchDetail.Bookings[0].BookingId;
                    }
                    else
                    {
                        ViewBag.FacilityPitchTimingId = Guid.Empty;
                    }
                    ViewBag.PitchDetail = pitchDetail;
                    ViewBag.Sport = sport.Where(s => s.SportId == pitchDetail.SportId).FirstOrDefault().Name;
                    ViewBag.Current = "Pitches";
                    return View("ViewPitch", pitchDetail);
                }
            }

            return View();
        }


        [HttpGet]
        public IActionResult FacilityPitchBookingDetail(string bookingID)
        {
            if (!string.IsNullOrWhiteSpace(bookingID.ToString()))
            {
                var pitchDetailResponse = JsonConvert.DeserializeObject<APIResponse>(MainHTTPClient.GetHttpClientRequest($"FacilityPitch/Get/BookingDetails/{bookingID}"));
                if (!IsTokenInvalidUsingResponse(pitchDetailResponse, "Unathorized access."))
                {
                    var pitchDetail = JsonConvert.DeserializeObject<PlayBookingModel>(pitchDetailResponse.Payload.ToString());
                    if (pitchDetail != null)
                    {
                        ViewBag.FacilityPitchTimingId = pitchDetail.BookingId;
                    }
                    else
                    {
                        ViewBag.FacilityPitchTimingId = Guid.Empty;
                    }
                    ViewBag.PitchDetail = pitchDetail;
                    ViewBag.Sport = pitchDetail.SportName;
                    ViewBag.Current = "Pitches";
                    return View("ViewPitch", pitchDetail);
                }
            }

            return View();
        }

        [HttpGet]
        public IActionResult GetFacilityPitchTiming()
        {
            var facilityPitchTimingId = Request.Query["id"];

            if (!string.IsNullOrEmpty(facilityPitchTimingId))
            {
                ViewBag.FacilityPitchTimingId = facilityPitchTimingId;
            }
            else
            {
                ViewBag.FacilityPitchTimingId = Guid.Empty;
            }

            var facilityPitchTimingResponse = JsonConvert.DeserializeObject<APIResponse>(MainHTTPClient.GetHttpClientRequest($"FacilityPitchTiming/GetTiming/{facilityPitchTimingId}"));
            return Json(facilityPitchTimingResponse);
        }

        [HttpGet]
        public IActionResult CancelBooking()
        {
            var facilityPitchTimingId = Request.Query["id"];
            var facilityPitchTimingResponse = JsonConvert.DeserializeObject<APIResponse>(MainHTTPClient.GetHttpClientRequest($"Booking/CancelSlotBooking/{new Guid(facilityPitchTimingId)}"));
            return Json(facilityPitchTimingResponse);
        }

        [HttpPost]
        public IActionResult SendMessageToAllplayers([FromBody] FacilitySendContactMessageToPlayerRequestModel sendEmailBody)
        {
            var emailResponse = JsonConvert.DeserializeObject<APIResponse>(MainHTTPClient.PostHttpClientRequest($"Play/SendContactMessageToPlayer", sendEmailBody));
            return Json(emailResponse);
        }

        public IActionResult ViewPitch()
        {
            ViewBag.Current = "Pitches";
            return View();
        }
    }
}
