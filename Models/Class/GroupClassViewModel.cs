using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sidekick.Model.Class
{
    public class GroupClassViewModel
    {
        public Guid? GroupClassId { get; set; }

        public Guid? CoachId { get; set; }

        public string CoachFirstName { get; set; }

        public string CoachLastName { get; set; }

        [Required(ErrorMessage = "{0} is required!")]
        [StringLength(250)]
        public string Title { get; set; }

        public bool ByLevel { get; set; }
        public int AreaId { get; set; }

        public Guid LevelId { get; set; }

        public DateTime? Start { get; set; }

        public DateTime? End { get; set; }

        public string StartTime { get; set; }

        public int Duration { get; set; }

        public bool RepeatEveryWeek { get; set; }

        public int? DuringNo { get; set; }

        public EDuring? During { get; set; }

        [Required(ErrorMessage = "{0} is required!")]
        public int Participants { get; set; }

        [Required(ErrorMessage = "{0} is required!")]
        public decimal Price { get; set; }

        public bool IsLocation { get; set; }
        public Guid? LocationId { get; set; }
        public Guid? GymId { get; set; }

        public bool IsOnline { get; set; }

        [StringLength(2000)]
        public string Notes { get; set; }
    }
}
