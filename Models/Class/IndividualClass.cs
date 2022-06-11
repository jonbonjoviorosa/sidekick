using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sidekick.Model.Class
{
    public class IndividualClass: APIBaseModel
    {
        public Guid ClassId { get; set; }

        public Guid CoachId { get; set; }

        [Required]
        public decimal Price { get; set; }

        public bool ParticipateToOffer { get; set; }
    }
}
