using Microsoft.EntityFrameworkCore;
using Sidekick.Api.DataAccessLayer.Interfaces;
using Sidekick.Api.Helpers;
using Sidekick.Api.Helpers.IHelpers;
using Sidekick.Model;
using Sidekick.Model.UserNotification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sidekick.Api.DataAccessLayer.Repositories
{
    public class UserNotificationRepository : IUserNotificationRepository
    {
        private readonly APIDBContext context;
        private readonly IUserHelper userHelper;
        ILoggerManager LogManager { get; }

        public UserNotificationRepository(APIDBContext context,
            IUserHelper userHelper)
        {
            this.context = context;
            this.userHelper = userHelper;
        }
        public async Task<UserNotification> GetNotification(Guid notificationId)
        {
            var currentLogin = userHelper.GetCurrentUserGuidLogin();
            return await context.UserNotifications.FirstOrDefaultAsync(x => x.NotificationId == notificationId);
        }

        public async Task<UserNotification> GetNotificationByBookingId(Guid bookingId)
        {
            return await context.UserNotifications.FirstOrDefaultAsync(x => x.BookingId == bookingId);
        }

        public async Task<List<UserNotificationViewModel>> GetUserNotification()
        {
            List<UserNotificationViewModel> userNotifications = new List<UserNotificationViewModel>();
            var userId = userHelper.GetCurrentUserGuidLogin();

            var userNotification = await (from x in context.UserNotifications
                                          join y in context.Users
                                           on x.UserId equals y.UserId
                                          where y.UserId == userId && x.IsFacility == false
                                          orderby x.Id descending
                                          select new UserNotificationViewModel()
                                          {
                                              UserId = x.UserId,
                                              BookingId = x.BookingId,
                                              CreatedDate = x.CreatedDate,
                                              NotificationId = x.NotificationId,
                                              NotificationTitle = x.NotificationTitle,
                                              NotificationType = x.NotificationType,
                                              UserImage = y.ImageUrl
                                          }).ToListAsync();

            foreach (var item in userNotification)
            {
                if (item.NotificationType == (int)ENotificationType.Individualbooking)
                {
                    var IndividualBooking = context.IndividualBookings.FirstOrDefault(s => s.BookingId == item.BookingId);
                    if (IndividualBooking != null)
                    {
                        item.BookingConfirmed = !(IndividualBooking.Status == EBookingStatus.Pending);
                    }
                }
                else if (item.NotificationType == (int)ENotificationType.Groupbooking)
                {
                    var groupBooking = context.GroupBookings.FirstOrDefault(s => s.GroupBookingId == item.BookingId);
                    if (groupBooking != null)
                    {
                        item.BookingConfirmed = !(groupBooking.Status == EBookingStatus.Pending);
                    }
                }
                else if (item.NotificationType == (int)ENotificationType.PitchBooking)
                {
                    item.BookingConfirmed = true;
                }
                userNotifications.Add(item);
            }
            return userNotifications;
        }

        public async Task InsertUpdateNotification(UserNotification notification)
        {
            var currentLogin = userHelper.GetCurrentUserGuidLogin();
            var dateNow = Helper.GetDateTime();
            var getNotification = await GetNotification(notification.NotificationId);
            if (getNotification != null)
            {
                getNotification.UserId = currentLogin;
                getNotification.LastEditedBy = currentLogin;
                getNotification.LastEditedDate = dateNow;
                context.Update(getNotification);
            }
            else
            {

                notification.NotificationId = Guid.NewGuid();
                notification.UserId = currentLogin;
                notification.CreatedBy = currentLogin;
                notification.CreatedDate = dateNow;
                context.UserNotifications.Add(notification);
            }
            await context.SaveChangesAsync();
        }

        public async Task InsertNotification(UserNotification notification)
        {
            var currentLogin = userHelper.GetCurrentUserGuidLogin();
            var dateNow = Helper.GetDateTime();
            notification.NotificationId = Guid.NewGuid();
            notification.CreatedBy = currentLogin;
            notification.CreatedDate = dateNow;
            context.UserNotifications.Add(notification);
            await context.SaveChangesAsync();
        }

        public async Task<IEnumerable<UserNotificationViewModel>> GetNotifications(Guid facilityId)
        {
            var notifications = await (from x in context.UserNotifications
                                       join y in context.Facilities
                                        on x.FacilityId equals y.FacilityId
                                       where y.FacilityId == facilityId && x.IsFacility == true
                                       orderby x.Id descending
                                       select new UserNotificationViewModel()
                                       {
                                           UserId = x.UserId,
                                           BookingConfirmed = x.BookingConfirmed,
                                           BookingId = x.BookingId,
                                           CreatedDate = x.CreatedDate,
                                           NotificationId = x.NotificationId,
                                           NotificationTitle = x.NotificationTitle,
                                           NotificationType = x.NotificationType,
                                           UserImage = y.ImageUrl,
                                       }).ToListAsync();
            var users = await context.Users.ToListAsync();
            foreach (var notification in notifications)
            {
                var user = users.Where(u => u.UserId == notification.UserId).FirstOrDefault();
                if (user != null)
                {
                    notification.Name = $"{user.FirstName} {user.LastName}";
                }
            }

            return notifications.Any() ? notifications.OrderByDescending(n => n.CreatedDate).ToList() : notifications;
        }
    }
}
