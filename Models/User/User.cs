using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sidekick.Model
{
    public class User : APIBaseModel
    {
        public Guid UserId { get; set; }
        public string Email { get; set; }
        public string MobileNumber { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string TelRRefNo { get; set; }
        public string TransactionNo { get; set; }
        public Genders Gender { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public EUserType UserType { get; set; }
        public Guid NationalityId { get; set; }
        public EDevicePlatform DeviceRegistrationPlatform { get; set; }
        public UserRegistrationPlatform UserRegistrationPlatform { get; set; }
        public string AppleUserId { get; set; }
    }

    public class UserDevice : APIBaseModel
    {
        public Guid UserId { get; set; }
        public string DeviceFCMToken { get; set; }
        public EDevicePlatform DeviceType { get; set; }
    }

    public class UserLoginTransaction : APIBaseModel
    {
        public Guid UserId { get; set; }
        public EUserType UserType { get; set; }
        public EDevicePlatform Device { get; set; }
        public string Token { get; set; }
        public DateTime TokenExpiration { get; set; }
        public string RefreshToken { get; set; }
        public DateTime RefreshTokenExpiration { get; set; }
        public DateTime DateCreated { get; set; }
    }

    public class UserVerificationCode : APIBaseModel
    {
        public Guid UserId { get; set; }
        public EDevicePlatform Device { get; set; }
        public string Email { get; set; }
        public string MobileNumber { get; set; }
        public EVerificationType VerificationType { get; set; }
        public string VerificationCode { get; set; }
    }

    public class UserAddress : APIBaseModel
    {
        public Guid UserId { get; set; }
        public string AddressName { get; set; }
        public string FloorNum { get; set; }
        public string DoorNum { get; set; }
        public string Street { get; set; }
        public string PostalCode { get; set; }
        public string City { get; set; }
        public string AddressNote { get; set; }
        [Column(TypeName = "decimal(10,8)")]
        public decimal Latitude { get; set; }
        [Column(TypeName = "decimal(11,8)")]
        public decimal Longitude { get; set; }
        public bool IsCurrentAddress { get; set; }
        public string CountryName { get; set; }
        public string CountryAlpha3Code { get; set; }
        public int AreaId { get; set; }

        [NotMapped]
        public string CompleteAddress { get; set; }
    }
}
