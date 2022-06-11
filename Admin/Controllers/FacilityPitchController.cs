using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Sidekick.Admin.DataLayer;
using Sidekick.Admin.Helpers;
using Sidekick.Model;
using Sidekick.Model.SetupConfiguration.Size;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sidekick.Admin.Controllers
{
    public class FacilityPitchController : BaseController
    {
        private IMainHttpClient MainHTTPClient { get; }
        public FacilityUserContext FacilityUCtxt { get; set; }
        private ConfigMaster MConf { get; }
        private readonly IHttpContextAccessor _httpCtxtAcc;
        private ISession _session => _httpCtxtAcc.HttpContext.Session;

        public FacilityPitchController(IMainHttpClient _mhttpc, IHttpContextAccessor httpContextAccessor, ConfigMaster _conf)
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
                Guid facilityId = FacilityUCtxt.FacilityUserInfo.FacilityId;
                APIResponse returnRes = JsonConvert.DeserializeObject<APIResponse>(MainHTTPClient.GetHttpClientRequest("FacilityPitch/Get/" + facilityId));
                ViewBag.TimeSlotsAM = PopulateTimeSlots("00:00", "23:30");
                ViewBag.TimeSlotsPM = PopulateTimeSlots("00:00", "23:30");
                if (!IsTokenInvalidUsingResponse(returnRes, "Unathorized access."))
                {
                    IEnumerable<FacilityPitchList> returnList = returnRes.Payload != null ?  JsonConvert.DeserializeObject<IEnumerable<FacilityPitchList>>(returnRes.Payload.ToString()) : new List<FacilityPitchList>();

                    ViewBag.Current = "Settings";
                    return View(returnList);
                }
            }
            return RedirectToAction("Logout", "Home");
        }

        [HttpGet]
        public IActionResult Add()
        {
            if (IsFCUserLoggedIn(FacilityUCtxt))
            {
                Guid facilityId = FacilityUCtxt.FacilityUserInfo.FacilityId;

                ViewBag.TimeSlotsAM = PopulateTimeSlots("00:00", "23:30");
                ViewBag.TimeSlotsPM = PopulateTimeSlots("00:00", "23:30");

                APIResponse returnProfile = JsonConvert.DeserializeObject<APIResponse>(MainHTTPClient.GetHttpClientRequest("Facility/Profile/" + facilityId));
                var result = JsonConvert.DeserializeObject<FacilityProfile>(returnProfile.Payload.ToString());
                //populate facility sports
                APIResponse returnRes = JsonConvert.DeserializeObject<APIResponse>(MainHTTPClient.GetHttpClientRequest("FacilityPitch/PitchSports/" + facilityId));
                ViewBag.Sports = JsonConvert.DeserializeObject<List<SportDto>>(returnRes.Payload.ToString());

                APIResponse getSurfaces = JsonConvert.DeserializeObject<APIResponse>(MainHTTPClient.GetHttpClientRequest("Surface/Get"));
                ViewBag.Surfaces = JsonConvert.DeserializeObject<List<Surface>>(getSurfaces.Payload.ToString());

                APIResponse getSizes = JsonConvert.DeserializeObject<APIResponse>(MainHTTPClient.GetHttpClientRequest("Size/Get"));
                ViewBag.Sizes = JsonConvert.DeserializeObject<List<TeamSize>>(getSizes.Payload.ToString());

                var location = JsonConvert.DeserializeObject<APIResponse>(MainHTTPClient.GetHttpClientRequest("Location/Get"));
                ViewBag.Locations = JsonConvert.DeserializeObject<List<Location>>(location.Payload.ToString());

                var facilityPitch = new FacilityPitchVM
                {
                    FacilityPitchTimings = new List<FacilityPitchTiming>()
                };
                facilityPitch.FacilityPitchTimings.Add(new FacilityPitchTiming());
                if (result.IsEveryday)
                {
                    facilityPitch.FacilityPitchTimings.FirstOrDefault().TimeStart = result.FacilityHours.TimeStart;
                    facilityPitch.FacilityPitchTimings.FirstOrDefault().TimeEnd = result.FacilityHours.TimeEnd;
                }
                ViewBag.Current = "Settings";
                return View(facilityPitch); 
            }
            return RedirectToAction("Logout", "Home");
        }

        public IActionResult AddTimingSlot()
        {
            ViewBag.TimeSlotsAM = PopulateTimeSlots("00:00", "23:30");
            ViewBag.TimeSlotsPM = PopulateTimeSlots("00:00", "23:30");
            return PartialView("~/Views/FacilityPitch/PartialViews/_SlotTiming.cshtml", new FacilityPitchTiming());
        }


        public IActionResult Edit(string facilityPitchId, string sportId)
        {
            if (IsFCUserLoggedIn(FacilityUCtxt))
            {
                Guid facilityId = FacilityUCtxt.FacilityUserInfo.FacilityId;
                APIResponse returnRes = JsonConvert.DeserializeObject<APIResponse>(MainHTTPClient.GetHttpClientRequest("FacilityPitch/Get/" + facilityId + "/" + new Guid(facilityPitchId) + "/" + sportId));

                if (!IsTokenInvalidUsingResponse(returnRes, "Unathorized access."))
                {
                    var returnList = JsonConvert.DeserializeObject<FacilityPitchVM>(returnRes.Payload.ToString());

                    ViewBag.TimeSlotsAM = PopulateTimeSlots("00:00", "23:30");
                    ViewBag.TimeSlotsPM = PopulateTimeSlots("00:00", "23:30");

                    //populate facility sports
                    APIResponse sport = JsonConvert.DeserializeObject<APIResponse>(MainHTTPClient.GetHttpClientRequest("FacilityPitch/PitchSports/" + facilityId));
                    var sports = JsonConvert.DeserializeObject<List<SportDto>>(sport.Payload.ToString());
                    var facilityPitchesResponse = JsonConvert.DeserializeObject<APIResponse>(MainHTTPClient.GetHttpClientRequest("FacilityPitch/GetFacilityPitchesByFacilityId/" + facilityId +"/"+ facilityPitchId));
                    var facilityPitches = JsonConvert.DeserializeObject<List<FacilityPitch>>(facilityPitchesResponse.Payload.ToString());
                    var newSportList = new List<SportDto>();
                    foreach (var item in sports)
                    {
                        var sportHasAlreadyPitch = facilityPitches.Where(f => f.SportId == item.SportId).ToList();
                        if (!sportHasAlreadyPitch.Any())
                        {
                            newSportList.Add(item);
                        }

                        if (item.SportId == returnList.SportId)
                        {
                            newSportList.Add(item);
                        }
                    }

                    ViewBag.Sports = newSportList;

                    APIResponse getSurfaces = JsonConvert.DeserializeObject<APIResponse>(MainHTTPClient.GetHttpClientRequest("Surface/Get"));
                    ViewBag.Surfaces = JsonConvert.DeserializeObject<List<Surface>>(getSurfaces.Payload.ToString());

                    APIResponse getSizes = JsonConvert.DeserializeObject<APIResponse>(MainHTTPClient.GetHttpClientRequest("Size/Get"));
                    ViewBag.Sizes = JsonConvert.DeserializeObject<List<TeamSize>>(getSizes.Payload.ToString());

                    var location = JsonConvert.DeserializeObject<APIResponse>(MainHTTPClient.GetHttpClientRequest("Location/Get"));
                    ViewBag.Locations = JsonConvert.DeserializeObject<List<Location>>(location.Payload.ToString());

                    //var pitchTimings = JsonConvert.DeserializeObject<APIResponse>(MainHTTPClient.GetHttpClientRequest($"FacilityPitchTiming/Get/{facilityPitchId}"));
                    ViewBag.Timings = returnList.FacilityPitchTimings;

                    ViewBag.Current = "Settings";
                    return View("Add", returnList);
                }
            }
            return RedirectToAction("Logout", "Home");
        }

        [HttpPost]
        public IActionResult Add([FromBody]FacilityPitchVM _pitch)
        {
            if (ModelState.IsValid)
            {

                //_pitch.FacilityPitchTimings = JsonConvert.DeserializeObject<List<FacilityPitchTimings>>(_facility.SelectedSports.ToString());
                if(_pitch.FacilityId == null)
                {
                    _pitch.FacilityId = FacilityUCtxt.FacilityUserInfo.FacilityId;
                }
                
                var returnRes = JsonConvert.DeserializeObject<APIResponse>(MainHTTPClient.PostHttpClientRequest("FacilityPitch/Add", _pitch));

                // If token is invalid, redirect to login
                if (base.IsTokenInvalidUsingResponse(returnRes, "Unathorized access.")) { return RedirectToAction("Logout", "Home"); }

                ViewBag.ModalTitle = returnRes.Message;
                if (returnRes.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    ViewBag.ShowModal = "true";

                    ViewBag.TimeSlotsAM = PopulateTimeSlots("00:00", "23:30");
                    ViewBag.TimeSlotsPM = PopulateTimeSlots("00:00", "23:30");

                    //populate facility sports
                    APIResponse result = JsonConvert.DeserializeObject<APIResponse>(MainHTTPClient.GetHttpClientRequest("FacilityPitch/PitchSports/" + _pitch.FacilityId));
                    ViewBag.Sports = JsonConvert.DeserializeObject<List<SportDto>>(result.Payload.ToString());

                    APIResponse getSurfaces = JsonConvert.DeserializeObject<APIResponse>(MainHTTPClient.GetHttpClientRequest("Surface/Get"));
                    ViewBag.Surfaces = JsonConvert.DeserializeObject<List<Surface>>(getSurfaces.Payload.ToString());

                    APIResponse getSizes = JsonConvert.DeserializeObject<APIResponse>(MainHTTPClient.GetHttpClientRequest("Size/Get"));
                    ViewBag.Sizes = JsonConvert.DeserializeObject<List<TeamSize>>(getSizes.Payload.ToString());

                    var location = JsonConvert.DeserializeObject<APIResponse>(MainHTTPClient.GetHttpClientRequest("Location/Get"));
                    ViewBag.Locations = JsonConvert.DeserializeObject<List<Location>>(location.Payload.ToString());

                    ViewBag.Current = "Settings";
                    return Json(new APIResponse { StatusCode = System.Net.HttpStatusCode.OK, Message = returnRes.Message,  Payload = returnRes.Payload });
                }

                ViewBag.ShowModal = "false";
                return Json(new APIResponse { StatusCode = System.Net.HttpStatusCode.BadRequest, Message = returnRes.Message, ModelError = returnRes.ModelError });
            }

            return Json(new APIResponse { StatusCode = System.Net.HttpStatusCode.BadRequest, Message = "Invalid Object!", ModelError = ModelState.Errors() });
        }
    }
}
