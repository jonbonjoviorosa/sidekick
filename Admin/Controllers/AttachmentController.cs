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
    public class AttachmentController : BaseController
    {
        private IMainHttpClient AttachmentHttpClient { get; }

        public AdminUserContext AdminUCtxt { get; set; }
        public FacilityUserContext FacilityUCtxt { get; set; }

        private ConfigMaster MConf { get; }
        private readonly IHttpContextAccessor _httpCtxtAcc;
        private ISession _session => _httpCtxtAcc.HttpContext.Session;

        public AttachmentController(IMainHttpClient _mhttpc, IHttpContextAccessor httpContextAccessor, ConfigMaster _conf)
        {
            AttachmentHttpClient = _mhttpc;
            _httpCtxtAcc = httpContextAccessor;
            MConf = _conf;
            AdminUCtxt = _session.GetObjectFromJson<AdminUserContext>("adminUserContext");
            FacilityUCtxt = _session.GetObjectFromJson<FacilityUserContext>("facilityUserContext");
        }

        [HttpPost]
        public IActionResult UploadProfileImage()
        {
            return Ok(new { publicUrl = UploadFile(Request.Form.Files[0], UploadTypes.UploadProfileImage, AttachmentHttpClient) });
        }

        [HttpPost]
        public IActionResult UploadIconImage()
        {
            return Ok(new { publicUrl = UploadFile(Request.Form.Files[0], UploadTypes.UploadIcon, AttachmentHttpClient) });
        }

        [HttpPost]
        public IActionResult UploadBanner()
        {
            return Ok(new { publicUrl = UploadFile(Request.Form.Files[0], UploadTypes.UploadBanner, AttachmentHttpClient) });
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
