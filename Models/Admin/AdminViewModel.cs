using System;
using System.ComponentModel.DataAnnotations;

namespace Sidekick.Model
{
    public class AdminUserContext
    {
        public Admin AdminInfo { get; set; }
        public AdminLoginTransaction Tokens { get; set; }
    }

    public class AdminProfile
    {
        public Guid AdminId { get; set; }

        [Required(ErrorMessage = "This is required.")]
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string AdminIcon { get; set; }

        [Required]
        [DataType(DataType.EmailAddress)]
        [RegularExpression("^[a-zA-Z0-9_\\.-]+@([a-zA-Z0-9-]+\\.)+[a-zA-Z]{2,6}$", ErrorMessage = "E-mail is not valid")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        [RegularExpression("^(?=.*[A-Za-z])(?=.*\\d)[A-Za-z\\d]{5,}$",
            ErrorMessage = "Le mot de passe doit contenir au moins 5-20 caractères contenant uniquement des lettres et des chiffres.")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessage = "Le mot de passe et la confirmation de mot de passe ne correspondent pas")]
        public string ConfirmPassword { get; set; }

        public EAdminType AdminType { get; set; }
        public bool? IsEnabled { get; set; }
    }

    public class AdminLogin
    {
        [Required(ErrorMessage = "This is required.")]
        [DataType(DataType.EmailAddress)]
        [RegularExpression("^[a-zA-Z0-9_\\.-]+@([a-zA-Z0-9-]+\\.)+[a-zA-Z]{2,6}$", ErrorMessage = "E-mail is not valid")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "This is required.")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        [RegularExpression("^(?=.*[A-Za-z])(?=.*\\d)[A-Za-z\\d]{5,}$",
            ErrorMessage = "Le mot de passe doit contenir au moins 5-20 caractères contenant uniquement des lettres et des chiffres.")]
        public string Password { get; set; }

    }

    public class AdminList
    {
        public Guid AdminId { get; set; }
        public Guid ShopId { get; set; }
        public string ImageUrl { get; set; }
        public string FullName { get; set; }
        public EAdminType AdminType { get; set; }
        public bool? IsActive { get; set; }
    }

    public class ChangeRecordStatus
    {
        public int RecordId { get; set; }
        public bool IsActive { get; set; }
    }

    public class AdminForgotPassword
    {
        [Required(ErrorMessage = "Email address is required.")]
        [DataType(DataType.EmailAddress)]
        [RegularExpression("^[a-zA-Z0-9_\\.-]+@([a-zA-Z0-9-]+\\.)+[a-zA-Z]{2,6}$", ErrorMessage = "E-mail is not valid")]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }

    public class AdminLoginCredentials
    {
        [Required(ErrorMessage = "This is required.")]
        [DataType(DataType.EmailAddress)]
        [RegularExpression("^[a-zA-Z0-9_\\.-]+@([a-zA-Z0-9-]+\\.)+[a-zA-Z]{2,6}$", ErrorMessage = "Email invalide. Veuillez réessayer.")]
        public string Email { get; set; }

        public string Password { get; set; }

        [Required(ErrorMessage = "You must specify a registration Device Platform Web = 0, IOS = 1, Android = 2, Others = 3")]
        public EDevicePlatform Device { get; set; }

        public EAdminType AdminType { get; set; }
    }
}
