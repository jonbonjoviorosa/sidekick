using Sidekick.Api.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sidekick.Api.FireBase
{
    public interface IPushNotificationTemplateRepository
    {
        Task<int> BookingStartingPlay( ILoggerManager loggerManager, List<string> DeviceFCMTokens, string FacilityName, int Hours);
        Task<int> BookingStartingTrain(ILoggerManager loggerManager, List<string> DeviceFCMTokens, string CoachName, int Hours);
        Task<int> PaymentFailPlay(APIConfigurationManager APIConfig, ILoggerManager loggerManager, List<string> DeviceFCMTokens, BookingNotificationCommonTemplate commonTemplate);
        Task<int> PaymentFailTrain(APIConfigurationManager APIConfig, ILoggerManager loggerManager, List<string> DeviceFCMTokens, BookingNotificationCommonTemplate commonTemplate);
        Task<int> InviteShareEvent(APIConfigurationManager APIConfig, ILoggerManager loggerManager, List<string> DeviceFCMTokens, BookingNotificationCommonTemplate commonTemplate);
        Task<int> CaptainAcceptsTheRequest(APIConfigurationManager APIConfig, ILoggerManager loggerManager, List<string> DeviceFCMTokens, BookingNotificationCommonTemplate commonTemplate);
        int OneSpotIsFreeFromWaitingList(APIConfigurationManager APIConfig, ILoggerManager loggerManager, List<string> DeviceFCMTokens, BookingNotificationCommonTemplate commonTemplate);
    }
}
