using System;

namespace Sidekick.Model
{
    public class CancelPlayBookingViewModel
    {
        public Guid FacilityPitchTimingId { get; set; }
        public Guid FacilityPitchId { get; set; }
        public string TelRRefNo { get; set; }
        public string TransactionNo { get; set; }
        public decimal TotalAmount { get; set; }
        public bool IsFree { get; set; }
        public Guid BookingId { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public DayOfWeek Day { get; set; }
        public string Location { get; set; }
        public FacilityPlayer Player { get; set; }
        public DateTime BookingDate { get; set; }
        public string Sport { get; set; }
        public string Facility { get; set; }
        public decimal PricePerPlayer { get; set; }
    }
}
