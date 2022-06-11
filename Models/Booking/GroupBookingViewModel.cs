using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sidekick.Model.Booking
{
    public class GroupBookingViewModel
    {
        public Guid GroupBookingId { get; set; }

        public Guid GroupClassId { get; set; }

        public Guid ParticipantId { get; set; }

        public EBookingStatus Status { get; set; }

        public string ParticipantFirstName { get; set; }
        public string ParticipantLastName { get; set; }

        public string CoachFirstName { get; set; }

        public string CoachLastName { get; set; }
        public string CoachImage { get; set; }

        public string Title { get; set; }

        public bool ByLevel { get; set; }

        public string StartTime { get; set; }
        public string EndTime { get; set; }

        public int Duration { get; set; }

        public Guid Level { get; set; }

        public DateTime? Start { get; set; }

        public DateTime? End { get; set; }

        public bool RepeatEveryWeek { get; set; }

        public int? DuringNo { get; set; }

        public EDuring? During { get; set; }

        public int Participants { get; set; }
        public int ParticipantsJoined { get; set; }

        public decimal Price { get; set; }

        public Guid? LocationId { get; set; }
        public string LocationName { get; set; }
        public string GymLocationName { get; set; }

        public bool IsOnline { get; set; }

        public string Notes { get; set; }

        public decimal TotalAmount { get; set; }
        public decimal SideKickCommission { get; set; }
        public string Date { get; set; }

        public string TransactionNo { get; set; }
    }
}
