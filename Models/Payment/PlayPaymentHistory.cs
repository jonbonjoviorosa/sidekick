using System;

namespace Sidekick.Model
{
    public class PlayPaymentHistory
    {
        public int Id { get; set; }
        public Guid BookingId { get; set; }
        public int PitchNo { get; set; }
        public string PitchName { get; set; }
        public DateTime DateBooked { get; set; }
        public DateTime DatePlayed { get; set; }
        public string SlotPlayed { get; set; }
        public decimal TotalIncludingVat { get; set; }
        public decimal TotalExcludingVat { get; set; }
        public PaymentStatus Status { get; set; }
    }
}
