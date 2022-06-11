using Sidekick.Model.UserNotification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sidekick.Api.DataAccessLayer.Interfaces
{
    public interface IUserNotificationRepository
    {
        Task<UserNotification> GetNotification(Guid notificationId);
        Task<UserNotification> GetNotificationByBookingId(Guid bookingId);
        Task<List<UserNotificationViewModel>> GetUserNotification();
        Task InsertUpdateNotification(UserNotification notification);
        Task InsertNotification(UserNotification notification);
        Task<IEnumerable<UserNotificationViewModel>> GetNotifications(Guid facilityId);
    }
}
