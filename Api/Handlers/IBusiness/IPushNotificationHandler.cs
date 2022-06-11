using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sidekick.Api.Handlers.IBusiness
{
    public interface IPushNotificationHandler
    {
        Task SendBookingStartingPlayNotificationBefore24Hours();
        Task SendBookingStartingIndividualClassNotificationBefore24Hours();
        Task SendBookingStartingGroupClassNotificationBefore24Hours();
    }
}
