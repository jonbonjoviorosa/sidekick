using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Sidekick.Admin.DataLayer;
using Sidekick.Model;

namespace Sidekick.Admin.Controllers
{
    public class BaseController : Controller
    {
        public string UploadFile(IFormFile file, UploadTypes ut,IMainHttpClient mainHttpClient)
        {
            byte[] data;
            using (var br = new BinaryReader(file.OpenReadStream()))
            {
                data = br.ReadBytes((int)file.OpenReadStream().Length);
            }
            ByteArrayContent bytes = new ByteArrayContent(data);
            MultipartFormDataContent multiC = new MultipartFormDataContent();
            multiC.Add(bytes, "file", file.FileName);
            string res = mainHttpClient.PostFileHttpCientRequest("Attachment/" + ut.ToString(), multiC);

            return JsonConvert.DeserializeObject<Attachment>(
                JsonConvert.DeserializeObject<APIResponse>(
                    res
                ).Payload.ToString()
            ).AttachmentExternalUrl;
        }

        public bool IsTokenInvalidUsingResponse(APIResponse ApiResp, string CustomMessage = "Error")
        {
            if (ApiResp != null && ApiResp.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                // Set error message
                TempData["ErrorMessage"] = CustomMessage + " : " + ApiResp.Message;
                return true;
            }

            return false;
        }

        public bool IsTokenInvalidUsingHttpClient(IMainHttpClient MainHttpClient, string CustomMessage = "Error")
        {
            if (!(MainHttpClient as MainHttpClient).IsRefreshTokenExpired())
            {
                // Set error message
                TempData["ErrorMessage"] = CustomMessage == "Error" ? "Session has expired. Please login." : (CustomMessage + " : Session has expired. Please login.");
                return true;
            }

            return false;
        }

        public bool IsUserLoggedIn(AdminUserContext adminUserContext)
        {
            if (adminUserContext != null)
            {
                return true;
            }

            return false;
        }

        public bool IsFCUserLoggedIn(FacilityUserContext facilityUserContext)
        {
            if (facilityUserContext != null)
            {
                return true;
            }

            return false;
        }

        public Dictionary<TimeSpan, string> PopulateTimeSlots(string startTime, string endTime)
        {
            var timeSlots = new Dictionary<TimeSpan, string>();

            DateTime start = DateTime.ParseExact(startTime, "HH:mm", null);
            DateTime end = DateTime.ParseExact(endTime, "HH:mm", null);

            int interval = 30;
            for (DateTime i = start; i <= end; i = i.AddMinutes(interval))
            {
                TimeSpan timespan = new TimeSpan(i.Hour, i.Minute, 00);
                var hourFormat = i.ToShortTimeString();
                var Hour = hourFormat.Split(" ");
                //var hourFormats = i.ToString("hh:mm:ss tt");
                timeSlots.Add(timespan, hourFormat);
            }

            return timeSlots;
        }
    }
}