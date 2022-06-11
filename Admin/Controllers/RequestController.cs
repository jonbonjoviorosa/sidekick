using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Sidekick.Admin.DataLayer;
using Sidekick.Admin.Helpers;
using Sidekick.Admin.Models;
using Sidekick.Model;
using Sidekick.Model.Enums;
using System.Collections.Generic;
using System.Linq;

namespace Sidekick.Admin.Controllers
{
    public class RequestController : BaseController
    {
        private IMainHttpClient AdminHttpClient { get; }
        public AdminUserContext AdminUCtxt { get; set; }
        private ConfigMaster MConf { get; }
        private readonly IHttpContextAccessor _httpCtxtAcc;
        private ISession _session => _httpCtxtAcc.HttpContext.Session;

        public RequestController(IMainHttpClient _mhttpc,
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
                var reportResponse = JsonConvert.DeserializeObject<APIResponse>(AdminHttpClient.GetHttpClientRequest("User/GetAllReports/"));
                var userRequestsResponse = JsonConvert.DeserializeObject<APIResponse>(AdminHttpClient.GetHttpClientRequest("User/GetUserRequests/"));
                var coachRequestsResponse = JsonConvert.DeserializeObject<APIResponse>(AdminHttpClient.GetHttpClientRequest("User/GetCoachRequests/"));
                if (!IsTokenInvalidUsingResponse(reportResponse, "Unathorized access.")
                    && !IsTokenInvalidUsingResponse(userRequestsResponse, "Unathorized access.")
                    && !IsTokenInvalidUsingResponse(coachRequestsResponse, "Unathorized access."))
                {
                    var reports = reportResponse != null ? JsonConvert.DeserializeObject<IEnumerable<ReportDto>>(reportResponse.Payload.ToString()) : new List<ReportDto>();
                    var userRequests = userRequestsResponse != null ? JsonConvert.DeserializeObject<IEnumerable<UserRequestViewModel>>(userRequestsResponse.Payload.ToString()) : new List<UserRequestViewModel>();
                    var coachRequests = coachRequestsResponse != null ? JsonConvert.DeserializeObject<IEnumerable<CoachRequestViewModel>>(coachRequestsResponse.Payload.ToString()) : new List<CoachRequestViewModel>();
                    if (reports.Any())
                    {
                        reports = reports.OrderByDescending(r => r.ReportedDate).ToList();
                    }

                    if (userRequests.Any())
                    {
                        userRequests = userRequests.OrderByDescending(r => r.Date).ToList();
                    }

                    if (coachRequests.Any())
                    {
                        coachRequests = coachRequests.OrderByDescending(r => r.Date).ToList();
                    }

                    var viewModel = new RequestsViewModel
                    {
                        UserRequests = userRequests,
                        CoachRequests = coachRequests,
                        Reports = reports
                    };

                    return View(viewModel);
                }
            }
                ViewBag.Current = "Request";
            return View();
        }

        public IActionResult UpdateOrDeleteReport(ReportDto report)
        {
            if(report.Type == RequestType.Report)
            {
                var response = JsonConvert.DeserializeObject<APIResponse>(AdminHttpClient.PostHttpClientRequest("User/UpdateReport/", report));
                if (IsTokenInvalidUsingResponse(response, "Unathorized access.")) { return RedirectToAction("Logout", "Home"); }

                ViewBag.ModalTitle = response.StatusCode == System.Net.HttpStatusCode.OK ? response.Status : response.Status;
                ViewBag.ModalMessage = response.StatusCode == System.Net.HttpStatusCode.OK ? response.Message : response.Message;
                ViewBag.ShowModal = response.StatusCode == System.Net.HttpStatusCode.OK ? "true" : "false";
                ViewBag.Current = "Request";
                return View("Index");
            }
            else if (report.Type == RequestType.UserRequest)
            {
                var response = JsonConvert.DeserializeObject<APIResponse>(AdminHttpClient.PostHttpClientRequest("User/UpdateUser/", report));
                if (IsTokenInvalidUsingResponse(response, "Unathorized access.")) { return RedirectToAction("Logout", "Home"); }

                ViewBag.ModalTitle = response.StatusCode == System.Net.HttpStatusCode.OK ? response.Status : response.Status;
                ViewBag.ModalMessage = response.StatusCode == System.Net.HttpStatusCode.OK ? response.Message : response.Message;
                ViewBag.ShowModal = response.StatusCode == System.Net.HttpStatusCode.OK ? "true" : "false";
                ViewBag.Current = "Request";
                return View("Index");
            }
            return Json(new APIResponse { StatusCode = System.Net.HttpStatusCode.BadRequest, Message = "Name is Required", ModelError = ModelState.Errors() });
        }

        [HttpPost]
        public IActionResult DeleteReport([FromBody] ReportDelete report)
        {
            if (ModelState.IsValid)
            {
                var response = JsonConvert.DeserializeObject<APIResponse>(AdminHttpClient.PostHttpClientRequest("User/DeleteReport/", report));
                if (IsTokenInvalidUsingResponse(response, "Unathorized access.")) { return RedirectToAction("Logout", "Home"); }
                return Json(new APIResponse { StatusCode = response.StatusCode, Message = response.Message });
            }

            return Json(new APIResponse { StatusCode = System.Net.HttpStatusCode.BadRequest, Message = "Name is Required", ModelError = ModelState.Errors() });
        }
    }
}
