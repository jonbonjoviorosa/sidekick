using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sidekick.Model
{
    [Table("UserTeamMembers")]
    public class UserTeamMember: APIBaseModel
    {
        [ForeignKey("Fk_UserTeamId")]
        public int UserTeamId { get; set; }

        [ForeignKey("Fk_UserId")]
        public int MemberUserId { get; set; }

        public virtual UserTeam Fk_UserTeamId { get; set; }

        public virtual User Fk_UserId { get; set; }
    }
}
