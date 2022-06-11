using System;
using System.ComponentModel.DataAnnotations;

namespace Sidekick.Model.Class
{
    public class IndividualClassDetails: APIBaseModel
    {
        public Guid IndividualClassId { get; set; }
        public Guid GymId { get; set; }
        public int AreaId { get; set; }
        public bool IsOnline { get; set; }
        public int Participants { get; set; }
        public string Description { get; set; }
        public Guid LevelId { get; set; }
        public string Title { get; set; }
        public DateTime Date { get; set; }
        public bool IsRepeatEveryWeek { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }

        public CoachingDay? CoachingDay { get; set; }
        
        [Required]
        public decimal Price { get; set; }
    }
}
