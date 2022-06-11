using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace PushNotificationProcessor
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var builder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
            var configuration = builder.Build();
            string BaseUrl = configuration["APIBaseUrl"];
            await SendBookingStartingPlayPushNotificatinBefore24Hours(BaseUrl);
            await SendIndividualClassStartingPushNotificatinBefore24Hours(BaseUrl);
            await SendGroupClassStartingPushNotificatinBefore24Hours(BaseUrl);
        }

        private static async Task SendBookingStartingPlayPushNotificatinBefore24Hours(string baseUrl)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(baseUrl);
                HttpResponseMessage clientResponse = await client.GetAsync("/api/Notification/SendBookingStartingPlay");
                if (clientResponse.IsSuccessStatusCode)
                {
                    var response = await clientResponse.Content.ReadAsStringAsync();
                }
            }
        }

        private static async Task SendIndividualClassStartingPushNotificatinBefore24Hours(string baseUrl)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(baseUrl);
                HttpResponseMessage clientResponse = await client.GetAsync("/api/Notification/SendIndividualClassStarting");
                if (clientResponse.IsSuccessStatusCode)
                {
                    var response = await clientResponse.Content.ReadAsStringAsync();
                }
            }
        }

        private static async Task SendGroupClassStartingPushNotificatinBefore24Hours(string baseUrl)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(baseUrl);
                HttpResponseMessage clientResponse = await client.GetAsync("/api/Notification/SendGroupClassStarting");
                if (clientResponse.IsSuccessStatusCode)
                {
                    var response = await clientResponse.Content.ReadAsStringAsync();
                }
            }
        }
    }
}
