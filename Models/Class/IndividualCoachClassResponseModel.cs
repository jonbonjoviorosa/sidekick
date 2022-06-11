using Sidekick.Model.Gym;
using Sidekick.Model.Language;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sidekick.Model.Class
{
    public class IndividualCoachClassResponseModel
    {
        public Guid CoachId { get; set; }

        public string CoachFirstName { get; set; }
        public string CoachLastName { get; set; }
        public string CoachImage { get; set; }
        public string CoachDescrption { get; set; }
        public Genders CoachGender { get; set; }
        public int? CoachAge { get; set; }

        public decimal Price { get; set; }
        public bool ParticipateToOffer { get; set; }

        public double? Ratings { get; set; }

        public string Location { get; set; }

        public IEnumerable<string> Specialties { get; set; }
        public IEnumerable<string> Badges { get; set; }
        public IEnumerable<GymViewModel> Gyms { get; set; }
        public IEnumerable<LanguageViewModel> Languages { get; set; }
        public IEnumerable<CoachNotAvailableScheduleViewModel> CoachNotAvailableSchedule { get; set; }
        public IEnumerable<CoachCustomScheduleViewModel> CoachCustomSchedule { get; set; }
        public CoachEverydaySchedule CoachEverydayScheduleViewModel { get; set; }
    }
}
