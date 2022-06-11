using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sidekick.Model.Class
{
    [Table("GroupClasses")]
    public class GroupClass: APIBaseModel
    {
        public Guid GroupClassId { get; set; }

        public Guid CoachId { get; set; }

        [Required]
        [StringLength(250)]
        public string Title { get; set; }

        public bool ByLevel { get; set; }

        public Guid LevelId { get; set; }

        public DateTime? Start { get; set; }

        public DateTime? End { get; set; }

        public string StartTime { get; set; }

        public int? Duration { get; set; }

        public bool RepeatEveryWeek { get; set; }

        public int? DuringNo { get; set; }

        public EDuring? During { get; set; }

        [Required]
        public int Participants { get; set; }

        [Required]
        public decimal Price { get; set; }

        public bool IsLocation { get; set; }

        public Guid? LocationId { get; set; }
        public Guid? GymId { get; set; }

        public bool IsOnline { get; set; }

        [StringLength(2000)]
        public string Notes { get; set; }
        public int AreaId { get; set; }
    }
}
