using Sidekick.Model.Enums;
using System;

namespace Sidekick.Model
{
    public class Report : APIBaseModel
    {
        public string Reason { get; set; }
        public Guid ReportedByUser { get; set; }
        public Guid ReportedUser { get; set; }
        public RequestStatus Status { get; set; }
    }
}
