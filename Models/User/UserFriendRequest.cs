using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sidekick.Model
{
    [Table("UserFriendRequests")]
    public class UserFriendRequest : APIBaseModel
    {
        [ForeignKey("Fk_UserId")]
        public int UserId { get; set; }

        [ForeignKey("Fk_FriendRequestUserId")]
        public int FriendRequestUserId { get; set; }

        public bool IsBlockedUser { get; set; }
        public Guid BlockedUserBy { get; set; }

        public virtual User Fk_UserId { get; set; }
        public virtual User Fk_FriendRequestUserId { get; set; }
    }
}
