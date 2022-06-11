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
    [Route("api/Banner")]
    public class BannerController : APIBaseController
    {
        IBannerRepository BannerRepo { get; }
        private IMainHttpClient MainHttpClient { get; }
        private APIConfigurationManager MConf { get; }

        public BannerController(IBannerRepository _bRepo, IMainHttpClient _mhttpc, APIConfigurationManager _conf)
        {
            BannerRepo = _bRepo;
            MainHttpClient = _mhttpc;
            MConf = _conf;
        }

        [HttpPost("Add")]
        public IActionResult Add([FromHeader] string Authorization, [FromBody] BannerDto _banner)
        {
            if (ModelState.IsValid)
            {
                return Ok(BannerRepo.Add(Authorization.Split(' ')[1], _banner));
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
        public IActionResult Edit([FromHeader] string Authorization, [FromBody] BannerDto _banner)
        {
            if (ModelState.IsValid)
            {
                return Ok(BannerRepo.Edit(Authorization.Split(' ')[1], _banner));
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

        [HttpGet("List")]
        public IActionResult List()
        {
            APIResponse apiResp = BannerRepo.List();
            if (apiResp.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Ok(apiResp);
            }
            else
            {
                return BadRequest(apiResp);
            }
        }

        [HttpGet("GetBanner/{_bannerID}")]
        public IActionResult GetBanner(Guid _bannerID)
        {
            APIResponse apiResp = BannerRepo.GetBanner(_bannerID);
            if (apiResp.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Ok(apiResp);
            }
            else
            {
                return BadRequest(apiResp);
            }
        }

        [HttpGet("FacilityBanner/{_facilityID}")]
        public IActionResult FacilityBanner(Guid _facilityID)
        {
            APIResponse apiResp = BannerRepo.FacilityBanner(_facilityID);
            if (apiResp.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Ok(apiResp);
            }
            else
            {
                return BadRequest(apiResp);
            }
        }

        [HttpPost("Delete/{_bannerID}")]
        public async Task<APIResponse> Delete(Guid _bannerID)
        {
            return await BannerRepo.Delete(_bannerID);
        }
    }
}
