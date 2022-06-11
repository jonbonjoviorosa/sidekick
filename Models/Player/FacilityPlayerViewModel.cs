using System;

namespace Sidekick.Model
{
    public class FacilityPlayerViewModel
    {
        public Guid UserId { get; set; }
        public string ProfileImgUrl { get; set; }
        public string Name { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string AreaName { get; set; }
        public DateTime CreatedDate { get; set; }
        public string LastBooking { get; set; }
        public bool IsPaid { get; set; }
    }
}
