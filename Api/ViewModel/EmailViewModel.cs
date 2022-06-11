using System.ComponentModel.DataAnnotations;

namespace Sidekick.Api.ViewModel
{
    public class EmailViewModel
    {
        [Required(ErrorMessage = "{0} is required!")]
        public string Subject { get; set; }

        [Required(ErrorMessage = "{0} is required!")]
        public string Body { get; set; }

        [Required(ErrorMessage = "{0} is required!")]
        [EmailAddress]
        public string SendTo { get; set; }
    }

    public class EmailBodyViewModel
    {
        [Required(ErrorMessage = "{0} is required!")]
        public string Body { get; set; }
    }
}
