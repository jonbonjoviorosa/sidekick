using Sidekick.Admin.DataLayer;
using Sidekick.Admin.Helpers;
using Sidekick.Model;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Sidekick.Admin.DataLayer
{
    public class MainHttpClient : IMainHttpClient
    {
        private ConfigMaster MConfig { get; }
        private IConfiguration Configuration { get; }
        public AdminUserContext AdminUCtxt { get; set; }
        public FacilityUserContext FacilityUCtxt { get; set; }
        private IHttpContextAccessor _httpCtxtAcc;
        private ISession _session => _httpCtxtAcc.HttpContext.Session;
        private int flagTokenExpires = 0;

        public MainHttpClient(IWebHostEnvironment env, IHttpContextAccessor httpContextAccessor, ConfigMaster _conf)
        {
            MConfig = _conf;
            _httpCtxtAcc = httpContextAccessor;

            if (_session != null)
            {
                AdminUCtxt = _session.GetObjectFromJson<AdminUserContext>("adminUserContext");
                FacilityUCtxt = _session.GetObjectFromJson<FacilityUserContext>("facilityUserContext");
            }
        }

        public string PostHttpClientRequest(string requestEndPoint, object content)
        {
            if (flagTokenExpires == 0 && requestEndPoint != "Admin/Login" && requestEndPoint != "Admin/ForgotPassword")
            {
                if (!IsRefreshTokenExpired() && FacilityUCtxt == null)
                {
                    return JsonConvert.SerializeObject(new APIResponse()
                    {
                        StatusCode = System.Net.HttpStatusCode.Unauthorized,
                        Status = "Error",
                        Message = "User needs to login"
                    });
                } //Token-Expired
            }

            using (var client = new HttpClient())
            {
                //Passing service base url  
                client.BaseAddress = new Uri(MConfig.WebApiBaseUrl);

                client.DefaultRequestHeaders.Clear();

                if (AdminUCtxt != null)
                {
                    //Authorization Token for POST
                    client.DefaultRequestHeaders.Add("Authorization", "bearer " + AdminUCtxt.Tokens.Token);
                    //client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Authorization", UCtxt.Token);
                }

                if (FacilityUCtxt != null)
                {
                    //Authorization Token for POST
                    client.DefaultRequestHeaders.Add("Authorization", "bearer " + FacilityUCtxt.Tokens.Token);
                    //client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Authorization", UCtxt.Token);
                }

                //Define request data format  
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                //Sending request to find web api REST service resource GetAllEmployees using HttpClient  
                HttpResponseMessage Res = client.PostAsJsonAsync(requestEndPoint, content).Result;
                //Res.Content = new StringContent()
                return Res.Content.ReadAsStringAsync().Result;

            }

        }

        public string GetHttpClientRequest(string requestEndPoint)
        {
            using (var client = new HttpClient())
            {
                //Passing service base url  
                client.BaseAddress = new Uri(MConfig.WebApiBaseUrl);

                client.DefaultRequestHeaders.Clear();

                if (AdminUCtxt != null)
                {
                    //Authorization Token for GET
                    client.DefaultRequestHeaders.Add("Authorization", "bearer " + AdminUCtxt.Tokens.Token);
                }

                if (FacilityUCtxt != null)
                {
                    //Authorization Token for GET
                    client.DefaultRequestHeaders.Add("Authorization", "bearer " + FacilityUCtxt.Tokens.Token);
                }

                //Define request data format  
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                //Sending request to find web api REST service resource GetAllEmployees using HttpClient  
                HttpResponseMessage Res = client.GetAsync(requestEndPoint).Result;
                return Res.Content.ReadAsStringAsync().Result;
            }
        }

        public bool IsRefreshTokenExpired()
        {
            if (AdminUCtxt != null && AdminUCtxt.Tokens.IsEnabled == true)
            {
                if (DateTime.Now > AdminUCtxt.Tokens.TokenExpiration)
                {
                    AdminLoginTransaction LoginTrans = new AdminLoginTransaction
                    {
                        AdminId = AdminUCtxt.Tokens.AdminId,
                        AdminType = AdminUCtxt.AdminInfo.AdminType,
                        Token = AdminUCtxt.Tokens.Token,
                        TokenExpiration = AdminUCtxt.Tokens.TokenExpiration,
                        RefreshToken = AdminUCtxt.Tokens.RefreshToken,
                        RefreshTokenExpiration = AdminUCtxt.Tokens.RefreshTokenExpiration,
                        Device = AdminUCtxt.Tokens.Device,
                        IsEnabled = AdminUCtxt.Tokens.IsEnabled
                    };

                    flagTokenExpires = 1;
                    string returnResult = this.PostHttpClientRequest("Admin/ReGenerateTokens", LoginTrans);

                    try
                    {
                        APIResponse ApiResp = JsonConvert.DeserializeObject<APIResponse>(returnResult);

                        if (ApiResp.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            if (AdminUCtxt != null)
                            {
                                _session.Remove("adminUserContext");
                            }
                            AdminUCtxt = JsonConvert.DeserializeObject<AdminUserContext>(ApiResp.Payload.ToString());
                            _session.SetObjectAsJson("adminUserContext", AdminUCtxt);
                        }

                        if (AdminUCtxt.Tokens.IsEnabled == false) { return false; } else { return true; }
                    }
                    catch (Exception ex)
                    {
                        string errmsg = ex.Message;
                    }
                }
                return true;
            }
            return false;
        }

        public string PostFileHttpCientRequest(string _rEndPoint, HttpContent _content)
        {
            if (flagTokenExpires == 0 && _rEndPoint != "Admin/AdminLogin")
            {
                if (!IsRefreshTokenExpired() && FacilityUCtxt == null) { return "1"; } //Token-Expired
            }

            using (var client = new HttpClient())
            {
                //Passing service base url  
                client.BaseAddress = new Uri(MConfig.WebApiBaseUrl);

                client.DefaultRequestHeaders.Clear();

                if (AdminUCtxt != null)
                {
                    //Authorization Token for POST
                    client.DefaultRequestHeaders.Add("Authorization", "bearer " + AdminUCtxt.Tokens.Token);
                    //client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Authorization", UCtxt.Token);
                }

                if (FacilityUCtxt != null)
                {
                    //Authorization Token for POST
                    client.DefaultRequestHeaders.Add("Authorization", "bearer " + FacilityUCtxt.Tokens.Token);
                    //client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Authorization", UCtxt.Token);
                }

                //Define request data format  
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("multipart/form-data"));
                //Sending request to find web api REST service resource GetAllEmployees using HttpClient  
                //HttpResponseMessage Res = client.PostAsJsonAsync(_rEndPoint, _content).Result;
                HttpResponseMessage Res = client.PostAsync(_rEndPoint, _content).Result;
                //Res.Content = new StringContent()
                return Res.Content.ReadAsStringAsync().Result;

            }
        }

        public string GetAnnonHttpClientRequest(string requestEndPoint)
        {
            using (var client = new HttpClient())
            {
                //Passing service base url  
                client.BaseAddress = new Uri(MConfig.WebApiBaseUrl);

                client.DefaultRequestHeaders.Clear();

                //Define request data format  
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                //Sending request to find web api REST service resource GetAllEmployees using HttpClient  
                HttpResponseMessage Res = client.GetAsync(requestEndPoint).Result;
                return Res.Content.ReadAsStringAsync().Result;
            }
        }
    }
}
