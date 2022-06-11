using Sidekick.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sidekick.Api.Handlers.IBusiness
{
    public interface IUserHandler
    {
        Task<APIResponse<UserProfile>> GetUserProfile();
        Task<APIResponse<UserProfileProgressViewModel>> GetUserProfileProgress();
        Task<APIResponse> UpdateUserProfile(UserProfile userProfile);
        Task<APIResponse> UpdateProfileProgress(UserProfileProgress_UpdateViewModel progress);
        Task<APIResponse> GetFriendRequest(Guid friendUserId);
        Task<APIResponse> AcceptFriendRequest(Guid friendUserId);
        Task<APIResponse> GetAllFriendRequest(string filter);
        Task<APIResponse> DeclineFriendRequest(Guid userFriendRequestId);
        Task<APIResponse> AddToFriendRequest(Guid friendUserId);
        Task<APIResponse> GetFriendDetail(Guid friendUserId);
        Task<APIResponse> ReportFriend(string reason, Guid userId);
        Task<APIResponse> GetReports(Guid friendUserId);
        Task<APIResponse> BlockUser(Guid userId);
        Task<APIResponse> UnBlockUser(Guid userId);
        Task<APIResponse> GetBlockedList();
        Task<APIResponse> GetFriends(string filter);
        Task<APIResponse> DeleteFriend(Guid friendUserId);
        Task<APIResponse> CreateTeam(string teamName, List<Guid> memberUserId);
        Task<APIResponse> GetTeams(string filter);
        Task<APIResponse> EditTeam(UserTeamViewModel viewModel);
        Task<APIResponse> GetUserTeam(Guid userTeamId, string filter);
        Task<APIResponse> GetAllUsers(string filter);
        Task<APIResponse> AddedFriendRequests(string filter);
        Task<APIResponse> GetCoachUserProfile(Guid userId);
    }
}
