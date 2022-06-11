using System;

namespace Sidekick.Model.UserNotification
{
    public class UserNotificationViewModel
    {
        public Guid NotificationId { get; set; }
        public Guid UserId { get; set; }
        public Guid BookingId { get; set; }
        public string NotificationTitle { get; set; }
        public string UserImage { get; set; }
        public bool BookingConfirmed { get; set; }
        public int NotificationType { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string Name { get; set; }

    }
}
