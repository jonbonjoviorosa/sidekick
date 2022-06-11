using ClosedXML.Excel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Sidekick.Admin.DataLayer;
using Sidekick.Admin.Helpers;
using Sidekick.Admin.Models;
using Sidekick.Model;
using Sidekick.Model.Booking;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Sidekick.Admin.Controllers
{
    public class CommissionController : BaseController
    {
        private IMainHttpClient AdminHttpClient { get; }
        public AdminUserContext AdminUCtxt { get; set; }
        private ConfigMaster MConf { get; }
        private readonly IHttpContextAccessor _httpCtxtAcc;
        private ISession _session => _httpCtxtAcc.HttpContext.Session;

        public CommissionController(IMainHttpClient _mhttpc,
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
            if (IsUserLoggedIn(AdminUCtxt))
            {
                var comissionPlayResponse = JsonConvert.DeserializeObject<APIResponse>(AdminHttpClient.GetHttpClientRequest("Commission/GetComissionPlays/"));
                var comissionTrainResponse = JsonConvert.DeserializeObject<APIResponse>(AdminHttpClient.GetHttpClientRequest("Commission/GetComissionTrains/"));
                var sportsResponse = JsonConvert.DeserializeObject<APIResponse>(AdminHttpClient.GetHttpClientRequest("Sport/Get/"));
                //var userResponse = JsonConvert.DeserializeObject<APIResponse>(AdminHttpClient.GetHttpClientRequest("User/GetUsers/"));
                var reportsResponse = JsonConvert.DeserializeObject<APIResponse>(AdminHttpClient.GetHttpClientRequest("Commission/GetComissionReport/"));
                if (!IsTokenInvalidUsingResponse(comissionPlayResponse, "Unathorized access.")
                    && !IsTokenInvalidUsingResponse(comissionTrainResponse, "Unathorized access."))
                {
                    var comissionPlays = comissionPlayResponse != null ? JsonConvert.DeserializeObject<IEnumerable<CommissionPlaySportViewModel>>(comissionPlayResponse.Payload.ToString()) : new List<CommissionPlaySportViewModel>();
                    var comissionTrains = comissionTrainResponse.Payload != null ? JsonConvert.DeserializeObject<CommissionTrain>(comissionTrainResponse.Payload.ToString()) : new CommissionTrain();
                    var sports = JsonConvert.DeserializeObject<IEnumerable<Sport>>(sportsResponse.Payload.ToString());
                    //var user = userResponse != null ? JsonConvert.DeserializeObject<IEnumerable<User>>(userResponse.Payload.ToString()) : new List<User>();
                    var comissionReports = reportsResponse.Payload != null ? JsonConvert.DeserializeObject<IEnumerable<CommisionReport>>(reportsResponse.Payload.ToString()) : new List<CommisionReport>();

                    ViewBag.CommissionReports = comissionReports;
                    ViewBag.CommissionPlays = comissionPlays;
                    ViewBag.Sports = sports;
                    ViewBag.Current = "Commission";
                    return View(comissionTrains);
                }
            }

            return View();
        }

        [HttpPost]
        public IActionResult AddOrEditCommissionPlay([FromBody] CommissionPlayView formData)
        {
            var response = JsonConvert.DeserializeObject<APIResponse>(AdminHttpClient.PostHttpClientRequest("Commission/AddOrEditCommissionPlay", formData.Plays));
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                ViewBag.ModalTitle = response.StatusCode == System.Net.HttpStatusCode.OK ? response.Status : response.Status;
                ViewBag.ModalMessage = response.StatusCode == System.Net.HttpStatusCode.OK ? response.Message : response.Message;
                ViewBag.ShowModal = response.StatusCode == System.Net.HttpStatusCode.OK ? "true" : "false";
                return Json(new APIResponse { StatusCode = System.Net.HttpStatusCode.OK, Message = response.Message });
            }

            return Json(new APIResponse { StatusCode = System.Net.HttpStatusCode.BadRequest, Message = "Name is Required", ModelError = ModelState.Errors() });
        }

        [HttpPost]
        public IActionResult AddOrEditCommissionTrains(CommissionTrain commissionTrain)
        {
            var response = JsonConvert.DeserializeObject<APIResponse>(AdminHttpClient.PostHttpClientRequest("Commission/AddOrEditCommissionTrain", commissionTrain));
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                ViewBag.ModalTitle = response.StatusCode == System.Net.HttpStatusCode.OK ? response.Status : response.Status;
                ViewBag.ModalMessage = response.StatusCode == System.Net.HttpStatusCode.OK ? response.Message : response.Message;
                ViewBag.ShowModal = response.StatusCode == System.Net.HttpStatusCode.OK ? "true" : "false";
                return View("Index");
            }

            return View("Index");
        }

        public IActionResult Export()
        {
            if (IsUserLoggedIn(AdminUCtxt))
            {
                APIResponse returnRes = JsonConvert.DeserializeObject<APIResponse>(AdminHttpClient.GetHttpClientRequest("Commission/GetComissionReport"));
                IEnumerable<CommisionReport> returnList = new List<CommisionReport>();
                if (!IsTokenInvalidUsingResponse(returnRes, "Unathorized access."))
                {
                    returnList = JsonConvert.DeserializeObject<IEnumerable<CommisionReport>>(returnRes.Payload.ToString());
                }

                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Commission Reports");
                    var currentRow = 1;
                    worksheet.Cell(currentRow, 1).Value = "Player";
                    worksheet.Cell(currentRow, 1).Style.Fill.BackgroundColor = XLColor.Yellow;
                    worksheet.Cell(currentRow, 2).Value = "Booking Type";
                    worksheet.Cell(currentRow, 2).Style.Fill.BackgroundColor = XLColor.Yellow;
                    worksheet.Cell(currentRow, 3).Value = "Total Sales";
                    worksheet.Cell(currentRow, 3).Style.Fill.BackgroundColor = XLColor.Yellow;
                    worksheet.Cell(currentRow, 4).Value = "Commission";
                    worksheet.Cell(currentRow, 4).Style.Fill.BackgroundColor = XLColor.Yellow;
                    worksheet.Cell(currentRow, 5).Value = "VAT";
                    worksheet.Cell(currentRow, 5).Style.Fill.BackgroundColor = XLColor.Yellow;

                    foreach (var item in returnList)
                    {
                        currentRow++;
                        worksheet.Cell(currentRow, 1).Value = item.FirstName + " " + item.LastName;
                        worksheet.Cell(currentRow, 2).Value = item.BookingType.ToString();
                        worksheet.Cell(currentRow, 3).Value = item.TotalSalesAmount;
                        worksheet.Cell(currentRow, 4).Value = item.CommissionAmount;
                        worksheet.Cell(currentRow, 5).Value = item.VatAmount;
                    }

                    using var stream = new MemoryStream();
                    worksheet.Columns().AdjustToContents();
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();

                    return File(
                        content,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        $"Commission Reports - {DateTime.Now}.xlsx");
                }
            }

            return View();

        }
    }
}
