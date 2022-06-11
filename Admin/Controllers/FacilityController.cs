using ClosedXML.Excel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using Sidekick.Admin.DataLayer;
using Sidekick.Admin.Extensions;
using Sidekick.Admin.Helpers;
using Sidekick.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Sidekick.Admin.Controllers
{
    public class FacilityController : BaseController
    {
        private IMainHttpClient MainHTTPClient { get; }

        public AdminUserContext AdminUCtxt { get; set; }
        public FacilityUserContext FacilityUCtxt { get; set; }

        private ConfigMaster MConf { get; }
        private readonly IHttpContextAccessor _httpCtxtAcc;
        private ISession _session => _httpCtxtAcc.HttpContext.Session;

        public FacilityController(IMainHttpClient _mhttpc, IHttpContextAccessor httpContextAccessor, ConfigMaster _conf)
        {
            MainHTTPClient = _mhttpc;
            _httpCtxtAcc = httpContextAccessor;
            MConf = _conf;
            AdminUCtxt = _session.GetObjectFromJson<AdminUserContext>("adminUserContext");
            FacilityUCtxt = _session.GetObjectFromJson<FacilityUserContext>("facilityUserContext");
        }

        public IActionResult Dashboard()
        {
            ViewBag.Current = "Dashboard";
            var viewModel = new List<SlotRenderViewModel>();
            if (IsFCUserLoggedIn(FacilityUCtxt))
            {
                var facilityId = FacilityUCtxt.FacilityUserInfo.FacilityId;
                var response = JsonConvert.DeserializeObject<APIResponse>(MainHTTPClient.GetHttpClientRequest("Play/GetFacilityPitchBooking/" + facilityId));
                var pitchResponse = JsonConvert.DeserializeObject<APIResponse>(MainHTTPClient.GetHttpClientRequest("FacilityPitch/Get/" + facilityId));
                var facilityPlayerResponse = JsonConvert.DeserializeObject<APIResponse>(MainHTTPClient.GetHttpClientRequest("FacilityPlayer/Get/" + facilityId));
                if (!IsTokenInvalidUsingResponse(response, "Unathorized access."))
                {
                    var settings = new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Ignore,
                        MissingMemberHandling = MissingMemberHandling.Ignore
                    };
                    var userPitchBookings = JsonConvert.DeserializeObject<IEnumerable<UserPitchBooking>>(response.Payload.ToString(), settings);
                    var pitches = pitchResponse.Payload == null ? new List<FacilityPitchList>() : JsonConvert.DeserializeObject<IEnumerable<FacilityPitchList>>(pitchResponse.Payload.ToString(), settings);
                    ViewBag.BookedPitchToday = pitches.Where(p => p.CreatedDate.Value.ToShortDateString() == DateTime.Now.ToShortDateString()).Count();
                    //if (userPitchBookings.Any())
                    //{
                    //    userPitchBookings = userPitchBookings.OrderBy(u => u.CreatedDate).ToList();
                    //    foreach (var booking in userPitchBookings)
                    //    {
                    //        var pitch = pitches.Any() ? pitches.Where(p => p.FacilityPitchId == booking.FacilityPitchId).ToList() : new List<FacilityPitchList>();
                    //        if (pitch.Count != 0)
                    //        {
                    //            var vm = new FacilityDashboardViewModel
                    //            {
                    //                PitchName = pitch != null ? pitch.FirstOrDefault().Name : string.Empty,
                    //                PitchStart = booking.PitchStart,
                    //                PitchEnd = booking.PitchEnd,
                    //                BookingDate = booking.Date
                    //            };

                    //            viewModel.Add(vm);
                    //        }
                    //    }
                    //}


                    ViewBag.Pitches = pitches;
                    

                    var facilityPlayers = facilityPlayerResponse.Payload != null ? JsonConvert.DeserializeObject<IEnumerable<FacilityPlayerViewModel>>(facilityPlayerResponse.Payload.ToString(), settings) : new List<FacilityPlayerViewModel>();
                    ViewBag.FacilityPlayers = facilityPlayers.Any() ? facilityPlayers.OrderByDescending(f => f.CreatedDate).ToList() : new List<FacilityPlayerViewModel>();
                    var dashboardVm = new DasboardViewModel();
                    dashboardVm.Players = facilityPlayers;
                    dashboardVm.FacilityPitches = pitches;
                    dashboardVm.Bookings = userPitchBookings;
                    var slotRender = new List<SlotRenderViewModel>();

                    //foreach (var pitch in pitches)
                    //{
                    //    var Ids = pitch.FacilityPitchTimingIds.Split(";");
                    //    if (Ids.Any())
                    //    {
                    //        foreach (var Id in Ids)
                    //        {
                    //            var timing = pitch.FacilityPitchTimings.Where(f => f.FacilityPitchTimingId.ToString() == Id).FirstOrDefault();
                    //            if(timing != null)
                    //            {
                    //                int PlayerCount = 0;
                    //                if (timing.PlayerIds != null)
                    //                { 
                    //                    var playerIds = timing.PlayerIds.Split(";");
                    //                    PlayerCount = playerIds.Count();
                    //                }
                    //                slotRender.Add(new SlotRenderViewModel
                    //                {
                    //                    PitchStart = timing.TimeStart,
                    //                    PitchEnd = timing.TimeEnd,
                    //                    MaxPlayers = pitch.MaxPlayers,
                    //                    PlayerCount = PlayerCount,
                    //                    PitchName = pitch.Name,
                    //                    PitchDate = GetNextDate(timing.Day, DateTime.Now),
                    //                });
                    //            }
                    //        }

                    //    }
                    //}


                    var x = userPitchBookings.GroupBy(f => new
                    {
                        BoookingID = f.BookingId,
                        PitchStart = f.PitchStart.ToShortTimeString(),
                        PitchEnd = f.PitchEnd.ToShortTimeString(),
                        Date = f.Date,
                        FacilityID = f.FacilityId,
                        FacilityPitchID = f.FacilityPitchId,
                        FacilityPitchTimingID= f.FacilityPitchTimingId,
                        Price = f.PricePerUser,
                        SportID = f.SportId,
                        PitchName = pitches.Any() ? pitches.Where(p => p.FacilityPitchId == f.FacilityPitchId).FirstOrDefault().Name : string.Empty,
                        MaxPlayers = pitches.Any() ? pitches.Where(p => p.FacilityPitchId == f.FacilityPitchId).FirstOrDefault().MaxPlayers : default,
                    }).ToDictionary(f => f.Key, f => f.ToList()).ToList();

                    foreach (var booking in x)
                    {
                        var slot = new SlotRenderViewModel();
                        var a = PopulatePitchTimingRecord(booking, slot, facilityPlayers);
                        slotRender.Add(a);
                    }

                    dashboardVm.MappedBookings = slotRender;
                    dashboardVm.MappedBookings = dashboardVm.MappedBookings.Count != 0 ? dashboardVm.MappedBookings.OrderBy(m => m.PitchStart).ToList() : new List<SlotRenderViewModel>();
                    
                    ViewBag.FacilityPitchesData = AddedGroupLastSevenDaysStats(dashboardVm, "facilityPitch");
                    ViewBag.FacilityPlayersData =  AddedGroupLastSevenDaysStats(dashboardVm, "facilityPlayers");
                    ViewBag.UserBookings = AddedGroupLastSevenDaysStats(dashboardVm, "bookings");

                    return View(slotRender);
                }
            }

            return View(viewModel);
        }

        private DateTime GetNextDate(DayOfWeek day, DateTime now)
        {
            if (day == now.DayOfWeek)
            {
                return now;
            }

            DateTime result = DateTime.Now.AddDays(1);
            while (result.DayOfWeek != day)
                result = result.AddDays(1);
            return result;
        }

        private static SlotRenderViewModel PopulatePitchTimingRecord<T>(KeyValuePair<T, List<UserPitchBooking>> item, SlotRenderViewModel model, IEnumerable<FacilityPlayerViewModel> player)
        {
            var getType = item.Key.GetType();
            IList<PropertyInfo> props = new List<PropertyInfo>(getType.GetProperties());
            foreach (var prop in props)
            {
                object property = prop.Name;
                object value = prop.GetValue(item.Key, null);
                if (property.ToString() == "PitchStart")
                    model.PitchStart = DateTime.Parse(value.ToString());
                if (property.ToString() == "PitchEnd")
                    model.PitchEnd = DateTime.Parse(value.ToString());
                if (property.ToString() == "Date")
                    model.PitchDate = (DateTime)value;
                if (property.ToString() == "Price")
                    model.Price = decimal.Parse(value.ToString());
                if (property.ToString() == "PitchName")
                    model.PitchName = (string)value;
                if (property.ToString() == "MaxPlayers")
                    model.MaxPlayers = (int)value;
                if (property.ToString() == "FacilityID")
                    model.FacilityID = (Guid)value;
                if (property.ToString() == "FacilityPitchID")
                    model.FacilityPitchID = (Guid)value;
                if (property.ToString() == "FacilityPitchTimingID")
                    model.FacilityPitchTimingID = (Guid)value;
                if (property.ToString() == "SportID")
                    model.SportID = (Guid)value;
                if (property.ToString() == "BoookingID")
                    model.BoookingID = (Guid)value;
            }
            model.Players = new List<PlayerPitchViewModel>();
            foreach (var value in item.Value)
            {
                var user = player.Where(p => p.UserId == value.UserId).FirstOrDefault();
                if (user != null)
                {
                    model.Players.Add(new PlayerPitchViewModel
                    {
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Email = user.Email,
                        IsPaid = user.IsPaid,
                        ProfileImgUrl = user.ProfileImgUrl
                    });
                }
            }
            model.GuidId = Guid.NewGuid();
            return model;
        }

        private static List<LastSevenDays> PopulateFacilityPlayerLastSevenDays(DasboardViewModel viewModel, DateTime lastSevenDays)
        {
            var filteredFacilityPlayerData = new List<FacilityPlayerViewModel>();
            foreach (var item in viewModel.Players)
            {
                if (item.CreatedDate >= lastSevenDays)
                {
                    filteredFacilityPlayerData.Add(item);
                }
            }
            viewModel.FacilityPlayerGroupLastSeven = new List<LastSevenDays>();
            if (filteredFacilityPlayerData.Any())
            {
                var facilityPlayerLastSeven = filteredFacilityPlayerData.GroupBy(f => new { Day = f.CreatedDate.Day, Year = f.CreatedDate.Year })
                                                                        .ToDictionary(g => g.Key, g => g.Count());
                foreach (var item in facilityPlayerLastSeven)
                {
                    viewModel.FacilityPlayerGroupLastSeven.Add(new LastSevenDays
                    {
                        Year = item.Key.Year,
                        Day = item.Key.Day,
                        ObjectCount = item.Value
                    });
                }
            }

            return viewModel.FacilityPlayerGroupLastSeven;
        }

        private static List<LastSevenDays> AddedGroupLastSevenDaysStats(DasboardViewModel viewModel, string identifier)
        {
            var lastSevenDays = DateTime.Now.Date.AddDays(-7);

            switch (identifier)
            {

                case "facilityPlayers":return PopulateFacilityPlayerLastSevenDays(viewModel, lastSevenDays);
                case "facilityPitch": return PopulateFacilityPitchesLastSevenDays(viewModel, lastSevenDays);
                default: return AddBookingsReportForLastSevenDays(viewModel, lastSevenDays);

            }
        }

        private static List<LastSevenDays> PopulateFacilityPitchesLastSevenDays(DasboardViewModel viewModel, DateTime lastSevenDays)
        {
            var filteredFacilityPitchesData = new List<FacilityPitchList>();
            foreach (var item in viewModel.FacilityPitches)
            {
                if (item.CreatedDate.Value >= lastSevenDays)
                {
                    filteredFacilityPitchesData.Add(item);
                }
            }
            viewModel.FacilityPitchesLastSeven = new List<LastSevenDays>();
            if (filteredFacilityPitchesData.Any())
            {
                var facilityPitchesLastSeven = filteredFacilityPitchesData.GroupBy(f => new { Day = f.CreatedDate.Value.Day, Year = f.CreatedDate.Value.Year })
                                                                        .ToDictionary(g => g.Key, g => g.Count());
                foreach (var item in facilityPitchesLastSeven)
                {
                    viewModel.FacilityPitchesLastSeven.Add(new LastSevenDays
                    {
                        Year = item.Key.Year,
                        Day = item.Key.Day,
                        ObjectCount = item.Value
                    });
                }
            }

            return viewModel.FacilityPitchesLastSeven;
        }
        private static List<LastSevenDays> AddBookingsReportForLastSevenDays(DasboardViewModel viewModel, DateTime lastSevenDays)
        {
            var filteredBookings = new List<UserPitchBooking>();
            foreach (var item in viewModel.Bookings)
            {
                if (item.CreatedDate.Value >= lastSevenDays)
                {
                    filteredBookings.Add(item);
                }
            }
            viewModel.BookingsGroupByLastSeven = new List<LastSevenDays>();
            if (filteredBookings.Any())
            {
                var bookingsGroup = filteredBookings.GroupBy(f => new { Day = f.CreatedDate.Value.Day, Year = f.CreatedDate.Value.Year })
                                                                        .ToDictionary(g => g.Key, g => g.Count());
                foreach (var item in bookingsGroup)
                {
                    viewModel.BookingsGroupByLastSeven.Add(new LastSevenDays
                    {
                        Year = item.Key.Year,
                        Day = item.Key.Day,
                        ObjectCount = item.Value
                    });
                }
            }

            return viewModel.BookingsGroupByLastSeven;
        }

        public IActionResult Index()
        {
            if (IsUserLoggedIn(AdminUCtxt))
            {
                APIResponse returnRes = JsonConvert.DeserializeObject<APIResponse>(MainHTTPClient.GetHttpClientRequest("Facility/AllFacilities"));

                if (!IsTokenInvalidUsingResponse(returnRes, "Unathorized access."))
                {
                    IEnumerable<FacilityList> returnList = JsonConvert.DeserializeObject<IEnumerable<FacilityList>>(returnRes.Payload.ToString());

                    ViewBag.Current = "User Management";
                    return View(returnList.Any() ? returnList.OrderByDescending(r => r.DateUpdated).ToList() : returnList);
                }
            }
            return RedirectToAction("Logout", "Home");
        }

        [HttpGet]
        public IActionResult Add()
        {
            if (IsUserLoggedIn(AdminUCtxt))
            {
                ViewBag.TimeSlotsAM = PopulateTimeSlots("00:00", "23:30");
                ViewBag.TimeSlotsPM = PopulateTimeSlots("00:00", "23:30");

                ViewBag.DefaultIcon = MConf.WebApiBaseUrl.Replace("api/", "") + "resources/Defaults/Facility_Default_Logo.jpg";

                //populate sports
                APIResponse returnRes = JsonConvert.DeserializeObject<APIResponse>(MainHTTPClient.GetHttpClientRequest("Sport/Get/0"));
                var areaResponse = JsonConvert.DeserializeObject<APIResponse>(MainHTTPClient.GetHttpClientRequest("Facility/GetAreas"));

                ViewBag.Sports = JsonConvert.DeserializeObject<List<SportDto>>(returnRes.Payload.ToString());
                ViewBag.Areas = JsonConvert.DeserializeObject<List<Area>>(areaResponse.Payload.ToString());
                ViewBag.IsEdit = false;
                var facilityProfile = new FacilityProfile();
                facilityProfile.FacilityTimings = new List<FacilityTiming>();
                facilityProfile.FacilityTimings.Add(new FacilityTiming());
                ViewBag.FacilityTimings = facilityProfile;

                ViewBag.Current = "User Management";

                return View(facilityProfile);
            }
            return RedirectToAction("Logout", "Home");
        }

        [HttpPost]
        public IActionResult Add([FromBody] FacilityProfile _facility)
        {
            if (ModelState.IsValid)
            {
                APIResponse returnRes = JsonConvert.DeserializeObject<APIResponse>(MainHTTPClient.PostHttpClientRequest("Facility/Add", _facility));

                APIResponse returnSports = JsonConvert.DeserializeObject<APIResponse>(MainHTTPClient.GetHttpClientRequest("Sport/Get/0"));
                ViewBag.Sports = JsonConvert.DeserializeObject<List<SportDto>>(returnSports.Payload.ToString());
                var areaResponse = JsonConvert.DeserializeObject<APIResponse>(MainHTTPClient.GetHttpClientRequest("Facility/GetAreas"));
                ViewBag.Areas = JsonConvert.DeserializeObject<List<Area>>(areaResponse.Payload.ToString());
                // If token is invalid, redirect to login
                if (base.IsTokenInvalidUsingResponse(returnRes, "Unathorized access.")) { return RedirectToAction("Logout", "Home"); }

                ViewBag.ModalTitle = returnRes.StatusCode == System.Net.HttpStatusCode.OK ? returnRes.Status : returnRes.Status;
                ViewBag.ModalMessage = returnRes.StatusCode == System.Net.HttpStatusCode.OK ? returnRes.Message : returnRes.Message;
                ViewBag.ShowModal = returnRes.StatusCode == System.Net.HttpStatusCode.OK ? "true" : "false";

                ViewBag.TimeSlotsAM = PopulateTimeSlots("00:00", "23:30");
                ViewBag.TimeSlotsPM = PopulateTimeSlots("00:00", "23:30");

                //returnProfile.StatusCode == System.Net.HttpStatusCode.BadRequest ? new FacilityProfile() : JsonConvert.DeserializeObject<FacilityProfile>(returnProfile.Payload.ToString());

                //return Json(new APIResponse { StatusCode = returnProfile.StatusCode, ModelError = returnProfile.ModelError, Payload = returnProfile.Payload });

                return Json(new APIResponse { StatusCode = returnRes.StatusCode, Message = returnRes.Message, Payload = returnRes.Payload, ModelError = returnRes.ModelError });
            }
            else
            {
                var customErrors = ModelState.Values.SelectMany(x => x.Errors);
                return Json(new APIResponse { StatusCode = System.Net.HttpStatusCode.BadRequest, Message = customErrors.FirstOrDefault().ErrorMessage, ModelError = ModelState.Errors() });
            }
        }

        public IActionResult Edit(Guid u)
        {
            if (IsUserLoggedIn(AdminUCtxt))
            {
                ViewBag.TimeSlotsAM = PopulateTimeSlots("00:00", "23:30");
                ViewBag.TimeSlotsPM = PopulateTimeSlots("00:00", "23:30");

                ViewBag.DefaultIcon = MConf.WebApiBaseUrl.Replace("api/", "") + "resources/Defaults/Facility_Default_Logo.jpg";

                //populate sports
                APIResponse returnRes = JsonConvert.DeserializeObject<APIResponse>(MainHTTPClient.GetHttpClientRequest("Sport/Get/0"));
                APIResponse returnProfile = JsonConvert.DeserializeObject<APIResponse>(MainHTTPClient.GetHttpClientRequest("Facility/Profile/" + u));
                var areaResponse = JsonConvert.DeserializeObject<APIResponse>(MainHTTPClient.GetHttpClientRequest("Facility/GetAreas"));

                ViewBag.Sports = JsonConvert.DeserializeObject<List<SportDto>>(returnRes.Payload.ToString());
                ViewBag.Areas = JsonConvert.DeserializeObject<List<Area>>(areaResponse.Payload.ToString());
                ViewBag.IsEdit = true;
                var facilityTimings = new List<FacilityTiming>();
                facilityTimings.Add(new FacilityTiming());
                ViewBag.FacilityTimings = facilityTimings;

                if (!IsTokenInvalidUsingResponse(returnProfile, "Unathorized access."))
                {
                    FacilityProfile result = JsonConvert.DeserializeObject<FacilityProfile>(returnProfile.Payload.ToString());
                    ViewBag.Current = "Settings";
                    return View("Add", result);
                }
            }
            return RedirectToAction("Logout", "Home");
        }

        public IActionResult AddDaySlot()
        {
            ViewBag.TimeSlotsAM = PopulateTimeSlots("00:00", "23:30");
            ViewBag.TimeSlotsPM = PopulateTimeSlots("00:00", "23:30");

            return PartialView("~/Views/Facility/PartialViews/_AddDay.cshtml", new FacilityTiming() { Day = DayOfWeek.Sunday});
        }

        public IActionResult UpdateProfile()
        {
            if (IsFCUserLoggedIn(FacilityUCtxt))
            {
                ViewBag.TimeSlotsAM = PopulateTimeSlots("00:00", "23:30");
                ViewBag.TimeSlotsPM = PopulateTimeSlots("00:00", "23:30");

                ViewBag.DefaultIcon = MConf.WebApiBaseUrl.Replace("api/", "") + "resources/Defaults/Facility_Default_Logo.jpg";

                //populate sports
                APIResponse returnRes = JsonConvert.DeserializeObject<APIResponse>(MainHTTPClient.GetHttpClientRequest("Sport/Get/0"));
                ViewBag.Sports = returnRes.Payload != null ? JsonConvert.DeserializeObject<List<SportDto>>(returnRes.Payload.ToString()) : new List<SportDto>();

                Guid facilityId = FacilityUCtxt.FacilityUserInfo.FacilityId;
                APIResponse returnProfile = JsonConvert.DeserializeObject<APIResponse>(MainHTTPClient.GetHttpClientRequest("Facility/Profile/" + facilityId));

                if (!IsTokenInvalidUsingResponse(returnProfile, "Unathorized access."))
                {
                    FacilityProfile result = returnProfile.Payload != null ? JsonConvert.DeserializeObject<FacilityProfile>(returnProfile.Payload.ToString()) : new FacilityProfile(); ;
                    ViewBag.Current = "Settings";
                    return View(result);
                }
            }
            return RedirectToAction("Logout", "Home");
        }

        [HttpPost]
        public IActionResult Status([FromBody] ChangeStatus _facility)
        {
            if (IsUserLoggedIn(AdminUCtxt))
            {
                APIResponse returnRes = JsonConvert.DeserializeObject<APIResponse>(MainHTTPClient.PostHttpClientRequest("Facility/ChangeStatus", _facility));

                if (base.IsTokenInvalidUsingResponse(returnRes, "Unathorized access.")) { return RedirectToAction("Logout", "Home"); }

                return Json(returnRes);
            }
            return RedirectToAction("Logout", "Home");
        }
        [HttpPost]
        public IActionResult UpdateFacilityProfile([FromBody] FacilityProfile facilityProfile)
        {
            if (IsFCUserLoggedIn(FacilityUCtxt))
            {
                if (ModelState.IsValid)
                {
                    ViewBag.TimeSlotsAM = PopulateTimeSlots("00:00", "23:30");
                    ViewBag.TimeSlotsPM = PopulateTimeSlots("00:00", "23:30");

                    ViewBag.DefaultIcon = MConf.WebApiBaseUrl.Replace("api/", "") + "resources/Defaults/Facility_Default_Logo.jpg";
                    try
                    {
                        //populate sports
                        APIResponse returnRes = JsonConvert.DeserializeObject<APIResponse>(MainHTTPClient.GetHttpClientRequest("Sport/Get/0"));
                        ViewBag.Sports = JsonConvert.DeserializeObject<List<SportDto>>(returnRes.Payload.ToString());

                        facilityProfile.FacilityId = FacilityUCtxt.FacilityUserInfo.FacilityId;
                        facilityProfile.UserLoggedIn = FacilityUCtxt.FacilityUserInfo.FacilityUserId;
                        APIResponse returnProfile = JsonConvert.DeserializeObject<APIResponse>(MainHTTPClient.PostHttpClientRequest("Facility/Update/", facilityProfile));
                        ViewBag.ModalTitle = returnProfile.Message;
                        if (!IsTokenInvalidUsingResponse(returnProfile, "Unathorized access."))
                        {
                            ViewBag.ShowModal = "true";
                            ViewBag.Current = "Settings";
                            ViewBag.ModalTitle = returnProfile.StatusCode == System.Net.HttpStatusCode.OK ? returnProfile.Status : returnProfile.Status;
                            ViewBag.ModalMessage = returnProfile.StatusCode == System.Net.HttpStatusCode.OK ? returnProfile.Message : returnProfile.Message;
                            ViewBag.ShowModal = returnProfile.StatusCode == System.Net.HttpStatusCode.OK ? "true" : "false";

                            FacilityProfile result = returnProfile.StatusCode == System.Net.HttpStatusCode.BadRequest ? new FacilityProfile() : JsonConvert.DeserializeObject<FacilityProfile>(returnProfile.Payload.ToString());

                            return Json(new APIResponse { StatusCode = returnProfile.StatusCode, ModelError = returnProfile.ModelError, Payload = returnProfile.Payload });
                        }
                    }
                    catch (Exception)
                    {

                    }
                }

                var errorMessage = ModelState.Values.FirstOrDefault().Errors;
                String[] test = new String[] { "test" };
                var modelError = new List<KeyValuePair<string, string[]>>
                {
                    new KeyValuePair<string, string[]>(ModelState.Keys.FirstOrDefault(), test)
                };
                return Json(new APIResponse { StatusCode = System.Net.HttpStatusCode.BadRequest, Message = errorMessage.FirstOrDefault().ErrorMessage, ModelError = modelError });
              
            }
            return RedirectToAction("Logout", "Home");
        }

        [HttpGet]
        public ActionResult GetTimings()
        {
            return Json(new[] { PopulateTimeSlots("00:00", "23:30"), PopulateTimeSlots("00:00", "23:30") });
        }
        public IActionResult PitchManagement()
        {
            ViewBag.Current = "Settings";
            return View();
        }

        public IActionResult NewPitch()
        {
            ViewBag.Current = "Settings";
            return View();
        }

        public IActionResult PaymentHistory()
        {
            var playPaymentHistory = new List<PlayPaymentHistory>();
            if (IsFCUserLoggedIn(FacilityUCtxt))
            {
                var response = JsonConvert.DeserializeObject<APIResponse>(MainHTTPClient.GetHttpClientRequest("Payment/GetPlayPaymentHistory/"));
                if (!IsTokenInvalidUsingResponse(response, "Unathorized access."))
                {
                    playPaymentHistory = response != null ? JsonConvert.DeserializeObject<List<PlayPaymentHistory>>(response.Payload.ToString()) : playPaymentHistory;
                }
            }

            ViewBag.Current = "Settings";
            return View(playPaymentHistory);
        }

        public IActionResult ExportPlayPaymentHistory()
        {
            if (IsFCUserLoggedIn(FacilityUCtxt))
            {
                var response = JsonConvert.DeserializeObject<APIResponse>(MainHTTPClient.GetHttpClientRequest("Payment/GetPlayPaymentHistory/"));
                var playPaymentHistory = new List<PlayPaymentHistory>();

                if (!IsTokenInvalidUsingResponse(response, "Unathorized access."))
                {
                    playPaymentHistory = response != null ? JsonConvert.DeserializeObject<List<PlayPaymentHistory>>(response.Payload.ToString()) : playPaymentHistory;
                }

                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Play Payment Reports");
                    var currentRow = 1;
                    worksheet.Cell(currentRow, 1).Value = "BookingId";
                    worksheet.Cell(currentRow, 1).Style.Fill.BackgroundColor = XLColor.Yellow;
                    worksheet.Cell(currentRow, 2).Value = "Pitch No";
                    worksheet.Cell(currentRow, 2).Style.Fill.BackgroundColor = XLColor.Yellow;
                    worksheet.Cell(currentRow, 3).Value = "Pitch Name";
                    worksheet.Cell(currentRow, 3).Style.Fill.BackgroundColor = XLColor.Yellow;
                    worksheet.Cell(currentRow, 4).Value = "Date Booked";
                    worksheet.Cell(currentRow, 4).Style.Fill.BackgroundColor = XLColor.Yellow;
                    worksheet.Cell(currentRow, 5).Value = "Date Played";
                    worksheet.Cell(currentRow, 5).Style.Fill.BackgroundColor = XLColor.Yellow;
                    worksheet.Cell(currentRow, 6).Value = "Slot Played";
                    worksheet.Cell(currentRow, 6).Style.Fill.BackgroundColor = XLColor.Yellow;
                    worksheet.Cell(currentRow, 7).Value = "Total (Including VAT)";
                    worksheet.Cell(currentRow, 7).Style.Fill.BackgroundColor = XLColor.Yellow;
                    worksheet.Cell(currentRow, 8).Value = "Total (Excluding VAT)";
                    worksheet.Cell(currentRow, 8).Style.Fill.BackgroundColor = XLColor.Yellow;
                    worksheet.Cell(currentRow, 9).Value = "Status";
                    worksheet.Cell(currentRow, 9).Style.Fill.BackgroundColor = XLColor.Yellow;

                    foreach (var paymentHistory in playPaymentHistory)
                    {
                        currentRow++;
                        worksheet.Cell(currentRow, 1).Value = paymentHistory.BookingId;
                        worksheet.Cell(currentRow, 2).Value = paymentHistory.PitchNo;
                        worksheet.Cell(currentRow, 3).Value = paymentHistory.PitchName;
                        worksheet.Cell(currentRow, 4).Value = paymentHistory.DateBooked.ToString("dd/MM/yyyy");


                        worksheet.Cell(currentRow, 5).Value = paymentHistory.DatePlayed.ToString("dd/MM/yyyy");
                        worksheet.Cell(currentRow, 6).Value = paymentHistory.SlotPlayed;
                        worksheet.Cell(currentRow, 7).Value = paymentHistory.TotalIncludingVat;
                        worksheet.Cell(currentRow, 8).Value = paymentHistory.TotalExcludingVat;
                        if (paymentHistory.Status == PaymentStatus.Paid)
                        {
                            worksheet.Cell(currentRow, 9).Value = "Paid";
                            worksheet.Cell(currentRow, 9).Style.Fill.BackgroundColor = XLColor.LightGreen;
                        }
                        else if(paymentHistory.Status == PaymentStatus.Cancelled)
                        {
                            worksheet.Cell(currentRow, 9).Value = "Cancelled";
                            worksheet.Cell(currentRow, 9).Style.Fill.BackgroundColor = XLColor.LightGray;
                        }
                        else if(paymentHistory.Status == PaymentStatus.Pending)
                        {
                            worksheet.Cell(currentRow, 9).Value = "Pending";
                            worksheet.Cell(currentRow, 9).Style.Fill.BackgroundColor = XLColor.OrangePeel;
                        }
                        else if (paymentHistory.Status == PaymentStatus.Failed)
                        {
                            worksheet.Cell(currentRow, 9).Value = "Failed";
                            worksheet.Cell(currentRow, 9).Style.Fill.BackgroundColor = XLColor.Red;
                        }
                    }

                    using var stream = new MemoryStream();
                    worksheet.Columns().AdjustToContents();
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();

                    return File(
                        content,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        $"Play Payment Reports - {DateTime.Now}.xlsx");
                }
            }

            return View();

        }

        public IActionResult Export()
        {
            if (IsUserLoggedIn(AdminUCtxt))
            {
                APIResponse returnRes = JsonConvert.DeserializeObject<APIResponse>(MainHTTPClient.GetHttpClientRequest("Facility/AllFacilities"));
                IEnumerable<FacilityList> returnList = new List<FacilityList>();

                if (!IsTokenInvalidUsingResponse(returnRes, "Unathorized access."))
                {
                    returnList = JsonConvert.DeserializeObject<IEnumerable<FacilityList>>(returnRes.Payload.ToString());
                }

                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Facilities Reports");
                    var currentRow = 1;
                    worksheet.Cell(currentRow, 1).Value = "Facility";
                    worksheet.Cell(currentRow, 1).Style.Fill.BackgroundColor = XLColor.Yellow;
                    worksheet.Cell(currentRow, 2).Value = "Email";
                    worksheet.Cell(currentRow, 2).Style.Fill.BackgroundColor = XLColor.Yellow;
                    worksheet.Cell(currentRow, 3).Value = "Location";
                    worksheet.Cell(currentRow, 3).Style.Fill.BackgroundColor = XLColor.Yellow;
                    worksheet.Cell(currentRow, 4).Value = "Date";
                    worksheet.Cell(currentRow, 4).Style.Fill.BackgroundColor = XLColor.Yellow;
                    worksheet.Cell(currentRow, 5).Value = "Number of Pitches";
                    worksheet.Cell(currentRow, 5).Style.Fill.BackgroundColor = XLColor.Yellow;
                    worksheet.Cell(currentRow, 6).Value = "Booking Commission";
                    worksheet.Cell(currentRow, 6).Style.Fill.BackgroundColor = XLColor.Yellow;
                    worksheet.Cell(currentRow, 7).Value = "Status";
                    worksheet.Cell(currentRow, 7).Style.Fill.BackgroundColor = XLColor.Yellow;

                    foreach (var facility in returnList)
                    {
                        currentRow++;
                        worksheet.Cell(currentRow, 1).Value = facility.Name;
                        worksheet.Cell(currentRow, 2).Value = facility.Email;
                        worksheet.Cell(currentRow, 3).Value = facility.Location;
                            worksheet.Cell(currentRow, 4).Value = facility.CreatedDate.GetValueOrDefault().ToString("dd/MM/yyyy");


                        worksheet.Cell(currentRow, 5).Value = facility.PitchNo;
                        worksheet.Cell(currentRow, 6).Value = facility.Commission;
                        if (facility.IsEnabled.Value)
                        {
                            worksheet.Cell(currentRow, 7).Value = "Active";
                            worksheet.Cell(currentRow, 7).Style.Fill.BackgroundColor = XLColor.LightGreen;
                        }
                        else
                        {
                            worksheet.Cell(currentRow, 7).Value = "Deleted";
                            worksheet.Cell(currentRow, 7).Style.Fill.BackgroundColor = XLColor.LightGray;
                        }
                    }

                    using var stream = new MemoryStream();
                    worksheet.Columns().AdjustToContents();
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();

                    return File(
                        content,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        $"Facilities - {DateTime.Now}.xlsx");
                }
            }

            return View();

        }

        public IActionResult Reports(string dateFrom, string dateTo)
        {
            var viewModel = new DasboardViewModel();
            if (IsFCUserLoggedIn(FacilityUCtxt))
            {
                var facilityPlayersResponse = new APIResponse();
                var bookedPitchesResponse = new APIResponse();

                if (dateFrom != null && dateTo != null)
                {
                    var range = dateFrom + "/" + dateTo;
                    facilityPlayersResponse = JsonConvert.DeserializeObject<APIResponse>(MainHTTPClient.GetHttpClientRequest("FacilityPlayer/AllDate/" + dateFrom + "/" + dateTo));
                    bookedPitchesResponse = JsonConvert.DeserializeObject<APIResponse>(MainHTTPClient.GetHttpClientRequest("Play/GetFacilityPitchBookingsDate/" + dateFrom + "/" + dateTo));
                }
                else
                {
                    facilityPlayersResponse = JsonConvert.DeserializeObject<APIResponse>(MainHTTPClient.GetHttpClientRequest("FacilityPlayer/All/"));
                    bookedPitchesResponse = JsonConvert.DeserializeObject<APIResponse>(MainHTTPClient.GetHttpClientRequest("Play/GetAllFacilityPitchBookings/"));
                }

                if (!IsTokenInvalidUsingResponse(facilityPlayersResponse, "Unathorized access.")
                    && !IsTokenInvalidUsingResponse(bookedPitchesResponse, "Unathorized access."))
                {
                    var facilityPlayers = facilityPlayersResponse != null ? JsonConvert.DeserializeObject<IEnumerable<FacilityPlayer>>(facilityPlayersResponse.Payload.ToString()) : new List<FacilityPlayer>();
                    var pitchBookings = bookedPitchesResponse != null ? JsonConvert.DeserializeObject<IEnumerable<UserPitchBooking>>(bookedPitchesResponse.Payload.ToString()) : new List<UserPitchBooking>();

                    viewModel.FacilityPlayers = facilityPlayers;
                    viewModel.Bookings = pitchBookings;

                    var dateRange = new List<SelectListItem>();
                    var dates = viewModel.FacilityPlayers.GroupBy(f => new { Date = f.DateCreated.Value.ToString("mm/dd/yyyy") }).ToDictionary(g => g.Key, g => g.Count());
                    foreach (var item in dates)
                    {
                        dateRange.Add(new SelectListItem
                        {
                            Text = item.Key.ToString(),
                            Value = item.Key.ToString()
                        });
                    }

                    AddedGroupYearStats(viewModel);
                    AddBookingsReportForYear(viewModel);

                    ViewBag.Current = "Settings";
                    return View(viewModel);
                }
            }
            ViewBag.Current = "Settings";
            return View();
        }

        [HttpGet]
        public IActionResult GetNotifications()
        {
            var facilityId = Request.Query["id"];
            var response = JsonConvert.DeserializeObject<APIResponse>(MainHTTPClient.GetHttpClientRequest($"UserNotification/GetNotifications/{facilityId}"));
            return Json(response);
        }

        private static void AddedGroupYearStats(DasboardViewModel viewModel)
        {
            var facilityPlayerGroupBy = viewModel.FacilityPlayers.GroupBy(f => new { Month = f.DateCreated.Value.Month, Year = f.DateCreated.Value.Year }).ToDictionary(g => g.Key, g => g.Count());
            viewModel.FacilityPlayerGroup = new List<GroupYear>();
            foreach (var item in facilityPlayerGroupBy)
            {
                var model = item.PopulateThisYearStats(new GroupYear());
                viewModel.FacilityPlayerGroup.Add(model);
            }
        }

        private static void AddBookingsReportForYear(DasboardViewModel viewModel)
        {
            var bookingsGroup = viewModel.Bookings.GroupBy(f => new { Month = f.CreatedDate.Value.Month, Year = f.CreatedDate.Value.Year }).ToDictionary(g => g.Key, g => g.Count());
            viewModel.BookingsGroupByYear = new List<GroupYear>();
            foreach (var item in bookingsGroup)
            {
                var model = item.PopulateThisYearStats(new GroupYear());
                viewModel.BookingsGroupByYear.Add(model);
            }
        }
    }
}
