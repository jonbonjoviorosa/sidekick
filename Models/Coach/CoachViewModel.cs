using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sidekick.Model
{
    public class CoachViewModel
    {
        public int? UserNo { get; set; }
        public string ProfileName { get; set; }
        public string ImageUrl { get; set; }
        public string Email { get; set; }
        public string MobileNo { get; set; }
        public DateTime? DateCreated { get; set; }
        public DateTime? LastCoachingDate { get; set; }
        public string Status { get; set; }
        public Guid CoachUserId { get; set; }
    }

    public class CoachDetailedViewModel
    {
        public int? UserID { get; set; }
        public int? UserNo { get; set; }
        public string ProfileName { get; set; }
        public string ImageUrl { get; set; }
        public string Email { get; set; }
        public string MobileNo { get; set; }
        public decimal Experience { get; set; }
        public DateTime? Birthday { get; set; }
        public DateTime? DateCreated { get; set; }
        public DateTime? LastCoachingDate { get; set; }
        public string Status { get; set; }
        public Guid CoachUserId { get; set; }
        public string Location { get; set; }
        public string LocationLong { get; set; }
        public string LocationLat { get; set; }
        public Guid NationalityId { get; set; }
        public string Description { get; set; }
    }
}
