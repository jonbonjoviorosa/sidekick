using System;
using System.ComponentModel.DataAnnotations;

namespace Sidekick.Model
{
    public class FacilityUserContext
    {
        public FacilityUser FacilityUserInfo { get; set; }
        public FacilityUserLoginTransaction Tokens { get; set; }
    }

    public class FacilityUserLogin
    {
        [Required(ErrorMessage = "This field is required.")]
        [DataType(DataType.EmailAddress)]
        [RegularExpression("^[a-zA-Z0-9_\\.-]+@([a-zA-Z0-9-]+\\.)+[a-zA-Z]{2,6}$", ErrorMessage = "Invalid email address.")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "This field is required.")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        [RegularExpression("^(?=.*[A-Za-z])(?=.*\\d)[A-Za-z\\d]{5,}$", ErrorMessage = "Password must be at least 5-20 characters long containing only letters and numbers.")]
        public string Password { get; set; }
    }

    public class FacilityUserChangePassword
    {
        [Required(ErrorMessage = "FacilityUserId is required.")]
        public Guid FacilityUserId { get; set; }

        [Required(ErrorMessage = "This field is required.")]
        [DataType(DataType.Password)]
        [Display(Name = "Current Password")]
        public string CurrentPassword { get; set; }

        [Required(ErrorMessage = "This field is required.")]
        [DataType(DataType.Password)]
        [Display(Name = "New Password")]
        [RegularExpression("^(?=.*[A-Za-z])(?=.*\\d)[A-Za-z\\d]{5,}$", ErrorMessage = "Password must be at least 5-20 characters long containing only letters and numbers.")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("NewPassword", ErrorMessage = "New password and password confirmation do not match.")]
        public string ConfirmPassword { get; set; }
    }

    public class FacilityUserForgotPassword
    {
        [Required(ErrorMessage = "This field is required.")]
        [DataType(DataType.EmailAddress)]
        [RegularExpression("^[a-zA-Z0-9_\\.-]+@([a-zA-Z0-9-]+\\.)+[a-zA-Z]{2,6}$", ErrorMessage = "Invalid email address.")]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }

    public class FacilityUserList
    {
        public Guid FacilityUserId { get; set; }
        public string ImageUrl { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public EFacilityUserType FacilityUserType { get; set; }
        public DateTime? CreatedDate { get; set; }
        public bool? IsEnabled { get; set; }
    }

    public class FacilityUserProfile
    {
        public Guid FacilityUserId { get; set; }
        public Guid FacilityId { get; set; }

        [Required(ErrorMessage = "This field is required.")]
        [DataType(DataType.EmailAddress)]
        [RegularExpression("^[a-zA-Z0-9_\\.-]+@([a-zA-Z0-9-]+\\.)+[a-zA-Z]{2,6}$", ErrorMessage = "Invalid email address.")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "This field is required.")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        [RegularExpression("^(?=.*[A-Za-z])(?=.*\\d)[A-Za-z\\d]{5,}$", ErrorMessage = "Password must be at least 5-20 characters long containing only letters and numbers.")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessage = "Password and password confirmation do not match.")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "This field is required.")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "This field is required.")]
        public string LastName { get; set; }

        //[Required(ErrorMessage = "This field is required.")]
        [RegularExpression(@"^[0-9]+$", ErrorMessage = "Invalid mobile number.")]
        [DataType(DataType.PhoneNumber, ErrorMessage = "Invalid mobile number.")]
        public string MobileNumber { get; set; }

        [Required(ErrorMessage = "This field is required.")]
        public string ImageUrl { get; set; }

        public bool? IsEnabled { get; set; }
        public EFacilityUserType FacilityUserType { get; set; }
        public Guid FacilityRoleId { get; set; }
        public EDevicePlatform DevicePlatform { get; set; }
    }
}
