using ClosedXML.Excel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using Sidekick.Admin.DataLayer;
using Sidekick.Admin.Helpers;
using Sidekick.Model;
using Sidekick.Model.Booking;
using Sidekick.Model.Class;
using Sidekick.Model.Gym;
using Sidekick.Model.SetupConfiguration.Level;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Sidekick.Admin.Controllers
{
    public class CoachingController : BaseController
    {
        private IMainHttpClient AdminHttpClient { get; }
        public AdminUserContext AdminUCtxt { get; set; }
        private ConfigMaster MConf { get; }
        private readonly IHttpContextAccessor _httpCtxtAcc;
        private ISession _session => _httpCtxtAcc.HttpContext.Session;

        public CoachingController(IMainHttpClient _mhttpc,
            IHttpContextAccessor httpContextAccessor,
            ConfigMaster _conf)
        {
            AdminHttpClient = _mhttpc;
            _httpCtxtAcc = httpContextAccessor;
            MConf = _conf;
            AdminUCtxt = _session.GetObjectFromJson<AdminUserContext>("adminUserContext");
        }

        public IActionResult Index()
        {
            var trainRenderView = new TrainRenderViewModel();
            var trainingClasses = new List<ClassRenderViewModel>();
            var trainingBookings = new List<IndividualBookingViewModel>();
            if (IsUserLoggedIn(AdminUCtxt))
            {
                var response = JsonConvert.DeserializeObject<APIResponse>(AdminHttpClient.GetHttpClientRequest("Class/GetAllGroupClasses/"));
                if (!IsTokenInvalidUsingResponse(response, "Unathorized access."))
                {
                    trainingClasses = response != null ? JsonConvert.DeserializeObject<List<ClassRenderViewModel>>(response.Payload.ToString()) : new List<ClassRenderViewModel>();
                }

                var responseBooking = JsonConvert.DeserializeObject<APIResponse>(AdminHttpClient.GetHttpClientRequest("Booking/GetAllTrainBookings/"));
                if (!IsTokenInvalidUsingResponse(responseBooking, "Unathorized access."))
                {
                    trainingBookings = response != null ? JsonConvert.DeserializeObject<List<IndividualBookingViewModel>>(responseBooking.Payload.ToString()) : new List<IndividualBookingViewModel>();
                }

            }
            ViewBag.Current = "Event Management";
            trainRenderView.Classes = trainingClasses.Any() ? trainingClasses.OrderByDescending(t => t.DateUpdated).ToList() : trainingClasses;
            trainRenderView.Bookings = trainingBookings.Any() ? trainingBookings.OrderByDescending(t => t.Date).ToList() : trainingBookings;
            return View(trainRenderView);
        }

        [HttpGet]
        public IActionResult AddCoaching()
        {
            var groupClassViewModel = new ClassRenderViewModel();
            if (IsUserLoggedIn(AdminUCtxt))
            {
                ViewBag.TimeSlotsAM = PopulateTimeSlots("00:00", "23:30");
                ViewBag.TimeSlotsPM = PopulateTimeSlots("00:00", "23:30");
                var levelResponse = JsonConvert.DeserializeObject<APIResponse>(AdminHttpClient.GetHttpClientRequest("Level/Get/"));
                //var gymResponse = JsonConvert.DeserializeObject<APIResponse>(AdminHttpClient.GetHttpClientRequest("Gym/Get/"));
                var areaResponse = JsonConvert.DeserializeObject<APIResponse>(AdminHttpClient.GetHttpClientRequest("Facility/GetAreas"));
                var coachResponse = JsonConvert.DeserializeObject<APIResponse>(AdminHttpClient.GetHttpClientRequest("Coach/GetCoaches/"));
                var gymResponse = JsonConvert.DeserializeObject<APIResponse>(AdminHttpClient.GetHttpClientRequest("Gym/Get/"));
                if (!IsTokenInvalidUsingResponse(levelResponse, "Unathorized access.")
                    && !IsTokenInvalidUsingResponse(areaResponse, "Unathorized access.")
                    && !IsTokenInvalidUsingResponse(coachResponse, "Unathorized access."))
                {
                    var levels = levelResponse != null ? JsonConvert.DeserializeObject<IEnumerable<Level>>(levelResponse.Payload.ToString()) : new List<Level>();
                    var areas = areaResponse != null ? JsonConvert.DeserializeObject<List<Area>>(areaResponse.Payload.ToString()) : new List<Area>();
                    var coaches = coachResponse != null ? JsonConvert.DeserializeObject<IEnumerable<CoachViewModel>>(coachResponse.Payload.ToString()) : new List<CoachViewModel>();
                    var gyms = JsonConvert.DeserializeObject<IEnumerable<Gym>>(gymResponse.Payload.ToString());
                    var levelList = new List<SelectListItem>
                    {
                        new SelectListItem
                        {
                            Value = string.Empty,
                            Text = "Select Level"
                        }
                    };

                    if (levels.Any())
                    {
                        foreach (var level in levels)
                        {
                            levelList.Add(new SelectListItem
                            {
                                Value = level.LevelId.ToString(),
                                Text = level.Name
                            });
                        }
                    }

                    var coachList = new List<SelectListItem>
                    {
                        new SelectListItem
                        {
                            Value = string.Empty,
                            Text = "Select Coach"
                        }
                    };


                    if (coaches.Any())
                    {
                        foreach (var coach in coaches.Where(c => c.Status == "Active").ToList())
                        {
                            coachList.Add(new SelectListItem
                            {
                                Value = coach.CoachUserId.ToString(),
                                Text = coach.ProfileName
                            });
                        }
                    }

                    var areaList = new List<SelectListItem>() { new SelectListItem
                    {
                        Value = default,
                        Text = "Select Area"
                    } };

                    if (areas.Any())
                    {
                        foreach (var area in areas)
                        {
                            areaList.Add(new SelectListItem
                            {
                                Value = area.AreaId.ToString(),
                                Text = area.AreaName
                            });
                        }
                    }

                    var gymList = new List<SelectListItem>() { new SelectListItem
                    {
                        Value = string.Empty,
                        Text = "Select Gym"
                    } };

                    if (gyms.Any())
                    {
                        foreach (var gym in gyms)
                        {
                            var area = areas.Where(g => g.AreaId == gym.AreaId).FirstOrDefault();
                            var areaName = area != null ? area.AreaName : String.Empty;
                            gymList.Add(new SelectListItem
                            {
                                Value = gym.GymId.ToString(),
                                Text = $"{gym.GymName} - {areaName}"
                            });
                        }
                    }

                    groupClassViewModel.Coaches = coachList;
                    groupClassViewModel.Levels = levelList;
                    groupClassViewModel.Areas = areaList;
                    groupClassViewModel.Gyms = gymList;

                    return View(groupClassViewModel);
                }

            }
            ViewBag.Current = "Event Management";
            return View();
        }

        [HttpPost]
        public IActionResult AddCoaching(ClassRenderViewModel coaching)
        {
            if (ModelState.IsValid)
            {
                if (!string.IsNullOrWhiteSpace(coaching.TrainingType))
                {
                    if (coaching.TrainingType.Equals("Group", StringComparison.OrdinalIgnoreCase))
                    {
                        var dateFrom = new DateTime(coaching.Date.Year, coaching.Date.Month, coaching.Date.Day, coaching.ScheduleFrom.Hour, coaching.ScheduleFrom.Minute, coaching.ScheduleFrom.Second);
                        var dateTo = new DateTime(coaching.Date.Year, coaching.Date.Month, coaching.Date.Day, coaching.ScheduleTo.Hour, coaching.ScheduleTo.Minute, coaching.ScheduleTo.Second);
                        var groupClassViewModel = new GroupClassViewModel
                        {
                            CoachId = coaching.CoachId,
                            Title = coaching.Title,
                            LevelId = coaching.LevelId,
                            Start = dateFrom,
                            End = dateTo,
                            RepeatEveryWeek = coaching.IsRepeat,
                            Participants = coaching.Participants,
                            GymId = coaching.GymId,
                            IsLocation = coaching.IsLocation,
                            //AreaId = coaching.AreaId,
                            Notes = coaching.Description,
                            Price = coaching.Price,
                            Duration = coaching.Duration ?? 0,
                            DuringNo = coaching.Duration,
                            StartTime = dateFrom.ToString("HH:mm")
                        };

                        var response = JsonConvert.DeserializeObject<APIResponse>(AdminHttpClient.PostHttpClientRequest("Class/Group/", groupClassViewModel));
                        ViewBag.ModalTitle = response.StatusCode == System.Net.HttpStatusCode.OK ? response.Status : response.Status;
                        ViewBag.ModalMessage = response.StatusCode == System.Net.HttpStatusCode.OK ? response.Message : response.Message;
                        ViewBag.ShowModal = response.StatusCode == System.Net.HttpStatusCode.OK ? "true" : "false";
                        ViewBag.Current = "Event Management";
                        return View("Index");
                    }
                    else
                    {
                        var dateFrom = new DateTime(coaching.Date.Year, coaching.Date.Month, coaching.Date.Day, coaching.ScheduleFrom.Hour, coaching.ScheduleFrom.Minute, coaching.ScheduleFrom.Second);
                        var dateTo = new DateTime(coaching.Date.Year, coaching.Date.Month, coaching.Date.Day, coaching.ScheduleTo.Hour, coaching.ScheduleTo.Minute, coaching.ScheduleTo.Second);
                        coaching.ScheduleFrom = dateFrom;
                        coaching.ScheduleTo = dateTo;
                        var response = JsonConvert.DeserializeObject<APIResponse>(AdminHttpClient.PostHttpClientRequest("Class/InsertOrUpdateIndividual/", coaching));
                        ViewBag.ModalTitle = response.StatusCode == System.Net.HttpStatusCode.OK ? response.Status : response.Status;
                        ViewBag.ModalMessage = response.StatusCode == System.Net.HttpStatusCode.OK ? response.Message : response.Message;
                        ViewBag.ShowModal = response.StatusCode == System.Net.HttpStatusCode.OK ? "true" : "false";
                        ViewBag.Current = "Event Management";
                        return View("Index");
                    }
                }
            }
            else
            {
                return Json(new APIResponse { StatusCode = System.Net.HttpStatusCode.BadRequest, Message = "Invalid Object!", ModelError = ModelState.Errors() });
            }
            return View();
        }

        public IActionResult Export()
        {
            var trainingBookings = new List<ClassRenderViewModel>();
            if (IsUserLoggedIn(AdminUCtxt))
            {
                var response = JsonConvert.DeserializeObject<APIResponse>(AdminHttpClient.GetHttpClientRequest("Class/GetAllGroupClasses/"));
                if (!IsTokenInvalidUsingResponse(response, "Unathorized access."))
                {
                    trainingBookings = response != null ? JsonConvert.DeserializeObject<List<ClassRenderViewModel>>(response.Payload.ToString()) : new List<ClassRenderViewModel>();
                }

                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Train Booking Reports");
                    var currentRow = 1;
                    worksheet.Cell(currentRow, 1).Value = "Type";
                    worksheet.Cell(currentRow, 1).Style.Fill.BackgroundColor = XLColor.Yellow;
                    worksheet.Cell(currentRow, 2).Value = "Coach Name";
                    worksheet.Cell(currentRow, 2).Style.Fill.BackgroundColor = XLColor.Yellow;
                    worksheet.Cell(currentRow, 3).Value = "Email";
                    worksheet.Cell(currentRow, 3).Style.Fill.BackgroundColor = XLColor.Yellow;
                    worksheet.Cell(currentRow, 4).Value = "Slot";
                    worksheet.Cell(currentRow, 4).Style.Fill.BackgroundColor = XLColor.Yellow;
                    worksheet.Cell(currentRow, 5).Value = "Date";
                    worksheet.Cell(currentRow, 5).Style.Fill.BackgroundColor = XLColor.Yellow;
                    worksheet.Cell(currentRow, 6).Value = "Price";
                    worksheet.Cell(currentRow, 6).Style.Fill.BackgroundColor = XLColor.Yellow;
                    worksheet.Cell(currentRow, 7).Value = "Booking Commission";
                    worksheet.Cell(currentRow, 7).Style.Fill.BackgroundColor = XLColor.Yellow;
                    worksheet.Cell(currentRow, 8).Value = "Status";
                    worksheet.Cell(currentRow, 8).Style.Fill.BackgroundColor = XLColor.Yellow;

                    foreach (var trainBooking in trainingBookings)
                    {
                        currentRow++;
                        worksheet.Cell(currentRow, 1).Value = trainBooking.TrainingType;
                        worksheet.Cell(currentRow, 2).Value = trainBooking.CoachName;
                        worksheet.Cell(currentRow, 3).Value = trainBooking.CoachUserEmail;
                        if (trainBooking.ScheduleFrom.ToShortTimeString() == "12:00 AM" || trainBooking.ScheduleTo.ToShortTimeString() == "12:00 AM")
                        {
                            worksheet.Cell(currentRow, 4).Value = "Closed";
                        }
                        else
                        {
                            worksheet.Cell(currentRow, 4).Value = $"{trainBooking.ScheduleFrom.ToShortTimeString()} - {trainBooking.ScheduleTo.ToShortTimeString()}";
                        }

                        worksheet.Cell(currentRow, 5).Value = trainBooking.ScheduleFrom.ToString("dd/MM/yyyy");
                        worksheet.Cell(currentRow, 6).Value = trainBooking.Price + " AED";
                        worksheet.Cell(currentRow, 7).Value = trainBooking.Commission.ToString("0.00") + "%";
                        if (trainBooking.IsEnabled)
                        {
                            worksheet.Cell(currentRow, 8).Value = "Active";
                            worksheet.Cell(currentRow, 8).Style.Fill.BackgroundColor = XLColor.LightGreen;
                        }
                        else
                        {
                            worksheet.Cell(currentRow, 8).Value = "Inactive";
                            worksheet.Cell(currentRow, 8).Style.Fill.BackgroundColor = XLColor.LightGray;
                        }
                    }

                    using var stream = new MemoryStream();
                    worksheet.Columns().AdjustToContents();
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();

                    return File(
                        content,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        $"TrainBookings - {DateTime.Now}.xlsx");
                }
            }

            return View();

        }


        public IActionResult BookingExport()
        {
            var trainingBookings = new List<IndividualBookingViewModel>();
            if (IsUserLoggedIn(AdminUCtxt))
            {
                var response = JsonConvert.DeserializeObject<APIResponse>(AdminHttpClient.GetHttpClientRequest("Booking/GetAllTrainBookings/"));
                if (!IsTokenInvalidUsingResponse(response, "Unathorized access."))
                {
                    trainingBookings = response != null ? JsonConvert.DeserializeObject<List<IndividualBookingViewModel>>(response.Payload.ToString()) : new List<IndividualBookingViewModel>();
                }

                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Train Booking Reports");
                    var currentRow = 1;
                    worksheet.Cell(currentRow, 1).Value = "ID";
                    worksheet.Cell(currentRow, 1).Style.Fill.BackgroundColor = XLColor.Yellow;
                    worksheet.Cell(currentRow, 2).Value = "Type";
                    worksheet.Cell(currentRow, 2).Style.Fill.BackgroundColor = XLColor.Yellow;
                    worksheet.Cell(currentRow, 3).Value = "UserName";
                    worksheet.Cell(currentRow, 3).Style.Fill.BackgroundColor = XLColor.Yellow;
                    worksheet.Cell(currentRow, 4).Value = "Slot";
                    worksheet.Cell(currentRow, 4).Style.Fill.BackgroundColor = XLColor.Yellow;
                    worksheet.Cell(currentRow, 5).Value = "Booking Date";
                    worksheet.Cell(currentRow, 5).Style.Fill.BackgroundColor = XLColor.Yellow;
                    worksheet.Cell(currentRow, 6).Value = "Booking Price";
                    worksheet.Cell(currentRow, 6).Style.Fill.BackgroundColor = XLColor.Yellow;
                    worksheet.Cell(currentRow, 7).Value = "Booking Comission";
                    worksheet.Cell(currentRow, 7).Style.Fill.BackgroundColor = XLColor.Yellow;
                    worksheet.Cell(currentRow, 8).Value = "Status";
                    worksheet.Cell(currentRow, 9).Style.Fill.BackgroundColor = XLColor.Yellow;

                    foreach (var trainBooking in trainingBookings)
                    {
                        currentRow++;
                        worksheet.Cell(currentRow, 1).Value = currentRow;
                        worksheet.Cell(currentRow, 2).Value = trainBooking.BookingType;
                        worksheet.Cell(currentRow, 3).Value = trainBooking.CoachFirstName + " " + trainBooking.CoachLastName;
                        worksheet.Cell(currentRow, 4).Value = trainBooking.Date.ToShortTimeString() + "-" + trainBooking.EndDate.ToShortTimeString();
                        worksheet.Cell(currentRow, 5).Value = trainBooking.Date.ToString("dd/MM/yyyy");
                        worksheet.Cell(currentRow, 6).Value = trainBooking.BookingAmount + " AED";
                        worksheet.Cell(currentRow, 7).Value = trainBooking.CommissionAmount.ToString("0.00") + "%";
                        worksheet.Cell(currentRow, 8).Value = trainBooking.Status;
                    }

                    using var stream = new MemoryStream();
                    worksheet.Columns().AdjustToContents();
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();

                    return File(
                        content,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        $"TrainBookings - {DateTime.Now}.xlsx");
                }
            }

            return View();

        }

        public IActionResult EditCoaching(Guid u)
        {
            var groupClassViewModel = new ClassRenderViewModel();
            if (IsUserLoggedIn(AdminUCtxt))
            {
                ViewBag.TimeSlotsAM = PopulateTimeSlots("00:00", "23:30");
                ViewBag.TimeSlotsPM = PopulateTimeSlots("00:00", "23:30");
                var levelResponse = JsonConvert.DeserializeObject<APIResponse>(AdminHttpClient.GetHttpClientRequest("Level/Get/"));
                var areaResponse = JsonConvert.DeserializeObject<APIResponse>(AdminHttpClient.GetHttpClientRequest("Facility/GetAreas"));
                var coachResponse = JsonConvert.DeserializeObject<APIResponse>(AdminHttpClient.GetHttpClientRequest("Coach/GetCoaches/"));
                var classResponse = JsonConvert.DeserializeObject<APIResponse>(AdminHttpClient.GetHttpClientRequest($"Class/GetCoachingClass/{u}"));
                var gymResponse = JsonConvert.DeserializeObject<APIResponse>(AdminHttpClient.GetHttpClientRequest("Gym/Get/"));
                if (!IsTokenInvalidUsingResponse(levelResponse, "Unathorized access.")
                    && !IsTokenInvalidUsingResponse(areaResponse, "Unathorized access.")
                    && !IsTokenInvalidUsingResponse(coachResponse, "Unathorized access.")
                    && !IsTokenInvalidUsingResponse(classResponse, "Unathorized access."))
                {
                    var levels = levelResponse != null ? JsonConvert.DeserializeObject<IEnumerable<Level>>(levelResponse.Payload.ToString()) : new List<Level>();
                    var areas = areaResponse != null ? JsonConvert.DeserializeObject<List<Area>>(areaResponse.Payload.ToString()) : new List<Area>();
                    var coaches = coachResponse != null ? JsonConvert.DeserializeObject<IEnumerable<CoachViewModel>>(coachResponse.Payload.ToString()) : new List<CoachViewModel>();
                    var classObject = classResponse != null ? JsonConvert.DeserializeObject<ClassRenderViewModel>(classResponse.Payload.ToString()) : new ClassRenderViewModel();
                    var gyms = JsonConvert.DeserializeObject<IEnumerable<Gym>>(gymResponse.Payload.ToString());
                    var levelList = new List<SelectListItem>
                    {
                        new SelectListItem
                        {
                            Value = "",
                            Text = "Select Level"
                        }
                    };

                    if (levels.Any())
                    {
                        foreach (var level in levels)
                        {
                            levelList.Add(new SelectListItem
                            {
                                Value = level.LevelId.ToString(),
                                Text = level.Name
                            });
                        }
                    }

                    var coachList = new List<SelectListItem>
                    {
                        new SelectListItem
                        {
                            Value = "",
                            Text = "Select Coach"
                        }
                    };


                    if (coaches.Any())
                    {
                        foreach (var coach in coaches)
                        {
                            coachList.Add(new SelectListItem
                            {
                                Value = coach.CoachUserId.ToString(),
                                Text = coach.ProfileName
                            });
                        }
                    }

                    var areaList = new List<SelectListItem>() { new SelectListItem
                    {
                        Value = "",
                        Text = "Select Area"
                    } };

                    if (areas.Any())
                    {
                        foreach (var area in areas)
                        {
                            areaList.Add(new SelectListItem
                            {
                                Value = area.AreaId.ToString(),
                                Text = area.AreaName
                            });
                        }
                    }

                    var gymList = new List<SelectListItem>() { new SelectListItem
                    {
                        Value = default,
                        Text = "Select Gym"
                    } };

                    if (gyms.Any())
                    {
                        foreach (var gym in gyms)
                        {
                            var area = areas.Where(g => g.AreaId == gym.AreaId).FirstOrDefault();
                            var areaName = area != null ? area.AreaName : String.Empty;
                            gymList.Add(new SelectListItem
                            {
                                Value = gym.GymId.ToString(),
                                Text = $"{gym.GymName} - {areaName}"
                            });
                        }
                    }


                    ViewBag.Coaches = coachList;
                    ViewBag.Levels = levelList;
                    ViewBag.Areas = areaList;
                    ViewBag.Gyms = gymList;

                    return View(classObject);
                }
            }
            ViewBag.Current = "Event Management";
            return View();
        }

        [HttpPost]
        public IActionResult Delete([FromBody] ChangeStatus Id)
        {
            if (IsUserLoggedIn(AdminUCtxt))
            {
                var response = JsonConvert.DeserializeObject<APIResponse>(AdminHttpClient.PostHttpClientRequest("Class/DeleteClass/", Id));
                if (IsTokenInvalidUsingResponse(response, "Unathorized access.")) { return RedirectToAction("Logout", "Home"); }

                return Json(response);
            }

            return RedirectToAction("Logout", "Home");

        }

        [HttpPost]
        public IActionResult Edit(ClassRenderViewModel coaching)
        {
            if (ModelState.IsValid)
            {
                if (!string.IsNullOrWhiteSpace(coaching.TrainingType))
                {
                    if (coaching.TrainingType.Equals("Group", StringComparison.OrdinalIgnoreCase))
                    {
                        var dateFrom = new DateTime(coaching.Date.Year, coaching.Date.Month, coaching.Date.Day, coaching.ScheduleFrom.Hour, coaching.ScheduleFrom.Minute, coaching.ScheduleFrom.Second);
                        var dateTo = new DateTime(coaching.Date.Year, coaching.Date.Month, coaching.Date.Day, coaching.ScheduleTo.Hour, coaching.ScheduleTo.Minute, coaching.ScheduleTo.Second);
                        var groupClassViewModel = new GroupClassViewModel
                        {
                            GroupClassId = coaching.GroupClassId,
                            CoachId = coaching.CoachId,
                            Title = coaching.Title,
                            LevelId = coaching.LevelId,
                            Start = dateFrom,
                            End = dateTo,
                            RepeatEveryWeek = coaching.IsRepeat,
                            IsLocation = coaching.IsLocation,
                            //AreaId = coaching.AreaId,
                            GymId = coaching.GymId,
                            Price = coaching.Price
                        };

                        var response = JsonConvert.DeserializeObject<APIResponse>(AdminHttpClient.PostHttpClientRequest("Class/Group/", groupClassViewModel));
                        ViewBag.ModalTitle = response.StatusCode == System.Net.HttpStatusCode.OK ? response.Status : response.Status;
                        ViewBag.ModalMessage = response.StatusCode == System.Net.HttpStatusCode.OK ? "Updated Group Class" : response.Message;
                        ViewBag.ShowModal = response.StatusCode == System.Net.HttpStatusCode.OK ? "true" : "false";
                        ViewBag.Current = "Event Management";
                        return View("Index");
                    }
                    else
                    {
                        var dateFrom = new DateTime(coaching.Date.Year, coaching.Date.Month, coaching.Date.Day, coaching.ScheduleFrom.Hour, coaching.ScheduleFrom.Minute, coaching.ScheduleFrom.Second);
                        var dateTo = new DateTime(coaching.Date.Year, coaching.Date.Month, coaching.Date.Day, coaching.ScheduleTo.Hour, coaching.ScheduleTo.Minute, coaching.ScheduleTo.Second);
                        coaching.ScheduleFrom = dateFrom;
                        coaching.ScheduleTo = dateTo;
                        var response = JsonConvert.DeserializeObject<APIResponse>(AdminHttpClient.PostHttpClientRequest("Class/InsertOrUpdateIndividual/", coaching));
                        ViewBag.ModalTitle = response.StatusCode == System.Net.HttpStatusCode.OK ? response.Status : response.Status;
                        ViewBag.ModalMessage = response.StatusCode == System.Net.HttpStatusCode.OK ? "Updated Individual Class" : response.Message;
                        ViewBag.ShowModal = response.StatusCode == System.Net.HttpStatusCode.OK ? "true" : "false";
                        ViewBag.Current = "Event Management";
                        return View("Index");
                    }
                }
            }

            return View();
        }
    }
}
