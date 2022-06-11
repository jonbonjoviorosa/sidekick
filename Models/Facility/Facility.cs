using System;
using System.Collections.Generic;

namespace Sidekick.Model
{
    public class Facility : APIBaseModel
    {
        public Guid FacilityId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string OwnerFirstName { get; set; }
        public string OwnerLastName { get; set; }
        public string MobileNumber { get; set; }
        public string Email { get; set; }
        public string ImageUrl { get; set; }
        public bool IsEveryday { get; set; }
        public bool IsHalfHourAllowed { get; set; }

        public string Street { get; set; }
        public string AreaName { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public virtual Area Area { get; set; }
        public Guid FacilityOwnerId { get; set; }

    }
}
