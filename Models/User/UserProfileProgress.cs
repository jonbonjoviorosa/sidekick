using Sidekick.Model.Badges;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sidekick.Model
{
    public class UserProfileProgressViewModel
    {
        public string Progress { get; set; }
        public bool DonePlayBadges { get; set; }
        public bool DoneTrainBadges { get; set; }
        public bool DoneUserGoals { get; set; }
    }

    public class UserProfileProgress_UpdateViewModel
    {
        public IEnumerable<UserPlayBadgeViewModel> PlayBadges { get; set; }
        public IEnumerable<UserTrainBadgeViewModel> TrainBadges { get; set; }

        public IEnumerable<Guid> Goals { get; set; }

    }
}
