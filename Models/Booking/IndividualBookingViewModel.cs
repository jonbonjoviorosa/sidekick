using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sidekick.Model.Booking
{
    public class IndividualBookingViewModel
    {
        public Guid BookingId { get; set; }

        public Guid CoachId { get; set; }
        public Guid ClassId { get; set; }
        public string CoachFirstName { get; set; }

        public string CoachLastName { get; set; }

        public Guid TraineeId { get; set; }

        public DateTime Date { get; set; }
        public DateTime EndDate { get; set; }

        public string StartTime { get; set; }

        public string EndTime { get; set; }

        public string Coaching { get; set; }

        public string Location { get; set; }
        public string UserImage { get; set; }
        public string TransactionNo { get; set; }

        public double BookingAmount { get; set; }
        public double CommissionAmount { get; set; }
        public double VatAmount { get; set; }
        public int Notes { get; set; }
        public int GroupDuration { get; set; }
        public string GroupNotes { get; set; }

        public EBookingStatus Status { get; set; }
        public EBookingType BookingType { get; set; }
    }

    public class MyBookingViewModel
    {
        public List<IndividualBookingViewModel> UpComingBooking { get; set;}
        public List<IndividualBookingViewModel> BookingHistory { get; set; }
    }

}
