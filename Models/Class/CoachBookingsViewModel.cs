using System;

namespace Sidekick.Model
{
    public class CoachBookingsViewModel
    {
        public string ClassCategory { get; set; }
        public DateTime  Start { get; set; }
        public DateTime End { get; set; }
        public DateTime Date { get; set; }
        public decimal Price { get; set; }
    }
}
