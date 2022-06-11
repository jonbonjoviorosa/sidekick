using System;
using System.Collections.Generic;

namespace Sidekick.Model
{
    public class FreeGame : APIBaseModel
    {
        public Guid? GameId { get; set; }
        public Guid SportId { get; set; }
        public Guid UserId { get; set; }
        public string EventName { get; set; }
        public DateTime TimeStart { get; set; }
        public DateTime TimeEnd { get; set; }
        public bool IsPrivate { get; set; }
        public bool? IsCaptain { get; set; }
        public bool? IsPublic { get; set; }
        public int NoOfPlayers { get; set; }
    }

    //public List<UserFriend> FriendUserId { get; set; }
}
