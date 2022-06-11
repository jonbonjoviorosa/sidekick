using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sidekick.Model.Notification
{
    public class NotificationViewModel
    {
        public bool PlayRelatedNotification { get; set; }

        public bool TrainRelatedNotification { get; set; }

        public bool BookingConfirmedNotification { get; set; }

        public bool PaymentNotification { get; set; }
        public bool NewsPromotionalNotification { get; set; }
        
    }
}
