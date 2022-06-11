using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Sidekick.Model;
using Sidekick.Admin.DataLayer;
using Sidekick.Admin.Helpers;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Linq;
using Sidekick.Admin.Extensions;
using ClosedXML.Excel;
using System.IO;
using Sidekick.Model.Payment;

namespace Sidekick.Admin.Controllers
{
    public class AdminController : BaseController
    {
        private IMainHttpClient AdminHttpClient { get; }
        public AdminUserContext AdminUCtxt { get; set; }
        private ConfigMaster MConf { get; }
        private readonly IHttpContextAccessor _httpCtxtAcc;
        private ISession _session => _httpCtxtAcc.HttpContext.Session;

        public AdminController(IMainHttpClient _mhttpc, IHttpContextAccessor httpContextAccessor, ConfigMaster _conf)
        {
            AdminHttpClient = _mhttpc;
            _httpCtxtAcc = httpContextAccessor;
            MConf = _conf;
            AdminUCtxt = _session.GetObjectFromJson<AdminUserContext>("adminUserContext");
        }
        public IActionResult Export()
        {
            var viewModel = new DasboardViewModel();
            PopulateDashboardValues(viewModel);
            var playerSummaryValues = viewModel.FacilityPlayers.Where(f => f.IsEnabled == true).GroupBy(f => f.DateCreated.Value.Year).ToDictionary(g => g.Key, g => g.Count());
            var coachSummaryValues = viewModel.Coaches.GroupBy(f => f.DateCreated.Value.Year).ToDictionary(g => g.Key, g => g.Count());
            var facilitySummaryValues = viewModel.Facilities.Where(f => f.IsEnabled == true).GroupBy(f => f.CreatedDate.Value.Year).ToDictionary(g => g.Key, g => g.Count());
            try
            {
                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("User Reports");
                    var currentRow = 1;
                    worksheet.Cell(currentRow, 1).Value = "YEAR";
                    worksheet.Cell(currentRow, 1).Style.Fill.BackgroundColor = XLColor.Yellow;
                    worksheet.Cell(currentRow, 2).Value = "NUMBER OF PLAYERS";
                    worksheet.Cell(currentRow, 2).Style.Fill.BackgroundColor = XLColor.Yellow;

                    worksheet.Cell(currentRow, 4).Value = "YEAR";
                    worksheet.Cell(currentRow, 4).Style.Fill.BackgroundColor = XLColor.Yellow;
                    worksheet.Cell(currentRow, 5).Value = "NUMBER OF COACHES";
                    worksheet.Cell(currentRow, 5).Style.Fill.BackgroundColor = XLColor.Yellow;

                    worksheet.Cell(currentRow, 7).Value = "YEAR";
                    worksheet.Cell(currentRow, 7).Style.Fill.BackgroundColor = XLColor.Yellow;
                    worksheet.Cell(currentRow, 8).Value = "NUMBER OF FACILITIES";
                    worksheet.Cell(currentRow, 8).Style.Fill.BackgroundColor = XLColor.Yellow;
                    foreach (var playerSummary in playerSummaryValues)
                    {
                        currentRow++;
                        worksheet.Cell(currentRow, 1).Value = playerSummary.Key;
                        worksheet.Cell(currentRow, 2).Value = playerSummary.Value;
                    }

                    currentRow = 1;

                    foreach (var coachSummary in coachSummaryValues)
                    {
                        currentRow++;
                        worksheet.Cell(currentRow, 4).Value = coachSummary.Key;
                        worksheet.Cell(currentRow, 5).Value = coachSummary.Value;
                    }

                    currentRow = 1;
                    foreach (var facilitySummary in facilitySummaryValues)
                    {
                        currentRow++;
                        worksheet.Cell(currentRow, 7).Value = facilitySummary.Key;
                        worksheet.Cell(currentRow, 8).Value = facilitySummary.Value;
                    }

                    var bookingSummaryValues = viewModel.Bookings.GroupBy(f => f.CreatedDate.Value.Year).ToDictionary(g => g.Key, g => g.Count());
                    var bookingsReportSheet = workbook.Worksheets.Add("Booking Reports");
                    var currentBookingsReportRow = 1;
                    bookingsReportSheet.Cell(currentBookingsReportRow, 1).Value = "YEAR";
                    bookingsReportSheet.Cell(currentBookingsReportRow, 1).Style.Fill.BackgroundColor = XLColor.Yellow;
                    bookingsReportSheet.Cell(currentBookingsReportRow, 2).Value = "NUMBER OF BOOKINGS";
                    bookingsReportSheet.Cell(currentBookingsReportRow, 2).Style.Fill.BackgroundColor = XLColor.Yellow;

                    foreach (var bookingSummary in bookingSummaryValues)
                    {
                        currentBookingsReportRow++;
                        bookingsReportSheet.Cell(currentBookingsReportRow, 1).Value = bookingSummary.Key;
                        bookingsReportSheet.Cell(currentBookingsReportRow, 2).Value = bookingSummary.Value;
                    }

                    using var stream = new MemoryStream();
                    worksheet.Columns().AdjustToContents();
                    bookingsReportSheet.Columns().AdjustToContents();
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();

                    return File(
                        content,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        $"Summary - {DateTime.Now}.xlsx");
                }         
            }
            catch (Exception)
            {
                return View();
            }
        }

        public IActionResult Index()
        {
            var viewModel = new DasboardViewModel();
            if (IsUserLoggedIn(AdminUCtxt))
            {
                PopulateDashboardValues(viewModel);
            }
            ViewBag.Current = "Dashboard";
            return View(viewModel);
        }

        private void PopulateDashboardValues(DasboardViewModel viewModel)
        {
            var facilityPlayersResponse = JsonConvert.DeserializeObject<APIResponse>(AdminHttpClient.GetHttpClientRequest("FacilityPlayer/All/"));
            var facilitiesResponse = JsonConvert.DeserializeObject<APIResponse>(AdminHttpClient.GetHttpClientRequest("Facility/AllFacilities/"));
            var coachesResponse = JsonConvert.DeserializeObject<APIResponse>(AdminHttpClient.GetHttpClientRequest("Coach/GetCoaches/"));
            var usersResponse = JsonConvert.DeserializeObject<APIResponse>(AdminHttpClient.GetHttpClientRequest("User/GetUsers/"));
            var bookedPitchesResponse = JsonConvert.DeserializeObject<APIResponse>(AdminHttpClient.GetHttpClientRequest("Play/GetAllFacilityPitchBookings/"));
            var playBookingReponse = JsonConvert.DeserializeObject<APIResponse>(AdminHttpClient.GetHttpClientRequest("FacilityPitch/GetAllFacilityPitchTiming/"));
            var trainBookingResponse = JsonConvert.DeserializeObject<APIResponse>(AdminHttpClient.GetHttpClientRequest("Class/GetAllGroupClasses/"));
            var paymentsResponse = JsonConvert.DeserializeObject<APIResponse>(AdminHttpClient.GetHttpClientRequest("Payment/GetPaymentSummaries/"));
            var paymentsResponseSummary = JsonConvert.DeserializeObject<APIResponse>(AdminHttpClient.GetHttpClientRequest("Payment/PaymentSummaries/"));
            if (!IsTokenInvalidUsingResponse(facilityPlayersResponse, "Unathorized access.")
                && !IsTokenInvalidUsingResponse(facilitiesResponse, "Unathorized access.")
                && !IsTokenInvalidUsingResponse(coachesResponse, "Unathorized access.")
                && !IsTokenInvalidUsingResponse(usersResponse, "Unathorized access.")
                && !IsTokenInvalidUsingResponse(bookedPitchesResponse, "Unathorized access."))
            {
                var facilityPlayers = facilityPlayersResponse.Payload != null ? JsonConvert.DeserializeObject<IEnumerable<FacilityPlayer>>(facilityPlayersResponse.Payload.ToString()) : new List<FacilityPlayer>();
                var facilities = facilitiesResponse.Payload != null ? JsonConvert.DeserializeObject<IEnumerable<Facility>>(facilitiesResponse.Payload.ToString()) : new List<Facility>();
                var coaches = coachesResponse.Payload != null ? JsonConvert.DeserializeObject<IEnumerable<CoachViewModel>>(coachesResponse.Payload.ToString()) : new List<CoachViewModel>();
                var users = usersResponse.Payload != null ? JsonConvert.DeserializeObject<IEnumerable<User>>(usersResponse.Payload.ToString()) : new List<User>();
                var pitchBookings = bookedPitchesResponse != null ? JsonConvert.DeserializeObject<IEnumerable<UserPitchBooking>>(bookedPitchesResponse.Payload.ToString()) : new List<UserPitchBooking>();
                var playBookings = playBookingReponse.Payload != null ? JsonConvert.DeserializeObject<List<AddSlotViewModel>>(playBookingReponse.Payload.ToString()) : new List<AddSlotViewModel>();
                var trainingBookings = trainBookingResponse.Payload != null ? JsonConvert.DeserializeObject<List<ClassRenderViewModel>>(trainBookingResponse.Payload.ToString()) : new List<ClassRenderViewModel>();
                var payments = paymentsResponse.Payload != null ? JsonConvert.DeserializeObject<List<Payment>>(paymentsResponse.Payload.ToString()) : new List<Payment>();
                var paymentsSummary = paymentsResponseSummary.Payload != null ? JsonConvert.DeserializeObject<PaymentViewModel>(paymentsResponseSummary.Payload.ToString()) : new PaymentViewModel();

                viewModel.FacilityPlayers = facilityPlayers.Where(f => f.FacilityId == Guid.Empty);
                viewModel.Facilities = facilities;
                viewModel.Coaches = coaches;
                viewModel.Users = users;
                viewModel.Bookings = pitchBookings;
                viewModel.PlayBookings = playBookings;
                viewModel.TrainBookings = trainingBookings;
                viewModel.Payments = payments;
                viewModel.PaymentSummary = paymentsSummary;

                viewModel.TotalRevenue = viewModel.Payments.Any() ? viewModel.Payments.Sum(p => p.Amount) : default;
                if(paymentsSummary.PaymentCoachings != null)
                {
                    viewModel.TotalTrainRevenue = viewModel.PaymentSummary.PaymentCoachings.Any() ? viewModel.PaymentSummary.PaymentCoachings.Where(p => p.Status == PaymentStatus.Paid).Sum(p => p.AmountPaid) : default;
                }
                
                //viewModel.TotalPlayRevenue = viewModel.PaymentSummary.PaymentFacilityPitches.Any() ? viewModel.PaymentSummary.PaymentCoachings.Sum(p => p.AmountPaid) : default;

                var years = new List<SelectListItem>
                    {
                        new SelectListItem { Value = DateTime.Now.Year.ToString(), Text = DateTime.Now.Year.ToString() }
                    };
                foreach (var year in users)
                {
                    if (DateTime.Now.Year != year.CreatedDate.Value.Year)
                    {
                        var yearItem = new SelectListItem
                        {
                            Value = year.CreatedDate.Value.Year.ToString(),
                            Text = year.CreatedDate.Value.Year.ToString()
                        };

                        years.Add(yearItem);
                    }
                }

                viewModel.Years = years.GroupBy(s => s.Value).Select(s => s.First()).ToList();

                #region Users Report
                AddedGroupYearStats(viewModel);
                AddedGroupLastMonthStats(viewModel);
                AddedGroupThisMonthStats(viewModel);
                AddedGroupLastSevenDaysStats(viewModel);
                #endregion
                #region Bookings Report
                //AddBookingsReportForYear(viewModel);
                //AddBookingsReportForLastMonth(viewModel);
                //AddBookingsReportForCurrentMonth(viewModel);
                //AddBookingsReportForLastSevenDays(viewModel);

                AddPlayBookingsReportForYear(viewModel);
                AddPlayBookingsReportForLastMonth(viewModel);
                AddPlayBookingsReportForCurrentMonth(viewModel);
                AddPlayBookingsReportForLastSevenDays(viewModel);

                AddTrainBookingsReportForYear(viewModel);
                AddTrainBookingsReportForLastMonth(viewModel);
                AddTrainBookingsReportForCurrentMonth(viewModel);
                AddTrainBookingsReportForLastSevenDays(viewModel);

                #endregion
                #region Payment Reports
                AddTrainPaymentReportForYear(viewModel);
                AddTrainPaymentReportForLastMonth(viewModel);
                AddTrainPaymentReportForCurrentMonth(viewModel);
                AddTrainPaymentReportLastSevenDays(viewModel);
                #endregion
            }
        }

        private void AddTrainBookingsReportForCurrentMonth(DasboardViewModel viewModel)
        {
            var monthToday = DateTime.Now.Month;
            var bookingsGroup = viewModel.TrainBookings.Where(f => f.CreatedDate.Month == monthToday)
                                                                .GroupBy(f => new { Month = f.CreatedDate.Month, Year = f.CreatedDate.Year, WeekNumber = GetWeekNumberOfMonth(f.CreatedDate) })
                                                                .ToDictionary(g => g.Key, g => g.Count());
            viewModel.TrainBookingsCurrentMonth = new List<GroupLastMonthYear>();
            foreach (var item in bookingsGroup)
            {
                var model = item.PopulateLastMonthStats(new GroupLastMonthYear());
                viewModel.TrainBookingsCurrentMonth.Add(model);
            }
        }

        private void AddTrainBookingsReportForLastMonth(DasboardViewModel viewModel)
        {
            var monthToday = DateTime.Now.Month;
            var lastMonth = monthToday - 1;
            var bookingsGroup = viewModel.TrainBookings.Where(f => f.CreatedDate.Month == lastMonth)
                                                                 .GroupBy(f => new { Month = f.CreatedDate.Month, Year = f.CreatedDate.Year, WeekNumber = GetWeekNumberOfMonth(f.CreatedDate) })
                                                                 .ToDictionary(g => g.Key, g => g.Count());
            viewModel.TrainBookingsLastMonth = new List<GroupLastMonthYear>();
            foreach (var item in bookingsGroup)
            {
                var model = item.PopulateLastMonthStats(new GroupLastMonthYear());
                viewModel.TrainBookingsLastMonth.Add(model);
            }
        }

        private void AddTrainBookingsReportForYear(DasboardViewModel viewModel)
        {
            var bookingsGroup = viewModel.TrainBookings.GroupBy(f => new { Month = f.CreatedDate.Month, Year = f.CreatedDate.Year }).ToDictionary(g => g.Key, g => g.Count());
            viewModel.TrainBookingsThisYear = new List<GroupYear>();
            foreach (var item in bookingsGroup)
            {
                var model = item.PopulateThisYearStats(new GroupYear());
                viewModel.TrainBookingsThisYear.Add(model);
            }
        }

        private void AddPlayBookingsReportForCurrentMonth(DasboardViewModel viewModel)
        {
            var monthToday = DateTime.Now.Month;
            var bookingsGroup = viewModel.PlayBookings.Where(f => f.DateUpdated.Month == monthToday)
                                                                .GroupBy(f => new { Month = f.DateUpdated.Month, Year = f.DateUpdated.Year, WeekNumber = GetWeekNumberOfMonth(f.DateUpdated) })
                                                                .ToDictionary(g => g.Key, g => g.Count());
            viewModel.PlayBookingsCurrentMonth = new List<GroupLastMonthYear>();
            foreach (var item in bookingsGroup)
            {
                var model = item.PopulateLastMonthStats(new GroupLastMonthYear());
                viewModel.PlayBookingsCurrentMonth.Add(model);
            }
        }

        private void AddPlayBookingsReportForLastMonth(DasboardViewModel viewModel)
        {
            var monthToday = DateTime.Now.Month;
            var lastMonth = monthToday - 1;
            var bookingsGroup = viewModel.PlayBookings.Where(f => f.DateUpdated.Month == lastMonth)
                                                                 .GroupBy(f => new { Month = f.DateUpdated.Month, Year = f.DateUpdated.Year, WeekNumber = GetWeekNumberOfMonth(f.DateUpdated) })
                                                                 .ToDictionary(g => g.Key, g => g.Count());
            viewModel.PlayBookingsLastMonth = new List<GroupLastMonthYear>();
            foreach (var item in bookingsGroup)
            {
                var model = item.PopulateLastMonthStats(new GroupLastMonthYear());
                viewModel.PlayBookingsLastMonth.Add(model);
            }
        }

        private void AddPlayBookingsReportForYear(DasboardViewModel viewModel)
        {
            var bookingsGroup = viewModel.PlayBookings.GroupBy(f => new { Month = f.DateUpdated.Month, Year = f.DateUpdated.Year }).ToDictionary(g => g.Key, g => g.Count());
            viewModel.PlayBookingsThisYear = new List<GroupYear>();
            foreach (var item in bookingsGroup)
            {
                var model = item.PopulateThisYearStats(new GroupYear());
                viewModel.PlayBookingsThisYear.Add(model);
            }
        }

        private void AddTrainPaymentReportLastSevenDays(DasboardViewModel viewModel)
        {
            viewModel.TrainPaymentLastSevenDays = new List<LastSevenDays>();
            if (viewModel.PaymentSummary.PaymentCoachings != null)
            {
                var lastSevenDays = DateTime.Now.Date.AddDays(-7);
                var filteredTrainPayments = new List<PaymentCoachingViewModel>();
                foreach (var item in viewModel.PaymentSummary.PaymentCoachings)
                {
                    if (item.PaymentDate >= lastSevenDays)
                    {
                        filteredTrainPayments.Add(item);
                    }
                }

                if (filteredTrainPayments.Any())
                {
                    var paymentsGroup = filteredTrainPayments.Where(f => f.Status == PaymentStatus.Paid).GroupBy(f => new { Day = f.PaymentDate.Day, Year = f.PaymentDate.Year })
                                                                            .ToDictionary(g => g.Key, g => g.Sum(g => g.AmountPaid));
                    foreach (var item in paymentsGroup)
                    {
                        viewModel.TrainPaymentLastSevenDays.Add(new LastSevenDays
                        {
                            Year = item.Key.Year,
                            Day = item.Key.Day,
                            TotalAmount = item.Value
                        });
                    }
                }
            }
            
           
            
        }

        private void AddTrainPaymentReportForCurrentMonth(DasboardViewModel viewModel)
        {

            viewModel.TrainPaymentsGroupThisMonth = new List<GroupLastMonthYear>();
            if (viewModel.PaymentSummary.PaymentCoachings != null)
            {
                var monthToday = DateTime.Now.Month;
                var paymentsGroup = viewModel.PaymentSummary.PaymentCoachings.Where(f => f.PaymentDate.Month == monthToday && f.Status == PaymentStatus.Paid)
                                                                    .GroupBy(f => new { Month = f.PaymentDate.Month, Year = f.PaymentDate.Year, WeekNumber = GetWeekNumberOfMonth(f.PaymentDate) })
                                                                    .ToDictionary(g => g.Key, g => g.Sum(g => g.AmountPaid));

                foreach (var item in paymentsGroup)
                {
                    var model = item.PopulatePaymentLastMonthStats(new GroupLastMonthYear());
                    viewModel.TrainPaymentsGroupThisMonth.Add(model);
                }
            }
            
            
        }

        private void AddTrainPaymentReportForYear(DasboardViewModel viewModel)
        {
            viewModel.TrainPaymentsGroupYear = new List<GroupYear>();
            if (viewModel.PaymentSummary.PaymentCoachings != null)
            {
                var paymentsGroup = viewModel.PaymentSummary.PaymentCoachings.Where(p => p.Status == PaymentStatus.Paid)
                                                                        .GroupBy(f => new { Month = f.PaymentDate.Month, Year = f.PaymentDate.Year })
                                                                        .ToDictionary(g => g.Key, g => g.Sum(g => g.AmountPaid));

                foreach (var item in paymentsGroup)
                {
                    var model = item.PopulatePaymentYearStats(new GroupYear());
                    viewModel.TrainPaymentsGroupYear.Add(model);
                }
            }                              
        }

        private static void AddTrainPaymentReportForLastMonth(DasboardViewModel viewModel)
        {
            viewModel.TrainPaymentsGroupLastMonth = new List<GroupLastMonthYear>();
            if (viewModel.PaymentSummary.PaymentCoachings != null)
            {
                var monthToday = DateTime.Now.Month;
                var lastMonth = monthToday - 1;
                var paymentsGroup = viewModel.PaymentSummary.PaymentCoachings.Where(f => f.PaymentDate.Month == lastMonth && f.Status == PaymentStatus.Paid)
                                                                     .GroupBy(f => new { Month = f.PaymentDate.Month, Year = f.PaymentDate.Year, WeekNumber = GetWeekNumberOfMonth(f.PaymentDate) })
                                                                     .ToDictionary(g => g.Key, g => g.Sum(g => g.AmountPaid));

                foreach (var item in paymentsGroup)
                {
                    var model = item.PopulatePaymentLastMonthStats(new GroupLastMonthYear());
                    viewModel.TrainPaymentsGroupLastMonth.Add(model);
                }
            }      
        }

        private void AddTrainBookingsReportForLastSevenDays(DasboardViewModel viewModel)
        {
            var lastSevenDays = DateTime.Now.Date.AddDays(-7);
            var filteredBookings = new List<ClassRenderViewModel>();
            foreach (var item in viewModel.TrainBookings)
            {
                if (item.CreatedDate >= lastSevenDays)
                {
                    filteredBookings.Add(item);
                }
            }
            viewModel.TrainBookingsLastSeven = new List<LastSevenDays>();
            if (filteredBookings.Any())
            {
                var bookingsGroup = filteredBookings.GroupBy(f => new { Day = f.CreatedDate.Day, Year = f.CreatedDate.Year })
                                                                        .ToDictionary(g => g.Key, g => g.Count());
                foreach (var item in bookingsGroup)
                {
                    viewModel.TrainBookingsLastSeven.Add(new LastSevenDays
                    {
                        Year = item.Key.Year,
                        Day = item.Key.Day,
                        ObjectCount = item.Value
                    });
                }
            }
        }

        private void AddPlayBookingsReportForLastSevenDays(DasboardViewModel viewModel)
        {
            var lastSevenDays = DateTime.Now.Date.AddDays(-7);
            var filteredBookings = new List<AddSlotViewModel>();
            foreach (var item in viewModel.PlayBookings)
            {
                if (item.DateUpdated >= lastSevenDays)
                {
                    filteredBookings.Add(item);
                }
            }
            viewModel.PlayBookingsLastSeven = new List<LastSevenDays>();
            if (filteredBookings.Any())
            {
                var bookingsGroup = filteredBookings.GroupBy(f => new { Day = f.DateUpdated.Day, Year = f.DateUpdated.Year })
                                                                        .ToDictionary(g => g.Key, g => g.Count());
                foreach (var item in bookingsGroup)
                {
                    viewModel.PlayBookingsLastSeven.Add(new LastSevenDays
                    {
                        Year = item.Key.Year,
                        Day = item.Key.Day,
                        ObjectCount = item.Value
                    });
                }
            }
        }

        private static void AddBookingsReportForLastSevenDays(DasboardViewModel viewModel)
        {
            var lastSevenDays = DateTime.Now.Date.AddDays(-7);
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
        }

        private static void AddBookingsReportForCurrentMonth(DasboardViewModel viewModel)
        {
            var monthToday = DateTime.Now.Month;
            var bookingsGroup = viewModel.Bookings.Where(f => f.CreatedDate.Value.Month == monthToday)
                                                                .GroupBy(f => new { Month = f.CreatedDate.Value.Month, Year = f.CreatedDate.Value.Year, WeekNumber = GetWeekNumberOfMonth(f.CreatedDate.Value) })
                                                                .ToDictionary(g => g.Key, g => g.Count());
            viewModel.BookingsGroupByCurrentMonth = new List<GroupLastMonthYear>();
            foreach (var item in bookingsGroup)
            {
                var model = item.PopulateLastMonthStats(new GroupLastMonthYear());
                viewModel.BookingsGroupByCurrentMonth.Add(model);
            }
        }

        private static void AddBookingsReportForLastMonth(DasboardViewModel viewModel)
        {
            var monthToday = DateTime.Now.Month;
            var lastMonth = monthToday - 1;
            var bookingsGroup = viewModel.Bookings.Where(f => f.CreatedDate.Value.Month == lastMonth)
                                                                 .GroupBy(f => new { Month = f.CreatedDate.Value.Month, Year = f.CreatedDate.Value.Year, WeekNumber = GetWeekNumberOfMonth(f.CreatedDate.Value) })
                                                                 .ToDictionary(g => g.Key, g => g.Count());
            viewModel.BookingsGroupByLastMonth = new List<GroupLastMonthYear>();
            foreach (var item in bookingsGroup)
            {
                var model = item.PopulateLastMonthStats(new GroupLastMonthYear());
                viewModel.BookingsGroupByLastMonth.Add(model);
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

        private static void AddedGroupLastSevenDaysStats(DasboardViewModel viewModel)
        {
            var lastSevenDays = DateTime.Now.Date.AddDays(-7);

            PopulateFacilityPlayerLastSevenDays(viewModel, lastSevenDays);
            PopulateFacilitiesLastSevenDays(viewModel, lastSevenDays);
            PopulateCoachLastSevenDays(viewModel, lastSevenDays);
        }

        private static void PopulateCoachLastSevenDays(DasboardViewModel viewModel, DateTime lastSevenDays)
        {
            var filteredCoachData = new List<CoachViewModel>();
            foreach (var item in viewModel.Coaches)
            {
                if (item.DateCreated.Value >= lastSevenDays)
                {
                    filteredCoachData.Add(item);
                }
            }
            viewModel.CoachesGroupLastSeven = new List<LastSevenDays>();
            if (filteredCoachData.Any())
            {
                var coachLastSeven = filteredCoachData.GroupBy(f => new { Day = f.DateCreated.Value.Day, Year = f.DateCreated.Value.Year })
                                                                        .ToDictionary(g => g.Key, g => g.Count());
                foreach (var item in coachLastSeven)
                {
                    viewModel.CoachesGroupLastSeven.Add(new LastSevenDays
                    {
                        Year = item.Key.Year,
                        Day = item.Key.Day,
                        ObjectCount = item.Value
                    });
                }
            }
        }

        private static void PopulateFacilitiesLastSevenDays(DasboardViewModel viewModel, DateTime lastSevenDays)
        {
            var filteredFacilityData = new List<Facility>();
            foreach (var item in viewModel.Facilities)
            {
                if (item.CreatedDate.Value >= lastSevenDays)
                {
                    filteredFacilityData.Add(item);
                }
            }
            viewModel.FacilitiesGroupLastSeven = new List<LastSevenDays>();
            if (filteredFacilityData.Any())
            {
                var facilityLasySeven = filteredFacilityData.GroupBy(f => new { Day = f.CreatedDate.Value.Day, Year = f.CreatedDate.Value.Year })
                                                                        .ToDictionary(g => g.Key, g => g.Count());
                foreach (var item in facilityLasySeven)
                {
                    viewModel.FacilitiesGroupLastSeven.Add(new LastSevenDays
                    {
                        Year = item.Key.Year,
                        Day = item.Key.Day,
                        ObjectCount = item.Value
                    });
                }
            }
        }

        private static void PopulateFacilityPlayerLastSevenDays(DasboardViewModel viewModel, DateTime lastSevenDays)
        {
            var filteredFacilityPlayerData = new List<FacilityPlayer>();
            foreach (var item in viewModel.FacilityPlayers)
            {
                if (item.DateCreated.Value >= lastSevenDays)
                {
                    filteredFacilityPlayerData.Add(item);
                }
            }
            viewModel.FacilityPlayerGroupLastSeven = new List<LastSevenDays>();
            if (filteredFacilityPlayerData.Any())
            {
                var facilityPlayerLastSeven = filteredFacilityPlayerData.GroupBy(f => new { Day = f.DateCreated.Value.Day, Year = f.DateCreated.Value.Year })
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
        }

        private static void AddedGroupThisMonthStats(DasboardViewModel viewModel)
        {
            var monthToday = DateTime.Now.Month;
            var facilityPlayerGroupByMonth = viewModel.FacilityPlayers.Where(f => f.DateCreated.Value.Month == monthToday)
                                                                .GroupBy(f => new { Month = f.DateCreated.Value.Month, Year = f.DateCreated.Value.Year, WeekNumber = GetWeekNumberOfMonth(f.DateCreated.Value) })
                                                                .ToDictionary(g => g.Key, g => g.Count());

            var facilitiesGroupByMonth = viewModel.Facilities.Where(f => f.CreatedDate.Value.Month == monthToday)
                                                                .GroupBy(f => new { Month = f.CreatedDate.Value.Month, Year = f.CreatedDate.Value.Year, WeekNumber = GetWeekNumberOfMonth(f.CreatedDate.Value) })
                                                                .ToDictionary(g => g.Key, g => g.Count());
            var coachesGroupByMonth = viewModel.Coaches.Where(f => f.DateCreated.Value.Month == monthToday)
                                                                 .GroupBy(f => new { Month = f.DateCreated.Value.Month, Year = f.DateCreated.Value.Year, WeekNumber = GetWeekNumberOfMonth(f.DateCreated.Value) })
                                                                 .ToDictionary(g => g.Key, g => g.Count());

            viewModel.FacilityPlayerGroupThisMonth = new List<GroupLastMonthYear>();
            viewModel.FacilitiesGroupThisMonth = new List<GroupLastMonthYear>();
            viewModel.CoachesGroupThisMonth = new List<GroupLastMonthYear>();
            foreach (var item in facilityPlayerGroupByMonth)
            {
                var model = item.PopulateLastMonthStats(new GroupLastMonthYear());
                viewModel.FacilityPlayerGroupThisMonth.Add(model);
            }

            foreach (var item in facilitiesGroupByMonth)
            {
                var model = item.PopulateLastMonthStats(new GroupLastMonthYear());
                viewModel.FacilitiesGroupThisMonth.Add(model);
            }

            foreach (var item in coachesGroupByMonth)
            {
                var model = item.PopulateLastMonthStats(new GroupLastMonthYear());
                viewModel.CoachesGroupThisMonth.Add(model);
            }
        }

        private static void AddedGroupLastMonthStats(DasboardViewModel viewModel)
        {
            var monthToday = DateTime.Now.Month;
            var lastMonth = monthToday - 1;
            var facilityPlayerGroupBy = viewModel.FacilityPlayers.Where(f => f.DateCreated.Value.Month == lastMonth)
                                                                 .GroupBy(f => new { Month = f.DateCreated.Value.Month, Year = f.DateCreated.Value.Year, WeekNumber = GetWeekNumberOfMonth(f.DateCreated.Value) })
                                                                 .ToDictionary(g => g.Key, g => g.Count());

            var facilitiesGroupBy = viewModel.Facilities.Where(f => f.CreatedDate.Value.Month == lastMonth)
                                                                 .GroupBy(f => new { Month = f.CreatedDate.Value.Month, Year = f.CreatedDate.Value.Year, WeekNumber = GetWeekNumberOfMonth(f.CreatedDate.Value) })
                                                                 .ToDictionary(g => g.Key, g => g.Count());
            var coachesGroupBy = viewModel.Coaches.Where(f => f.DateCreated.Value.Month == lastMonth)
                                                                 .GroupBy(f => new { Month = f.DateCreated.Value.Month, Year = f.DateCreated.Value.Year, WeekNumber = GetWeekNumberOfMonth(f.DateCreated.Value) })
                                                                 .ToDictionary(g => g.Key, g => g.Count());
            viewModel.FacilityPlayerGroupLastMonth = new List<GroupLastMonthYear>();
            viewModel.FacilitiesGroupLastMonth = new List<GroupLastMonthYear>();
            viewModel.CoachesGroupLastMonth = new List<GroupLastMonthYear>();
            foreach (var item in facilityPlayerGroupBy)
            {
                var model = item.PopulateLastMonthStats(new GroupLastMonthYear());
                viewModel.FacilityPlayerGroupLastMonth.Add(model);
            }

            foreach (var item in facilitiesGroupBy)
            {
                var model = item.PopulateLastMonthStats(new GroupLastMonthYear());
                viewModel.FacilitiesGroupLastMonth.Add(model);
            }

            foreach (var item in coachesGroupBy)
            {
                var model = item.PopulateLastMonthStats(new GroupLastMonthYear());
                viewModel.CoachesGroupLastMonth.Add(model);
            }
        }

        private static int GetWeekNumberOfMonth(DateTime value)
        {
            var firstMonthDay = new DateTime(value.Year, value.Month, 1);
            var firstMonthMonday = firstMonthDay.AddDays((DayOfWeek.Monday + 7 - firstMonthDay.DayOfWeek) % 7);
            if (firstMonthMonday > value)
            {
                firstMonthDay = firstMonthDay.AddMonths(-1);
                firstMonthMonday = firstMonthDay.AddDays((DayOfWeek.Monday + 7 - firstMonthDay.DayOfWeek) % 7);
            }
            return (value - firstMonthMonday).Days / 7 + 1;
        }

        private static void AddedGroupYearStats(DasboardViewModel viewModel)
        {
            var facilityPlayerGroupBy = viewModel.FacilityPlayers.GroupBy(f => new { Month = f.DateCreated.Value.Month, Year = f.DateCreated.Value.Year }).ToDictionary(g => g.Key, g => g.Count());
            var facilitiesGroup = viewModel.Facilities.GroupBy(f => new { Month = f.CreatedDate.Value.Month, Year = f.CreatedDate.Value.Year }).ToDictionary(g => g.Key, g => g.Count());
            var coachesGroup = viewModel.Coaches.GroupBy(f => new { Month = f.DateCreated.Value.Month, Year = f.DateCreated.Value.Year }).ToDictionary(g => g.Key, g => g.Count());
            var usersGroup = viewModel.Users.GroupBy(f => new { Month = f.CreatedDate.Value.Month, Year = f.CreatedDate.Value.Year }).ToDictionary(g => g.Key, g => g.Count());
            viewModel.FacilityPlayerGroup = new List<GroupYear>();
            foreach (var item in facilityPlayerGroupBy)
            {
                var model = item.PopulateThisYearStats(new GroupYear());
                viewModel.FacilityPlayerGroup.Add(model);
            }

            viewModel.FacilitiesGroup = new List<GroupYear>();
            foreach (var item in facilitiesGroup)
            {
                var model = item.PopulateThisYearStats(new GroupYear());
                viewModel.FacilitiesGroup.Add(model);
            }

            viewModel.CoachesGroup = new List<GroupYear>();
            foreach (var item in coachesGroup)
            {
                var model = item.PopulateThisYearStats(new GroupYear());
                viewModel.CoachesGroup.Add(model);
            }
        }
    }
}
