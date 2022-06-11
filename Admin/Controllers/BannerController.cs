using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Sidekick.Model;
using Sidekick.Admin.DataLayer;
using Sidekick.Admin.Helpers;

namespace Sidekick.Admin.Controllers
{
    public class BannerController : BaseController
    {
        private IMainHttpClient MainHTTPClient { get; }
        public AdminUserContext AdminUCtxt { get; set; }
        private ConfigMaster MConf { get; }
        private readonly IHttpContextAccessor _httpCtxtAcc;
        private ISession _session => _httpCtxtAcc.HttpContext.Session;

        public BannerController(IMainHttpClient _mhttpc, IHttpContextAccessor httpContextAccessor, ConfigMaster _conf)
        {
            MainHTTPClient = _mhttpc;
            _httpCtxtAcc = httpContextAccessor;
            MConf = _conf;
            AdminUCtxt = _session.GetObjectFromJson<AdminUserContext>("adminUserContext");
        }

        [HttpGet]
        public IActionResult Index()
        {
            if (IsUserLoggedIn(AdminUCtxt))
            {
                APIResponse returnRes = JsonConvert.DeserializeObject<APIResponse>(MainHTTPClient.GetHttpClientRequest("Banner/List"));

                if (!IsTokenInvalidUsingResponse(returnRes, "Unathorized access."))
                {
                    IEnumerable<BannerList> returnList = JsonConvert.DeserializeObject<IEnumerable<BannerList>>(returnRes.Payload.ToString());

                    ViewBag.Current = "Banners";
                    return View(returnList);
                }
            }
            return RedirectToAction("Logout", "Home");
        }

        [HttpGet]
        public IActionResult Add()
        {
            if (IsUserLoggedIn(AdminUCtxt))
            {
                ViewBag.DefaultIcon = MConf.WebApiBaseUrl.Replace("api/", "") + "resources/Defaults/default-img.jpg";
                ViewBag.Facilities = JsonConvert.DeserializeObject<List<FacilityList>>(JsonConvert.DeserializeObject<APIResponse>
                (MainHTTPClient.GetHttpClientRequest("Facility/AllFacilities")).Payload.ToString());
                ViewBag.Current = "Banners";
                return View();
            }
            return RedirectToAction("Logout", "Home");
        }

        [HttpPost]
        public IActionResult Add(BannerDto _banner)
        {
            if (ModelState.IsValid)
            {
                APIResponse returnRes = JsonConvert.DeserializeObject<APIResponse>(MainHTTPClient.PostHttpClientRequest("Banner/Add", _banner));
                if (base.IsTokenInvalidUsingResponse(returnRes, "Unathorized access.")) { return RedirectToAction("Logout", "Home"); }

                ViewBag.Facilities = JsonConvert.DeserializeObject<List<FacilityList>>(JsonConvert.DeserializeObject<APIResponse>
                (MainHTTPClient.GetHttpClientRequest("Facility/AllFacilities")).Payload.ToString());

                ViewBag.ModalTitle = returnRes.Message;
                if (returnRes.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    ViewBag.ShowModal = "true";
                }
                else
                {
                    ViewBag.ShowModal = "false";
                    ViewBag.DefaultIcon = _banner.ImageUrl;
                }
                ViewBag.Current = "Banners";
                return View("Index");
            }

            else
            {
                return Json(new APIResponse { StatusCode = System.Net.HttpStatusCode.BadRequest, Message = "Invalid Object!", ModelError = ModelState.Errors() });
            }
        }
    
        [HttpGet]
        public IActionResult Edit(Guid b)
        {
            if (IsUserLoggedIn(AdminUCtxt))
            {
                APIResponse returnRes = JsonConvert.DeserializeObject<APIResponse>(MainHTTPClient.GetHttpClientRequest("Banner/GetBanner/" + b));

                if (!IsTokenInvalidUsingResponse(returnRes, "Unathorized access."))
                {
                    BannerDto result = JsonConvert.DeserializeObject<BannerDto>(returnRes.Payload.ToString());
                    ViewBag.Facilities = JsonConvert.DeserializeObject<List<FacilityList>>(JsonConvert.DeserializeObject<APIResponse>
                (MainHTTPClient.GetHttpClientRequest("Facility/AllFacilities")).Payload.ToString());
                    return View(result);
                }

                ViewBag.DefaultIcon = MConf.WebApiBaseUrl.Replace("api/", "") + "resources/Defaults/default-img.jpg";
                ViewBag.Current = "Banners";
                return View();
            }
            return RedirectToAction("Logout", "Home");
        }
    
        [HttpPost]
        public IActionResult Edit(BannerDto _banner)
        {
            if (ModelState.IsValid)
            {
                APIResponse returnRes = JsonConvert.DeserializeObject<APIResponse>(MainHTTPClient.PostHttpClientRequest("Banner/Edit", _banner));
                if (base.IsTokenInvalidUsingResponse(returnRes, "Unathorized access.")) { return RedirectToAction("Logout", "Home"); }

                ViewBag.Facilities = JsonConvert.DeserializeObject<List<FacilityList>>(JsonConvert.DeserializeObject<APIResponse>
                (MainHTTPClient.GetHttpClientRequest("Facility/AllFacilities")).Payload.ToString());

                ViewBag.ModalTitle = returnRes.Message;
                if (returnRes.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    ViewBag.ShowModal = "true";
                }
                else
                {
                    ViewBag.ShowModal = "false";
                    ViewBag.DefaultIcon = _banner.ImageUrl;
                }
                ViewBag.Current = "Banners";
                return View("Index");
            }

            else
            {
                return Json(new APIResponse { StatusCode = System.Net.HttpStatusCode.BadRequest, Message = "Invalid Object!", ModelError = ModelState.Errors() });
            }
        }

        [HttpPost]
        public IActionResult Delete(Guid _bannerID)
        {
            if (IsUserLoggedIn(AdminUCtxt))
            {
                APIResponse returnRes = JsonConvert.DeserializeObject<APIResponse>(MainHTTPClient.PostHttpClientRequest("Banner/Delete/" + _bannerID, null));

                if (base.IsTokenInvalidUsingResponse(returnRes, "Unathorized access.")) { return RedirectToAction("Logout", "Home"); }

                ViewBag.ModalTitle = returnRes.Message;

                if (returnRes.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    ViewBag.ShowModal = "true";
                }

                else
                {
                    ViewBag.ShowModal = "false";
                }

                ViewBag.Current = "Banners";
                return View("Index");

            }

            else
            {
                return Json(new APIResponse { StatusCode = System.Net.HttpStatusCode.BadRequest, Message = "Invalid Object!", ModelError = ModelState.Errors() });
            }
        }
    }
}
