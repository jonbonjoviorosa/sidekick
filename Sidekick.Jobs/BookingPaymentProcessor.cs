using Newtonsoft.Json;
using Sidekick.Jobs.Enum;
using Sidekick.Jobs.Helpers;
using Sidekick.Jobs.ViewModel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;

namespace Sidekick.Jobs
{
    public partial class BookingPaymentProcessor : ServiceBase
    {
        private string title = "Booking Payment Processor";
        public BookingPaymentProcessor()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            BaseUrl = ConfigurationManager.AppSettings["BaseUrl"];
            Helper.WriteToFile(title, "Started");
            Task.Run(() =>
            {
                ScheduleService().Wait();
            });
        }

        protected override void OnStop()
        {
            Helper.WriteToFile(title, "Stopped");
            this.Schedular.Dispose();
        }

        private Timer Schedular;
        private string BaseUrl;

        public async Task ScheduleService()
        {
            try
            {
                Schedular = new Timer(new TimerCallback(SchedularCallback));
                DateTime scheduledTime = DateTime.MinValue;

                int intervalMinutes = Convert.ToInt32(ConfigurationManager.AppSettings["IntervalMinutes"]);

                scheduledTime = DateTime.Now.AddMinutes(intervalMinutes);
                if (DateTime.Now > scheduledTime)
                {
                    scheduledTime = scheduledTime.AddMinutes(intervalMinutes);
                    await CallAllBookingBeforeAppointment();
                }

                TimeSpan timeSpan = scheduledTime.Subtract(DateTime.Now);
                int dueTime = Convert.ToInt32(timeSpan.TotalMilliseconds);
                Schedular.Change(dueTime, Timeout.Infinite);
            }
            catch (Exception ex)
            {
                Helper.WriteToFile(title, "Error on: " + ex.Message + ex.StackTrace);

                using (System.ServiceProcess.ServiceController serviceController = new System.ServiceProcess.ServiceController("SimpleService"))
                {
                    serviceController.Stop();
                }
            }
        }

        private async Task CallAllBookingBeforeAppointment()
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
                                    await ProcessPaymentForIndividualBooking(client, item.BookingId);
                                    break;
                            }
                        }
                    }
                }
            }
        }

        private async Task ProcessPaymentForIndividualBooking(HttpClient client, Guid bookingId)
        {
            var clientResponse = await client.GetAsync($"api/Booking/Individual/PaymentProcess/{bookingId}");
            if (clientResponse.IsSuccessStatusCode)
            {
                var response = await clientResponse.Content.ReadAsStringAsync();
                var convertedData = JsonConvert.DeserializeObject<APIResponse<TelRPaymentReponseViewModel>>(response);
                if (convertedData.Payload != null)
                {
                    if (convertedData.Payload.IsSuccess)
                    {
                        await PaymentValidated(client, EBookingType.Individual, bookingId);
                    }
                }
            }
        }

        private async Task ProcessPaymentForGroupBooking(HttpClient client, Guid bookingId)
        {
            var clientResponse = await client.GetAsync($"api/Booking/Group/PaymentProcess/{bookingId}");
            if (clientResponse.IsSuccessStatusCode)
            {
                var response = await clientResponse.Content.ReadAsStringAsync();
                var convertedData = JsonConvert.DeserializeObject<APIResponse<TelRPaymentReponseViewModel>>(response);
                if (convertedData.Payload != null)
                {
                    if (convertedData.Payload.IsSuccess)
                    {
                        await PaymentValidated(client, EBookingType.Group, bookingId);
                    }
                }
            }
        }

        private async Task PaymentValidated(HttpClient client,
            EBookingType bookingType,
            Guid bookingId)
        {
            var clientResponse = await client.GetAsync("api/Booking/UpdateBookingsToValidated");
            if (clientResponse.IsSuccessStatusCode)
            {
                var response = await clientResponse.Content.ReadAsStringAsync();
            }
        }


        private void SchedularCallback(object e)
        {
            Helper.WriteToFile(title, "Log");
            Task.Run(() =>
            {
                this.ScheduleService().Wait();
            });
        }

        
    }
}
