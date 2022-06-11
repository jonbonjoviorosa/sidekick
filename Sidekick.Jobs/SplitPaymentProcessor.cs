using Newtonsoft.Json;
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
    partial class SplitPaymentProcessor : ServiceBase
    {
        private string title = "Split Payment Processor";
        public SplitPaymentProcessor()
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
                    await CallAllPitchBookingPriorToStartDate();
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

        private async Task CallAllPitchBookingPriorToStartDate()
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(BaseUrl);
                HttpResponseMessage clientResponse = await client.GetAsync("/api/Play/GetAllPitchBookingPriorToStartDate");
                if (clientResponse.IsSuccessStatusCode)
                {
                    var response = await clientResponse.Content.ReadAsStringAsync();
                    var convertedData = JsonConvert.DeserializeObject<APIResponse<IEnumerable<UserPitchBooking>>>(response);
                    if (convertedData.Payload != null)
                    {
                        foreach (var booking in convertedData.Payload)
                        {
                            await CaptureRemainingPayments(client, booking);
                        }
                    }
                }
            }
        }

        private async Task CaptureRemainingPayments(HttpClient client, UserPitchBooking booking)
        {
            HttpResponseMessage clientResponse = await client.GetAsync($"/api/Play/Payment/Capture/false/{booking.BookingId}");
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
