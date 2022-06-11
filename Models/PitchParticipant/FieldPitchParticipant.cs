using System;
using System.Collections.Generic;

namespace Sidekick.Model
{
    public class FieldPitchParticipant : APIBaseModel
    {
        public Guid UserId { get; set; }
        public int FieldPitchBookingId { get; set; }
        public bool HasAccepted { get; set; }
        //public virtual User User { get; set; }
        //public virtual FieldPitchBooking FieldPitchBooking { get; set; }
    }

}
