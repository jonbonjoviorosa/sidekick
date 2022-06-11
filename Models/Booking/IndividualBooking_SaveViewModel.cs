using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sidekick.Model.Booking
{
    public class IndividualBooking_SaveViewModel
    {
        public Guid? BookingId { get; set; }

        public Guid IndivdualClassId { get; set; }


        [Required]
        public DateTime Date { get; set; }

        [Required]
        [StringLength(50)]
        public string StartTime { get; set; }

        [StringLength(50)]
        public string EndTime { get; set; }

        [Required]
        public decimal PricePerHour { get; set; }

        [StringLength(100)]
        public string Coaching { get; set; }

        [Required]
        [StringLength(250)]
        public string Location { get; set; }

        public int Notes { get; set; }

        public int Duration { get; set; }

        /*
        [Required]
        [StringLength(200)]
        public int Notes { get; set; }
        */

        public EBookingStatus Status { get; set; }
    }

    public class IndividualConfirmBookingViewModel
    {
        public Guid? BookingId { get; set; }

        public string CoachFirstName { get; set; }
        public string CoachLasttName { get; set; }
        public string UserFirstName { get; set; }
        public string UserLastName { get; set; }
        public string CoachLocation { get; set; }
        public string coachImage { get; set; }

        public Guid IndivdualClassId { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        [StringLength(50)]
        public string StartTime { get; set; }

        [Required]
        [StringLength(50)]
        public string EndTime { get; set; }

        [Required]
        public decimal PricePerHour { get; set; }

        public decimal PriceIncludingVat { get; set; }

        public decimal TotalPrice { get; set; }
        public decimal SideKickCommission { get; set; }

        [StringLength(100)]
        public string Coaching { get; set; }

        [Required]
        [StringLength(250)]
        public string Location { get; set; }

        [Required]
        [StringLength(200)]
        public int Notes { get; set; }

        public EBookingStatus Status { get; set; }
        public string TransactionNo { get; set; }
    }
}
