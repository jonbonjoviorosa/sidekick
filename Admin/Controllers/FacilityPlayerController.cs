using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Sidekick.Admin.DataLayer;
using Sidekick.Admin.Helpers;
using Sidekick.Model;
using System;
using System.Collections.Generic;

namespace Sidekick.Admin.Controllers
{
    public class FacilityPlayerController : BaseController
    {
        private IMainHttpClient MainHTTPClient { get; }
        public FacilityUserContext FacilityUCtxt { get; set; } 
        private ConfigMaster MConf { get; }
        private readonly IHttpContextAccessor _httpCtxtAcc;
        private ISession Session => _httpCtxtAcc.HttpContext.Session;

        public FacilityPlayerController(IMainHttpClient _mhttpc, IHttpContextAccessor httpContextAccessor, ConfigMaster _conf)
        {
            MainHTTPClient = _mhttpc;
            _httpCtxtAcc = httpContextAccessor;
            MConf = _conf;
            FacilityUCtxt = Session.GetObjectFromJson<FacilityUserContext>("facilityUserContext");
        }
        public IActionResult Index()
        {
            if (IsFCUserLoggedIn(FacilityUCtxt))
            {
                Guid facilityId = FacilityUCtxt.FacilityUserInfo.FacilityId;
                APIResponse returnRes = JsonConvert.DeserializeObject<APIResponse>(MainHTTPClient.GetHttpClientRequest("FacilityPlayer/Get/" + facilityId));

                if (!IsTokenInvalidUsingResponse(returnRes, "Unathorized access."))
                {
                    IEnumerable<FacilityPlayerViewModel> returnList = JsonConvert.DeserializeObject<IEnumerable<FacilityPlayerViewModel>>(returnRes.Payload.ToString());

                    ViewBag.Current = "Settings";
                    return View(returnList);
                }
            }
            return RedirectToAction("Logout", "Home");
        }
        [HttpPost]
        public IActionResult Delete(string userNo)
        {
            if (IsFCUserLoggedIn(FacilityUCtxt))
            {
                Guid facilityId = FacilityUCtxt.FacilityUserInfo.FacilityId;
                APIResponse returnRes = JsonConvert.DeserializeObject<APIResponse>(MainHTTPClient.PostHttpClientRequest("FacilityPlayer/Delete/" + facilityId + "/" + userNo, null));
                ViewBag.ModalTitle = returnRes.Message;
                if (!IsTokenInvalidUsingResponse(returnRes, "Unathorized access."))
                {

                    ViewBag.ShowModal = "true";
                    ViewBag.Current = "Settings";
                    return View("Index");
                }
            }
            return RedirectToAction("Logout", "Home");
        }
    }
}
