using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sidekick.Model
{
    [Table("UserPlayBadges")]
    public class UserPlayBadge: APIBaseModel
    {
        public Guid UserId { get; set; }

        public Guid SportId { get; set; }

        public int Level { get; set; }
    }
}
