
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Sidekick.Api.DataAccessLayer.Interfaces;
using Sidekick.Api.Helpers;
using Sidekick.Model;

namespace Sidekick.Api.Controllers
{
    [Produces("application/json")]
    [Route("api/SocialLogin")]
    public class SocialLoginController : Controller
    {
        ISocialLoginRepository MainRepo { get; }

        public SocialLoginController(ISocialLoginRepository _mRepo)
        {
            MainRepo = _mRepo;
        }

        [HttpPost("Apple")]
        public IActionResult AppleSignIn([FromBody] LoginAppleCredentials user)
        {
            APIResponse apiResp = MainRepo.AppleSignIn(user);
            if (apiResp.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Ok(apiResp);
            }

            return BadRequest(apiResp);
        }

        [HttpGet("AppleRefreshToken/{refreshToken}")]
        public IActionResult ValidateAppleToken(string refreshToken)
        {
            APIResponse apiResp = MainRepo.ValidateAppleToken(refreshToken);
            if (apiResp.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Ok(apiResp);
            }

            return BadRequest(apiResp);
        }

        [HttpGet("GetAppleClientSecret")]
        public IActionResult GetAppleClientSecret()
        {
            APIResponse apiResp = MainRepo.GetAppleClientSecret();
            if (apiResp.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Ok(apiResp);
            }

            return BadRequest(apiResp);
        }
    }
}
