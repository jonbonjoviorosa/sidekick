using Microsoft.EntityFrameworkCore;
using Sidekick.Api.DataAccessLayer.Interfaces;
using Sidekick.Api.Helpers;
using Sidekick.Api.Helpers.IHelpers;
using Sidekick.Model.Notification;
using System.Threading.Tasks;

namespace Sidekick.Api.DataAccessLayer.Repositories
{
    public class NotificationRepository: INotificationRepository
    {
        private readonly APIDBContext context;
        private readonly IUserHelper userHelper;

        public NotificationRepository(APIDBContext context,
            IUserHelper userHelper)
        {
            this.context = context;
            this.userHelper = userHelper;
        }

        public async Task<Notification> GetNotification()
        {
            var currentLogin = userHelper.GetCurrentUserGuidLogin();
            return await context.Notifications.FirstOrDefaultAsync(x => x.UserId == currentLogin);
        }

        public async Task InsertUpdateNotification(Notification notification)
        {
            var currentLogin = userHelper.GetCurrentUserGuidLogin();
            var dateNow = Helper.GetDateTime();
            var getNotification = await GetNotification();
            if (getNotification != null)
            {
                getNotification.BookingConfirmedNotification = notification.BookingConfirmedNotification;
                getNotification.NewsPromotionalNotification = notification.NewsPromotionalNotification;
                getNotification.PaymentNotification = notification.PaymentNotification;
                getNotification.PlayRelatedNotification = notification.PlayRelatedNotification;
                getNotification.TrainRelatedNotification = notification.TrainRelatedNotification;
                getNotification.UserId = currentLogin;
                getNotification.LastEditedBy = currentLogin;
                getNotification.LastEditedDate = dateNow;
                context.Update(getNotification);
            }
            else
            {
                notification.UserId = currentLogin;
                notification.CreatedBy = currentLogin;
                notification.CreatedDate = dateNow;
                context.Notifications.Add(notification);
            }
            await context.SaveChangesAsync();
        }
    }
}
