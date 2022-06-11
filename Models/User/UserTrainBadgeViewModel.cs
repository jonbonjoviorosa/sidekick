using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sidekick.Model
{
    public class UserTrainBadgeViewModel
    {
        [Required(ErrorMessage = "{0} is required!")]
        public Guid SpecialtyId { get; set; }


        public string Specialty { get; set; }
        public string Icon { get; set; }
    }
}
