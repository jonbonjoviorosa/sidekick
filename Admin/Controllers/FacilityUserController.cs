using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using Sidekick.Admin.DataLayer;
using Sidekick.Admin.Helpers;
using Sidekick.Model;
using System;
using System.Collections.Generic;

namespace Sidekick.Admin.Controllers
{
    public class FacilityUserController : BaseController
    {
        private IMainHttpClient MainHTTPClient { get; }
        public FacilityUserContext FacilityUCtxt { get; set; }
        private ConfigMaster MConf { get; }
        private readonly IHttpContextAccessor _httpCtxtAcc;
        private ISession _session => _httpCtxtAcc.HttpContext.Session;

        public FacilityUserController(IMainHttpClient _mhttpc, IHttpContextAccessor httpContextAccessor, ConfigMaster _conf)
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
                APIResponse returnRes = JsonConvert.DeserializeObject<APIResponse>(MainHTTPClient.GetHttpClientRequest("FacilityUser/Get/" + facilityId + "/true"));

                if (!IsTokenInvalidUsingResponse(returnRes, "Unathorized access."))
                {
                    IEnumerable<FacilityUserList> returnList = JsonConvert.DeserializeObject<IEnumerable<FacilityUserList>>(returnRes.Payload.ToString());
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
                var roleResponse = JsonConvert.DeserializeObject<APIResponse>(MainHTTPClient.GetHttpClientRequest("Facility/FacilityUserTypes/"));
                var roles = JsonConvert.DeserializeObject<IEnumerable<FacilityUserType>>(roleResponse.Payload.ToString());
                ViewBag.DefaultIcon = MConf.WebApiBaseUrl.Replace("api/", "") + "resources/Defaults/User_Default_Logo.jpg";
                var roleList = new List<SelectListItem>();
                foreach (var role in roles)
                {
                    roleList.Add(new SelectListItem
                    {
                        Text = role.FacilityRoleName,
                        Value = role.FacilityRoleId.ToString()
                    });
                }

                ViewBag.Roles = roleList;
                ViewBag.Current = "Settings";
                return View();
            }
            return RedirectToAction("Logout", "Home");
        }

        [HttpPost]
        public IActionResult Add(FacilityUserProfile _user)
        {
            if (ModelState.IsValid)
            {
                _user.FacilityId = FacilityUCtxt.FacilityUserInfo.FacilityId;
                APIResponse returnRes = JsonConvert.DeserializeObject<APIResponse>(MainHTTPClient.PostHttpClientRequest("FacilityUser/Add", _user));

                // If token is invalid, redirect to login
                if (base.IsTokenInvalidUsingResponse(returnRes, "Unathorized access.")) { return RedirectToAction("Logout", "Home"); }

                ViewBag.ModalTitle = returnRes.Message;
                if (returnRes.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    ViewBag.ShowModal = "true";
                }
                else
                {
                    ViewBag.ShowModal = "false";
                    ViewBag.DefaultIcon = _user.ImageUrl;
                }
                ViewBag.Current = "Settings";
                return View();
            }
            else
            {
                return Json(new APIResponse { StatusCode = System.Net.HttpStatusCode.BadRequest, Message = "Invalid Object!", ModelError = ModelState.Errors() });
            }
        }

        [HttpGet]
        public IActionResult Edit(Guid u)
        {
            if (IsFCUserLoggedIn(FacilityUCtxt))
            {
                var roleResponse = JsonConvert.DeserializeObject<APIResponse>(MainHTTPClient.GetHttpClientRequest("Facility/FacilityUserTypes/"));
                var roles = JsonConvert.DeserializeObject<IEnumerable<FacilityUserType>>(roleResponse.Payload.ToString());
                APIResponse returnRes = JsonConvert.DeserializeObject<APIResponse>(MainHTTPClient.GetHttpClientRequest("FacilityUser/Get/" + u + "/false"));

                if (!IsTokenInvalidUsingResponse(returnRes, "Unathorized access."))
                {
                    FacilityUserProfile result = JsonConvert.DeserializeObject<FacilityUserProfile>(returnRes.Payload.ToString());
                    var roleList = new List<SelectListItem>();
                    foreach (var role in roles)
                    {
                        roleList.Add(new SelectListItem
                        {
                            Text = role.FacilityRoleName,
                            Value = role.FacilityRoleId.ToString()
                        });
                    }

                    ViewBag.Roles = roleList;

                    return View(result);
                }

                ViewBag.DefaultIcon = MConf.WebApiBaseUrl.Replace("api/", "") + "resources/Defaults/User_Default_Logo.jpg";
                ViewBag.Current = "Settings";
                return View();
            }
            return RedirectToAction("Logout", "Home");
        }

        [HttpPost]
        public IActionResult Edit(FacilityUserProfile _user)
        {
            ModelState.Remove("Password");
            ModelState.Remove("ConfirmPassword");

            if (ModelState.IsValid)
            {
                _user.FacilityId = FacilityUCtxt.FacilityUserInfo.FacilityId;
                APIResponse returnRes = JsonConvert.DeserializeObject<APIResponse>(MainHTTPClient.PostHttpClientRequest("FacilityUser/Edit", _user));

                // If token is invalid, redirect to login
                if (base.IsTokenInvalidUsingResponse(returnRes, "Unathorized access.")) { return RedirectToAction("Logout", "Home"); }

                ViewBag.ModalTitle = returnRes.Message;
                if (returnRes.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    ViewBag.ShowModal = "true";
                }
                else
                {
                    ViewBag.ShowModal = "false";
                    ViewBag.DefaultIcon = _user.ImageUrl;
                }
                ViewBag.Current = "Settings";
                return View();
            }
            else
            {
                return Json(new APIResponse { StatusCode = System.Net.HttpStatusCode.BadRequest, Message = "Invalid Object!", ModelError = ModelState.Errors() });
            }
        }

        [HttpPost]
        public IActionResult Status([FromBody] ChangeStatus _user)
        {
            if (IsFCUserLoggedIn(FacilityUCtxt))
            {
                APIResponse returnRes = JsonConvert.DeserializeObject<APIResponse>(MainHTTPClient.PostHttpClientRequest("FacilityUser/ChangeStatus", _user));

                if (base.IsTokenInvalidUsingResponse(returnRes, "Unathorized access.")) { return RedirectToAction("Logout", "Home"); }

                return Json(returnRes);
            }
            return RedirectToAction("Logout", "Home");
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {

            return View();
        }

        [HttpPost]
        public IActionResult ChangePassword(FacilityUserChangePassword _user)
        {
            if (ModelState.IsValid)
            {
                _user.FacilityUserId = FacilityUCtxt.FacilityUserInfo.FacilityUserId;
                APIResponse returnRes = JsonConvert.DeserializeObject<APIResponse>(MainHTTPClient.PostHttpClientRequest("FacilityUser/ChangePassword", _user));

                if (base.IsTokenInvalidUsingResponse(returnRes, "Unathorized access.")) { return RedirectToAction("Logout", "Home"); }

                if (returnRes.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return RedirectToAction("Dashboard", "Facility");
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
    }

}
