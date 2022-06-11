using System;

namespace Sidekick.Model
{
    public class FacilitySport : APIBaseModel
    {
        public Guid SportId { get; set; }
        public Guid FacilityId { get; set; }
    }
}
