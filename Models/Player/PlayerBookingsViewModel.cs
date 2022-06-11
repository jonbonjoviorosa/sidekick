using System;

namespace Sidekick.Model.Player
{
    public class PlayerBookingsViewModel
    {
        public string Title { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public DateTime Date { get; set; }
        public decimal Price { get; set; }
    }
}
