using System;

namespace Sidekick.Model
{
    public class FacilityUser : APIBaseModel
    {
        public Guid FacilityUserId { get; set; }
        public Guid FacilityId { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MobileNumber { get; set; }
        public string ImageUrl { get; set; } 
        public EAccountType FacilityAccountType { get; set; }
        public EFacilityUserType FacilityUserType { get; set; }
        public Guid FacilityRoleId { get; set; }
        public string FacilityRole { get; set; }
        public EDevicePlatform DevicePlatform { get; set; }
    }

    public class FacilityUserLoginTransaction : APIBaseModel
    {
        public Guid FacilityUserId { get; set; }
        public EDevicePlatform DevicePlatform { get; set; }
        public string Token { get; set; }
        public DateTime TokenExpiration { get; set; }
        public string RefreshToken { get; set; }
        public DateTime RefreshTokenExpiration { get; set; }
    }
}
