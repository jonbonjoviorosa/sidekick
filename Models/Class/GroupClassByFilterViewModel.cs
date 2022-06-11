using Sidekick.Model.Gym;
using Sidekick.Model.Language;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sidekick.Model.Class
{
    public class GroupClassByFilterViewModel
    {
        public Guid? GroupClassId { get; set; }

        public Guid? CoachId { get; set; }
        public int ChatReceiverId { get; set; }
        public string CoachFirstName { get; set; }

        public string CoachLastName { get; set; }
        public string CoachImage { get; set; }
        public string CoachDescription { get; set; }
        public Genders CoachGender { get; set; }

        public string Title { get; set; }
        public bool ByLevel { get; set; }

        public Guid LevelId { get; set; }
        public string Level { get; set; }

        public DateTime? Start { get; set; }

        public DateTime? End { get; set; }

        public bool RepeatEveryWeek { get; set; }
        public int dayOfWeek { get; set; }
        public int? DuringNo { get; set; }

        public EDuring? During { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public int Duration { get; set; }

        public int MaxParticipants { get; set; }

        public decimal Price { get; set; }

        public bool IsLocation { get; set; }
        public Guid? LocationId { get; set; }
        public string Location { get; set; }
        public bool IsOnline { get; set; }
        public string Notes { get; set; }
        public IEnumerable<GymLocationViewModel> GymLocation { get; set; }
        
        public IEnumerable<GroupClassParticipantsViewModel> Participants { get; set; }
        public IEnumerable<string> Specialties { get; set; }
        public IEnumerable<string> Badges { get; set; }
        public IEnumerable<GymViewModel> Gyms { get; set; }
        public IEnumerable<UserGoalViewModel> UserGoals { get; set; }
        public IEnumerable<LanguageViewModel> Languages { get; set; }
        public Notation Notation { get; set; }
    }
}
