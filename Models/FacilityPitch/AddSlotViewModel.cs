using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Sidekick.Model
{
    public class AddSlotViewModel
    {
        [Required(ErrorMessage = "This is required!")]
        public Guid FacilityId { get; set; }
        public string FacilityName { get; set; }
        public decimal Commissions { get; set; }
        [Required(ErrorMessage = "This is required!")]
        public Guid FacilityPitchId { get; set; }
        public int FacilityPitchIdTable { get; set; }
        [Required(ErrorMessage = "This is required!")]
        public Guid SportId { get; set; }
        public DateTime Date { get; set; }
        public DayOfWeek Day { get; set; }
        public bool IsRepeatEveryWeek { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public string PlayerIds { get; set; }
        public int PlayerCount { get; set; }
        public int MaxPlayers { get; set; }
        [RegularExpression(@"^[0-9]+$", ErrorMessage = "Invalid Price number.")]
        [DataType(DataType.PhoneNumber, ErrorMessage = "Invalid Price number.")]
        public decimal TotalPrice { get; set; }
        public bool IsFree { get; set; }
        public string Description { get; set; }
        public List<FacilityPlayer> FacilityPlayers { get; set; }
        public Guid FacilityPitchTimingId { get; set; }
        public DateTime DateUpdated { get; set; }
    }

    public class SlotViewModel
    {
        public List<AddSlotViewModel> PitchTimings { get; set; }
        public List<AddSlotViewModel> PitchBookings { get; set; }
    }
}
