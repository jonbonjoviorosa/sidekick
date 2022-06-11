using System;

namespace Sidekick.Model
{
    public class PlayRequest : APIBaseModel
    {
        public Guid RequestId { get; set; }
        public Guid SportId { get; set; }
        public Guid UserId { get; set; }
        public Guid FacilityId { get; set; }
        public Guid FacilityPitchId { get; set; }
        public Guid BookingId { get; set; }
        public EGamePlayerStatus Status { get; set; }
        public decimal? Price { get; set; }
    }
}
