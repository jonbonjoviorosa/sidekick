using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Sidekick.Admin.Models;
using Microsoft.AspNetCore.Authorization;
using Sidekick.Model;
using Sidekick.Admin.DataLayer;
using Sidekick.Admin.Helpers;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Sidekick.Admin.Controllers
{
    public class HomeController : BaseController
    {
        private IMainHttpClient HomeHttpClient { get; }

        public AdminUserContext AdminUCtxt { get; set; }
        public FacilityUserContext FacilityUCtxt { get; set; }

        private ConfigMaster MConf { get; }
        private readonly IHttpContextAccessor _httpCtxtAcc;
        private ISession _session => _httpCtxtAcc.HttpContext.Session;

        public HomeController(IMainHttpClient _mhttpc, IHttpContextAccessor httpContextAccessor, ConfigMaster _conf)
        {
            HomeHttpClient = _mhttpc;
            _httpCtxtAcc = httpContextAccessor;
            MConf = _conf;
            AdminUCtxt = _session.GetObjectFromJson<AdminUserContext>("adminUserContext");
            FacilityUCtxt = _session.GetObjectFromJson<FacilityUserContext>("facilityUserContext");
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public IActionResult Index(LoginCredentials _admin)
        {
            if (ModelState.IsValid)
            {
                _admin.Device = EDevicePlatform.Web;
                string returnResult = HomeHttpClient.PostHttpClientRequest("Admin/Login", _admin);

                try
                {
                    APIResponse ApiResp = JsonConvert.DeserializeObject<APIResponse>(returnResult);

                    if (ApiResp != null)
                    {
                        if (ApiResp.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            if (ApiResp.ResponseCode == APIResponseCode.IsSuperAdminUser)
                            {
                                AdminUCtxt = JsonConvert.DeserializeObject<AdminUserContext>(ApiResp.Payload.ToString());
                                if (AdminUCtxt != null)
                                {
                                    _session.Remove("adminUserContext");
                                    _session.Remove("facilityUserContext");
                                }
                                HttpContext.Session.SetObjectAsJson("adminUserContext", AdminUCtxt);

                                return RedirectToAction("Index", "Admin");
                            }

                            if (ApiResp.ResponseCode == APIResponseCode.IsFacilityUser)
                            {
                                FacilityUCtxt = JsonConvert.DeserializeObject<FacilityUserContext>(ApiResp.Payload.ToString());
                                                        
                                if (FacilityUCtxt != null)
                                {
                                    _session.Remove("adminUserContext");
                                    _session.Remove("facilityUserContext");
                                }
                                HttpContext.Session.SetObjectAsJson("facilityUserContext", FacilityUCtxt);
                                return RedirectToAction("Dashboard", "Facility");
                            }
                        }
                        else if (ApiResp.StatusCode == System.Net.HttpStatusCode.Redirect)
                        {
                            if (ApiResp.ResponseCode == APIResponseCode.IsSuperAdminUser)
                            {
                                AdminUCtxt = JsonConvert.DeserializeObject<AdminUserContext>(ApiResp.Payload.ToString());
                                if (AdminUCtxt != null)
                                {
                                    _session.Remove("adminUserContext");
                                    _session.Remove("facilityUserContext");
                                }
                                HttpContext.Session.SetObjectAsJson("adminUserContext", AdminUCtxt);

                                return RedirectToAction("ChangePassword", "Admin");
                            }

                            if (ApiResp.ResponseCode == APIResponseCode.IsFacilityUser)
                            {
                                FacilityUCtxt = JsonConvert.DeserializeObject<FacilityUserContext>(ApiResp.Payload.ToString());
                                if (FacilityUCtxt != null)
                                {
                                    _session.Remove("adminUserContext");
                                    _session.Remove("facilityUserContext");
                                }
                                HttpContext.Session.SetObjectAsJson("facilityUserContext", FacilityUCtxt);

                                return RedirectToAction("ChangePassword", "FacilityUser");
                            }
                        }
                        else
                        {
                            TempData["ErrorMessage"] = ApiResp.Message;
                        }
                    }
                }
                catch (Exception ex)
                {
                    string errmsg = ex.Message;
                    ModelState.AddModelError("Email", errmsg);
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Identifiants invalides";
            }

            return View(_admin);
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Logout()
        {
            if (AdminUCtxt != null)
            {
                _session.Remove("adminUserContext");
            }

            if (FacilityUCtxt != null)
            {
                _session.Remove("facilityUserContext");
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ChangePassword(UserChangePassword _admin)
        {
            if (ModelState.IsValid)
            {
                _admin.UserId = AdminUCtxt.AdminInfo.AdminId;

                APIResponse returnRes = JsonConvert.DeserializeObject<APIResponse>(HomeHttpClient.PostHttpClientRequest("Account/ChangePassword", _admin));

                // If token is invalid, redirect to login
                if (base.IsTokenInvalidUsingResponse(returnRes, "Unathorized access.")) { return RedirectToAction("Logout", "Home"); }

                if (returnRes.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    if (AdminUCtxt.AdminInfo.AdminType == EAdminType.STAFF)
                    {
                        return RedirectToAction("Dashboard", "Shop");
                    }
                    else
                    {
                        return RedirectToAction("Dashboard", "SAdmin");
                    }
                }
                else
                {
                    TempData["ErrorMessage"] = returnRes.Status + " : " + returnRes.Message;
                }

                return View();
            }
            else
            {
                TempData["ErrorMessage"] = "Invalid Object Model";
            }

            return View();
        }

        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ForgotPassword(AdminForgotPassword _admin)
        {
            if (ModelState.IsValid)
            {
                APIResponse returnRes = JsonConvert.DeserializeObject<APIResponse>(HomeHttpClient.PostHttpClientRequest("Admin/ForgotPassword", _admin));

                // If token is invalid, redirect to login
                if (base.IsTokenInvalidUsingResponse(returnRes, "Unathorized access.")) { return RedirectToAction("Logout", "Home"); }

                if (returnRes.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    EAdminType accountType = JsonConvert.DeserializeObject<EAdminType>(returnRes.Payload.ToString());

                    return RedirectToAction("PasswordReset", new { aT = (int)accountType });
                }
                else
                {
                    TempData["ErrorMessage"] = returnRes.Status + " : " + returnRes.Message;
                }

                return View();
            }
            else
            {
                TempData["ErrorMessage"] = "Invalid Object Model";
            }

            return View();
        }

        public IActionResult PasswordReset()
        {
            var accType = Request.Query["aT"];
            if (accType.Count > 0)
            {
                ViewBag.AccountType = accType;
            }

            return View();
        }


        //BASE
        [HttpPost]
        public IActionResult UploadIcon()
        {
            return Ok(new { publicUrl = UploadFile(Request.Form.Files[0], UploadTypes.UploadIcon, HomeHttpClient) });
        }

        [HttpPost]
        public IActionResult UploadDocument()
        {
            return Ok(new { publicUrl = UploadFile(Request.Form.Files[0], UploadTypes.UploadDocument, HomeHttpClient) });
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
