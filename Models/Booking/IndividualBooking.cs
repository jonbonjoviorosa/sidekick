using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sidekick.Model.Booking
{
    public class IndividualBooking : APIBaseModel
    {
        public Guid BookingId { get; set; }

        [StringLength(50)]
        public string TransactionNo { get; set; }

        [StringLength(200)]
        public string TelRRefNo { get; set; }

        public bool IsPaymentValidated { get; set; }
        public DateTime? DatePaymentValidated { get; set; }

        public decimal AmountPerHour { get; set; }
        public decimal TotalAmount { get; set; }

        public decimal SideKickCommission { get; set; }

        public Guid ClassId { get; set; }

        public Guid TraineeId { get; set; }

        public DateTime Date { get; set; }

        [Required]
        [StringLength(50)]
        public string StartTime { get; set; }

        [Required]
        [StringLength(50)]
        public string EndTime { get; set; }

        [Required]
        [StringLength(100)]
        public string Coaching { get; set; }

        [Required]
        [StringLength(250)]
        public string Location { get; set; }

        [Required]
        [StringLength(200)]
        public int Notes { get; set; }

        public EBookingStatus Status { get; set; }

        public bool IsPaid { get; set; }
        public DateTime? DatePaid { get; set; }
        public decimal ServiceFees { get; set; }
        public string AuthCode { get; set; }
        public string DepositeAuthCode { get; set; }
    }
}
