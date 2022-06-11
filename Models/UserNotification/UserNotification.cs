using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sidekick.Model.UserNotification
{
    [Table("UserNotifications")]
    public class UserNotification : APIBaseModel
    {
        public Guid FacilityId { get; set; }
        public Guid NotificationId { get; set; }
        public Guid UserId { get; set; }
        public Guid BookingId { get; set; }
        public string NotificationTitle { get; set; }
        public bool BookingConfirmed { get; set; }
        public int NotificationType { get; set; }
        public int NotificationTemplateType { get; set; }
        public bool IsFacility { get; set; }
    }
}
