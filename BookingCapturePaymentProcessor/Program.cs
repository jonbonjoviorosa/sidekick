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

namespace BookingPaymentProcessor
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var builder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
            var configuration = builder.Build();
            string BaseUrl = configuration["APIBaseUrl"];
            await CallAllBookingBeforeAppointment(BaseUrl);
        }

        public static async Task CallAllBookingBeforeAppointment(string BaseUrl)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(BaseUrl);
                HttpResponseMessage clientResponse = await client.GetAsync("api/Booking/AllBookingBeforeTimeOfAppointment");
                if (clientResponse.IsSuccessStatusCode)
                {
                    var response = await clientResponse.Content.ReadAsStringAsync();
                    var convertedData = JsonConvert.DeserializeObject<APIResponse<IEnumerable<BookingViewModel>>>(response);
                    if (convertedData.Payload != null)
                    {
                        foreach (var item in convertedData.Payload)
                        {
                            switch (item.BookingType)
                            {
                                case EBookingType.Individual:
                                    await ProcessPaymentForIndividualBooking(client, item.BookingId);
                                    break;
                                case EBookingType.Group:
                                    await ProcessPaymentForGroupBooking(client, item.BookingId);
                                    break;
                            }
                        }
                    }
                }
            }
        }

        private static async Task ProcessPaymentForIndividualBooking(HttpClient client, Guid bookingId)
        {
            await client.GetAsync($"api/Booking/Individual/PaymentProcess/{bookingId}");
        }

        private static async Task ProcessPaymentForGroupBooking(HttpClient client, Guid bookingId)
        {
           await client.GetAsync($"api/Booking/Group/PaymentAuthProcess/{bookingId}");
        }

    }
}
