using System;
using System.ComponentModel.DataAnnotations;

namespace Sidekick.Model
{
    public class FacilityPlayer : APIBaseModel
    {
        public Guid FacilityPlayerId { get; set; }
        public Guid FacilityId { get; set; }
        public Guid SportId { get; set; }
        public Guid? FacilityPitchId { get; set; }
        public string UserNo { get; set; }
        public Guid UserId { get; set; }
        public int? AreaId { get; set; }
        public string Profile { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Name { get; set; }
        public DateTime? DateCreated { get; set; }
        public DateTime? LastBooking { get; set; }
        public string ProfileImgUrl { get; set; }
        public string Email { get; set; }
        public string ContactNumber { get; set; }
        public bool IsPaid { get; set; }
        public Guid BookingId { get; set; }
        public bool IsCaptain { get; set; }
        public int WaitingCount { get; set; }
        public bool IsCanInvite { get; set; }
        [StringLength(200)]
        public string TelRRefNo { get; set; }
        [StringLength(50)]
        public string InitialTransactionNo { get; set; }
        public EGamePlayerStatus PlayerStatus { get; set; }
        public DateTime? DatePaid { get; set; }
        public decimal TotalAmount { get; set; }

        public bool IsPaymentValidated { get; set; }
        public DateTime? DatePaymentValidated { get; set; }
        public decimal AuthorizedAmount { get; set; }
        public string DepositeAuthCode { get; set; }
        public string AuthCode { get; set; }
    }


    public class FacilityPlayerModel
    {
        public Guid FacilityId { get; set; }
        public Guid UserId { get; set; }
        public int? AreaId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Name { get; set; }
        public DateTime? DateCreated { get; set; }
        public DateTime? LastBooking { get; set; }
        public string ProfileImgUrl { get; set; }
        public string Email { get; set; }
        public string ContactNumber { get; set; }
        public bool IsPaid { get; set; }
        public Guid BookingId { get; set; }
        public Guid SportId { get; set; }
        public bool IsCaptain { get; set; }
        public bool IsCanInvite { get; set; }
        public int WaitingCount { get; set; }
        public EGamePlayerStatus PlayerStatus { get; set; }
        public decimal TotalAmount { get; set; }

    }
}
