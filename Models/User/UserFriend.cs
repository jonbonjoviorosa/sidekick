using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sidekick.Model
{
    [Table("UserFriends")]
    public class UserFriend : APIBaseModel
    {
        [ForeignKey("Fk_UserId")]
        public int UserId { get; set; }

        [ForeignKey("Fk_FriendUserId")]
        public int FriendUserId { get; set; }
        public bool IsBlockedUser { get; set; }
        public Guid BlockedUserBy { get; set; }

        public virtual User Fk_UserId { get; set; }
        public virtual User Fk_FriendUserId { get; set; }
    }

}
