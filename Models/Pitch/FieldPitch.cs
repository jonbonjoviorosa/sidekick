using System;
using System.Collections.Generic;

namespace Sidekick.Model
{
    public class FieldPitch : APIBaseModel
    {
        public Guid FacilityFieldId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int MaxPlayers { get; set; }
        public virtual FacilityPitch FacilityField { get; set; }

        //public virtual ICollection<FieldPitchBooking> FieldPitchBookings { get; set; }
    }

}
