using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Sidekick.Model
{
    public class GamePlayer : APIBaseModel
    {
        public Guid? GameId { get; set; }
        public Guid? BookingId { get; set; }
        public Guid? FacilityPitchId { get; set; }
        public Guid SportId { get; set; }
        public Guid UserId { get; set; }
        public Guid UserFriendId { get; set; }
        public bool HasAccepted { get; set; }

        public bool? IsCaptain { get; set; }
        [StringLength(50)]
        public string InitialTransactionNo { get; set; }
        [StringLength(200)]
        public string TelRInitialTransactionNo { get; set; }

        [StringLength(50)]
        public string DepositTransactionNo { get; set; }

        [StringLength(50)]
        public string BalanceTransactionNo { get; set; }

        [StringLength(200)]
        public string DepositTelRRefNo { get; set; }

        [StringLength(200)]
        public string BalanceTelRRefNo { get; set; }

        public decimal DepositAmount { get; set; }
        public decimal BalanceAmount { get; set; }

        public bool IsDepositPaid { get; set; }
        public DateTime? DateDepositPaid { get; set; }

        public bool IsBalancePaid { get; set; }
        public DateTime? DateBalancePaid { get; set; }

        public bool IsDepositRefunded { get; set; }

        public DateTime? DateDepositRefunded { get; set; }
        
        [StringLength(200)]
        public string RefundTelRTransactionNo { get; set; }

        public EGamePlayerStatus PlayerStatus { get; set; }
    }
}
