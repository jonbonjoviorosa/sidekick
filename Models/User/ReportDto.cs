using Sidekick.Model.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace Sidekick.Model
{
    public class ReportDto
    {
        [Required]
        public Guid ReportedByUserId { get; set; }
        public string ReportedByUser { get; set; }
        public int Id { get; set; }
        [Required]
        public Guid ReportedUserId { get; set; }
        public string ReportedUser { get; set; }
        public string Reasons { get; set; }
        public DateTime ReportedDate { get; set; }
        public RequestStatus Status { get; set; }
        public bool IsDelete { get; set; }
        public RequestType Type { get; set; }
    }
}
