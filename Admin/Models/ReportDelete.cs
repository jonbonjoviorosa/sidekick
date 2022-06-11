using System;

namespace Sidekick.Admin.Models
{
    public class ReportDelete
    {
        public Guid ReportedByUserId { get; set; }
        public Guid ReportedUserId { get; set; }
    }
}
