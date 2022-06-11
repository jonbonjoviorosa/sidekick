using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sidekick.Api.Configurations.Resources;
using Sidekick.Api.DataAccessLayer.Interfaces;
using Sidekick.Api.Handlers.IBusiness;
using Sidekick.Api.Helpers;
using Sidekick.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sidekick.Api.Controllers
{
    [Authorize]
    [Produces("application/json")]
    [Route("api/User")]
    public class UserController : APIBaseController
    {
        private readonly IUserHandler userHandler;

        IUserRepository UserRepo { get; }
        private IMainHttpClient MainHttpClient { get; }
        private APIConfigurationManager MConf { get; }

        public UserController(IUserRepository _uRepo,
            IUserHandler userHandler,
            IMainHttpClient _mhttpc,
            APIConfigurationManager _conf)
        {
            UserRepo = _uRepo;
            this.userHandler = userHandler;
            MainHttpClient = _mhttpc;
            MConf = _conf;
        }

        [AllowAnonymous]
        [HttpPost("Register")]
        public IActionResult RegisterUser([FromBody] UserRegistration _user)
        {
            if (ModelState.IsValid)
            {
                APIResponse apiResp = UserRepo.Add(_user, MainHttpClient, MConf);
                if (apiResp.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return Ok(apiResp);
                }
                else
                {
                    return BadRequest(apiResp);
                }
            }
            else
            {
                return BadRequest(new APIResponse
                {
                    Message = "Invalid Model Object",
                    StatusCode = System.Net.HttpStatusCode.BadRequest,
                    Status = "Object level error.",
                    ModelError = ModelState.Errors()
                });
            }
        }

        [AllowAnonymous]
        [HttpPost("VerifyCode")]
        public IActionResult VerifyUserCode([FromBody] VerifyCode _verification)
        {
            if (ModelState.IsValid)
            {
                APIResponse returnResp = UserRepo.VerifyUserCode(_verification,MConf);
                if (returnResp.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return Ok(returnResp);
                }
                else
                {
                    return BadRequest(returnResp);
                }

            }
            else
            {
                return BadRequest(new APIResponse
                {
                    Message = "Invalid Model Object",
                    StatusCode = System.Net.HttpStatusCode.BadRequest,
                    Status = "Object level error.",
                    ModelError = ModelState.Errors()
                });
            }
        }

        [AllowAnonymous]
        [HttpPost("ResendCode")]
        public IActionResult ResendVerificationCode([FromBody] ResendCode _verification)
        {
            if (ModelState.IsValid)
            {
                APIResponse apiResp = UserRepo.ResendVerificationCode(_verification, MConf, MainHttpClient);
                if (apiResp.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return Ok(apiResp);
                }
                else
                {
                    return BadRequest(apiResp);
                }
            }
            else
            {
                return BadRequest(new APIResponse
                {
                    Message = "Invalid Model Object",
                    StatusCode = System.Net.HttpStatusCode.BadRequest,
                    Status = "Object level error.",
                    ModelError = ModelState.Errors()
                });
            }
        }

        [AllowAnonymous]
        [HttpPost("Login")]
        public IActionResult Login([FromBody] LoginCredentials _user)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new APIResponse
                {
                    Message = "Invalid Model Object",
                    ModelError = ModelState.Errors(),
                    StatusCode = System.Net.HttpStatusCode.BadRequest
                });
            }
            else
            {
                return Ok(UserRepo.Login(_user));
            }
        }

        [AllowAnonymous]
        [HttpPost("MobileLogout")]
        public IActionResult Logout([FromBody] LogoutCredentials _user)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new APIResponse
                {
                    Message = "Bad Request",
                    ModelError = ModelState.Errors(),
                    StatusCode = System.Net.HttpStatusCode.BadRequest
                });
            }
            else
            {
                return Ok(UserRepo.MobileLogout(_user));
            }
        }

        [HttpGet("Profile")]
        public async Task<ActionResult<APIResponse<UserProfile>>> GetUserProfile()
        {
            return Ok(await userHandler.GetUserProfile());
        }

        [HttpGet("GetCoachUserProfile/{userId}")]
        public async Task<ActionResult<APIResponse<CoachRenderViewModel>>> GetCoachUserProfile(Guid userId)
        {
            return Ok(await userHandler.GetCoachUserProfile(userId));
        }

        [AllowAnonymous]
        [HttpPost("ReGenerateTokens")]
        public IActionResult ReGenerateTokens([FromBody] UserLoginTransaction _loginParam)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new APIResponse
                {
                    Message = "Bad Request",
                    ModelError = ModelState.Errors(),
                    StatusCode = System.Net.HttpStatusCode.BadRequest
                });
            }
            else
            {
                return Ok(UserRepo.ReGenerateTokens(_loginParam));
            }
        }

        [Authorize]
        [HttpGet("SetFCMToken")]
        public IActionResult SetFCMToken([FromHeader] string Authorization, string FCMToken, EDevicePlatform DeviceType)
        {
            APIResponse returnResp = UserRepo.SetFCMToken(Authorization, FCMToken, DeviceType);
            if (returnResp.StatusCode != System.Net.HttpStatusCode.OK)
            {
                return BadRequest(returnResp);
            }

            return Ok(returnResp);
        }

        [AllowAnonymous]
        [HttpPost("ForgotPassword")]
        public IActionResult ForgotUserPassword([FromBody] UserForgotPassword _user)
        {
            if (ModelState.IsValid)
            {
                APIResponse apiResp = UserRepo.ForgotUserPassword(_user, MConf);
                if (apiResp.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return Ok(apiResp);
                }
                else
                {
                    return BadRequest(apiResp);
                }

            }
            else
            {
                return BadRequest(new APIResponse
                {
                    Message = "Invalid Model Object",
                    StatusCode = System.Net.HttpStatusCode.BadRequest,
                    Status = "Object level error.",
                    ModelError = ModelState.Errors()
                });
            }
        }

        [HttpPost("ChangePassword")]
        public IActionResult ChangeUserPassword([FromBody] UserChangePassword _user)
        {
            if (ModelState.IsValid)
            {
                APIResponse apiResp = UserRepo.ChangeUserPassword(_user);
                if (apiResp.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return Ok(apiResp);
                }
                else
                {
                    return BadRequest(apiResp);
                }

            }
            else
            {
                return BadRequest(new APIResponse
                {
                    Message = "Invalid Model Object",
                    StatusCode = System.Net.HttpStatusCode.BadRequest,
                    Status = "Object level error.",
                    ModelError = ModelState.Errors()
                });
            }
        }

        [HttpPost("UpdateUserProfile")]
        [Authorize]
        public async Task<IActionResult> UpdateUserProfle([FromBody] UserProfile _userProfile)
        {
            APIResponse apiResp = await userHandler.UpdateUserProfile(_userProfile);
            if (apiResp.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Ok(apiResp);
            }
            else
            {
                return BadRequest(apiResp);
            }
        }

        [HttpGet("ProfileProgress")]
        [Authorize]
        public async Task<ActionResult<UserProfileProgressViewModel>> GetUserProfileProgress()
        {
            return Ok(await userHandler.GetUserProfileProgress());
        }

        [HttpGet("UpdateUserStatus/{id}")]
        public IActionResult UpdateUserStatus([FromHeader] string Authorization, int id)
        {
            APIResponse apiResp = UserRepo.UpdateUserStatus(Authorization.Split(' ')[1], id);
            if (apiResp.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Ok(apiResp);
            }
            else
            {
                return BadRequest(apiResp);
            }
        }


        [HttpGet("GetAllUsers")]
        public async Task<IActionResult> GetAllUsers(string filter)
        {
            var response = await userHandler.GetAllUsers(filter);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }

        [HttpGet("GetCountries")]
        public IActionResult GetCountryList()
        {
            //CountryList country = new CountryList();
            APIResponse apiResp = new APIResponse();
            apiResp = UserRepo.GetCountries();
            //apiResp.StatusCode = System.Net.HttpStatusCode.OK;
            //apiResp.Payload = country.countrylists;
            if (apiResp.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Ok(apiResp);
            }
            else
            {
                return BadRequest(apiResp);
            }
        }

        [HttpPost("UpdateProfileProgress")]
        public async Task<ActionResult<APIResponse>> UpdateProfileProgress([FromBody] UserProfileProgress_UpdateViewModel progress)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new APIResponse
                {
                    Message = Messages.InvalidModelObject,
                    ModelError = ModelState.Errors(),
                    StatusCode = System.Net.HttpStatusCode.BadRequest
                });
            }
            else
            {
                var response = await userHandler.UpdateProfileProgress(progress);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return Ok(response);
                }
                else
                {
                    return BadRequest(response);
                }
            }
        }

        [HttpGet("GetFriendRequest/{friendUserId}")]
        public async Task<ActionResult<APIResponse>> GetFriendRequest(Guid friendUserId)
        {
            var response = await userHandler.GetFriendRequest(friendUserId);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }

        [HttpPost("AcceptFriendRequest/{friendUserId}")]
        public async Task<ActionResult<APIResponse>> AcceptFriendRequest(Guid friendUserId)
        {
            var response = await userHandler.AcceptFriendRequest(friendUserId);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }

        [HttpGet("GetAllFriendRequest")]
        public async Task<ActionResult<APIResponse>> GetAllFriendRequest(string filter)
        {
            var response = await userHandler.GetAllFriendRequest(filter);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }

        [HttpPost("DeclineFriendRequest/{userFriendRequestId}")]
        public async Task<ActionResult<APIResponse>> DeclineFriendRequest(Guid userFriendRequestId)
        {
            var response = await userHandler.DeclineFriendRequest(userFriendRequestId);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }

        [HttpPost("AddFriend/{friendUserId}")]
        public async Task<ActionResult<APIResponse>> AddFriend(Guid friendUserId)
        {
            var response = await userHandler.AddToFriendRequest(friendUserId);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }

        [HttpGet("GetFriendDetail/{friendUserId}")]
        public async Task<ActionResult<APIResponse>> GetFriendDetail(Guid friendUserId)
        {
            var response = await userHandler.GetFriendDetail(friendUserId);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }

        [HttpPost("ReportUser/{reason}/{userId}")]
        public async Task<ActionResult<APIResponse>> ReportUser(string reason, Guid userId)
        {
            var response = await userHandler.ReportFriend(reason, userId);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }

        [HttpGet("GetReports/{friendUserId}")]
        public async Task<ActionResult<APIResponse>> GetReports(Guid friendUserId)
        {
            var response = await userHandler.GetReports(friendUserId);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }

        [HttpGet("GetAllReports")]
        public async Task<ActionResult<APIResponse>> GetReports()
        {
            var response = await UserRepo.GetAllReports();
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }

        [HttpGet("GetUsers")]
        public async Task<ActionResult<APIResponse>> GetUsers([FromHeader] string Authorization)
        {
            var response = await UserRepo.GetUserList(Authorization.Split(' ')[1]);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }

        [HttpPost("BlockUser/{userId}")]
        public async Task<ActionResult<APIResponse>> BlockUser(Guid userId)
        {
            var response = await userHandler.BlockUser(userId);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }

        [HttpPost("UnBlockUser/{userId}")]
        public async Task<ActionResult<APIResponse>> UnBlockUser(Guid userId)
        {
            var response = await userHandler.UnBlockUser(userId);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }

        [HttpGet("BlockedList")]
        public async Task<ActionResult<APIResponse>> BlockedList()
        {
            var response = await userHandler.GetBlockedList();
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }

        [HttpGet("MyFriendList")]
        public async Task<ActionResult<APIResponse>> MyFriendList(string filter)
        {
            var response = await userHandler.GetFriends(filter);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }

        [HttpGet("DeleteFriend/{friendUserId}")]
        public async Task<ActionResult<APIResponse>> DeleteFriend(Guid friendUserId)
        {
            var response = await userHandler.DeleteFriend(friendUserId);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }

        [HttpPost("CreateTeam/{teamName}")]
        public async Task<ActionResult<APIResponse>> CreateTeam(string teamName, List<Guid> memberUserId)
        {
            var response = await userHandler.CreateTeam(teamName, memberUserId);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }

        [HttpGet("GetUserTeams")]
        public async Task<ActionResult<APIResponse>> GetUserTeams(string filter)
        {
            var response = await userHandler.GetTeams(filter);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }

        [HttpGet("GetUserTeam/{userTeamId}")]
        public async Task<ActionResult<APIResponse>> GetUserTeam(Guid userTeamId, string filter)
        {
            var response = await userHandler.GetUserTeam(userTeamId, filter);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }

        [HttpPost("EditTeam")]
        public async Task<ActionResult<APIResponse>> EditTeam([FromBody] UserTeamViewModel viewModel)
        {
            var response = await userHandler.EditTeam(viewModel);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }

        [HttpGet("AddedFriendRequests")]
        public async Task<ActionResult<APIResponse>> AddedFriendRequests(string filter)
        {
            var response = await userHandler.AddedFriendRequests(filter);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }

        [HttpPost("UpdateReport")]
        public async Task<ActionResult<APIResponse>> UpdateReport([FromHeader] string Authorization, [FromBody] ReportDto report)
        {
            var response = new APIResponse();
            if (ModelState.IsValid)
            {
                response = await UserRepo.UpdateReport(Authorization.Split(' ')[1], report);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return Ok(response);
                }
            }

            return BadRequest(response);
        }

        [HttpPost("UpdateUser")]
        public async Task<ActionResult<APIResponse>> UpdateUser([FromHeader] string Authorization, [FromBody] ReportDto report)
        {
            var response = new APIResponse();
            if (ModelState.IsValid)
            {
                response = await UserRepo.UpdateUser(Authorization.Split(' ')[1], report);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return Ok(response);
                }
            }

            return BadRequest(response);
        }

        [HttpPost("DeleteReport")]
        public async Task<ActionResult<APIResponse>> DeleteReport([FromHeader] string Authorization, [FromBody] ReportDto report)
        {
            var response = new APIResponse();
            if (ModelState.IsValid)
            {
                response = await UserRepo.DeleteReport(Authorization.Split(' ')[1], report);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return Ok(response);
                }

            }

            return BadRequest(response);
        }

        [HttpPost("ChangeStatus")]
        public async Task<ActionResult<APIResponse>> ChangeStatus([FromHeader] string Authorization, [FromBody]ChangeStatus user)
        {
            var response = await UserRepo.ChangeStatus(Authorization.Split(' ')[1], user);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }

        [HttpPost("SendRequest")]
        public async Task<ActionResult<APIResponse>> SendRequest([FromBody] RequestViewModel model)
        {
            var response = await UserRepo.SendRequest(model);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }

        [HttpGet("GetUserRequests")]
        public async Task<ActionResult<APIResponse>> GetUserRequests()
        {
            var response = await UserRepo.GetUserRequests();
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }

        [HttpGet("GetCoachRequests")]
        public async Task<ActionResult<APIResponse>> GetCoachRequests()
        {
            var response = await UserRepo.GetCoachRequests();
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }

        [HttpPost("UpdateUserRequest")]
        public async Task<ActionResult<APIResponse>> UpdateUserRequest([FromHeader] string Authorization, [FromBody] UserRequestViewModel request)
        {
            var response = await UserRepo.UpdateUserRequest(Authorization.Split(' ')[1], request);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }

        [HttpPost("UpdateCoachRequest")]
        public async Task<ActionResult<APIResponse>> UpdateCoachRequest([FromHeader] string Authorization, [FromBody] CoachRequestViewModel request)
        {
            var response = await UserRepo.UpdateCoachRequest(Authorization.Split(' ')[1], request);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }

        [HttpGet("GetUserUpdates")]
        public async Task<ActionResult<APIResponse>> GetUserUpdates()
        {
            var response = await UserRepo.GetUserUpdates();
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }

        [HttpGet("GetUserUpdatesByDate")]
        public async Task<ActionResult<APIResponse>> GetUserUpdatesByDate(string date)
        {
            var response = await UserRepo.GetUserUpdatesByDate(date);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }

        [HttpGet("GetAllActivities")]
        public async Task<ActionResult<APIResponse>> GetAllActivities()
        {
            var response = await UserRepo.GetAllActivities();
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }

        [HttpGet("GetAllRequest")]
        public async Task<ActionResult<APIResponse>> GetAllRequest()
        {
            var response = await UserRepo.GetAllRequest();
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }

        [HttpGet("GetAllWhatsNew")]
        public async Task<ActionResult<APIResponse>> GetAllWhatsNew()
        {
            var response = await UserRepo.GetAllWhatsNew();
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }

        [HttpPost("AddUserReviews")]
        public async Task<ActionResult<APIResponse>> AddUserReviews([FromBody] UserReviews model)
        {
            var response = await UserRepo.AddUserReviews(model);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }

        [HttpGet("GetUserReviews")]
        public async Task<ActionResult<APIResponse>> GetUserReviews()
        {
            var response = await UserRepo.GetUserReviews();
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }

        [HttpGet("GetCoachToReview")]
        public async Task<ActionResult<APIResponse>> GetCoachToReview()
        {
            var response = await UserRepo.GetCoachToReview();
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }
    }
}
