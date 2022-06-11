using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Sidekick.Model
{
    public class EditSlotViewModel
    {
        public string FacilityName { get; set; }
        public Guid FacilityId { get; set; }
        public Guid FacilityPitchTimingId { get; set; }
        public int FacilityPitchIdTable { get; set; }
        public string FacilityImgUrl { get; set; }
        public Guid SportId { get; set; }
        public Guid FacilityPitchId { get; set; }
        public DateTime Date { get; set; }
        public bool IsRepeatEveryWeek { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        [RegularExpression(@"^[0-9]+$", ErrorMessage = "Invalid Price number.")]
        [DataType(DataType.PhoneNumber, ErrorMessage = "Invalid Price number.")]
        public decimal Price { get; set; }
        public List<FacilityPlayer> Players { get; set; }
        public int MaxPlayers { get; set; }
        public List<FacilityPitch> FacilityPitches { get; set; }
        public DayOfWeek Day { get; set; }
        public string Area { get; set; }
        public bool IsFree { get; set; }
    }
}
