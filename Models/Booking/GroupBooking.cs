using System;
using System.ComponentModel.DataAnnotations;

namespace Sidekick.Model.Booking
{

    public class GroupBooking: APIBaseModel
    {
        public Guid GroupBookingId { get; set; }

        [StringLength(50)]
        public string TransactionNo { get; set; }

        [StringLength(200)]
        public string TelRRefNo { get; set; }

        public bool IsPaymentValidated { get; set; }
        public DateTime? DatePaymentValidated { get; set; }

        public Guid GroupClassId { get; set; }

        public Guid ParticipantId { get; set; }

        public EBookingStatus Status { get; set; }

        public bool IsPaid { get; set; }
        public DateTime? DatePaid { get; set; }

        public decimal TotalAmount { get; set; }
        public decimal SideKickCommission { get; set; }
        public DateTime? Date { get; set; }

        public decimal ServiceFees { get; set; }

        public string AuthCode { get; set; }
        public string DepositeAuthCode { get; set; }
    }
}
