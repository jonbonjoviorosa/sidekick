using System;
using System.Collections.Generic;

namespace Sidekick.Model
{
    public class UserTeamViewModel
    {
        public Guid UserTeamId { get; set; }
        public string TeamName { get; set; }
        public int MemberCount { get; set; }
        public List<UserTeamMemberViewModel> TeamMembers { get; set; }
    }
}
