using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sidekick.Model.Play
{
    public class InviteRequestResponseModel
    {
        public InviteRequestResponseModel()
        {
            PlayRequests = new List<PlayRequestViewModel>();
            UserTeams = new List<UserTeamResponseModel>();
        }
        public List<PlayRequestViewModel> PlayRequests { get; set; }
        public List<UserTeamResponseModel> UserTeams { get; set; }
    }
    public class PlayRequestViewModel
    {
        public Guid RequestId { get; set; }
        public Guid SportId { get; set; }
        public Guid UserId { get; set; }
        public Guid BookingId { get; set; }
        public string UserImage { get; set; }
        public string UserName { get; set; }
        public EGamePlayerStatus PlayerStatus { get; set; }

    }

    public class UserTeamResponseModel
    {
        public Guid UserTeamId { get; set; }
        
        public string TeamName { get; set; }

        public List<UserTeamMemberResponseModel> UserTeamMembers { get; set; }
    }

    public class UserTeamMemberResponseModel
    {
        public Guid UserId { get; set; }

        public string UserName { get; set; }
    }
}
