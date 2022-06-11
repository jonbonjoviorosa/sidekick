
using System;
using System.ComponentModel.DataAnnotations;

namespace Sidekick.Model
{
    public class UserPlayBadgeViewModel
    {
        [Required(ErrorMessage = "{0} is required!")]
        public Guid SportId { get; set; }

        [Required(ErrorMessage = "{0} is required!")]
        [Range(typeof(int), "1", "10", ErrorMessage = "Minimum value is {1} and the maximum value is {2}")]
        public int Level { get; set; }


        public string Sport { get; set; }
        public string Icon { get; set; }
    }
}
