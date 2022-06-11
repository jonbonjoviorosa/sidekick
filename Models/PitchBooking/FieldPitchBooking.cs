using System;
using System.Collections.Generic;

namespace Sidekick.Model
{
    public class FieldPitchBooking : APIBaseModel
    {
        public Guid UserId { get; set; }
        public Guid FacilityFieldId { get; set; }
        public bool IsFullFieldBooking { get; set; }
        public int PlayerCount { get; set; }
        public decimal PricePerUser { get; set; }
        public DateTime BookingStart { get; set; }
        public DateTime BookingEnd { get; set; }
        //public virtual User User { get; set; }
        //public virtual FacilityField FacilityField { get; set; }
        //public virtual ICollection<FieldPitch> FieldPitches { get; set; }
        //public virtual ICollection<FieldPitchParticipant> FieldPitchParticipants { get; set; }
    }

}
