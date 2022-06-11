using System;

namespace Sidekick.Model
{
    public class FacilityTiming : APIBaseModel
    {
        public Guid FacilityId { get; set; }
        public bool IsEveryday { get; set; }
        public DayOfWeek Day { get; set; }
        public DateTime TimeStart { get; set; }
        public DateTime TimeEnd { get; set; }
    }
}
