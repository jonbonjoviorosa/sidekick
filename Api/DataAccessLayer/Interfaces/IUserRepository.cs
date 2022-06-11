using Sidekick.Api.Helpers;
using Sidekick.Model;
using Sidekick.Model.Badges;
using Sidekick.Model.Class;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sidekick.Api.DataAccessLayer.Interfaces
{
    public interface IUserRepository
    {
        APIResponse Add(UserRegistration _user, IMainHttpClient _mhttpc = null, APIConfigurationManager _conf = null);
        //APIResponse Update(UserProfile _userProfile);
        Task<User> GetUser(Guid userId);
        //APIResponse Add(UserRegistrationViaEmail _user, IMainHttpClient _mhttpc = null, APIConfigurationManager _conf = null);
        Task<APIResponse> Update(UserProfile _userProfile);
        Task<User> UpdateUser(User user, UserProfile updatedUserProfile);
        Task UpdateUser(User user);
        Task InsertUpdateUserAddress(UserAddress userAddress);
        Task InsertUserPlayBadges(IEnumerable<UserPlayBadgeViewModel> userBadges);
        Task InsertUserTrainBadges(IEnumerable<UserTrainBadgeViewModel> userBadges);
        Task InsertUserGoals(IEnumerable<UserGoal> goals, Guid userId);
        Task<IEnumerable<UserPlayBadge>> GetUserPlayBadges();
        Task<IEnumerable<UserPlayBadgeViewModel>> GetPlayBadgesWithIcon(Guid userId);
        Task<IEnumerable<UserTrainBadgeViewModel>> GetTrainBadgesWithIcon(Guid userId);
        Task<IEnumerable<UserFriend>> GetUserFriend(int userId);
        Task<IEnumerable<UpcomingBooking>> GetUpcomingBooking(Guid userId);
        Task<IEnumerable<UserTrainBadge>> GetUserTrainBadges();
        Task<IEnumerable<UserGoal>> GetUserGoals(Guid userId);
        Task DeleteUserPlayBadges(IEnumerable<UserPlayBadge> userBadges);
        Task DeleteUserTrainBadges(IEnumerable<UserTrainBadge> userBadges);
        Task DeleteUserGoals(IEnumerable<UserGoal> goals);
        Task<UserAddress> GetUserAddress(Guid userId);
        APIResponse Login(LoginCredentials _user);
        APIResponse SetFCMToken(string UserToken, string FCMToken, EDevicePlatform DeviceType);
        APIResponse ReGenerateTokens(UserLoginTransaction _user);
        APIResponse MobileLogout(LogoutCredentials _user);
        APIResponse UpdateUserStatus(string auth, int id);
        APIResponse VerifyUserCode(VerifyCode _verification, APIConfigurationManager _conf = null);
        APIResponse ResendVerificationCode(ResendCode _verification, APIConfigurationManager _conf = null, IMainHttpClient _mhttpc = null);
        APIResponse ForgotUserPassword(UserForgotPassword _user, APIConfigurationManager _conf = null);
        APIResponse ChangeUserPassword(UserChangePassword _user);

        APIResponse GetCountries();
        Task<UserFriendViewModel> GetFriendRequest(Guid friendUserId);
        Task<bool> AcceptFriendRequest(Guid friendUserId);
        Task<IEnumerable<UserFriendViewModel>> GetAllFriendRequest(string filter);
        Task<bool> DeclineFriendRequest(Guid userFriendRequestId);
        Task<bool> AddToFriendRequest(Guid friendUserId);
        Task<UserFriendViewModel> GetFriendDetail(Guid friendUserId);
        Task<bool> ReportFriend(string reason, Guid userId);
        Task<IEnumerable<Report>> GetReports(Guid friendUserId);
        Task<APIResponse> GetAllReports();
        Task BlockUser(Guid userId);
        Task UnBlockUser(Guid userId);
        Task<IEnumerable<BlockViewModel>> GetBlockedList(Guid userId);
        Task<IEnumerable<UserFriendViewModel>> GetFriends(Guid userId, string filter);
        Task<bool> DeleteFriend(Guid currentUser, Guid friendUserId);
        Task<bool> CreateTeam(string teamName, List<Guid> memberUserId);
        Task<IEnumerable<UserTeamViewModel>> GetTeams(Guid currentUserId, string filter);
        Task<UserTeamViewModel> GetUserTeam(Guid userTeamId, string filter);
        Task<bool> EditTeam(UserTeamViewModel viewModel);
        Task<IEnumerable<UserFriendViewModel>> GetAllUsers(Guid currentUser, string filter);
        Task<CoachRenderViewModel> GetCoachUserProfile(Guid userId);
        Task<APIResponse> GetUserList(string auth);
        Task<IEnumerable<UserFriendViewModel>> AddedFriendRequests(Guid currentUser, string filter);
        Task<APIResponse> UpdateReport(string auth, ReportDto report);
        Task<APIResponse> DeleteReport(string auth, ReportDto report);
        Task<APIResponse> ChangeStatus(string auth, ChangeStatus user);
        Task<APIResponse> SendRequest(RequestViewModel model);
        Task<APIResponse> GetUserRequests();
        Task<APIResponse> GetCoachRequests();
        Task<APIResponse> UpdateUserRequest(string auth, UserRequestViewModel userRequest);
        Task<APIResponse> UpdateCoachRequest(string auth, CoachRequestViewModel coachRequest);
        Task<APIResponse> GetUserUpdates();
        Task<APIResponse> GetAllActivities();
        Task<APIResponse> GetAllRequest();
        Task<APIResponse> GetAllWhatsNew();
        Task<APIResponse> GetUserUpdatesByDate(string date);
        Task<APIResponse> AddUserReviews(UserReviews _model);
        Task<APIResponse> GetUserReviews();
        Task<APIResponse> GetCoachToReview();
        Task<Notation> GetEnumUserReviews(Guid userId);
        Task<Notation> GetGroupUserReviews(Guid userId);
        Task<APIResponse> UpdateUser(string auth, ReportDto report);
        UserLoginTransaction AddLoginTransactionForUser(User _user);
    }
}
