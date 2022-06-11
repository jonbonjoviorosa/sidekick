using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sidekick.Model
{
    [Table("BlockedUsers")]
    public class BlockedUser: APIBaseModel
    {
        [ForeignKey("Fk_UserId")]
        public int UserId { get; set; }

        [ForeignKey("Fk_BlockedUserId")]
        public int BlockedUserId { get; set; }

        public virtual User Fk_UserId { get; set; }
        public virtual User Fk_BlockedUserId { get; set; }
    }
}
