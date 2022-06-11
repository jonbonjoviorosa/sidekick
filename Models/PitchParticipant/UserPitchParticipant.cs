using System;
using System.Collections.Generic;

namespace Sidekick.Model
{
    public class UserPitchParticipant : APIBaseModel
    {
        public Guid UserId { get; set; }
        public int UserPitchBookingId { get; set; }
        public bool HasAccepted { get; set; }
        //public virtual User User { get; set; }
        //public virtual UserPitchBooking UserPitchBooking { get; set; }
    }

}
