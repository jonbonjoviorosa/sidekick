using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sidekick.Model
{
    public class PaymentCoachingViewModel
    {
        public string ReferenceNumber { get; set; }
        public string ParticipantName { get; set; }
        public string CoachName { get; set; }
        public DateTime DateBooked { get; set; }
        public string Slot { get; set; }
        public DateTime PaymentDate { get; set; }
        public decimal AmountPaid { get; set; }
        public PaymentStatus Status { get; set; }
    }
}
