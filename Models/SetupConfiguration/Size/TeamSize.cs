using System;
using System.ComponentModel.DataAnnotations;

namespace Sidekick.Model.SetupConfiguration.Size
{
    public class TeamSize : APIBaseModel
    {
        public Guid SizeId { get; set; }
        [Required(ErrorMessage = "This field is required.")]
        public string SizeName { get; set; }
    }
}
