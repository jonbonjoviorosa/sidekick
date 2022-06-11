
using System;
using System.ComponentModel.DataAnnotations;

namespace Sidekick.Model
{
    public class ExternalConfigKey
    {
        [Key]
        public int Id { get; set; }
        public string Key { get; set; }
        public int Type { get; set; }
        public bool IsEnabled { get; set; }
        public Guid LastEditedBy { get; set; }
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{yyyy-MM-ddTHH\\:mm\\:ss}", ApplyFormatInEditMode = true)]
        public DateTime LastEditedDate { get; set; }
    }

    public class LoginAppleCredentials
    {
        [Required(ErrorMessage = "Ce champs est requis.")]
        public string AppleId { get; set; }

        [Required(ErrorMessage = "Ce champs est requis.")]
        public string AuthCode { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }
    }

    public class AppleTokenDto
    {
        public string access_token { get; set; }
        public string token_type { get; set; }
        public string expires_in { get; set; }
        public string refresh_token { get; set; }
        public string id_token { get; set; }
        public string error { get; set; }
        public string error_description { get; set; }
    }

    public class AppleTokenRequestDto
    {
        public string client_id { get; set; }
        public string client_secret { get; set; }
        public string code { get; set; }
        public string grant_type { get; set; }
    }

    public class AppleSignInConst
    {
        public const string authorization_code = "authorization_code";
        public const string refresh_token = "refresh_token";
        public const string client_id = "client_id";
        public const string client_secret = "client_secret";
        public const string grant_type = "grant_type";
        public const string code_type = "code";
    }
}
       