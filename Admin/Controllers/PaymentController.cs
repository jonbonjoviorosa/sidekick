using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Sidekick.Admin.DataLayer;
using Sidekick.Admin.Helpers;
using Sidekick.Model;
using Sidekick.Model.Payment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sidekick.Admin.Controllers
{
    public class PaymentController : BaseController
    {
        private IMainHttpClient AdminHttpClient { get; }
        public AdminUserContext AdminUCtxt { get; set; }
        private ConfigMaster MConf { get; }
        private readonly IHttpContextAccessor _httpCtxtAcc;
        private ISession _session => _httpCtxtAcc.HttpContext.Session;

        public PaymentController(IMainHttpClient _mhttpc,
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
            var paymentList = new PaymentViewModel();
            if (IsUserLoggedIn(AdminUCtxt))
            {
                var response = JsonConvert.DeserializeObject<APIResponse>(AdminHttpClient.GetHttpClientRequest("Payment/PaymentSummaries/"));
                if (!IsTokenInvalidUsingResponse(response, "Unathorized access."))
                {
                    paymentList = response != null ? JsonConvert.DeserializeObject<PaymentViewModel>(response.Payload.ToString()) : new PaymentViewModel();
                }
            }
            ViewBag.Current = "Payment";
            return View(paymentList);
        }
    }
}
