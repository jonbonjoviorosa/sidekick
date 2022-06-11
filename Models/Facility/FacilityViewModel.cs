using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Sidekick.Model
{
    public class FacilityProfile
    {
        public Guid FacilityId { get; set; }

        [Required(ErrorMessage = "This field is required.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "This field is required.")]
        public string Description { get; set; }

        [Required(ErrorMessage = "This field is required.")]
        public string OwnerFirstName { get; set; }

        [Required(ErrorMessage = "This field is required.")]
        public string OwnerLastName { get; set; }

        [Required(ErrorMessage = "This field is required.")]
        [DataType(DataType.EmailAddress)]
        [RegularExpression("^[a-zA-Z0-9_\\.-]+@([a-zA-Z0-9-]+\\.)+[a-zA-Z]{2,6}$", ErrorMessage = "Invalid email address.")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        //[Required(ErrorMessage = "This field is required.")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        [RegularExpression("^(?=.*[A-Za-z])(?=.*\\d)[A-Za-z\\d]{5,}$", ErrorMessage = "Password must be at least 5-20 characters long containing only letters and numbers.")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessage = "Password and password confirmation do not match.")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "This field is required.")]
        [MinLength(11, ErrorMessage = "Please enter a value less than or equal to 11.")]
        [MaxLength(11, ErrorMessage = "Please enter a value less than or equal to 11.")]
        [RegularExpression(@"^[0-9]+$", ErrorMessage = "Invalid mobile number.")]
        [DataType(DataType.PhoneNumber, ErrorMessage = "Invalid mobile number.")]
        public string MobileNumber { get; set; }

        [Required(ErrorMessage = "This field is required.")]
        public string ImageUrl { get; set; }

        public bool IsEveryday { get; set; }
        public DateTime TimeStart { get; set; }
        public DateTime TimeEnd { get; set; }

        public bool IsHalfHourAllowed { get; set; }

        [Required(ErrorMessage = "This field is required.")]
        public string Street { get; set; }

        public string AreaName { get; set; }
        public int AreaId { get; set; }
        public Area Area { get; set; }
        public List<Area> Areas { get; set; }

        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }

        public FacilityUser FacilityOwner { get; set; }
        public List<FacilityStaff> FacilityStaffs { get; set; }
        public FacilityHour FacilityHours { get; set; }
        public List<FacilityTiming> FacilityTimings { get; set; }
        public List<FacilitySportDto> FacilitySports { get; set; }

        public Guid UserLoggedIn { get; set; }
        public Guid FacilityOwnerId { get; set; }
    }

    public class FacilityStaff
    {
        public Guid FacilityId { get; set; }

        [Required(ErrorMessage = "This field is required.")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "This field is required.")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "This field is required.")]
        [DataType(DataType.EmailAddress)]
        [RegularExpression("^[a-zA-Z0-9_\\.-]+@([a-zA-Z0-9-]+\\.)+[a-zA-Z]{2,6}$", ErrorMessage = "Invalid email address.")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        public EFacilityUserType FacilityUserType { get; set; }
    }

    public class FacilityHour
    {
        public Guid FacilityId { get; set; }
        public bool IsEveryday { get; set; }
        public DayOfWeek Day { get; set; }
        public DateTime TimeStart { get; set; }
        public DateTime TimeEnd { get; set; }
    }

    public class FacilitySportDto
    {
        public Guid SportId { get; set; }
        public Guid FacilityId { get; set; }
        public string Name { get; set; }
    }

    public class FacilityList
    {
        public Guid FacilityId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Location { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int PitchNo { get; set; }
        public string Commission { get; set; }
        public bool? IsEnabled { get; set; }
        public string FacilityImage { get; set; }
        public DateTime DateUpdated { get; set; }
    }

    public class ChangeStatus
    {
        public Guid GuID { get; set; }
        public bool IsEnabled { get; set; }
    }
}
