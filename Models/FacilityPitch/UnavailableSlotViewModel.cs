using System;
using System.ComponentModel.DataAnnotations;

namespace Sidekick.Model
{
    public class UnavailableSlotViewModel
    {
        public Guid UnavailableSlotId { get; set; }
        public DateTime Starts { get; set; }
        public DateTime Ends { get; set; }
        public bool? AllDay { get; set; }
        public bool? RepeatEveryWeek { get; set; }
        public string During { get; set; }
        public bool? AllPitches { get; set; }
        public Guid FacilityPitchId { get; set; }
        [Required]
        public string Title { get; set; }
        public string Notes { get; set; }
        public Guid FacilityId { get; set; }
        public string FacilityPitchName { get; set; }
    }
}
