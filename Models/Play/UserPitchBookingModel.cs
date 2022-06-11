using System;
using System.Collections.Generic;

namespace Sidekick.Model
{
    public class UserPitchBookingModel
    {

        public Guid? BookingId { get; set; }
        public Guid FacilityId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Street { get; set; }
        //public string Area { get; set; }
        public string City { get; set; }
        public Guid FacilityPitchId { get; set; }
        public Guid SportId { get; set; }
        public int AreaId { get; set; }
        public Guid PitchLocationId { get; set; }
        public Guid SurfaceId { get; set; }
        public Guid SizeId { get; set; }
        public string TeamSize { get; set; }
        public Guid UserId { get; set; }
        public bool? IsCaptain { get; set; }
        public int PlayerCount { get; set; }
        public bool IsPrivate { get; set; }
        public bool IsPaid { get; set; }
        public bool IsCanInvite { get; set; }
        public bool IsWaitinglist { get; set; }
        public bool? IsCancelled { get; set; }
        public decimal? PricePerUser { get; set; }
        public string Location { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int PitchNo { get; set; }
        public string Commission { get; set; }
        public bool? IsEnabled { get; set; }
        public string FacilityImage { get; set; }
        public string FacilityPitchName { get; set; }
        public string SportName { get; set; }
        public bool IsFacilityTime { get; set; }
        public string PitchLocation { get; set; }
        public string SurfaceName { get; set; }
        public DateTime PitchStart { get; set; }
        public DateTime PitchEnd { get; set; }
        public DateTime Date { get; set; }
        public Area Area { get; set; }
        public int MaxPlayers { get; set; }
        public bool IsParticipate { get; set; }
        public bool IsLocked { get; set; }
        public int WaitingPlayerCount { get; set; }

        public bool IsPaymentPending { get; set; }
        public bool IsRequestsent { get; set; }
        public Guid FacilityPitchTimingId { get; set; }
        public int ApprovedPlayerCount { get; set; }
        public IEnumerable<GamePlayerModel> RegisteredPlayers { get; set; }
        public IEnumerable<GamePlayerModel> WaitingPlayers { get; set; }

        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }

    }


    public class GamePlayerModel
    {
        public Guid? GameId { get; set; }
        public Guid? BookingId { get; set; }
        public Guid? FacilityPitchId { get; set; }
        public Guid SportId { get; set; }
        public Guid UserId { get; set; }
        public Guid UserFriendId { get; set; }
        public bool HasAccepted { get; set; }
        public bool? IsCaptain { get; set; }
        public string PlayerName { get; set; }
        public string PlayerImage { get; set; }
        public DateTime ParticipateDate { get; set; }
        public EGamePlayerStatus PlayerStatus { get; set; }
        public string SportName { get; set; }
    }

    public class UserPitchBookingResponseModel
    {

        public Guid? BookingId { get; set; }
        public Guid FacilityId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Street { get; set; }
        //public string Area { get; set; }
        public string City { get; set; }
        public Guid FacilityPitchId { get; set; }
        public Guid SportId { get; set; }
        public int AreaId { get; set; }
        public Guid PitchLocationId { get; set; }
        public Guid SurfaceId { get; set; }
        public Guid SizeId { get; set; }
        public string TeamSize { get; set; }
        public Guid UserId { get; set; }
        public bool? IsCaptain { get; set; }
        public int PlayerCount { get; set; }
        public bool IsPrivate { get; set; }
        public bool IsPaid { get; set; }
        public bool IsCanInvite { get; set; }
        public bool IsWaitinglist { get; set; }
        public bool? IsCancelled { get; set; }
        public decimal? PricePerUser { get; set; }
        public string Location { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int PitchNo { get; set; }
        public string Commission { get; set; }
        public bool? IsEnabled { get; set; }
        public string FacilityImage { get; set; }
        public string FacilityPitchName { get; set; }
        public string SportName { get; set; }
        public bool IsFacilityTime { get; set; }
        public string PitchLocation { get; set; }
        public string SurfaceName { get; set; }
        public DateTime PitchStart { get; set; }
        public DateTime PitchEnd { get; set; }
        public string Date { get; set; }
        public Area Area { get; set; }
        public int MaxPlayers { get; set; }
        public bool IsParticipate { get; set; }
        public bool IsLocked { get; set; }
        public int WaitingPlayerCount { get; set; }

        public bool IsPaymentPending { get; set; }
        public bool IsRequestsent { get; set; }
        public bool IsWaitingRequest { get; set; }
        public int ApprovedPlayerCount { get; set; }
        public IEnumerable<GamePlayerModel> RegisteredPlayers { get; set; }
        public IEnumerable<GamePlayerModel> WaitingPlayers { get; set; }

        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }

    }
}
