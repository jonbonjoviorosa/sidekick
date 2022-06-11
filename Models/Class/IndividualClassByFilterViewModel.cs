using Sidekick.Model.Gym;
using Sidekick.Model.Language;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sidekick.Model.Class
{
    public class IndividualClassByFilterViewModel
    {
        public Guid? ClassId { get; set; }
        public Guid? CoachId { get; set; }
        public int ChatReceiverId { get; set; }
        public string CoachFirstName { get; set; }
        public string Title { get; set; }
        public string CoachLastName { get; set; }
        public string CoachImage { get; set; }
        public string CoachDescrption { get; set; }
        public Genders CoachGender { get; set; }
        public int? CoachAge { get; set; }
        public string Location { get; set; }
        public string startTime { get; set; }
        public string endTime { get; set; }
        public decimal Price { get; set; }
        public bool ParticipateToOffer { get; set; }
        public double? Ratings { get; set; }
        public IEnumerable<IndividualClassDetailsViewModel> CustomSchedPrices { get; set; }
        public CoachScheduleViewModel CoachSchedule { get; set; }
        public IEnumerable<string> Specialties { get; set; }
        public IEnumerable<string> Badges { get; set; }
        public IEnumerable<GymViewModel> Gyms { get; set; }
        public IEnumerable<LanguageViewModel> Languages { get; set; }
        public IEnumerable<CoachNotAvailableScheduleViewModel> CoachNotAvailableSchedule { get; set; }
        public Notation Notation { get; set; }
        public IEnumerable<UserFriendViewModel> Friends { get; set; }
        public IEnumerable<CoachCustomScheduleViewModel> CoachCustomSchedule { get; set; }
        public CoachEverydaySchedule CoachEverydayScheduleViewModel { get; set; }
    }

    public class CoachNotAvailableScheduleViewModel
    {
        public DateTime Date { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
    }

    public class Notation
    {
        public decimal totalRatings { get; set; }
        public List<NotationDetails> Details { get; set; }
    }

    public class NotationDetails
    {
        public string Image { get; set; }
        public string Name { get; set; }
        public Ratings Ratings { get; set; }
        public string Date { get; set; }
    }
}
