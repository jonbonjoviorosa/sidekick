using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sidekick.Model
{
    public class UserFriendViewModel
    {
        public Guid UserId { get; set; }
        public int UserChatId { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string ImageUrl { get; set; }

        public string Location { get; set; }
        public string Description { get; set; }
        public IEnumerable<UserTrainBadgeViewModel> TrainBadges { get; set; }
        public IEnumerable<UserPlayBadgeViewModel> PlayBadges { get; set; }
    }
}
