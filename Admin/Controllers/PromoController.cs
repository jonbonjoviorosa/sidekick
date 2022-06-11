using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Sidekick.Admin.DataLayer;
using Sidekick.Admin.Helpers;
using Sidekick.Model;
using Sidekick.Model.Enums;
using Sidekick.Model.Promo;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sidekick.Admin.Controllers
{
    public class PromoController : BaseController
    {
        private IMainHttpClient MainHTTPClient { get; }

        public AdminUserContext AdminUCtxt { get; set; }
        public FacilityUserContext FacilityUCtxt { get; set; }

        private ConfigMaster MConf { get; }
        private readonly IHttpContextAccessor _httpCtxtAcc;
        private ISession _session => _httpCtxtAcc.HttpContext.Session;
        public PromoController(IMainHttpClient _mhttpc, IHttpContextAccessor httpContextAccessor, ConfigMaster _conf)
        {
            MainHTTPClient = _mhttpc;
            _httpCtxtAcc = httpContextAccessor;
            MConf = _conf;
            AdminUCtxt = _session.GetObjectFromJson<AdminUserContext>("adminUserContext");
            FacilityUCtxt = _session.GetObjectFromJson<FacilityUserContext>("facilityUserContext");
        }
        public IActionResult Index()
        {
            if (IsUserLoggedIn(AdminUCtxt))
            {
                var response = JsonConvert.DeserializeObject<APIResponse>(MainHTTPClient.GetHttpClientRequest("Promo/Get"));
                if (!IsTokenInvalidUsingResponse(response, "Unathorized access."))
                {
                    var promos = JsonConvert.DeserializeObject<IEnumerable<Promo>>(response.Payload.ToString());
                    var playTypes = new List<Promo>();
                    var trainTypes = new List<Promo>();
                    foreach (var item in promos)
                    {
                        if((EventTypes)item.EventType == EventTypes.Play)
                        {
                            playTypes.Add(item);
                        }
                        else
                        {
                            trainTypes.Add(item);
                        }
                    }

                    ViewBag.Play = playTypes;
                    ViewBag.Train = trainTypes;
                    return View(promos);
                }
            }

            return View();

        }

        public IActionResult Add(string promoId = null)
        {
            ViewBag.Current = "Banners";
            var promo = new Promo();
            if (IsUserLoggedIn(AdminUCtxt))
            {

                var facilityResponse = JsonConvert.DeserializeObject<APIResponse>(MainHTTPClient.GetHttpClientRequest("Facility/AllFacilities/"));
                var coachResponse = JsonConvert.DeserializeObject<APIResponse>(MainHTTPClient.GetHttpClientRequest("Coach/GetCoaches"));

                if (!string.IsNullOrWhiteSpace(promoId))
                {
                    var promoResponse = JsonConvert.DeserializeObject<APIResponse>(MainHTTPClient.GetHttpClientRequest($"Promo/Promo/{new Guid(promoId)}"));
                    if (!IsTokenInvalidUsingResponse(promoResponse, "Unathorized access."))
                    {
                        ViewBag.IsEdit = true;
                        promo = JsonConvert.DeserializeObject<Promo>(promoResponse.Payload.ToString());
                    }
                }
                else
                {
                    ViewBag.IsEdit = false;
                }
                
                if (!IsTokenInvalidUsingResponse(facilityResponse, "Unathorized access."))
                {
                    var facilities = JsonConvert.DeserializeObject<IEnumerable<Facility>>(facilityResponse.Payload.ToString());
                    var coaches = JsonConvert.DeserializeObject<IEnumerable<CoachViewModel>>(coachResponse.Payload.ToString());
                    ViewBag.Facilities = facilities;
                    ViewBag.Coaches = coaches.Any() ? coaches.Where(f => f.Status == "Active").ToList() : coaches;
                }
            }
            return View(promo);
        }

        [HttpPost]
        public IActionResult AddOrEditPromo(Promo promo)
        {
            if (ModelState.IsValid)
            {
                if (IsUserLoggedIn(AdminUCtxt))
                {
                    promo.AllCoaches = promo.CoachId == Guid.Empty;
                    var response = JsonConvert.DeserializeObject<APIResponse>(MainHTTPClient.PostHttpClientRequest("Promo/AddOrEdit/", promo));

                    if (IsTokenInvalidUsingResponse(response, "Unathorized access.")) { return RedirectToAction("Logout", "Home"); }

                    ViewBag.ModalTitle = response.StatusCode == System.Net.HttpStatusCode.OK ? response.Status : response.Status;
                    ViewBag.ModalMessage = response.StatusCode == System.Net.HttpStatusCode.OK ? response.Message : response.Message;
                    ViewBag.ShowModal = response.StatusCode == System.Net.HttpStatusCode.OK ? "true" : "false";
                    ViewBag.IsEdit = false;

                    ViewBag.Current = "Banners";
                    return View("Add");
                }
            }

            return Json(new APIResponse { StatusCode = System.Net.HttpStatusCode.BadRequest, Message = "Name is Required", ModelError = ModelState.Errors() });
        }

        public IActionResult DeletePromo(string promoId)
        {
            if (!string.IsNullOrWhiteSpace(promoId.ToString()) && ModelState.IsValid)
            {
                if (IsUserLoggedIn(AdminUCtxt))
                {
                    var response = JsonConvert.DeserializeObject<APIResponse>(MainHTTPClient.PostHttpClientRequest($"Promo/Delete/{promoId}", new Guid(promoId)));

                    if (IsTokenInvalidUsingResponse(response, "Unathorized access.")) { return RedirectToAction("Logout", "Home"); }

                    ViewBag.ModalTitle = response.StatusCode == System.Net.HttpStatusCode.OK ? response.Status : response.Status;
                    ViewBag.ModalMessage = response.StatusCode == System.Net.HttpStatusCode.OK ? response.Message : response.Message;
                    ViewBag.ShowModal = response.StatusCode == System.Net.HttpStatusCode.OK ? "true" : "false";
                    ViewBag.Current = "Setup";
                    return View("Index");
                }
            }

            return Json(new APIResponse { StatusCode = System.Net.HttpStatusCode.BadRequest, Message = "Name is Required", ModelError = ModelState.Errors() });
        }
    }
}
