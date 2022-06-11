using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sidekick.Model.Booking
{
    public class GroupBooking_UpdateStatusViewModel
    {
        [Required(ErrorMessage = "{0} is required!")]
        public Guid GroupBookingId { get; set; }

        [Required(ErrorMessage = "{0} is required!")]
        public Guid ParticipantId { get; set; }
        
        [Required(ErrorMessage = "{0} is required!")]
        public EBookingStatus Status { get; set; }
    }
}
