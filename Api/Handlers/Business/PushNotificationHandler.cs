using AutoMapper;
using Sidekick.Api.DataAccessLayer.Interfaces;
using Sidekick.Api.FireBase;
using Sidekick.Api.Handlers.IBusiness;
using Sidekick.Api.Helpers;
using Sidekick.Api.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sidekick.Api.Handlers.Business
{
    public class PushNotificationHandler : IPushNotificationHandler
    {
        private readonly IUserPitchBookingRepository userPitchBookingRepository;
        private readonly IUserRepository userRepository;
        private readonly ILoggerManager loggerManager;
        private readonly APIConfigurationManager APIConfig;
        private readonly IPushNotificationTemplateRepository pushNotificationTemplateRepository;
        private readonly IUserDevicesRepository userDevicesRepository;
        private readonly IBookingRepository bookingRepository;
        public PushNotificationHandler(IUserPitchBookingRepository userPitchBookingRepository, IUserRepository userRepository, ILoggerManager loggerManager, APIConfigurationManager APIConfig, IPushNotificationTemplateRepository pushNotificationTemplateRepository, IUserDevicesRepository userDevicesRepository, IBookingRepository bookingRepository)
        {
            this.userPitchBookingRepository = userPitchBookingRepository;
            this.userRepository = userRepository;
            this.loggerManager = loggerManager;
            this.APIConfig = APIConfig;
            this.pushNotificationTemplateRepository = pushNotificationTemplateRepository;
            this.userDevicesRepository = userDevicesRepository;
            this.bookingRepository = bookingRepository;
        }

        public async Task SendBookingStartingPlayNotificationBefore24Hours()
        {
            DateTime startDate = DateTime.Now.AddHours(24);
            startDate = new DateTime(startDate.Year, startDate.Month, startDate.Day, startDate.Hour, 0, 0); // for start hour from 0 seconds
            DateTime endDate = startDate.AddHours(1).AddTicks(-1);// for end hour on last tick of hour

            var PushNotificationList = await userPitchBookingRepository.GetMatchListForPushNotification(startDate, endDate);

            if (PushNotificationList != null && PushNotificationList.Any())
            {
                foreach (var PushNotificationItem in PushNotificationList)
                {
                    // get device token
                    var fcmTokenDetails = await userDevicesRepository.GetLatestDeviceFcmToken(PushNotificationItem.UserId);
                    if (fcmTokenDetails != null && !string.IsNullOrWhiteSpace(fcmTokenDetails.DeviceFCMToken))
                    {
                        // send notification
                        List<string> DeviceFCMTokens = new List<string>() { fcmTokenDetails.DeviceFCMToken };
                        await pushNotificationTemplateRepository.BookingStartingPlay(loggerManager, DeviceFCMTokens, PushNotificationItem.FacilityName, 24);
                    }
                }
            }
        }

        public async Task SendBookingStartingIndividualClassNotificationBefore24Hours()
        {
            DateTime startDate = DateTime.Now.AddHours(24);
            startDate = new DateTime(startDate.Year, startDate.Month, startDate.Day, startDate.Hour, 0, 0); // for start hour from 0 seconds
            DateTime endDate = startDate.AddHours(1).AddTicks(-1);// for end hour on last tick of hour

            var PushNotificationList = await bookingRepository.GetIndividualClassListForPushNotification(startDate, endDate);

            if (PushNotificationList != null && PushNotificationList.Any())
            {
                // check for start and end datetime betweeen.

                PushNotificationList = PushNotificationList.Where(x => x.BookingDate.AddHours(Convert.ToDateTime(x.StartTime).Hour) >= startDate
                && x.BookingDate.AddHours(Convert.ToDateTime(x.StartTime).Hour) <= endDate).ToList();


                foreach (var PushNotificationItem in PushNotificationList)
                {
                    // get device token
                    var fcmTokenDetails = await userDevicesRepository.GetLatestDeviceFcmToken(PushNotificationItem.UserId);
                    if (fcmTokenDetails != null && !string.IsNullOrWhiteSpace(fcmTokenDetails.DeviceFCMToken))
                    {
                        // send notification
                        List<string> DeviceFCMTokens = new List<string>() { fcmTokenDetails.DeviceFCMToken };
                        await pushNotificationTemplateRepository.BookingStartingTrain(loggerManager, DeviceFCMTokens, PushNotificationItem.CoachName, 24);
                    }
                }
            }
        }

        public async Task SendBookingStartingGroupClassNotificationBefore24Hours()
        {
            DateTime startDate = DateTime.Now.AddHours(24);
            startDate = new DateTime(startDate.Year, startDate.Month, startDate.Day, startDate.Hour, 0, 0); // for start hour from 0 seconds
            DateTime endDate = startDate.AddHours(1).AddTicks(-1);// for end hour on last tick of hour

            var PushNotificationList = await bookingRepository.GetGroupClassListForPushNotification(startDate, endDate);

            if (PushNotificationList != null && PushNotificationList.Any())
            {
                foreach (var PushNotificationItem in PushNotificationList)
                {
                    // get device token
                    var fcmTokenDetails = await userDevicesRepository.GetLatestDeviceFcmToken(PushNotificationItem.UserId);
                    if (fcmTokenDetails != null && !string.IsNullOrWhiteSpace(fcmTokenDetails.DeviceFCMToken))
                    {
                        // send notification
                        List<string> DeviceFCMTokens = new List<string>() { fcmTokenDetails.DeviceFCMToken };
                        await pushNotificationTemplateRepository.BookingStartingTrain(loggerManager, DeviceFCMTokens, PushNotificationItem.CoachName, 24);
                    }
                }
            }
        }
    }
}
