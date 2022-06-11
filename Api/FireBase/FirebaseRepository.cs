using Newtonsoft.Json;
using Sidekick.Api.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sidekick.Api.FireBase
{
    public class FirebaseRepository : IFirebaseRepository
    {
        private readonly HttpClient httpClient;
        public FirebaseRepository(HttpClient _httpClient)
        {
            this.httpClient = _httpClient;
        }

        public async Task<int> SendPushNotificationAsync(APIConfigurationManager APIConfig, FCMNotificationModel fcmmodel, ILoggerManager _logMgr)
        {
            _logMgr.LogInfo("Send push notification Start");
            int sendStatus = 0;
            var request = new HttpRequestMessage(HttpMethod.Post, new Uri(APIConfig.PNConfig.FirebaseURL));
            request.Headers.Accept.Clear();
            request.Headers.Add("Authorization", new System.Net.Http.Headers.AuthenticationHeaderValue("key", "=" + APIConfig.PNConfig.FirebaseServerKey).ToString());
            request.Headers.Add("Sender", "id=" + APIConfig.PNConfig.FirebaseSenderId);
            request.Content = new StringContent(JsonConvert.SerializeObject(fcmmodel), Encoding.UTF8, "application/json");
            var result = await httpClient.SendAsync(request, CancellationToken.None);
            string response = result.Content.ReadAsStringAsync().Result;
            if (result.StatusCode == HttpStatusCode.OK)
            {
                _logMgr.LogInfo("Send push notification ok result");
                try
                {
                    _logMgr.LogInfo("Send push notification ok response " + response);
                    FirebaseApiResult apiResult = JsonConvert.DeserializeObject<FirebaseApiResult>(response);
                    if (apiResult != null && apiResult.success == 0)
                    {
                        _logMgr.LogInfo("Push notification not sent");
                        sendStatus = -1;
                    }
                    else
                    {
                        _logMgr.LogInfo("push notification sent successfully");
                    }
                }
                catch (Exception ex)
                {
                    _logMgr.LogInfo("Send push notification error " + ex.Message);
                    if (ex.InnerException != null)
                    {
                        _logMgr.LogInfo("Send push notification error details " + ex.InnerException.Message);
                    }
                    sendStatus = -1;
                }
            }
            else
            {
                _logMgr.LogInfo("Send push notification fail response" + result.StatusCode);
                sendStatus = -1;
            }
            _logMgr.LogInfo("Send push notification over");
            return sendStatus;
        }
    }
}
