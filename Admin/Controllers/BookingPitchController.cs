using ClosedXML.Excel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using Sidekick.Admin.DataLayer;
using Sidekick.Admin.Helpers;
using Sidekick.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Sidekick.Admin.Controllers
{
    public class BookingPitchController : BaseController
    {
        private IMainHttpClient AdminHttpClient { get; }
        public AdminUserContext AdminUCtxt { get; set; }
        public FacilityUserContext FacilityUCtxt { get; set; }
        private ConfigMaster MConf { get; }
        private readonly IHttpContextAccessor _httpCtxtAcc;
        private ISession _session => _httpCtxtAcc.HttpContext.Session;
        public BookingPitchController(IMainHttpClient _mhttpc,
            IHttpContextAccessor httpContextAccessor,
            ConfigMaster _conf)
        {
            AdminHttpClient = _mhttpc;
            _httpCtxtAcc = httpContextAccessor;
            MConf = _conf;
            AdminUCtxt = _session.GetObjectFromJson<AdminUserContext>("adminUserContext");
            FacilityUCtxt = _session.GetObjectFromJson<FacilityUserContext>("facilityUserContext");
        }
        public IActionResult Index()
        {
            var SlotViewModel = new SlotViewModel();
            var PitchTimings = new List<AddSlotViewModel>();
            var PitchBookings = new List<AddSlotViewModel>();
            if (IsUserLoggedIn(AdminUCtxt))
            {
                var response = JsonConvert.DeserializeObject<APIResponse>(AdminHttpClient.GetHttpClientRequest("FacilityPitch/GetAllFacilityPitchTiming/"));
                if (!IsTokenInvalidUsingResponse(response, "Unathorized access."))
                {
                    PitchTimings = response.Payload != null ? JsonConvert.DeserializeObject<List<AddSlotViewModel>>(response.Payload.ToString()) : new List<AddSlotViewModel>();
                }
                var responseBooking = JsonConvert.DeserializeObject<APIResponse>(AdminHttpClient.GetHttpClientRequest("FacilityPitch/GetAllFacilityPitchBooking/"));
                if (!IsTokenInvalidUsingResponse(responseBooking, "Unathorized access."))
                {
                    PitchBookings = responseBooking.Payload != null ? JsonConvert.DeserializeObject<List<AddSlotViewModel>>(responseBooking.Payload.ToString()) : new List<AddSlotViewModel>();
                }
                SlotViewModel.PitchTimings = PitchTimings.Count != 0 ? PitchTimings.OrderByDescending(p => p.DateUpdated).ToList() : PitchTimings;
                SlotViewModel.PitchBookings = PitchBookings.Count != 0 ? PitchBookings.OrderByDescending(p => p.DateUpdated).ToList() : PitchBookings;
            }
            ViewBag.Current = "Event Management";
            return View(SlotViewModel);
        }

        public IActionResult AddSlot()
        {
            if (IsUserLoggedIn(AdminUCtxt))
            {
                ViewBag.TimeSlotsAM = PopulateTimeSlots("00:00", "23:30");
                ViewBag.TimeSlotsPM = PopulateTimeSlots("00:00", "23:30");
                var facilityResponse = JsonConvert.DeserializeObject<APIResponse>(AdminHttpClient.GetHttpClientRequest("Facility/AllFacilities/"));
                var sportsResponse = JsonConvert.DeserializeObject<APIResponse>(AdminHttpClient.GetHttpClientRequest("Sport/Get/"));
                var facilityPlayerResponse = JsonConvert.DeserializeObject<APIResponse>(AdminHttpClient.GetHttpClientRequest("FacilityPlayer/All/"));
                //var facilityPitchResponse = 
                if (!IsTokenInvalidUsingResponse(facilityResponse, "Unathorized access.")
                    && !IsTokenInvalidUsingResponse(sportsResponse, "Unathorized access.")
                    && !IsTokenInvalidUsingResponse(facilityPlayerResponse, "Unauthorized access."))
                {
                    var facilities = facilityResponse != null ? JsonConvert.DeserializeObject<IEnumerable<Facility>>(facilityResponse.Payload.ToString()) : new List<Facility>();
                    var sports = JsonConvert.DeserializeObject<IEnumerable<Sport>>(sportsResponse.Payload.ToString());
                    var facilityPlayers = facilityPlayerResponse != null ? JsonConvert.DeserializeObject<IEnumerable<FacilityPlayer>>(facilityPlayerResponse.Payload.ToString()) : new List<FacilityPlayer>();
                    var facilitiesList = new List<SelectListItem>();
                    if (facilities.Any())
                    {
                        foreach (var facility in facilities)
                        {
                            facilitiesList.Add(new SelectListItem
                            {
                                Value = facility.FacilityId.ToString(),
                                Text = facility.Name
                            });
                        }
                    }

                    var facilityPlayerList = new List<SelectListItem>();
                    if (facilityPlayers.Any())
                    {
                        foreach (var facilityPlayer in facilityPlayers.Where(f => f.FacilityId == Guid.Empty).ToList())
                        {
                            facilityPlayerList.Add(new SelectListItem
                            {
                                Value = facilityPlayer.UserId.ToString(),
                                Text = $"{facilityPlayer.FirstName} {facilityPlayer.LastName}"
                            });
                        }
                    }

                    var sportLists = new List<SelectListItem>();
                    if (sports.Any())
                    {
                        foreach (var sport in sports)
                        {
                            sportLists.Add(new SelectListItem
                            {
                                Value = sport.SportId.ToString(),
                                Text = sport.Name
                            });
                        }
                    }
                    ViewBag.Sports = sportLists;
                    ViewBag.Facilities = facilitiesList;
                    ViewBag.FacilityPlayers = facilityPlayerList;
                    var facilityPitchViewModel = new AddSlotViewModel();
                    facilityPitchViewModel.FacilityPlayers = new List<FacilityPlayer>
                    {
                        new FacilityPlayer()
                    };
                    return View(facilityPitchViewModel);

                }
            }
            ViewBag.Current = "Event Management";
            return View();
        }

        public IActionResult AddPlayer()
        {
            ViewBag.TimeSlotsAM = PopulateTimeSlots("00:00", "23:30");
            ViewBag.TimeSlotsPM = PopulateTimeSlots("00:00", "23:30");
            var facilityPlayerResponse = JsonConvert.DeserializeObject<APIResponse>(AdminHttpClient.GetHttpClientRequest("FacilityPlayer/All/"));
            var facilityPlayers = facilityPlayerResponse != null ? JsonConvert.DeserializeObject<IEnumerable<FacilityPlayer>>(facilityPlayerResponse.Payload.ToString()) : new List<FacilityPlayer>();
            var facilityPlayerList = new List<SelectListItem>();
            if (facilityPlayers.Any())
            {
                foreach (var facilityPlayer in facilityPlayers.Where(f => f.FacilityId == Guid.Empty).ToList())
                {
                    facilityPlayerList.Add(new SelectListItem
                    {
                        Value = facilityPlayer.UserId.ToString(),
                        Text = $"{facilityPlayer.FirstName} {facilityPlayer.LastName}"
                    });
                }
            }

            ViewBag.FacilityPlayers = facilityPlayerList;
            return PartialView("~/Views/BookingPitch/PartialViews/_AddPlayer.cshtml", new FacilityPlayer());
        }

        [HttpPost]
        public IActionResult AddFacilityPitchSlot([FromBody] AddSlotViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                if (viewModel.FacilityPitchId == Guid.Empty)
                {
                    return Json(new APIResponse { StatusCode = System.Net.HttpStatusCode.BadRequest, Message = "Need to create a Facility Pitch first" });
                }

                var facilityPitchVm = new FacilityPitchVM()
                {
                    FacilityId = viewModel.FacilityId,
                    FacilityPitchId = viewModel.FacilityPitchId,
                    Description = viewModel.Description,
                    MaxPlayers = viewModel.MaxPlayers,
                    SportId = viewModel.SportId,
                    FacilityPitchTimings = new List<FacilityPitchTiming>()
                {
                    new FacilityPitchTiming
                    {
                        Date = viewModel.Date,
                        FacilityPitchId = viewModel.FacilityPitchId,
                        TimeStart = viewModel.Start,
                        TimeEnd = viewModel.End,
                        IsRepeatEveryWeek = viewModel.IsRepeatEveryWeek,
                        IsFree = viewModel.IsFree,
                        CustomPrice = viewModel.TotalPrice,
                        PlayerIds = viewModel.PlayerIds,
                        Day = viewModel.Day
                    }
                }
                };

                var response = JsonConvert.DeserializeObject<APIResponse>(AdminHttpClient.PostHttpClientRequest("FacilityPitch/CreateSlot", facilityPitchVm));
                if (IsTokenInvalidUsingResponse(response, "Unathorized access.")) { return RedirectToAction("Logout", "Home"); }

                return Json(new APIResponse { StatusCode = response.StatusCode, Message = response.Message, Payload = response.Payload });
            }
            else
            {
                return Json(new APIResponse { StatusCode = System.Net.HttpStatusCode.BadRequest, Message = "Please complete all fields.", ModelError = ModelState.Errors() });
            }
        }

        [HttpGet]
        public IActionResult GetCommissionPrice()
        {
            var sportId = Request.Query["id"];
            var commissionPlayResponse = JsonConvert.DeserializeObject<APIResponse>(AdminHttpClient.GetHttpClientRequest("Commission/GetComissionPlays/"));
            var commissionPlays = commissionPlayResponse != null ? JsonConvert.DeserializeObject<IEnumerable<CommissionPlaySportViewModel>>(commissionPlayResponse.Payload.ToString()) : new List<CommissionPlaySportViewModel>();
            var commissionPerSport = commissionPlays.Where(c => c.SportId.ToString() == sportId).FirstOrDefault();
            return Json(commissionPerSport);
        }

        [HttpGet]
        public IActionResult GetFacilityPitches(Guid facilityId, Guid sportId)
        {
            //var facilityId = Request.Query["id"];
            var facilityPitches = new List<FacilityPitchList>();
            var facilityPitchResponse = JsonConvert.DeserializeObject<APIResponse>(AdminHttpClient.GetHttpClientRequest("FacilityPitch/Get/" + facilityId));
            if (!IsTokenInvalidUsingResponse(facilityPitchResponse, "Unathorized access."))
            {
                facilityPitches = facilityPitchResponse != null ? JsonConvert.DeserializeObject<List<FacilityPitchList>>(facilityPitchResponse.Payload.ToString()) : facilityPitches;
                if (facilityPitches.Any())
                {
                    var facilityPitchList = new List<SelectListItem>();
                    foreach (var facilityPitch in facilityPitches.Where(x => x.SportId == sportId))
                    {
                        facilityPitchList.Add(new SelectListItem
                        {
                            Text = facilityPitch.Name,
                            Value = facilityPitch.FacilityPitchId.ToString()
                        });
                    }

                    return Json(facilityPitchList);
                }

                return Json(facilityPitches);
            }

            return View();
        }

        [HttpGet]
        public IActionResult GetFacilityPitchSports(Guid facilityId)
        {
            //var facilityId = Request.Query["id"];
            var facilityPitches = new List<SportDto>();
            var facilityPitchResponse = JsonConvert.DeserializeObject<APIResponse>(AdminHttpClient.GetHttpClientRequest("FacilityPitch/PitchSports/" + facilityId));
            if (!IsTokenInvalidUsingResponse(facilityPitchResponse, "Unathorized access."))
            {
                facilityPitches = facilityPitchResponse != null ? JsonConvert.DeserializeObject<List<SportDto>>(facilityPitchResponse.Payload.ToString()) : facilityPitches;
                if (facilityPitches.Any())
                {
                    var facilityPitchList = new List<SelectListItem>();
                    foreach (var facilityPitch in facilityPitches)
                    {
                        facilityPitchList.Add(new SelectListItem
                        {
                            Text = facilityPitch.Name,
                            Value = facilityPitch.SportId.ToString()
                        });
                    }

                    return Json(facilityPitchList.GroupBy(f => f.Value).Select(s => s.First()).ToList());
                }

                return Json(facilityPitches);
            }

            return View();
        }


        [HttpGet]
        public IActionResult EditSlot(Guid u)
        {
            var vm = new EditSlotViewModel();
            var slot = JsonConvert.DeserializeObject<APIResponse>(AdminHttpClient.GetHttpClientRequest($"FacilityPitch/GetSlot/{u}"));

            var sportsResponse = JsonConvert.DeserializeObject<APIResponse>(AdminHttpClient.GetHttpClientRequest("Sport/Get/"));
            var sports = JsonConvert.DeserializeObject<IEnumerable<Sport>>(sportsResponse.Payload.ToString());

            vm = slot != null ? JsonConvert.DeserializeObject<EditSlotViewModel>(slot.Payload.ToString()) : vm;

            ViewBag.TimeSlotsAM = PopulateTimeSlots("00:00", "23:30");
            ViewBag.TimeSlotsPM = PopulateTimeSlots("00:00", "23:30");
            var facilityPitchList = new List<SelectListItem>();
            if (vm.FacilityPitches.Any())
            {
                var facilityPitchesByFacilities = vm.FacilityPitches.Where(f => f.FacilityId == vm.FacilityId).ToList();
                foreach (var facilityPitch in facilityPitchesByFacilities)
                {
                    facilityPitchList.Add(new SelectListItem
                    {
                        Text = facilityPitch.Name,
                        Value = facilityPitch.FacilityPitchId.ToString()
                    });
                }
            }

            var sportLists = new List<SelectListItem>();
            if (sports.Any())
            {
                foreach (var sport in sports)
                {
                    sportLists.Add(new SelectListItem
                    {
                        Value = sport.SportId.ToString(),
                        Text = sport.Name
                    });
                }
            }

            ViewBag.FacilityPitches = facilityPitchList.GroupBy(f => f.Value).Select(s => s.First()).ToList();
            ViewBag.Sports = sportLists;

            ViewBag.Current = "Edit Slot";
            return View(vm);
        }

        [HttpPost]
        public IActionResult UpdateFacilityPitchTiming(EditSlotViewModel viewModel)
        {
            var response = JsonConvert.DeserializeObject<APIResponse>(AdminHttpClient.PostHttpClientRequest("FacilityPitch/UpdateSlot/", viewModel));
            if (IsTokenInvalidUsingResponse(response, "Unathorized access.")) { return RedirectToAction("Logout", "Home"); }

            ViewBag.ModalTitle = response.StatusCode == System.Net.HttpStatusCode.OK ? response.Status : response.Status;
            ViewBag.ModalMessage = response.StatusCode == System.Net.HttpStatusCode.OK ? response.Message : response.Message;
            ViewBag.ShowModal = response.StatusCode == System.Net.HttpStatusCode.OK ? "true" : "false";
            ViewBag.Id = viewModel.FacilityPitchTimingId;
            //ViewBag.Current = "Event Management";
            return View("EditSlot");

        }

        [HttpPost]
        public IActionResult Delete([FromBody] ChangeStatus Id)
        {
            var response = new APIResponse();
            if (IsUserLoggedIn(AdminUCtxt))
            {
                response = JsonConvert.DeserializeObject<APIResponse>(AdminHttpClient.PostHttpClientRequest("FacilityPitch/DeleteSlot/", Id));
                if (IsTokenInvalidUsingResponse(response, "Unathorized access.")) { return RedirectToAction("Logout", "Home"); }

                return Json(response);
            }

            if (IsFCUserLoggedIn(FacilityUCtxt))
            {
                response = JsonConvert.DeserializeObject<APIResponse>(AdminHttpClient.PostHttpClientRequest("FacilityPitch/DeleteSlot/", Id));
                if (IsTokenInvalidUsingResponse(response, "Unathorized access.")) { return RedirectToAction("Logout", "Home"); }

                return Json(response);
            }

            return RedirectToAction("Logout", "Home");

        }

        public IActionResult Export()
        {
            var playBookings = new List<AddSlotViewModel>();
            if (IsUserLoggedIn(AdminUCtxt))
            {
                var response = JsonConvert.DeserializeObject<APIResponse>(AdminHttpClient.GetHttpClientRequest("FacilityPitch/GetAllFacilityPitchTiming/"));
                if (!IsTokenInvalidUsingResponse(response, "Unathorized access."))
                {
                    playBookings = response.Payload != null ? JsonConvert.DeserializeObject<List<AddSlotViewModel>>(response.Payload.ToString()) : new List<AddSlotViewModel>();
                }

                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Play Booking Reports");
                    var currentRow = 1;
                    worksheet.Cell(currentRow, 1).Value = "Facility";
                    worksheet.Cell(currentRow, 1).Style.Fill.BackgroundColor = XLColor.Yellow;
                    worksheet.Cell(currentRow, 2).Value = "Slot";
                    worksheet.Cell(currentRow, 2).Style.Fill.BackgroundColor = XLColor.Yellow;
                    worksheet.Cell(currentRow, 3).Value = "Players";
                    worksheet.Cell(currentRow, 3).Style.Fill.BackgroundColor = XLColor.Yellow;
                    worksheet.Cell(currentRow, 4).Value = "Date";
                    worksheet.Cell(currentRow, 4).Style.Fill.BackgroundColor = XLColor.Yellow;
                    worksheet.Cell(currentRow, 5).Value = "Price";
                    worksheet.Cell(currentRow, 5).Style.Fill.BackgroundColor = XLColor.Yellow;
                    worksheet.Cell(currentRow, 6).Value = "Booking Commission";
                    worksheet.Cell(currentRow, 6).Style.Fill.BackgroundColor = XLColor.Yellow;
                    worksheet.Cell(currentRow, 7).Value = "Status";
                    worksheet.Cell(currentRow, 7).Style.Fill.BackgroundColor = XLColor.Yellow;

                    foreach (var playBooking in playBookings)
                    {
                        var playerCounts = playBooking.PlayerCount + "/" + playBooking.MaxPlayers;
                        currentRow++;
                        worksheet.Cell(currentRow, 1).Value = playBooking.FacilityName;
                        worksheet.Cell(currentRow, 2).Value = $"{playBooking.Start.ToShortTimeString()} - {playBooking.End.ToShortTimeString()}";
                        worksheet.Cell(currentRow, 3).Value = playBooking.PlayerCount + "|" + playBooking.MaxPlayers; 
                        worksheet.Cell(currentRow, 4).Value = playBooking.Date.ToString("dd/MM/yyyy");
                        if (playBooking.IsFree)
                        {
                            worksheet.Cell(currentRow, 5).Value = "Free";
                        }
                        else
                        {
                            worksheet.Cell(currentRow, 5).Value = playBooking.TotalPrice + " AED";
                        }

                      
                        worksheet.Cell(currentRow, 6).Value = playBooking.Commissions.ToString() + " AED";
                        if (playBooking.PlayerCount == playBooking.MaxPlayers)
                        {
                            worksheet.Cell(currentRow, 7).Value = "FULL";
                            worksheet.Cell(currentRow, 7).Style.Fill.BackgroundColor = XLColor.LightGreen;
                        }
                        else
                        {
                            worksheet.Cell(currentRow, 7).Value = "PENDING";
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
                        $"Play Bookings - {DateTime.Now}.xlsx");
                }
            }

            return View();
        }

        public IActionResult PlayBookingExport()
        {
            var playBookings = new List<AddSlotViewModel>();
            if (IsUserLoggedIn(AdminUCtxt))
            {
                var response = JsonConvert.DeserializeObject<APIResponse>(AdminHttpClient.GetHttpClientRequest("FacilityPitch/GetAllFacilityPitchBooking/"));
                if (!IsTokenInvalidUsingResponse(response, "Unathorized access."))
                {
                    playBookings = response.Payload != null ? JsonConvert.DeserializeObject<List<AddSlotViewModel>>(response.Payload.ToString()) : new List<AddSlotViewModel>();
                }

                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Play Booking Reports");
                    var currentRow = 1;
                    worksheet.Cell(currentRow, 1).Value = "Facility";
                    worksheet.Cell(currentRow, 1).Style.Fill.BackgroundColor = XLColor.Yellow;
                    worksheet.Cell(currentRow, 2).Value = "Slot";
                    worksheet.Cell(currentRow, 2).Style.Fill.BackgroundColor = XLColor.Yellow;
                    worksheet.Cell(currentRow, 3).Value = "Players";
                    worksheet.Cell(currentRow, 3).Style.Fill.BackgroundColor = XLColor.Yellow;
                    worksheet.Cell(currentRow, 4).Value = "Date";
                    worksheet.Cell(currentRow, 4).Style.Fill.BackgroundColor = XLColor.Yellow;
                    worksheet.Cell(currentRow, 5).Value = "Price";
                    worksheet.Cell(currentRow, 5).Style.Fill.BackgroundColor = XLColor.Yellow;
                    worksheet.Cell(currentRow, 6).Value = "Booking Commission";
                    worksheet.Cell(currentRow, 6).Style.Fill.BackgroundColor = XLColor.Yellow;
                    worksheet.Cell(currentRow, 7).Value = "Status";
                    worksheet.Cell(currentRow, 7).Style.Fill.BackgroundColor = XLColor.Yellow;

                    foreach (var playBooking in playBookings)
                    {
                        var playerCounts = playBooking.PlayerCount + "/" + playBooking.MaxPlayers;
                        currentRow++;
                        worksheet.Cell(currentRow, 1).Value = playBooking.FacilityName;
                        worksheet.Cell(currentRow, 2).Value = $"{playBooking.Start.ToShortTimeString()} - {playBooking.End.ToShortTimeString()}";
                        worksheet.Cell(currentRow, 3).Value = playBooking.PlayerCount + "|" + playBooking.MaxPlayers;
                        worksheet.Cell(currentRow, 4).Value = playBooking.Date.ToString("dd/MM/yyyy");
                        if (playBooking.IsFree)
                        {
                            worksheet.Cell(currentRow, 5).Value = "Free";
                        }
                        else
                        {
                            worksheet.Cell(currentRow, 5).Value = playBooking.TotalPrice + " AED";
                        }


                        worksheet.Cell(currentRow, 6).Value = playBooking.Commissions.ToString() + " AED";
                        if (playBooking.PlayerCount == playBooking.MaxPlayers)
                        {
                            worksheet.Cell(currentRow, 7).Value = "FULL";
                            worksheet.Cell(currentRow, 7).Style.Fill.BackgroundColor = XLColor.LightGreen;
                        }
                        else
                        {
                            worksheet.Cell(currentRow, 7).Value = "PENDING";
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
                        $"Play Bookings - {DateTime.Now}.xlsx");
                }
            }

            return View();
        }
    }
}
