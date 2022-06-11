using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Sidekick.Api.ViewModel;
using Sidekick.Model;
using Sidekick.Model.Booking;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace PlayCapturePaymentProcessor
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var builder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
            var configuration = builder.Build();
            string BaseUrl = configuration["APIBaseUrl"];
            await CallAllPlayBeforeAppointment(BaseUrl);
        }

        public static async Task CallAllPlayBeforeAppointment(string BaseUrl)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(BaseUrl);
                HttpResponseMessage clientResponse = await client.GetAsync("api/Play/GetAllPitchBookingPriorToStartDate");
                if (clientResponse.IsSuccessStatusCode)
                {
                    var response = await clientResponse.Content.ReadAsStringAsync();
                    var convertedData = JsonConvert.DeserializeObject<APIResponse<IEnumerable<BookingViewModel>>>(response);
                    if (convertedData.Payload != null)
                    {
                        foreach (var item in convertedData.Payload)
                        {
                            await ProcessPaymentForPlay(client, item.BookingId);
                        }
                    }
                }
            }
        }

        private static async Task ProcessPaymentForPlay(HttpClient client, Guid bookingId)
        {
            var clientResponse = await client.GetAsync($"api/Play/PaymentCaptureProcess/{bookingId}");
            if (clientResponse.IsSuccessStatusCode)
            {
                var response = await clientResponse.Content.ReadAsStringAsync();
                var convertedData = JsonConvert.DeserializeObject<APIResponse<TelRPaymentReponseViewModel>>(response);
            }
        }
    }
}
