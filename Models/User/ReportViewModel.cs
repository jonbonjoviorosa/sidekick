using System;

namespace Sidekick.Model
{
    public class ReportViewModel
    {
        public string Reason { get; set; }
        public Guid ReportedUser { get; set; }
        public DateTime ReportedDate { get; set; }
    }
}
