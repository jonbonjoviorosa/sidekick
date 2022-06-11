using System;
using System.Collections.Generic;

namespace Sidekick.Model
{
    public class PlayFacilitiesModel
    {
        public Guid FacilityId { get; set; }
        public string Name { get; set; }
        public string Street { get; set; }
        public string Area { get; set; }
        public string City { get; set; }
        public string Location { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int PitchNo { get; set; }
        public string Commission { get; set; }
        public bool? IsEnabled { get; set; }
        public string FacilityImage { get; set; }
        public IEnumerable<FacilityPitch> FacilityPitches { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
    }

    public class PlayFacilitiesViewModel
    {
        public Guid FacilityId { get; set; }
        public string Name { get; set; }
        public string Street { get; set; }
        public string Area { get; set; }
        public string AreaId { get; set; }
        public string City { get; set; }
        public string Location { get; set; }
        public string Description { get; set; }
        public string TimeStart { get; set; }
        public string TimeEnd { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int PitchNo { get; set; }
        public string Commission { get; set; }
        public bool? IsEnabled { get; set; }
        public string FacilityImage { get; set; }
        public List<FacilityPitchDto> FacilityPitches { get; set; }
    }
}
