using System;
using System.ComponentModel.DataAnnotations;

namespace Sidekick.Model
{
    public class Admin : APIBaseModel
    {
        public Guid AdminId { get; set; }
        public string Email { get; set; }
        public string MobileNumber { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string ImageUrl { get; set; }
        public EAdminType AdminType { get; set; }
        public EAccountType AccountType { get; set; }
    }

    public class AdminLoginTransaction : APIBaseModel
    {
        public Guid AdminId { get; set; }
        public EAdminType AdminType { get; set; }
        public EDevicePlatform Device { get; set; }
        public string Token { get; set; }
        public DateTime TokenExpiration { get; set; }
        public string RefreshToken { get; set; }
        public DateTime RefreshTokenExpiration { get; set; }
        public DateTime DateCreated { get; set; }
    }

}
