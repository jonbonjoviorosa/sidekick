using System;

namespace Sidekick.Model
{
    public class TrainingBooking : APIBaseModel
    {
        public Guid UserId { get; set; }
        public int? TrainingBookingId { get; set; }
        public string Type { get; set; }
        public string Email { get; set; }
        public DateTime BookingDate { get; set; }
        public int Price { get; set; }
        public decimal Comission { get; set; }
        public virtual TrainingTiming TrainingTiming { get; set; }
        public Guid CoachId { get; set; }
        public string CoachName { get; set; }
    }

}
