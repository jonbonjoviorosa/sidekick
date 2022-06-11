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
    public class FAQsController : BaseController
    {

        private IMainHttpClient MainHTTPClient { get; }
        public AdminUserContext AdminUCtxt { get; set; }
        private ConfigMaster MConf { get; }
        private readonly IHttpContextAccessor _httpCtxtAcc;
        private ISession _session => _httpCtxtAcc.HttpContext.Session;

        public FAQsController(IMainHttpClient _mhttpc, IHttpContextAccessor httpContextAccessor, ConfigMaster _conf)
        {
            MainHTTPClient = _mhttpc;
            _httpCtxtAcc = httpContextAccessor;
            MConf = _conf;
            AdminUCtxt = _session.GetObjectFromJson<AdminUserContext>("adminUserContext");
        }

        public IActionResult Index()
        {
            if (IsUserLoggedIn(AdminUCtxt))
            {
                APIResponse returnRes = JsonConvert.DeserializeObject<APIResponse>(MainHTTPClient.GetHttpClientRequest("FAQs/List"));

                if (!IsTokenInvalidUsingResponse(returnRes, "Unathorized access."))
                {
                    if (returnRes.Payload != null)
                    {
                        IEnumerable<FAQsDto> returnList = JsonConvert.DeserializeObject<IEnumerable<FAQsDto>>(returnRes.Payload.ToString());

                        ViewBag.Current = "FAQ";
                        return View(returnList);
                    }

                    else
                    {
                        ViewBag.Current = "FAQ";
                        TempData["Message"] = "No records found";
                        return View();
                    }

                }
            }
            return RedirectToAction("Logout", "Home");
        }

        [HttpPost]
        public IActionResult Add(FAQsDto _faq)
        {
            if (ModelState.IsValid)
            {
                APIResponse returnRes = JsonConvert.DeserializeObject<APIResponse>(MainHTTPClient.PostHttpClientRequest("FAQs/Add", _faq));

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

                ViewBag.Current = "FAQ";
                return View("Index");

            }
            else
            {
                return Json(new APIResponse { StatusCode = System.Net.HttpStatusCode.BadRequest, Message = "Invalid Object!", ModelError = ModelState.Errors() });
            }
        }

        [HttpPost]
        public IActionResult Edit(FAQsDto _faq)
        {
            if (ModelState.IsValid)
            {
                APIResponse returnRes = JsonConvert.DeserializeObject<APIResponse>(MainHTTPClient.PostHttpClientRequest("FAQs/Edit", _faq));

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

                ViewBag.Current = "FAQ";
                return View("Index");

            }

            else
            {
                return Json(new APIResponse { StatusCode = System.Net.HttpStatusCode.BadRequest, Message = "Invalid Object!", ModelError = ModelState.Errors() });
            }
        }

        [HttpPost]
        public IActionResult Status([FromBody] FAQStatus _status)
        {
            if (IsUserLoggedIn(AdminUCtxt))
            {
                _status.IsEnabledBy = AdminUCtxt.AdminInfo.AdminId;

                APIResponse returnRes = JsonConvert.DeserializeObject<APIResponse>(MainHTTPClient.PostHttpClientRequest("FAQs/Status", _status));
                if (base.IsTokenInvalidUsingResponse(returnRes, "Unathorized access.")) { return RedirectToAction("Logout", "Home"); }

                return Json(returnRes);
            }
            return RedirectToAction("Logout", "Home");
        }

        [HttpGet]
        public IActionResult TermsAndConditions()
        {
            if (IsUserLoggedIn(AdminUCtxt))
            {
                APIResponse returnRes = JsonConvert.DeserializeObject<APIResponse>(MainHTTPClient.GetHttpClientRequest("FAQs/LegalDocument/0"));
                if (base.IsTokenInvalidUsingResponse(returnRes, "Unathorized access.")) { return RedirectToAction("Logout", "Home"); }

                if (returnRes.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    LegalDocumentDto ReturnData = new LegalDocumentDto();
                    if (returnRes.Payload != null)
                    {
                        ReturnData = JsonConvert.DeserializeObject<LegalDocumentDto>(returnRes.Payload.ToString());
                    }
                    ViewBag.Current = "FAQ";
                    return View(ReturnData);
                }
                else
                {
                    ViewBag.Current = "FAQ";
                    return View(new LegalDocumentDto());
                }
            }
            return RedirectToAction("Logout", "Home");
        }

        [HttpPost]
        public IActionResult TermsAndConditions(LegalDocumentDto _termsAndCondition)
        {
            if (IsUserLoggedIn(AdminUCtxt))
            {
                _termsAndCondition.DocType = ELegalDocType.TermsAndCondition;

                APIResponse returnRes = JsonConvert.DeserializeObject<APIResponse>(MainHTTPClient.PostHttpClientRequest("FAQs/AddEditLegalDocument", _termsAndCondition));
                if (base.IsTokenInvalidUsingResponse(returnRes, "Unathorized access.")) { return RedirectToAction("Logout", "Home"); }

                ViewBag.ModalTitle = returnRes.Message;
                if (returnRes.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    LegalDocumentDto returnShop = JsonConvert.DeserializeObject<LegalDocumentDto>(returnRes.Payload.ToString());

                    ViewBag.ShowModal = "true";
                    ViewBag.Current = "FAQ";
                    return View(returnShop);
                }
                else
                {
                    ViewBag.ShowModal = "false";
                    ViewBag.Current = "FAQ";
                    return View(new LegalDocumentDto());
                }
            }
            return RedirectToAction("Logout", "Home");
        }
        
        [HttpGet]
        public IActionResult PrivacyPolicy()
        {
            if (IsUserLoggedIn(AdminUCtxt))
            {
                APIResponse returnRes = JsonConvert.DeserializeObject<APIResponse>(MainHTTPClient.GetHttpClientRequest("FAQs/LegalDocument/1"));
                if (base.IsTokenInvalidUsingResponse(returnRes, "Unathorized access.")) { return RedirectToAction("Logout", "Home"); }

                if (returnRes.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    LegalDocumentDto ReturnData = new LegalDocumentDto();
                    if (returnRes.Payload != null)
                    {
                        ReturnData = JsonConvert.DeserializeObject<LegalDocumentDto>(returnRes.Payload.ToString());
                    }
                    ViewBag.Current = "FAQ";
                    return View(ReturnData);
                }
                else
                {
                    ViewBag.Current = "FAQ";
                    return View(new LegalDocumentDto());
                }
            }
            return RedirectToAction("Logout", "Home");
        }

        [HttpPost]
        public IActionResult PrivacyPolicy(LegalDocumentDto _termsAndCondition)
        {
            if (IsUserLoggedIn(AdminUCtxt))
            {
                _termsAndCondition.DocType = ELegalDocType.PrivacyPolicy;

                APIResponse returnRes = JsonConvert.DeserializeObject<APIResponse>(MainHTTPClient.PostHttpClientRequest("FAQs/AddEditLegalDocument", _termsAndCondition));
                if (base.IsTokenInvalidUsingResponse(returnRes, "Unathorized access.")) { return RedirectToAction("Logout", "Home"); }

                ViewBag.ModalTitle = returnRes.Message;
                if (returnRes.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    LegalDocumentDto returnShop = JsonConvert.DeserializeObject<LegalDocumentDto>(returnRes.Payload.ToString());

                    ViewBag.ShowModal = "true";
                    ViewBag.Current = "FAQ";
                    return View(returnShop);
                }
                else
                {
                    ViewBag.ShowModal = "false";
                    ViewBag.Current = "FAQ";
                    return View(new LegalDocumentDto());
                }
            }
            return RedirectToAction("Logout", "Home");
        }

        [HttpGet]
        public IActionResult CancellationPolicy()
        {
            if (IsUserLoggedIn(AdminUCtxt))
            {
                APIResponse returnRes = JsonConvert.DeserializeObject<APIResponse>(MainHTTPClient.GetHttpClientRequest("FAQs/LegalDocument/2"));
                if (base.IsTokenInvalidUsingResponse(returnRes, "Unathorized access.")) { return RedirectToAction("Logout", "Home"); }

                if (returnRes.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    LegalDocumentDto ReturnData = new LegalDocumentDto();
                    if (returnRes.Payload != null)
                    {
                        ReturnData = JsonConvert.DeserializeObject<LegalDocumentDto>(returnRes.Payload.ToString());
                    }
                    ViewBag.Current = "FAQ";
                    return View(ReturnData);
                }
                else
                {
                    ViewBag.Current = "FAQ";
                    return View(new LegalDocumentDto());
                }
            }
            return RedirectToAction("Logout", "Home");
        }

        [HttpPost]
        public IActionResult CancellationPolicy(LegalDocumentDto _termsAndCondition)
        {
            if (IsUserLoggedIn(AdminUCtxt))
            {
                _termsAndCondition.DocType = ELegalDocType.CancellationPolicy;

                APIResponse returnRes = JsonConvert.DeserializeObject<APIResponse>(MainHTTPClient.PostHttpClientRequest("FAQs/AddEditLegalDocument", _termsAndCondition));
                if (base.IsTokenInvalidUsingResponse(returnRes, "Unathorized access.")) { return RedirectToAction("Logout", "Home"); }

                ViewBag.ModalTitle = returnRes.Message;
                if (returnRes.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    LegalDocumentDto returnShop = JsonConvert.DeserializeObject<LegalDocumentDto>(returnRes.Payload.ToString());

                    ViewBag.ShowModal = "true";
                    ViewBag.Current = "FAQ";
                    return View(returnShop);
                }
                else
                {
                    ViewBag.ShowModal = "false";
                    ViewBag.Current = "FAQ";
                    return View(new LegalDocumentDto());
                }
            }
            return RedirectToAction("Logout", "Home");
        }

        public IActionResult FAQList()
        {
            APIResponse returnRes = JsonConvert
                .DeserializeObject<APIResponse>
                (MainHTTPClient.GetAnnonHttpClientRequest("FAQs/List"));

            if (returnRes.StatusCode == System.Net.HttpStatusCode.OK)
            {
                List<FAQsDto> returnList = JsonConvert.DeserializeObject<List<FAQsDto>>(returnRes.Payload.ToString());
                return View(returnList);
            }
            else
            {
                return View(new List<FAQsDto>());
            }
        }

        public IActionResult Terms()
        {
            APIResponse returnRes = JsonConvert
            .DeserializeObject<APIResponse>
            (MainHTTPClient.GetAnnonHttpClientRequest("FAQs/LegalDocument/0"));

            if (returnRes.StatusCode == System.Net.HttpStatusCode.OK)
            {
                LegalDocumentDto ReturnData = new LegalDocumentDto();
                if (returnRes.Payload != null)
                {
                    ReturnData = JsonConvert.DeserializeObject<LegalDocumentDto>(returnRes.Payload.ToString());
                }
                return View(ReturnData);
            }
            else
            {
                return View(new LegalDocumentDto());
            }
        }

        public IActionResult Privacy()
        {
            APIResponse returnRes = JsonConvert
            .DeserializeObject<APIResponse>
            (MainHTTPClient.GetAnnonHttpClientRequest("FAQs/LegalDocument/1"));

            if (returnRes.StatusCode == System.Net.HttpStatusCode.OK)
            {
                LegalDocumentDto ReturnData = new LegalDocumentDto();
                if (returnRes.Payload != null)
                {
                    ReturnData = JsonConvert.DeserializeObject<LegalDocumentDto>(returnRes.Payload.ToString());
                }
                return View(ReturnData);
            }
            else
            {
                return View(new LegalDocumentDto());
            }
        }

        public IActionResult Cancellation()
        {
            APIResponse returnRes = JsonConvert
            .DeserializeObject<APIResponse>
            (MainHTTPClient.GetAnnonHttpClientRequest("FAQs/LegalDocument/2"));

            if (returnRes.StatusCode == System.Net.HttpStatusCode.OK)
            {
                LegalDocumentDto ReturnData = new LegalDocumentDto();
                if (returnRes.Payload != null)
                {
                    ReturnData = JsonConvert.DeserializeObject<LegalDocumentDto>(returnRes.Payload.ToString());
                }
                return View(ReturnData);
            }
            else
            {
                return View(new LegalDocumentDto());
            }
        }
    }
}
