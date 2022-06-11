
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using Sidekick.Api.DataAccessLayer.Interfaces;
using Sidekick.Api.Helpers;
using Sidekick.Model;

namespace Sidekick.Api.Controllers
{
    [Authorize]
    [Produces("application/json")]
    [Route("api/Admin")]
    public class AdminController : APIBaseController
    {
        IAdminRepository AdminRepo { get; }
        private IMainHttpClient MainHttpClient { get; }
        private APIConfigurationManager MConf { get; }

        public AdminController(IAdminRepository _aRepo, IMainHttpClient _mhttpc, APIConfigurationManager _conf)
        {
            AdminRepo = _aRepo;
            MainHttpClient = _mhttpc;
            MConf = _conf;
        }

        [HttpPost("RegisterAdmin")]
        public IActionResult RegisterAdmin([FromBody] AdminProfile _admin)
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
                return Ok(AdminRepo.RegisterAdmin(_admin));
            }
        }

        [AllowAnonymous]
        [HttpPost("Login")]
        public IActionResult AdminLogin([FromBody] LoginCredentials _admin)
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
                return Ok(AdminRepo.AdminLogin(_admin));
            }
        }

        [AllowAnonymous]
        [HttpPost("ReGenerateTokens")]
        public IActionResult ReGenerateTokens([FromBody] AdminLoginTransaction _admin)
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
                return Ok(AdminRepo.ReGenerateTokens(_admin));
            }
        }

        [HttpGet("AllAdmins/{_shop}/{_admin}")]
        public IActionResult GetAllAdmins(Guid _shop, int _admin)
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
                return Ok(AdminRepo.GetAllAdmins(_admin));
            }
        }

        [HttpPost("EditAdminProfile")]
        public IActionResult EditAdminProfile([FromBody] AdminProfile _admin)
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
                return Ok(AdminRepo.EditAdminProfile(_admin));
            }
        }

        [HttpPost("ChangeAdminStatus")]
        public IActionResult ChangeAdminStatus([FromBody] ChangeRecordStatus _admin,[FromHeader] string Authorization)
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
                return Ok(AdminRepo.ChangeAdminStatus(_admin, Authorization.Split(' ')[1]));
            }
        }

        [AllowAnonymous]
        [HttpPost("ForgotPassword")]
        public IActionResult ForgotPassword([FromBody] AdminForgotPassword _admin)
        {
            if (ModelState.IsValid)
            {
                APIResponse apiResp = AdminRepo.ForgotPassword(_admin, MainHttpClient, MConf);
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
    }
}