using Sidekick.Api.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sidekick.Api.FireBase
{
    public interface IFirebaseRepository
    {
        Task<int> SendPushNotificationAsync(APIConfigurationManager APIConfig, FCMNotificationModel fcmmodel,ILoggerManager _logMgr);
    }
}
