using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sidekick.Model.Class
{
    public class IndividualClassViewModel: APIBaseModel
    {
        public Guid? ClassId { get; set; }

        public Guid? CoachId { get; set; }

        public string CoachFirstName { get; set; }  
        public string CoachLastName { get; set; }

        [Required]
        public decimal Price { get; set; }

        public bool ParticipateToOffer { get; set; }
        public string startTime { get; set; }
        public string endTime { get; set; }
        public IEnumerable<IndividualClassDetailsViewModel> CustomSchedPrices { get; set; }
        public CoachScheduleViewModel CoachSchedule { get; set; }
    }
}
