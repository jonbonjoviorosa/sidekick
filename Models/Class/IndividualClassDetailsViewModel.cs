using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sidekick.Model.Class
{
    public class IndividualClassDetailsViewModel
    {
        [Required]
        public CoachingDay CoachingDay { get; set; }

        [Required]
        public decimal Price { get; set; }
    }
}
