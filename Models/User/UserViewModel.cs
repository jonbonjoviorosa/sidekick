using Sidekick.Model.Badges;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sidekick.Model
{
    public class UserContext
    {
        public User UserInfo { get; set; }
        public int UnReadCount { get; set; }
        public UserLoginTransaction Tokens { get; set; }
        public CoachProfile CoachProfile { get; set; }
    }

    public class UserRegistration
    {
        [Required(ErrorMessage = "First name is required")]
        public string FirstName { get; set; }

        public string LastName { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [DataType(DataType.EmailAddress)]
        [RegularExpression("^[a-zA-Z0-9_\\.-]+@([a-zA-Z0-9-]+\\.)+[a-zA-Z]{2,6}$", ErrorMessage = "Email Is not valid.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        [RegularExpression("^(?=.*[A-Za-z])(?=.*\\d)[A-Za-z\\d]{5,}$",
            ErrorMessage = "The password must be at least 5-20 characters long containing only letters and numbers. ")]
        public string Password { get; set; }

        [Required(ErrorMessage = "You must specify an account type: Normal = 0, Normal And Coach= 1")]
        public EUserType UserType { get; set; }

        [Required(ErrorMessage = "You must specify a registration Device Platform Web = 0, iOS = 1, Android = 2, Others = 3")]
        public EDevicePlatform DevicePlatform { get; set; }

        [Required(ErrorMessage = "You must accept terms and conditions.")]
        public bool AcceptedTermsAndConditions { get; set; }

        public Genders Gender { get; set; }

        public UserRegistrationPlatform UserRegistrationPlatform { get; set; }

        [Required(ErrorMessage = "This field is required.")]
        [RegularExpression(@"^[0-9]+$", ErrorMessage = "Invalid mobile number.")]
        [DataType(DataType.PhoneNumber, ErrorMessage = "Invalid mobile number.")]
        public string MobileNumber { get; set; }

        public string ImageUrl { get; set; }
        public int? AreaId { get; set; }
        public Guid? NationalityId { get; set; }
    }

    public class LoginCredentials
    {
        [Required(ErrorMessage = "Email field is required.")]
        [DataType(DataType.EmailAddress)]
        [RegularExpression("^[a-zA-Z0-9_\\.-]+@([a-zA-Z0-9-]+\\.)+[a-zA-Z]{2,6}$", ErrorMessage = "Invalid email address.")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password field is required.")]
        [DataType(DataType.Password)]
        [RegularExpression("^(?=.*[A-Za-z])(?=.*\\d)[A-Za-z\\d]{5,}$",
            ErrorMessage = "The password must be at least 5-20 characters long containing only letters and numbers. ")]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Required(ErrorMessage = "You must specify a registration Device Platform Web = 0, IOS = 1, Android = 2, Others = 3")]
        public EDevicePlatform Device { get; set; }
    }

    public class LogoutCredentials
    {
        public Guid UserId { get; set; }
        public EDevicePlatform DevicePlatform { get; set; }
        public string DeviceFCMToken { get; set; }
        public string AuthToken { get; set; }
    }

    public class UserProfile
    {
        [Required(ErrorMessage = "Ce champs est requis.")]
        public Guid UserId { get; set; }

        [Required(ErrorMessage = "Ce champs est requis.")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Ce champs est requis.")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Email field is required.")]
        [DataType(DataType.EmailAddress)]
        [RegularExpression("^[a-zA-Z0-9_\\.-]+@([a-zA-Z0-9-]+\\.)+[a-zA-Z]{2,6}$", ErrorMessage = "Invalid email address.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "This field is required.")]
        [RegularExpression(@"^[0-9]+$", ErrorMessage = "Invalid mobile number.")]
        [DataType(DataType.PhoneNumber, ErrorMessage = "Invalid mobile number.")]
        public string MobileNumber { get; set; }

        public string ImageUrl { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        [RegularExpression("^(?=.*[A-Za-z])(?=.*\\d)[A-Za-z\\d]{5,}$", ErrorMessage = "The password must be at least 5-20 characters long containing only letters and numbers. ")]
        public string Password { get; set; }

        [Required(ErrorMessage = "You must specify a registration Device Platform Web = 0, IOS = 1, Android = 2, Others = 3")]
        public EDevicePlatform Device { get; set; }

        public Genders Gender { get; set; }

        [Required(ErrorMessage ="{0} is required!")]
        public Guid NationalityId { get; set; }

        public string NationalityName { get; set; }

        public DateTime DateOfBirth { get; set; }

        public string Description { get; set; }
        public int AreaId { get; set; }
        public int Age { get; set; }
        public int FriendCount { get; set; }
        public UserAddress UserAddress { get; set; }
        public bool IsActive { get; set; }
        public IEnumerable<UserPlayBadgeViewModel> PlayBadges { get; set; }

        public IEnumerable<UserTrainBadgeViewModel> TrainBadges { get; set; }
        public IEnumerable<UpcomingBooking> Bookings { get; set; }
    }

    public class VerifyCode
    {
        [Required(ErrorMessage = "This is required.")]
        public EVerificationType VerificationType { get; set; }

        [Required(ErrorMessage = "This is required.")]
        public string VerificationCode { get; set; }

        [Required(ErrorMessage = "This is required.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "You must specify a registration Device Platform Web = 0, iOS = 1, Android = 2, Others = 3")]
        public EDevicePlatform DevicePlatform { get; set; }
    }

    public class ResendCode
    {
        [Required(ErrorMessage = "This is required.")]
        public EVerificationType VerificationType { get; set; }

        [Required(ErrorMessage = "This is required.")]
        public string Email { get; set; }

        public string MobileNumber { get; set; }

        [Required(ErrorMessage = "You must specify a registration Device Platform Web = 0, iOS = 1, Android = 2, Others = 3")]
        public EDevicePlatform DevicePlatform { get; set; }
    }

    public class UserForgotPassword
    {
        [Required(ErrorMessage = "Email requis.")]
        [DataType(DataType.EmailAddress)]
        [RegularExpression("^[a-zA-Z0-9_\\.-]+@([a-zA-Z0-9-]+\\.)+[a-zA-Z]{2,6}$", ErrorMessage = "Email invalide. Veuillez réessayer.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "You must specify a registration Device Platform: Web = 0, IOS = 1, Android = 2, Others = 3")]
        public EDevicePlatform DevicePlatform { get; set; }
    }

    public class UserChangePassword
    {
        [Required(ErrorMessage = "UserId requis.")]
        public Guid UserId { get; set; }

        [Required(ErrorMessage = "Mot de passe actuel requis.")]
        [DataType(DataType.Password)]
        [Display(Name = "Current Password")]
        public string CurrentPassword { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        [RegularExpression("^(?=.*[A-Za-z])(?=.*\\d)[A-Za-z\\d]{5,}$", ErrorMessage = "The password must be at least 5-20 characters long containing only letters and numbers. ")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessage = "Password and password confirmation do not match.")]
        public string ConfirmPassword { get; set; }
    }

    public class RequestViewModel
    {
        public string Description { get; set; }
    }
}
