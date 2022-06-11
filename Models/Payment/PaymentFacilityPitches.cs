using System;

namespace Sidekick.Model
{
    public class PaymentFacilityPitches
    {
        public string ReferenceNumber { get; set; }
        public string PlayerName { get; set; }
        public string FacilityPitchName { get; set; }
        public DateTime DateBooked { get; set; }
        public string Slot { get; set; }
        public DateTime PaymentDate { get; set; }
        public decimal AmountPaid { get; set; }
        public PaymentStatus Status { get; set; }
    }
}
