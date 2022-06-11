using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sidekick.Model.Notification
{
    [Table("Notifications")]
    public class Notification: APIBaseModel
    {
        public Guid UserId { get; set; }
        public bool PlayRelatedNotification { get; set; }

        public bool TrainRelatedNotification { get; set; }

        public bool BookingConfirmedNotification { get; set; }

        public bool PaymentNotification { get; set; }
        public bool NewsPromotionalNotification { get; set; }
    }
}
