using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sidekick.Jobs.ViewModel
{
    public class UserPitchBooking
    {
        public Guid BookingId { get; set; }
        public Guid FacilityId { get; set; }
        public Guid FacilityPitchId { get; set; }
        public Guid UserId { get; set; }
        public bool IsCaptain { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int PlayerCount { get; set; }
        public bool IsPrivate { get; set; }
        public bool IsPaid { get; set; }
        public bool? IsCancelled { get; set; }

        // location
        public Guid AreaId { get; set; }
        public string City { get; set; }
        public Guid LocationId { get; set; }
        //public decimal Latitude { get; set; }
        //public decimal Longitude { get; set; }

        // booking
        public decimal? PricePerUser { get; set; }
        public decimal? Commission { get; set; }
        public DateTime PitchStart { get; set; }
        public DateTime PitchEnd { get; set; }
        public Guid SurfaceId { get; set; }
        public DateTime Date { get; set; }
        public Guid SportId { get; set; }
        //public List<User> RegisteredPlayers { get; set; }
    }
}
