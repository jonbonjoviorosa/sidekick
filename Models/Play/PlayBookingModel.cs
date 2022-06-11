using System;
using System.Collections.Generic;

namespace Sidekick.Model
{
    public class PlayBookingModel
    {
        public Guid? BookingId { get; set; }
        public Guid FacilityId { get; set; }
        public Guid SportId { get; set; }
        public string Name { get; set; }
        public string UserName { get; set; }
        public DateTime PitchStart { get; set; }
        public DateTime PitchEnd { get; set; }
        public string Date { get; set; }
        public int PlayerCount { get; set; }
        public bool IsCaptain { get; set; }
        public bool IsPrivate { get; set; }
        public bool IsWaitinglist { get; set; }
        public bool IsParticipate { get; set; }
        public bool IsRequestsent { get; set; }
        public bool IsPaid { get; set; }
        public bool IsCanInvite { get; set; }
        public bool IsWaitingRequest{ get; set; }
        public decimal PricePerPlayer { get; set; }
        public decimal PriceIncludingVat { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal Commission { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public Guid GamePlayerId { get; set; }
        public DateTime CreatedDate { get; set; }
        public virtual Facility Facility { get; set; }
        public virtual FacilityPitch FacilityPitch { get; set; }
        public virtual List<GamePlayerModel> RegisteredPlayers { get; set; }
        public virtual List<GamePlayerModel> WaitingPlayers { get; set; }
        public string LocationName { get; set; }
        public string SurfaceName { get; set; }
        public string TeamSizeName { get; set; }
        public int WaitingPlayerCount { get; set; }
        public int RequestPlayerCount { get; set; }
        public string SportName { get; set; }
        public bool IsPaymentPending { get; set; }
    }
}
