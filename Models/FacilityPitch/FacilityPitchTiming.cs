using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sidekick.Model
{
    public class FacilityPitchTiming : APIBaseModel
    {
        public Guid FacilityPitchTimingId { get; set; }
        public Guid FacilityPitchId { get; set; }
        public DateTime Date { get; set; }
        public bool IsRepeatEveryWeek { get; set; }
        public DayOfWeek Day { get; set; }
        public DateTime TimeStart { get; set; }
        public DateTime TimeEnd { get; set; }
        public decimal CustomPrice { get; set; }
        public bool IsFree { get; set; }
        public string PlayerIds { get; set; }
    }    
}
