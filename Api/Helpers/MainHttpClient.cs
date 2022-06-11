using Sidekick.Model;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Sidekick.Api.Helpers
{
    public class MainHttpClient : IMainHttpClient
    {
        private string FirebaseLink = "";
        private string FirebaseAPIKeyIOS = "";
        private string iOSSenderID = "";
        private string FirebaseAPIKeyAndroid = "";
        private string androidSenderKey = "";
        private APIConfigurationManager Configuration { get; }

        private IHttpContextAccessor _httpCtxtAcc;
        private ISession _session => _httpCtxtAcc.HttpContext.Session;

        public MainHttpClient(IWebHostEnvironment env, IHttpContextAccessor httpContextAccessor, APIConfigurationManager _config)
        {
            Configuration = _config;
            FirebaseLink = Configuration.PNConfig.FireBLink;
            FirebaseAPIKeyIOS = Configuration.PNConfig.IOSFireBKey;
            iOSSenderID = Configuration.PNConfig.IOSSenderID;
            FirebaseAPIKeyAndroid = Configuration.PNConfig.AndroidFireBKey;
            androidSenderKey = Configuration.PNConfig.AndroidSenderID;
            _httpCtxtAcc = httpContextAccessor;
        }

        public MainHttpClient(APIConfigurationManager _config) {
            Configuration = _config;
        }

        public string PostHttpClientRequest(string requestEndPoint, object content, int? platform)
        {
            using (var client = new HttpClient())
            {
                //Passing service base url  
                client.BaseAddress = new Uri(FirebaseLink);

                client.DefaultRequestHeaders.Clear();

                if (platform == 1) //iOS
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("key", "=" + FirebaseAPIKeyIOS);
                    client.DefaultRequestHeaders.Add("Sender", iOSSenderID);
                }
                else if (platform == 2) //android
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("key", "=" + FirebaseAPIKeyAndroid);
                    client.DefaultRequestHeaders.Add("Sender", androidSenderKey);
                }
                else //welp
                {
                    //nothing to see here
                }


                //Define request data format  
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                //Sending request to find web api REST service resource GetAllEmployees using HttpClient  
                HttpResponseMessage Res = client.PostAsJsonAsync(requestEndPoint, content).Result;
                //Res.Content = new StringContent()
                return Res.Content.ReadAsStringAsync().Result;

            }
        }

        public async Task<string> PostHttpClientRequestAsync(string requestEndPoint, object content, int? platform)
        {
            using (var client = new HttpClient())
            {
                //Passing service base url  
                client.BaseAddress = new Uri(FirebaseLink);
                client.DefaultRequestHeaders.Clear();

                if (platform == 1) //iOS
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("key", "=" + FirebaseAPIKeyIOS);
                    client.DefaultRequestHeaders.Add("Sender", iOSSenderID);
                }
                else if (platform == 2) //android
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("key", "=" + FirebaseAPIKeyAndroid);
                    client.DefaultRequestHeaders.Add("Sender", androidSenderKey);
                }

                //Define request data format  
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                //Sending request to find web api REST service resource GetAllEmployees using HttpClient  
                HttpResponseMessage Res = (await client.PostAsJsonAsync(requestEndPoint, content));

                return Res.Content.ReadAsStringAsync().Result;
            }
        }

        public string GetHttpClientRequest(string requestEndPoint)
        {
            using (var client = new HttpClient())
            {
                //Passing service base url  
                client.BaseAddress = new Uri(FirebaseLink);

                client.DefaultRequestHeaders.Clear();

                //Define request data format  
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                //Sending request to find web api REST service resource GetAllEmployees using HttpClient  
                HttpResponseMessage Res = client.GetAsync(requestEndPoint).Result;
                return Res.Content.ReadAsStringAsync().Result;
            }
        }

        public async Task<string> SendSmsHttpClientRequestAsync(string _smsParameters)
        {
            using (var client = new HttpClient())
            {
                //Passing service base url  
                client.BaseAddress = new Uri(Configuration.SmsConfig.Endpoint);

                client.DefaultRequestHeaders.Clear();

                //Define request data format  
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/plain"));

                //Sending request to find web api REST service resource GetAllEmployees using HttpClient  
                HttpResponseMessage Res = await client.GetAsync(_smsParameters);
                return await Res.Content.ReadAsStringAsync();
            }
        }

        #region Telr
        public string TelrHttpClient(string url, object content) 
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(url);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded"));
                string newStringcontent = JsonConvert.SerializeObject(content);
                Dictionary<string, string> newDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(newStringcontent);
                HttpResponseMessage Res = client.PostAsync("",new FormUrlEncodedContent(newDict)).Result;
                string retyr = Res.Content.ReadAsStringAsync().Result;
                return Res.Content.ReadAsStringAsync().Result;
            }
        }
        #endregion

        #region Apple Sign In
        public string PostAppleHttpClientRequest(string requestEndPoint, object content)
        {
            using var client = new HttpClient();
            client.BaseAddress = new Uri(requestEndPoint);
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            HttpResponseMessage Res = client.PostAsJsonAsync(requestEndPoint, content).Result;

            return Res.Content.ReadAsStringAsync().Result;
        }
        #endregion
    }
}
