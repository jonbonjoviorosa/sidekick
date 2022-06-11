using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Sidekick.Model
{
    public class CoachRenderViewModel
    {
        [Required(ErrorMessage = "First name is required")]
        public string FirstName { get; set; }

        public string LastName { get; set; }
        public string ImageUrl { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [DataType(DataType.EmailAddress)]
        [RegularExpression("^[a-zA-Z0-9_\\.-]+@([a-zA-Z0-9-]+\\.)+[a-zA-Z]{2,6}$", ErrorMessage = "Email Is not valid.")]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        [RegularExpression("^(?=.*[A-Za-z])(?=.*\\d)[A-Za-z\\d]{5,}$",
        ErrorMessage = "The password must be at least 5-20 characters long containing only letters and numbers. ")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessage = "Password and password confirmation do not match.")]
        public string ConfirmPassword { get; set; }

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
  
        public List<Guid> GymIds { get; set; }
       
        public List<Guid> SpecialtyIds { get; set; }

        public List<Guid> LanguageIds { get; set; }

        public Guid? NationalityId { get; set; }
        public int? AreaId { get; set; }
        public string Description { get; set; }
        public Guid UserId { get; set; }
        public bool IsActive { get; set; }
        public List<CoachBookingsViewModel> Bookings { get; set; }
        public bool IsPasswordEdit { get; set; }
    }
}
