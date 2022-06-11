using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sidekick.Api.ViewModel
{
    public class MatchListForPushNotificationViewModel
    {
        public Guid UserId { get; set; }
        public Guid BookingId { get; set; }
        public Guid FacilityId { get; set; }
        public string FacilityName { get; set; }
        public DateTime BookingDate { get; set; }
    }

    public class TrainingListForPushNotificationViewModel
    {
        public Guid UserId { get; set; }
        public Guid BookingId { get; set; }
        public Guid CoachId { get; set; }
        public string CoachName { get; set; }
        public DateTime BookingDate { get; set; }
        public string StartTime { get; set; }
    }

}
