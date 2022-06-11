
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using Sidekick.Api.DataAccessLayer.Interfaces;
using Sidekick.Api.Helpers;
using Sidekick.Model;
using System.Threading.Tasks;

namespace Sidekick.Api.Controllers
{
    [Authorize]
    [Produces("application/json")]
    [Route("api/FacilityUser")]
    public class FacilityUserController : APIBaseController
    {
        IFacilityUserRepository FacilityUserRepo { get; }
        private IMainHttpClient MainHttpClient { get; }
        private APIConfigurationManager MConf { get; }

        public FacilityUserController(IFacilityUserRepository _fuRepo, IMainHttpClient _mhttpc, APIConfigurationManager _conf)
        {
            FacilityUserRepo = _fuRepo;
            MainHttpClient = _mhttpc;
            MConf = _conf;
        }

        [AllowAnonymous]
        [HttpPost("Login")]
        public IActionResult LoginFacilityUser([FromBody] LoginCredentials _user)
        {
            if (ModelState.IsValid)
            {
                return Ok(FacilityUserRepo.LoginFacilityUser(_user));
            }
            else
            {
                return BadRequest(new APIResponse
                {
                    Message = "Invalid Model Object",
                    ModelError = ModelState.Errors(),
                    StatusCode = System.Net.HttpStatusCode.BadRequest
                });
            }
        }

        [HttpPost("Add")]
        public IActionResult AddFacilityUser([FromHeader] string Authorization, [FromBody] FacilityUserProfile _user)
        {
            if (ModelState.IsValid)
            {
                return Ok(FacilityUserRepo.AddFacilityUser(Authorization.Split(' ')[1], _user));
            }
            else
            {
                return BadRequest(new APIResponse
                {
                    Message = "Invalid Model Object",
                    ModelError = ModelState.Errors(),
                    StatusCode = System.Net.HttpStatusCode.BadRequest
                });
            }
        }

        [HttpPost("Edit")]
        public IActionResult EditFacilityUser([FromHeader] string Authorization, [FromBody] FacilityUserProfile _user)
        {
            ModelState.Remove("Password");
            ModelState.Remove("ConfirmPassword");

            if (ModelState.IsValid)
            {
                return Ok(FacilityUserRepo.EditFacilityUser(Authorization.Split(' ')[1], _user));
            }
            else
            {
                return BadRequest(new APIResponse
                {
                    Message = "Invalid Model Object",
                    ModelError = ModelState.Errors(),
                    StatusCode = System.Net.HttpStatusCode.BadRequest
                });
            }
        }

        [HttpGet("Get/{_guid}/{_takeAll}")]
        public IActionResult GetFacilityUser(Guid _guid, bool _takeAll)
        {
            APIResponse apiResp = FacilityUserRepo.GetFacilityUser(_guid, _takeAll);
            if (apiResp.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Ok(apiResp);
            }
            else
            {
                return BadRequest(apiResp);
            }
        }

        [HttpPost("ChangePassword")]
        public IActionResult FacilityUserChangePassword([FromHeader] string Authorization, [FromBody] FacilityUserChangePassword _user)
        {
            if (ModelState.IsValid)
            {
                return Ok(FacilityUserRepo.FacilityUserChangePassword(Authorization.Split(' ')[1], _user));
            }
            else
            {
                return BadRequest(new APIResponse
                {
                    Message = "Invalid Model Object",
                    ModelError = ModelState.Errors(),
                    StatusCode = System.Net.HttpStatusCode.BadRequest
                });
            }
        }

        [AllowAnonymous]
        [HttpPost("ForgotPassword")]
        public IActionResult FacilityUserForgotPassword([FromBody] FacilityUserForgotPassword _user)
        {
            if (ModelState.IsValid)
            {
                return Ok(FacilityUserRepo.FacilityUserForgotPassword(_user, MConf));
            }
            else
            {
                return BadRequest(new APIResponse
                {
                    Message = "Invalid Model Object",
                    ModelError = ModelState.Errors(),
                    StatusCode = System.Net.HttpStatusCode.BadRequest
                });
            }
        }

        [HttpPost("ChangeStatus")]
        public IActionResult FacilityUserChangeStatus([FromHeader] string Authorization, [FromBody] ChangeStatus _user)
        {
            if (ModelState.IsValid)
            {
                return Ok(FacilityUserRepo.FacilityUserChangeStatus(Authorization.Split(' ')[1], _user));
            }
            else
            {
                return BadRequest(new APIResponse
                {
                    Message = "Invalid Model Object",
                    ModelError = ModelState.Errors(),
                    StatusCode = System.Net.HttpStatusCode.BadRequest
                });
            }
        }
    }
}