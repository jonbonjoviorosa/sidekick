using System;
using System.ComponentModel.DataAnnotations;

namespace Sidekick.Model
{
    public class Location : APIBaseModel
    {
        public Guid LocationId { get; set; }
        [Required(ErrorMessage = "This field is required.")]
        public string Name { get; set; }
    }
}
