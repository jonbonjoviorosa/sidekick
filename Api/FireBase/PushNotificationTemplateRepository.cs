using Sidekick.Api.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sidekick.Api.FireBase
{
    public class PushNotificationTemplateRepository : IPushNotificationTemplateRepository
    {
        IFirebaseRepository firebaseRepository { get; }
        private readonly APIConfigurationManager APIConfig;
        public PushNotificationTemplateRepository(IFirebaseRepository firebaseRepository, APIConfigurationManager APIConfig)
        {
            this.firebaseRepository = firebaseRepository;
            this.APIConfig = APIConfig;
        }


        public async Task<int> BookingStartingPlay(ILoggerManager loggerManager, List<string> DeviceFCMTokens, string FacilityName, int Hours)
        {
            Notification notification = new Notification();
            notification.title = APIConfig.pushNotificationTemplateConfig.BookingStartingPlaySubject;
            notification.body = string.Format(APIConfig.pushNotificationTemplateConfig.BookingStartingPlay, FacilityName, Hours);
            FCMNotificationModel fCMNotificationModel = new FCMNotificationModel
            {
                priority = "high",
                registration_ids = DeviceFCMTokens,
                notification = notification
            };

            return await firebaseRepository.SendPushNotificationAsync(APIConfig, fCMNotificationModel, loggerManager);
        }

        public async Task<int> BookingStartingTrain(ILoggerManager loggerManager, List<string> DeviceFCMTokens, string CoachName, int Hours)
        {
            Notification notification = new Notification();
            notification.title = APIConfig.pushNotificationTemplateConfig.BookingStartingTrainSubject;
            notification.body = string.Format(APIConfig.pushNotificationTemplateConfig.BookingStartingTrain, CoachName, Hours);
            FCMNotificationModel fCMNotificationModel = new FCMNotificationModel
            {
                priority = "high",
                registration_ids = DeviceFCMTokens,
                notification = notification
            };

            return await firebaseRepository.SendPushNotificationAsync(APIConfig, fCMNotificationModel, loggerManager);
        }

        public async Task<int> PaymentFailPlay(APIConfigurationManager APIConfig, ILoggerManager loggerManager, List<string> DeviceFCMTokens, BookingNotificationCommonTemplate commonTemplate)
        {
            Notification notification = new Notification();
            notification.title = APIConfig.pushNotificationTemplateConfig.PaymentFailPlaySubject;
            notification.body = string.Format(APIConfig.pushNotificationTemplateConfig.PaymentFailPlay, 
                commonTemplate.UserName,
                commonTemplate.Sport,
                commonTemplate.FacilityName,
                commonTemplate.BookingDate.ToString("dd-MM-yyyy"),
                commonTemplate.BookingTime,
                commonTemplate.Location,
                commonTemplate.PricePerPlayer,
                commonTemplate.ServiceFees,
                commonTemplate.TotalAmount
                );
            FCMNotificationModel fCMNotificationModel = new FCMNotificationModel
            {
                priority = "high",
                registration_ids = DeviceFCMTokens,
                notification = notification
            };

            return  await firebaseRepository.SendPushNotificationAsync(APIConfig, fCMNotificationModel, loggerManager);
        }

        public async Task<int> PaymentFailTrain(APIConfigurationManager APIConfig, ILoggerManager loggerManager, List<string> DeviceFCMTokens, BookingNotificationCommonTemplate commonTemplate)
        {
            Notification notification = new Notification();
            notification.title = APIConfig.pushNotificationTemplateConfig.PaymentFailTrainSubject;
            notification.body = string.Format(APIConfig.pushNotificationTemplateConfig.PaymentFailTrain,
                commonTemplate.UserName,
                commonTemplate.Type,
                commonTemplate.Activity,
                commonTemplate.CoachName,
                commonTemplate.BookingDate.ToString("dd-MM-yyyy"),
                commonTemplate.BookingTime,
                commonTemplate.Location,
                commonTemplate.PriceCoaching,
                commonTemplate.ServiceFees,
                commonTemplate.TotalAmount
                );
            FCMNotificationModel fCMNotificationModel = new FCMNotificationModel
            {
                priority = "high",
                registration_ids = DeviceFCMTokens,
                notification = notification
            };

            return await firebaseRepository.SendPushNotificationAsync(APIConfig, fCMNotificationModel, loggerManager);
        }

        public async Task<int> InviteShareEvent(APIConfigurationManager APIConfig, ILoggerManager loggerManager, List<string> DeviceFCMTokens, BookingNotificationCommonTemplate commonTemplate)
        {
            Notification notification = new Notification();
            notification.title = APIConfig.pushNotificationTemplateConfig.ShareEventSubject;
            notification.body = string.Format(APIConfig.pushNotificationTemplateConfig.ShareEvent,
                commonTemplate.CaptainName,
                commonTemplate.Sport,
                commonTemplate.FacilityName,
                commonTemplate.BookingDate.ToString("dd-MM-yyyy"),
                commonTemplate.BookingTime
                );
            FCMNotificationModel fCMNotificationModel = new FCMNotificationModel
            {
                priority = "high",
                registration_ids = DeviceFCMTokens,
                notification = notification
            };

            return await firebaseRepository.SendPushNotificationAsync(APIConfig, fCMNotificationModel, loggerManager);
        }

        public async Task <int> CaptainAcceptsTheRequest(APIConfigurationManager APIConfig, ILoggerManager loggerManager, List<string> DeviceFCMTokens, BookingNotificationCommonTemplate commonTemplate)
        {
            Notification notification = new Notification();
            notification.title = APIConfig.pushNotificationTemplateConfig.CaptainAcceptsTheRequestSubject;
            notification.body = string.Format(APIConfig.pushNotificationTemplateConfig.CaptainAcceptsTheRequest,
                commonTemplate.CaptainName,
                commonTemplate.FacilityName,
                commonTemplate.BookingDate.ToString("dd-MM-yyyy"),
                commonTemplate.BookingTime
                );
            FCMNotificationModel fCMNotificationModel = new FCMNotificationModel
            {
                priority = "high",
                registration_ids = DeviceFCMTokens,
                notification = notification
            };

            return await firebaseRepository.SendPushNotificationAsync(APIConfig, fCMNotificationModel, loggerManager);
        }

        public int OneSpotIsFreeFromWaitingList(APIConfigurationManager APIConfig, ILoggerManager loggerManager, List<string> DeviceFCMTokens, BookingNotificationCommonTemplate commonTemplate)
        {
            Notification notification = new Notification();
            notification.title = APIConfig.pushNotificationTemplateConfig.OneSpotIsFreeFromWaitingListSubject;
            notification.body = string.Format(APIConfig.pushNotificationTemplateConfig.OneSpotIsFreeFromWaitingList,
                commonTemplate.Sport,
                commonTemplate.FacilityName,
                commonTemplate.BookingDate.ToString("dd-MM-yyyy"),
                commonTemplate.BookingTime
                );
            FCMNotificationModel fCMNotificationModel = new FCMNotificationModel
            {
                priority = "high",
                registration_ids = DeviceFCMTokens,
                notification = notification
            };

            return firebaseRepository.SendPushNotificationAsync(APIConfig, fCMNotificationModel, loggerManager).GetAwaiter().GetResult();
        }
    }
}
