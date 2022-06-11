using System;

namespace Sidekick.Model
{
    public class TimingCalendarViewModel
    {
        public Guid FacilityPitchId { get; set; }
        public Guid FacilityPitchTimingId { get; set; }
        public Guid BookingId { get; set; }
        public string FacilityName { get; set; }
        public string AreaName { get; set; }
        public string FacilityPitchName { get; set; }
        public Guid SportId { get; set; }
        public string SportName { get; set; }
        public DateTime TimeStart { get; set; }
        public DateTime TimeEnd { get; set; }
        public decimal CustomPrice { get; set; }
        public string Description { get; set; }
        public bool IsRepeatEveryWeek { get; set; }
        public string PlayerCount { get; set; }
        public DayOfWeek Day { get; set; }
    }
}
