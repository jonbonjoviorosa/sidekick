using AutoMapper;
using Sidekick.Api.Configurations.Resources;
using Sidekick.Api.DataAccessLayer.Interfaces;
using Sidekick.Api.Handlers.IBusiness;
using Sidekick.Api.Helpers;
using Sidekick.Api.Helpers.IHelpers;
using Sidekick.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sidekick.Api.Handlers.Business
{
    public class UserHandler : IUserHandler
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IUserRepository userRepository;
        private readonly ILoggerManager loggerManager;
        private readonly IUserHelper userHelper;
        private readonly IMapper mapper;
        private readonly INationalityRepository nationalityRepository;
        private readonly APIConfigurationManager APIConfig;

        public UserHandler(IUnitOfWork unitOfWork,
            IUserRepository userRepository,
            ILoggerManager loggerManager,
            IUserHelper userHelper,
            IMapper mapper,
            INationalityRepository nationalityRepository,
            APIConfigurationManager APIConfig)
        {
            this.unitOfWork = unitOfWork;
            this.userRepository = userRepository;
            this.loggerManager = loggerManager;
            this.userHelper = userHelper;
            this.mapper = mapper;
            this.nationalityRepository = nationalityRepository;
            this.APIConfig = APIConfig;
        }

        public async Task<APIResponse<UserProfile>> GetUserProfile()
        {
            try
            {
                loggerManager.LogInfo(ETransaction.SUCCESS, Helper.GetCurrentMethodName(), EOperation.SELECT);
                var currentLogin = userHelper.GetCurrentUserGuidLogin();
                var getUser = await userRepository.GetUser(currentLogin);
                var getAddress = await userRepository.GetUserAddress(currentLogin);
                var getPlayBadges = await userRepository.GetPlayBadgesWithIcon(currentLogin);
                var getTrainBadges = await userRepository.GetTrainBadgesWithIcon(currentLogin);
                var getFriends = await userRepository.GetUserFriend(getUser.Id);
                var getBookings = await userRepository.GetUpcomingBooking(currentLogin);
                // mapped
                var mappedUser = mapper.Map<UserProfile>(getUser);
                mappedUser.Age = DateTime.Today.Year - Convert.ToDateTime(mappedUser.DateOfBirth).Year;
                mappedUser.FriendCount = getFriends.Count();
                var getNationality = await nationalityRepository.GetNationality(getUser.NationalityId);
                if (getNationality != null)
                {
                    mappedUser.NationalityName = getNationality._Nationality;
                }
                
                mappedUser.UserAddress = getAddress;
                mappedUser.PlayBadges = getPlayBadges;
                mappedUser.TrainBadges = getTrainBadges;
                mappedUser.Bookings = getBookings;
                return new APIResponse<UserProfile>
                {
                    Status = Status.Success,
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Payload = mappedUser
                };
            }
            catch (Exception ex)
            {
                if (ex.Message == EResponseAction.Unauthorized.ToString())
                    return APIResponseHelper<UserProfile>.ReturnAPIResponse(EResponseAction.Unauthorized);

                loggerManager.LogInfo(ETransaction.FAILED, Helper.GetCurrentMethodName(), EOperation.SELECT);
                loggerManager.LogException(ex);
                return APIResponseHelper<UserProfile>.ReturnAPIResponse(EResponseAction.InternalServerError);
            }
        }

        public async Task<APIResponse> GetCoachUserProfile(Guid userId)
        {
            loggerManager.LogInfo(ETransaction.SUCCESS, Helper.GetCurrentMethodName(), EOperation.SELECT);
            loggerManager.LogDebugObject(userId);
            try
            {
                var userProfile = await userRepository.GetCoachUserProfile(userId);
                if (userProfile != null)
                {
                    return new APIResponse
                    {
                        Message = "Retrieved Coach Profile",
                        Payload = userProfile,
                        StatusCode = System.Net.HttpStatusCode.OK
                    };
                }

                return new APIResponse
                {
                    Message = "User is not existing",
                    Payload = userProfile,
                    StatusCode = System.Net.HttpStatusCode.NotFound
                };

            }
            catch(Exception ex)
            {
                if (ex.Message == EResponseAction.Unauthorized.ToString())
                    return APIResponseHelper.ReturnAPIResponse(EResponseAction.Unauthorized);

                loggerManager.LogInfo(ETransaction.FAILED, Helper.GetCurrentMethodName(), EOperation.SELECT);
                loggerManager.LogException(ex);
                return APIResponseHelper.ReturnAPIResponse(EResponseAction.InternalServerError);
            }
        }

        public async Task<APIResponse<UserProfileProgressViewModel>> GetUserProfileProgress()
        {
            try
            {
                int counter = 100;
                loggerManager.LogInfo(ETransaction.SUCCESS, Helper.GetCurrentMethodName(), EOperation.SELECT);
                var currentLogin = userHelper.GetCurrentUserGuidLogin();

                var getPlayBadges = await userRepository.GetPlayBadgesWithIcon(currentLogin);
                var getTrainBadges = await userRepository.GetTrainBadgesWithIcon(currentLogin);
                var getGoals = await userRepository.GetUserGoals(currentLogin);

                counter -= (getPlayBadges.Any()) ? 0 : 10;
                counter -= (getTrainBadges.Any()) ? 0 : 10;
                counter -= (getGoals.Any()) ? 0 : 10;

                return new APIResponse<UserProfileProgressViewModel>
                {
                    Status = Status.Success,
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Payload = new UserProfileProgressViewModel()
                    {
                        Progress = counter.ToString() + "%",
                        DonePlayBadges = getPlayBadges.Any() ? true: false,
                        DoneTrainBadges = getTrainBadges.Any() ? true : false,
                        DoneUserGoals = getGoals.Any() ? true : false,
                    }
                };
            }
            catch (Exception ex)
            {
                if (ex.Message == EResponseAction.Unauthorized.ToString())
                    return APIResponseHelper<UserProfileProgressViewModel>.ReturnAPIResponse(EResponseAction.Unauthorized);

                loggerManager.LogInfo(ETransaction.FAILED, Helper.GetCurrentMethodName(), EOperation.SELECT);
                loggerManager.LogException(ex);
                return APIResponseHelper<UserProfileProgressViewModel>.ReturnAPIResponse(EResponseAction.InternalServerError);
            }
        }

        public async Task<APIResponse> UpdateUserProfile(UserProfile userProfile)
        {
            APIResponse apiResp = new APIResponse();
            unitOfWork.BeginTransaction();
            try
            {
                loggerManager.LogInfo(ETransaction.RUN, Helper.GetCurrentMethodName(), EOperation.UPDATE);
                loggerManager.LogDebugObject(userProfile);
                var getUserProfile = await userRepository.GetUser(userHelper.GetCurrentUserGuidLogin());
                if (getUserProfile != null)
                {
                    var updatedUser = await userRepository.UpdateUser(getUserProfile, userProfile);
                    await userRepository.InsertUpdateUserAddress(userProfile.UserAddress);

                    // play badges    
                    var userPlayBadges = await userRepository.GetUserPlayBadges();
                    await userRepository.DeleteUserPlayBadges(userPlayBadges);
                    await userRepository.InsertUserPlayBadges(userProfile.PlayBadges);

                    // train badges
                    var userTrainBadges = await userRepository.GetUserTrainBadges();
                    await userRepository.DeleteUserTrainBadges(userTrainBadges);
                    await userRepository.InsertUserTrainBadges(userProfile.TrainBadges);

                    apiResp.Message = Messages.UserProfileUpdateSuccess;
                    apiResp.Status = Status.Success;
                    apiResp.StatusCode = System.Net.HttpStatusCode.OK;
                }
                //From SuperAdmin BO
                else if(userProfile.UserId != Guid.Empty)
                {
                    var userToUpdate = await userRepository.GetUser(userProfile.UserId);
                    if(userToUpdate != null)
                    {
                        var currentUser = userHelper.GetCurrentUserGuidLogin();
                        userToUpdate.ImageUrl = userProfile.ImageUrl;
                        userToUpdate.Description = userProfile.Description;
                        userToUpdate.NationalityId = userProfile.NationalityId;
                        userToUpdate.FirstName = userProfile.FirstName;
                        userToUpdate.LastName = userProfile.LastName;
                        userToUpdate.Email = userProfile.Email;
                        userToUpdate.MobileNumber = userProfile.MobileNumber;
                        //userToUpdate.IsEnabled = userProfile.IsActive;
                        if (!string.IsNullOrWhiteSpace(userProfile.Password))
                        {
                            userToUpdate.Password = userToUpdate.HashP(userProfile.Password, APIConfig.TokenKeys.Key);
                        }

                        userToUpdate.LastEditedBy = currentUser;
                        userToUpdate.LastEditedDate = DateTime.Now;

                        await userRepository.UpdateUser(userToUpdate);
                        apiResp.Message = Messages.UserProfileUpdateSuccess;
                        apiResp.Status = Status.Success;
                        apiResp.StatusCode = System.Net.HttpStatusCode.OK;
                    }
                    else
                    {
                        unitOfWork.RollbackTransaction();
                        apiResp.Message = "User Not Existing";
                        apiResp.Status = Status.Failed;
                        apiResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                    }                 
                }
                else
                {
                    unitOfWork.RollbackTransaction();
                    apiResp.Message = Messages.UserProfileUpdateFailed;
                    apiResp.Status = Status.Failed;
                    apiResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                }

                unitOfWork.CommitTransaction();
            }
            catch (Exception ex)
            {
                unitOfWork.RollbackTransaction();

                if (ex.Message == EResponseAction.Unauthorized.ToString())
                    return APIResponseHelper.ReturnAPIResponse(EResponseAction.Unauthorized);

                loggerManager.LogInfo(ETransaction.FAILED, Helper.GetCurrentMethodName(), EOperation.UPDATE);
                loggerManager.LogException(ex);

                return APIResponseHelper.ReturnAPIResponse(EResponseAction.InternalServerError);
            }
            return apiResp;
        }

        public async Task<APIResponse> UpdateProfileProgress(UserProfileProgress_UpdateViewModel progress)
        {
            APIResponse apiResp = new APIResponse();
            unitOfWork.BeginTransaction();
            try
            {
                loggerManager.LogInfo(ETransaction.RUN, Helper.GetCurrentMethodName(), EOperation.UPDATE);
                loggerManager.LogDebugObject(progress);
                var currentLogin = userHelper.GetCurrentUserGuidLogin();
                var getUserProfile = await userRepository.GetUser(currentLogin);
                if (getUserProfile != null)
                {
                    // play badges    
                    if (progress.PlayBadges != null)
                    {
                        if (progress.PlayBadges.Any())
                        {
                            var userPlayBadges = await userRepository.GetUserPlayBadges();
                            await userRepository.DeleteUserPlayBadges(userPlayBadges);
                            await userRepository.InsertUserPlayBadges(progress.PlayBadges);
                        }
                    }

                    if (progress.TrainBadges != null)
                    {
                        if (progress.TrainBadges.Any())
                        {
                            var userTrainBadges = await userRepository.GetUserTrainBadges();
                            await userRepository.DeleteUserTrainBadges(userTrainBadges);
                            await userRepository.InsertUserTrainBadges(progress.TrainBadges);
                        }
                    }

                    if (progress.Goals != null)
                    {
                        if (progress.Goals.Any())
                        {
                            var userGoals = await userRepository.GetUserGoals(currentLogin);
                            await userRepository.DeleteUserGoals(userGoals);
                            var mappedGoals = mapper.Map<IEnumerable<UserGoal>>(progress.Goals);
                            await userRepository.InsertUserGoals(mappedGoals, currentLogin);
                        }
                    }

                    apiResp.Message = Messages.UserProfileUpdateSuccess;
                    apiResp.Status = Status.Success;
                    apiResp.StatusCode = System.Net.HttpStatusCode.OK;
                }
                else
                {
                    unitOfWork.RollbackTransaction();
                    apiResp.Message = Messages.UserProfileUpdateFailed;
                    apiResp.Status = Status.Failed;
                    apiResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                }

                unitOfWork.CommitTransaction();
            }
            catch (Exception ex)
            {
                unitOfWork.RollbackTransaction();

                if (ex.Message == EResponseAction.Unauthorized.ToString())
                    return APIResponseHelper.ReturnAPIResponse(EResponseAction.Unauthorized);

                loggerManager.LogInfo(ETransaction.FAILED, Helper.GetCurrentMethodName(), EOperation.UPDATE);
                loggerManager.LogException(ex);

                return APIResponseHelper.ReturnAPIResponse(EResponseAction.InternalServerError);
            }
            return apiResp;
        }

        public async Task<APIResponse> GetFriendRequest(Guid friendUserId)
        {
            try
            {
                loggerManager.LogInfo(ETransaction.RUN, Helper.GetCurrentMethodName(), EOperation.SELECT);
                loggerManager.LogDebugObject(friendUserId);
                var friendRequest = await userRepository.GetFriendRequest(friendUserId);
                return new APIResponse
                {
                    Status = Status.Success,
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Payload = friendRequest
                };
            }
            catch (Exception ex)
            {
                unitOfWork.RollbackTransaction();

                if (ex.Message == EResponseAction.Unauthorized.ToString())
                    return APIResponseHelper.ReturnAPIResponse(EResponseAction.Unauthorized);

                loggerManager.LogInfo(ETransaction.FAILED, Helper.GetCurrentMethodName(), EOperation.SELECT);
                loggerManager.LogException(ex);

                return APIResponseHelper.ReturnAPIResponse(EResponseAction.InternalServerError);
            }
        }

        public async Task<APIResponse> GetAllFriendRequest(string filter)
        {
            try
            {
                loggerManager.LogInfo(ETransaction.RUN, Helper.GetCurrentMethodName(), EOperation.SELECT);
                var friendRequests = await userRepository.GetAllFriendRequest(filter);
                return new APIResponse
                {
                    Status = Status.Success,
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Payload = friendRequests, 
                    Message = !friendRequests.Any() ? "No Friend Requests" : $"Friend Request Count: {friendRequests.Count()}"
                };
            }
            catch (Exception ex)
            {
                unitOfWork.RollbackTransaction();

                if (ex.Message == EResponseAction.Unauthorized.ToString())
                    return APIResponseHelper.ReturnAPIResponse(EResponseAction.Unauthorized);

                loggerManager.LogInfo(ETransaction.FAILED, Helper.GetCurrentMethodName(), EOperation.SELECT);
                loggerManager.LogException(ex);

                return APIResponseHelper.ReturnAPIResponse(EResponseAction.InternalServerError);
            }
        }

        public async Task<APIResponse> AcceptFriendRequest(Guid friendUserId)
        {
            try
            {
                loggerManager.LogInfo(ETransaction.RUN, Helper.GetCurrentMethodName(), EOperation.INSERT_UPDATE);
                loggerManager.LogDebugObject(friendUserId);
                var result = await userRepository.AcceptFriendRequest(friendUserId);
                return new APIResponse
                {
                    Status = Status.Success,
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Message = result == true ? "Accepted Friend Request" : $"No request existing with user {friendUserId}"
                };
           }          
            catch (Exception ex)
            { 
                unitOfWork.RollbackTransaction();

                if (ex.Message == EResponseAction.Unauthorized.ToString())
                    return APIResponseHelper.ReturnAPIResponse(EResponseAction.Unauthorized);

                loggerManager.LogInfo(ETransaction.FAILED, Helper.GetCurrentMethodName(), EOperation.INSERT_UPDATE);
                loggerManager.LogException(ex);

                return APIResponseHelper.ReturnAPIResponse(EResponseAction.InternalServerError);
            }
        }

        public async Task<APIResponse> DeclineFriendRequest(Guid userFriendRequestId)
        {
            try
            {
                loggerManager.LogInfo(ETransaction.RUN, Helper.GetCurrentMethodName(), EOperation.DELETE);
                loggerManager.LogDebugObject(userFriendRequestId);
                var result = await userRepository.DeclineFriendRequest(userFriendRequestId);
                return new APIResponse
                {
                    Status = Status.Success,
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Message = result == true ? "Declined Friend Request" : $"User Id {userFriendRequestId} is not existing in the request"
                };
            }
            catch (Exception ex)
            {
                unitOfWork.RollbackTransaction();

                if (ex.Message == EResponseAction.Unauthorized.ToString())
                    return APIResponseHelper.ReturnAPIResponse(EResponseAction.Unauthorized);

                loggerManager.LogInfo(ETransaction.FAILED, Helper.GetCurrentMethodName(), EOperation.DELETE);
                loggerManager.LogException(ex);

                return APIResponseHelper.ReturnAPIResponse(EResponseAction.InternalServerError);
            }
        }

        public async Task<APIResponse> AddToFriendRequest(Guid friendUserId)
        {
            try
            {
                loggerManager.LogInfo(ETransaction.RUN, Helper.GetCurrentMethodName(), EOperation.INSERT);
                loggerManager.LogDebugObject(friendUserId);
                var result = await userRepository.AddToFriendRequest(friendUserId);
                return new APIResponse
                {
                    Status = Status.Success,
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Message = result == true ? "Added Friend Request" : $"Already have pending request with or already friends with {friendUserId}"
                };
            }
            catch (Exception ex)
            {
                unitOfWork.RollbackTransaction();

                if (ex.Message == EResponseAction.Unauthorized.ToString())
                    return APIResponseHelper.ReturnAPIResponse(EResponseAction.Unauthorized);

                loggerManager.LogInfo(ETransaction.FAILED, Helper.GetCurrentMethodName(), EOperation.INSERT);
                loggerManager.LogException(ex);

                return APIResponseHelper.ReturnAPIResponse(EResponseAction.InternalServerError);
            }
        }

        public async Task<APIResponse> GetFriendDetail(Guid friendUserId)
        {
            try
            {
                loggerManager.LogInfo(ETransaction.RUN, Helper.GetCurrentMethodName(), EOperation.SELECT);
                loggerManager.LogDebugObject(friendUserId);
                var friendDetail = await userRepository.GetFriendDetail(friendUserId);
                return new APIResponse
                {
                    Status = Status.Success,
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Payload = friendDetail
                };
            }
            catch (Exception ex)
            {
                unitOfWork.RollbackTransaction();

                if (ex.Message == EResponseAction.Unauthorized.ToString())
                    return APIResponseHelper.ReturnAPIResponse(EResponseAction.Unauthorized);

                loggerManager.LogInfo(ETransaction.FAILED, Helper.GetCurrentMethodName(), EOperation.SELECT);
                loggerManager.LogException(ex);

                return APIResponseHelper.ReturnAPIResponse(EResponseAction.InternalServerError);
            }
        }

        public async Task<APIResponse> ReportFriend(string reason, Guid userId)
        {
            try
            {
                loggerManager.LogInfo(ETransaction.RUN, Helper.GetCurrentMethodName(), EOperation.INSERT);
                loggerManager.LogDebugObject(reason);
                loggerManager.LogDebugObject(userId);
                var result = await userRepository.ReportFriend(reason, userId);
                var status = result == true ? Status.Success : "Failed";
                var statusCode = result == true ? System.Net.HttpStatusCode.OK : System.Net.HttpStatusCode.NotFound;
                return new APIResponse
                {
                    Status = status,
                    StatusCode = statusCode,
                    Message = result == true ? $"Report Successful for {userId}" : $"User {userId} is not existing"
                };
            }
            catch (Exception ex)
            {
                unitOfWork.RollbackTransaction();

                if (ex.Message == EResponseAction.Unauthorized.ToString())
                    return APIResponseHelper.ReturnAPIResponse(EResponseAction.Unauthorized);

                loggerManager.LogInfo(ETransaction.FAILED, Helper.GetCurrentMethodName(), EOperation.INSERT);
                loggerManager.LogException(ex);

                return APIResponseHelper.ReturnAPIResponse(EResponseAction.InternalServerError);
            }
        }

        public async Task<APIResponse> GetReports(Guid friendUserId)
        {
            try
            {
                loggerManager.LogInfo(ETransaction.RUN, Helper.GetCurrentMethodName(), EOperation.SELECT);
                loggerManager.LogDebugObject(friendUserId);
                var reports = await userRepository.GetReports(friendUserId);
                return new APIResponse
                {
                    Status = Status.Success,
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Payload = reports
                };
            }
            catch (Exception ex)
            {
                unitOfWork.RollbackTransaction();

                if (ex.Message == EResponseAction.Unauthorized.ToString())
                    return APIResponseHelper.ReturnAPIResponse(EResponseAction.Unauthorized);

                loggerManager.LogInfo(ETransaction.FAILED, Helper.GetCurrentMethodName(), EOperation.SELECT);
                loggerManager.LogException(ex);

                return APIResponseHelper.ReturnAPIResponse(EResponseAction.InternalServerError);
            }
        }

        public async Task<APIResponse> BlockUser(Guid userId)
        {
            try
            {
                loggerManager.LogInfo(ETransaction.RUN, Helper.GetCurrentMethodName(), EOperation.INSERT);
                loggerManager.LogDebugObject(userId);
                await userRepository.BlockUser(userId);
                return new APIResponse
                {
                    Status = Status.Success,
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Message = $"Successfully blocked user {userId}"
                };
            }
            catch (Exception ex)
            {
                unitOfWork.RollbackTransaction();

                if (ex.Message == EResponseAction.Unauthorized.ToString())
                    return APIResponseHelper.ReturnAPIResponse(EResponseAction.Unauthorized);

                loggerManager.LogInfo(ETransaction.FAILED, Helper.GetCurrentMethodName(), EOperation.INSERT);
                loggerManager.LogException(ex);

                return APIResponseHelper.ReturnAPIResponse(EResponseAction.InternalServerError);
            }
        }

        public async Task<APIResponse> UnBlockUser(Guid userId)
        {
            try
            {
                loggerManager.LogInfo(ETransaction.RUN, Helper.GetCurrentMethodName(), EOperation.DELETE);
                loggerManager.LogDebugObject(userId);
                await userRepository.UnBlockUser(userId);
                return new APIResponse
                {
                    Status = Status.Success,
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Message = $"Successfully unblocked user {userId}"
                };
            }
            catch (Exception ex)
            {
                unitOfWork.RollbackTransaction();

                if (ex.Message == EResponseAction.Unauthorized.ToString())
                    return APIResponseHelper.ReturnAPIResponse(EResponseAction.Unauthorized);

                loggerManager.LogInfo(ETransaction.FAILED, Helper.GetCurrentMethodName(), EOperation.DELETE);
                loggerManager.LogException(ex);

                return APIResponseHelper.ReturnAPIResponse(EResponseAction.InternalServerError);
            }
        }

        public async Task<APIResponse> GetBlockedList()
        {
            try
            {
                var currentUser = userHelper.GetCurrentUserGuidLogin();
                loggerManager.LogInfo(ETransaction.RUN, Helper.GetCurrentMethodName(), EOperation.SELECT);
                loggerManager.LogDebugObject(currentUser);
                var blockedList = await userRepository.GetBlockedList(currentUser);
                return new APIResponse
                {
                    Status = Status.Success,
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Payload = blockedList
                };
            }
            catch (Exception ex)
            {
                unitOfWork.RollbackTransaction();

                if (ex.Message == EResponseAction.Unauthorized.ToString())
                    return APIResponseHelper.ReturnAPIResponse(EResponseAction.Unauthorized);

                loggerManager.LogInfo(ETransaction.FAILED, Helper.GetCurrentMethodName(), EOperation.SELECT);
                loggerManager.LogException(ex);

                return APIResponseHelper.ReturnAPIResponse(EResponseAction.InternalServerError);
            }
        }

        public async Task<APIResponse> GetFriends(string filter)
        {
            try
            {
                var currentUser = userHelper.GetCurrentUserGuidLogin();
                loggerManager.LogInfo(ETransaction.RUN, Helper.GetCurrentMethodName(), EOperation.SELECT);
                loggerManager.LogDebugObject(currentUser);
                var friends = await userRepository.GetFriends(currentUser, filter);
                return new APIResponse
                {
                    Status = Status.Success,
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Payload = friends,
                    Message = friends.Any() ? $"Friend Count: {friends.Count()}" : "You don't have any friends with"
                };
            }
            catch (Exception ex)
            {
                unitOfWork.RollbackTransaction();

                if (ex.Message == EResponseAction.Unauthorized.ToString())
                    return APIResponseHelper.ReturnAPIResponse(EResponseAction.Unauthorized);

                loggerManager.LogInfo(ETransaction.FAILED, Helper.GetCurrentMethodName(), EOperation.SELECT);
                loggerManager.LogException(ex);

                return APIResponseHelper.ReturnAPIResponse(EResponseAction.InternalServerError);
            }
        }

        public async Task<APIResponse> DeleteFriend(Guid friendUserId)
        {
            try
            {
                var currentUser = userHelper.GetCurrentUserGuidLogin();
                loggerManager.LogInfo(ETransaction.RUN, Helper.GetCurrentMethodName(), EOperation.DELETE);
                loggerManager.LogDebugObject(currentUser);
                var result = await userRepository.DeleteFriend(currentUser, friendUserId);
                return new APIResponse
                {
                    Status = Status.Success,
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Message = result == true ? $"Successfully delete friend User Id {friendUserId}" : "Error Deleting a friend"
                };
            }
            catch (Exception ex)
            {
                unitOfWork.RollbackTransaction();

                if (ex.Message == EResponseAction.Unauthorized.ToString())
                    return APIResponseHelper.ReturnAPIResponse(EResponseAction.Unauthorized);

                loggerManager.LogInfo(ETransaction.FAILED, Helper.GetCurrentMethodName(), EOperation.DELETE);
                loggerManager.LogException(ex);

                return APIResponseHelper.ReturnAPIResponse(EResponseAction.InternalServerError);
            }
        }

        public async Task<APIResponse> CreateTeam(string teamName, List<Guid> memberUserId)
        {
            try
            {
                loggerManager.LogInfo(ETransaction.RUN, Helper.GetCurrentMethodName(), EOperation.INSERT);
                loggerManager.LogDebugObject(teamName);
                loggerManager.LogDebugObject(memberUserId);
                var result = await userRepository.CreateTeam(teamName, memberUserId);
                return new APIResponse
                {
                    Status = Status.Success,
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Message = result == true ? $"Successfully Created Team {teamName} with team members: {memberUserId}" : "Error Creating the Team"
                };
            }
            catch (Exception ex)
            {
                unitOfWork.RollbackTransaction();

                if (ex.Message == EResponseAction.Unauthorized.ToString())
                    return APIResponseHelper.ReturnAPIResponse(EResponseAction.Unauthorized);

                loggerManager.LogInfo(ETransaction.FAILED, Helper.GetCurrentMethodName(), EOperation.INSERT);
                loggerManager.LogException(ex);

                return APIResponseHelper.ReturnAPIResponse(EResponseAction.InternalServerError);
            }
        }

        public async Task<APIResponse> GetTeams(string filter)
        {
            try
            {
                var currentUser = userHelper.GetCurrentUserGuidLogin();
                loggerManager.LogInfo(ETransaction.RUN, Helper.GetCurrentMethodName(), EOperation.SELECT);
                loggerManager.LogDebugObject(currentUser);
                var teams = await userRepository.GetTeams(currentUser, filter);
                return new APIResponse
                {
                    Status = Status.Success,
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Payload = teams,
                    Message = teams.Any() ? $"Team Count: {teams.Count()}" : "You don't have any created teams"
                };
            }
            catch (Exception ex)
            {
                unitOfWork.RollbackTransaction();

                if (ex.Message == EResponseAction.Unauthorized.ToString())
                    return APIResponseHelper.ReturnAPIResponse(EResponseAction.Unauthorized);

                loggerManager.LogInfo(ETransaction.FAILED, Helper.GetCurrentMethodName(), EOperation.SELECT);
                loggerManager.LogException(ex);

                return APIResponseHelper.ReturnAPIResponse(EResponseAction.InternalServerError);
            }
        }

        public async Task<APIResponse> GetUserTeam(Guid userTeamId, string filter)
        {
            try
            {
                loggerManager.LogInfo(ETransaction.RUN, Helper.GetCurrentMethodName(), EOperation.SELECT);
                loggerManager.LogDebugObject(userTeamId);
                var team = await userRepository.GetUserTeam(userTeamId, filter);
                return new APIResponse
                {
                    Status = Status.Success,
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Payload = team,
                    Message = team != null ? $"Team Loaded Information: {team.TeamName}" : string.Empty
                };
            }
            catch (Exception ex)
            {
                unitOfWork.RollbackTransaction();

                if (ex.Message == EResponseAction.Unauthorized.ToString())
                    return APIResponseHelper.ReturnAPIResponse(EResponseAction.Unauthorized);

                loggerManager.LogInfo(ETransaction.FAILED, Helper.GetCurrentMethodName(), EOperation.SELECT);
                loggerManager.LogException(ex);

                return APIResponseHelper.ReturnAPIResponse(EResponseAction.InternalServerError);
            }
        }

        public async Task<APIResponse> EditTeam(UserTeamViewModel viewModel)
        {
            try
            {
                loggerManager.LogInfo(ETransaction.RUN, Helper.GetCurrentMethodName(), EOperation.UPDATE);
                loggerManager.LogDebugObject(viewModel);
                var result = await userRepository.EditTeam(viewModel);
                return new APIResponse
                {
                    Status = Status.Success,
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Message = result == true ? $"Updated Team for: {viewModel.TeamName}" : string.Empty
                };
            }
            catch (Exception ex)
            {
                unitOfWork.RollbackTransaction();

                if (ex.Message == EResponseAction.Unauthorized.ToString())
                    return APIResponseHelper.ReturnAPIResponse(EResponseAction.Unauthorized);

                loggerManager.LogInfo(ETransaction.FAILED, Helper.GetCurrentMethodName(), EOperation.UPDATE);
                loggerManager.LogException(ex);

                return APIResponseHelper.ReturnAPIResponse(EResponseAction.InternalServerError);
            }
        }

        public async Task<APIResponse> GetAllUsers(string filter)
        {
            try
            {
                loggerManager.LogInfo(ETransaction.RUN, Helper.GetCurrentMethodName(), EOperation.UPDATE);
                var currentUser = userHelper.GetCurrentUserGuidLogin();
                loggerManager.LogDebugObject(currentUser);
                var users = await userRepository.GetAllUsers(currentUser, filter);
                return new APIResponse
                {
                    Status = Status.Success,
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Payload = users,
                    Message = $"Retrieved Users not incuding current user {currentUser}"
                };
            }
            catch (Exception ex)
            {
                unitOfWork.RollbackTransaction();

                if (ex.Message == EResponseAction.Unauthorized.ToString())
                    return APIResponseHelper.ReturnAPIResponse(EResponseAction.Unauthorized);

                loggerManager.LogInfo(ETransaction.FAILED, Helper.GetCurrentMethodName(), EOperation.UPDATE);
                loggerManager.LogException(ex);

                return APIResponseHelper.ReturnAPIResponse(EResponseAction.InternalServerError);
            }
        }

        public async Task<APIResponse> AddedFriendRequests(string filter)
        {
            try
            {
                loggerManager.LogInfo(ETransaction.RUN, Helper.GetCurrentMethodName(), EOperation.SELECT);
                var currentUser = userHelper.GetCurrentUserGuidLogin();
                loggerManager.LogDebugObject(currentUser);
                var users = await userRepository.AddedFriendRequests(currentUser, filter);
                return new APIResponse
                {
                    Status = Status.Success,
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Payload = users,
                    Message = $"Retrieved Sent Friend Requests for user {currentUser}"
                };
            }
            catch (Exception ex)
            {
                unitOfWork.RollbackTransaction();

                if (ex.Message == EResponseAction.Unauthorized.ToString())
                    return APIResponseHelper.ReturnAPIResponse(EResponseAction.Unauthorized);

                loggerManager.LogInfo(ETransaction.FAILED, Helper.GetCurrentMethodName(), EOperation.SELECT);
                loggerManager.LogException(ex);

                return APIResponseHelper.ReturnAPIResponse(EResponseAction.InternalServerError);
            }
        }
    }
}
