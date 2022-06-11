using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sidekick.Model
{
    public class FacilityPitch : APIBaseModel
    {
        public Guid? FacilityPitchId { get; set; }
        public Guid? FacilityId { get; set; }
        public Guid SportId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public int MaxPlayers { get; set; }
        public bool IsFacilityTime { get; set; }
        public bool IsFixedPrice { get; set; }
        public int Divisions { get; set; } // number of divisions, minimum should be 1
        public decimal FixedPrice { get; set; }
        public Guid SurfaceId { get; set; }
        public Guid TeamSize { get; set; }
        public Guid LocationId { get; set; }
        public Guid AreaId { get; set; }
        public string FacilityPitchTimingIds { get; set; }
    }

    public class FacilityPitchTimingIdViewModel
    {
        public decimal FixedPrice { get; set; }
    }

}
