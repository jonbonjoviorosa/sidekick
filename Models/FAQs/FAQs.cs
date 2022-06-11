using System;
using System.ComponentModel.DataAnnotations;

namespace Sidekick.Model
{
    public class FAQs : APIBaseModel
    {
        public Guid FAQsId { get; set; }
        public string Question { get; set; }
        public string Answer { get; set; }
    }

    public class FAQsDto
    {
        public Guid? FAQsId { get; set; }

        [Required(ErrorMessage = "This field is required.")]
        public string Question { get; set; }

        [Required(ErrorMessage = "This field is required.")]
        public string Answer { get; set; }
    }

    public class FAQStatus
    {
        public Guid Id { get; set; }
        public bool IsEnabled { get; set; }
        public Guid IsEnabledBy { get; set; }
    }
}
